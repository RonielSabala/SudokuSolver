namespace Sudoku
{
    public static class SudokuSolver
    {
        public static SudokuBoard? TrySolve(SudokuBoard sudoku, int maxDegreeOfParallelism)
        {
            if (!sudoku.IsValid())
            {
                return null;
            }

            if (maxDegreeOfParallelism < 1)
            {
                maxDegreeOfParallelism = 1;
            }

            int blockSize = sudoku.BlockSize;
            int size = sudoku.Size;
            int[,] initialGrid = CopyGrid(sudoku.Grid, size);

            // Máscaras
            long[] rowMask = new long[size];
            long[] colMask = new long[size];
            long[] blockMask = new long[size];
            long allOnesMask = (size >= 64) ? ~0L : ((1L << size) - 1L);

            // Celdas vacías
            var emptyCells = new List<(int row, int col)>();

            // Incializar variables
            for (int row = 0; row < size; row++)
                for (int col = 0; col < size; col++)
                {
                    int cellValue = initialGrid[row, col];
                    if (cellValue == 0)
                    {
                        emptyCells.Add((row, col));
                        continue;
                    }

                    SetCell(initialGrid, rowMask, colMask, blockMask, blockSize, size, row, col, cellValue);
                }

            // Si no hay celdas vacías ya está resuelto
            if (emptyCells.Count == 0)
            {
                var solution = new SudokuBoard(blockSize);
                solution.Grid = initialGrid;
                return solution;
            }

            // Elegir la celda MRV (mínimos candidatos) para la paralelización inicial
            int firstIndex = -1;
            int minCandidatesForFirst = int.MaxValue;
            long[] initialCandidatesMasks = new long[emptyCells.Count];
            for (int i = 0; i < emptyCells.Count; i++)
            {
                var (row, col) = emptyCells[i];
                int blockIndex = (row / blockSize * blockSize) + (col / blockSize);
                long usedMask = rowMask[row] | colMask[col] | blockMask[blockIndex];
                long candidatesMask = (~usedMask) & allOnesMask;
                initialCandidatesMasks[i] = candidatesMask;

                int candidateCount = CountBits(candidatesMask);
                if (candidateCount == 0)
                {
                    // Celda sin candidatos, es decir, tablero imposible
                    return null;
                }

                if (candidateCount < minCandidatesForFirst)
                {
                    minCandidatesForFirst = candidateCount;
                    firstIndex = i;
                }
            }

            // Candidatos para la celda inicial (MRV)
            var initialCell = emptyCells[firstIndex];
            long initialCandidatesMask = initialCandidatesMasks[firstIndex];
            var initialCandidateValues = new List<int>();
            for (int v = 0; v < size; v++)
                if (((initialCandidatesMask >> v) & 1L) != 0)
                    initialCandidateValues.Add(v + 1);

            // Lista de empties sin la celda inicial
            var emptiesWithoutInitial = new List<(int row, int col)>();
            for (int i = 0; i < emptyCells.Count; i++)
            {
                if (i == firstIndex)
                {
                    continue;
                }

                emptiesWithoutInitial.Add(emptyCells[i]);
            }

            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var semaphore = new SemaphoreSlim(maxDegreeOfParallelism, maxDegreeOfParallelism);

            object solutionLock = new object();
            bool foundSolution = false;
            int[,]? solvedGrid = null;

            var tasks = new List<Task>();

            // Versión secuencial
            if (maxDegreeOfParallelism == 1 || initialCandidateValues.Count == 1)
            {
                // Ejecutar secuencialmente sobre candidatos hasta encontrar solución
                foreach (var candidateValue in initialCandidateValues)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    // Copiar estructuras locales
                    int[,] localGrid = CopyGrid(initialGrid, size);
                    long[] localRowMask = (long[])rowMask.Clone();
                    long[] localColMask = (long[])colMask.Clone();
                    long[] localBlockMask = (long[])blockMask.Clone();

                    SetCell(localGrid, localRowMask, localColMask, localBlockMask, blockSize, size, initialCell.row, initialCell.col, candidateValue);

                    if (Backtrack(emptiesWithoutInitial, localGrid, localRowMask, localColMask, localBlockMask, blockSize, allOnesMask, cancellationToken, out int[,] finishedGrid))
                    {
                        lock (solutionLock)
                        {
                            if (!foundSolution)
                            {
                                foundSolution = true;
                                solvedGrid = finishedGrid;
                                cts.Cancel();
                            }
                        }

                        break;
                    }
                }

                if (!foundSolution)
                {
                    return null;
                }

                var solution = new SudokuBoard(blockSize);
                solution.Grid = solvedGrid;
                return solution;
            }

            // Versión paralela
            foreach (var candidateValue in initialCandidateValues)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                semaphore.Wait(cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                {
                    semaphore.Release();
                    break;
                }

                // Crear copia local para la tarea
                int[,] localGrid = CopyGrid(initialGrid, size);
                long[] localRowMask = (long[])rowMask.Clone();
                long[] localColMask = (long[])colMask.Clone();
                long[] localBlockMask = (long[])blockMask.Clone();

                SetCell(localGrid, localRowMask, localColMask, localBlockMask, blockSize, size, initialCell.row, initialCell.col, candidateValue);

                var task = Task.Run(() =>
                {
                    try
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return;
                        }

                        if (Backtrack(emptiesWithoutInitial, localGrid, localRowMask, localColMask, localBlockMask, blockSize, allOnesMask, cancellationToken, out int[,] finishedGrid))
                        {
                            lock (solutionLock)
                            {
                                if (!foundSolution)
                                {
                                    foundSolution = true;
                                    solvedGrid = finishedGrid;

                                    // Cancelar las demás tareas
                                    cts.Cancel();
                                }
                            }
                        }
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }, cancellationToken);

                tasks.Add(task);
            }

            // Esperar a que todas las tareas terminen o se cancelen
            try
            {
                Task.WaitAll(tasks.ToArray());
            }
            catch (AggregateException)
            {
                // Ignorar excepciones de tareas canceladas
            }
            catch (OperationCanceledException)
            {
                // Ignorar
            }

            if (foundSolution && solvedGrid != null)
            {
                var solution = new SudokuBoard(blockSize);
                solution.Grid = solvedGrid;
                return solution;
            }

            return null;
        }

        #region Helpers

        private static int CountBits(long x)
        {
            int count = 0;
            while (x != 0)
            {
                x &= x - 1;
                count++;
            }

            return count;
        }

        private static int[,] CopyGrid(int[,] source, int size)
        {
            int[,] destiny = new int[size, size];
            for (int row = 0; row < size; row++)
                for (int col = 0; col < size; col++)
                    destiny[row, col] = source[row, col];

            return destiny;
        }

        private static void SetCell(int[,] grid, long[] rowMask, long[] colMask, long[] blockMask, int blockSize, int size, int row, int col, int value)
        {
            int blockIndex = (row / blockSize * blockSize) + (col / blockSize);
            long bit = 1L << (value - 1);

            grid[row, col] = value;
            rowMask[row] |= bit;
            colMask[col] |= bit;
            blockMask[blockIndex] |= bit;
        }

        private static bool Backtrack(List<(int row, int col)> empties, int[,] grid, long[] rowMask, long[] colMask, long[] blockMask, int blockSize, long allOnesMask, CancellationToken ct, out int[,] finishedGrid)
        {
            finishedGrid = null;
            if (ct.IsCancellationRequested)
            {
                return false;
            }

            if (empties.Count == 0)
            {
                finishedGrid = CopyGrid(grid, grid.GetLength(0));
                return true;
            }

            // Elegir siguiente celda MRV entre 'empties'
            int bestIndex = -1;
            int bestCount = int.MaxValue;
            long bestCandidatesMask = 0L;

            for (int i = 0; i < empties.Count; i++)
            {
                var (row, col) = empties[i];
                int blockIndex = (row / blockSize * blockSize) + (col / blockSize);
                long used = rowMask[row] | colMask[col] | blockMask[blockIndex];
                long candidatesMask = (~used) & allOnesMask;
                int count = CountBits(candidatesMask);
                if (count == 0)
                {
                    // Dead end
                    return false;
                }
                if (count < bestCount)
                {
                    bestCount = count;
                    bestIndex = i;
                    bestCandidatesMask = candidatesMask;

                    // Óptimo local
                    if (bestCount == 1)
                    {
                        break;
                    }
                }
            }

            // Extraer la celda seleccionada y construir nuevos empties
            var chosenCell = empties[bestIndex];

            // Preparar nueva lista de empties sin la elegida
            var newEmpties = new List<(int row, int col)>(empties.Count - 1);
            for (int i = 0; i < empties.Count; i++)
            {
                if (i == bestIndex) continue;
                newEmpties.Add(empties[i]);
            }

            int rowChosen = chosenCell.row;
            int colChosen = chosenCell.col;
            int blockIdxChosen = (rowChosen / blockSize * blockSize) + (colChosen / blockSize);

            int size = grid.GetLength(0);

            // Iterar por cada candidato
            for (int v = 0; v < size; v++)
            {
                if (ct.IsCancellationRequested) return false;

                if (((bestCandidatesMask >> v) & 1L) == 0) continue;
                int value = v + 1;
                long bit = 1L << v;

                // Colocar
                grid[rowChosen, colChosen] = value;
                rowMask[rowChosen] |= bit;
                colMask[colChosen] |= bit;
                blockMask[blockIdxChosen] |= bit;

                // Recurse
                if (Backtrack(newEmpties, grid, rowMask, colMask, blockMask, blockSize, allOnesMask, ct, out finishedGrid))
                {
                    return true;
                }

                // Deshacer
                grid[rowChosen, colChosen] = 0;
                rowMask[rowChosen] &= ~bit;
                colMask[colChosen] &= ~bit;
                blockMask[blockIdxChosen] &= ~bit;
            }

            return false;
        }

        #endregion
    }
}
