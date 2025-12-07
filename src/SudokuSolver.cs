namespace Sudoku
{
    public class SudokuMasks
    {
        private readonly int _size;
        private readonly int _blockSize;

        public long[] RowMask;
        public long[] ColMask;
        public long[] BlockMask;
        private long AllOnesMask;

        public SudokuMasks(int blockSize, SudokuMasks? instanceToCopy = null)
        {
            _blockSize = blockSize;
            _size = blockSize * blockSize;

            if (instanceToCopy == null)
            {
                RowMask = new long[_size];
                ColMask = new long[_size];
                BlockMask = new long[_size];
                AllOnesMask = _size >= 64 ? ~0L : (1L << _size) - 1L;
            }
            else
            {
                // Copiar matrices de máscaras
                RowMask = (long[])instanceToCopy.RowMask.Clone();
                ColMask = (long[])instanceToCopy.ColMask.Clone();
                BlockMask = (long[])instanceToCopy.BlockMask.Clone();
                AllOnesMask = instanceToCopy.AllOnesMask;
            }
        }

        private int GetBlockIndex(int row, int col)
        {
            return (row / _blockSize * _blockSize) + (col / _blockSize);
        }

        public void SetBit(int row, int col, long bit)
        {
            RowMask[row] |= bit;
            ColMask[col] |= bit;
            BlockMask[GetBlockIndex(row, col)] |= bit;
        }

        public void ClearBit(int row, int col, long bit)
        {
            RowMask[row] &= bit;
            ColMask[col] &= bit;
            BlockMask[GetBlockIndex(row, col)] &= bit;
        }

        public long GetCandidateMask(int row, int col)
        {
            long used = RowMask[row] | ColMask[col] | BlockMask[GetBlockIndex(row, col)];
            return (~used) & AllOnesMask;
        }
    }

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

            int size = sudoku.Size;
            int blockSize = sudoku.BlockSize;
            int[,] initialGrid = CopyGrid(sudoku.Grid, size);

            var initialMasks = new SudokuMasks(blockSize);
            var emptyCells = new List<(int row, int col)>();

            // Inicializar máscaras y lista de celdas vacías
            for (int row = 0; row < size; row++)
                for (int col = 0; col < size; col++)
                {
                    int cellValue = initialGrid[row, col];
                    if (cellValue == 0)
                    {
                        emptyCells.Add((row, col));
                        continue;
                    }

                    initialMasks.SetBit(row, col, GetBit(cellValue));
                }

            // No hay celdas vacias, ya está completo
            if (emptyCells.Count == 0)
            {
                return new SudokuBoard(blockSize) { Grid = initialGrid };
            }

            // Buscar celda vacía con menos candidatos
            var (initialCellIndex, initialCandidateMask, deadEnd) = FindMRV(emptyCells, initialMasks);
            if (deadEnd)
            {
                // Sudoku imposible
                return null;
            }

            var initialCandidateValues = GetNumericalCandidates(size, initialCandidateMask);
            var emptiesWithoutInitial = GetEmptiesWithoutInitial(emptyCells, initialCellIndex);
            var (initialRow, initialCol) = emptyCells[initialCellIndex];

            // Control de concurrencia
            var cts = new CancellationTokenSource();
            var token = cts.Token;
            object solutionLock = new object();
            bool foundSolution = false;
            int[,]? solvedGrid = null;

            // Versión secuencial
            if (maxDegreeOfParallelism == 1 || initialCandidateValues.Count == 1)
            {
                foreach (var candidateValue in initialCandidateValues)
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    int[,] localGrid = CopyGrid(initialGrid, size);
                    var localMasks = new SudokuMasks(blockSize, initialMasks);

                    // Aplicar el candidato
                    localGrid[initialRow, initialCol] = candidateValue;
                    localMasks.SetBit(initialRow, initialCol, GetBit(candidateValue));

                    if (Backtrack(emptiesWithoutInitial, localGrid, localMasks, size, blockSize, token, out int[,] finished))
                    {
                        solvedGrid = finished;
                        foundSolution = true;
                        break;
                    }
                }
            }

            // Versión paralela
            else
            {
                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = maxDegreeOfParallelism,
                    CancellationToken = token
                };

                try
                {
                    Parallel.ForEach(initialCandidateValues, parallelOptions, (candidateValue, loopState) =>
                    {
                        if (token.IsCancellationRequested)
                        {
                            return;
                        }

                        int[,] localGrid = CopyGrid(initialGrid, size);
                        var localMasks = new SudokuMasks(blockSize, initialMasks);

                        localGrid[initialRow, initialCol] = candidateValue;
                        localMasks.SetBit(initialRow, initialCol, GetBit(candidateValue));

                        if (Backtrack(emptiesWithoutInitial, localGrid, localMasks, size, blockSize, token, out int[,] finished))
                        {
                            lock (solutionLock)
                            {
                                if (!foundSolution)
                                {
                                    foundSolution = true;
                                    solvedGrid = finished;
                                    cts.Cancel();
                                    loopState.Stop();
                                }
                            }
                        }
                    });
                }
                // Cancelación esperada al encontrar solución
                catch (OperationCanceledException) { }
            }

            if (foundSolution && solvedGrid != null)
            {
                return new SudokuBoard(blockSize) { Grid = solvedGrid };
            }

            return null;
        }

        private static bool Backtrack(
            List<(int row, int col)> emptyCells,
            int[,] grid,
            SudokuMasks masks,
            int size,
            int blockSize,
            CancellationToken token,
            out int[,] finishedGrid)
        {
            finishedGrid = null!;
            if (token.IsCancellationRequested)
            {
                return false;
            }

            if (emptyCells.Count == 0)
            {
                finishedGrid = CopyGrid(grid, size);
                return true;
            }

            // Buscar celda vacía con menos candidatos
            var (cellIndex, candidateMask, deadEnd) = FindMRV(emptyCells, masks);
            if (deadEnd)
            {
                return false;
            }

            var candidateValues = GetNumericalCandidates(size, candidateMask);
            var nextEmpties = GetEmptiesWithoutInitial(emptyCells, cellIndex);
            var (row, col) = emptyCells[cellIndex];

            foreach (int candidateValue in candidateValues)
            {
                if (token.IsCancellationRequested)
                {
                    return false;
                }

                long bit = GetBit(candidateValue);

                grid[row, col] = candidateValue;
                masks.SetBit(row, col, bit);

                if (Backtrack(nextEmpties, grid, masks, size, blockSize, token, out finishedGrid))
                {
                    return true;
                }

                // Deshacer
                grid[row, col] = 0;
                masks.ClearBit(row, col, ~bit);
            }

            return false;
        }


        private static (int cellIndex, long cellCandidateMask, bool deadEnd) FindMRV(
            List<(int row, int col)> emptyCells, SudokuMasks masks)
        {
            int bestIndex = -1;
            int bestCount = int.MaxValue;
            long bestMask = 0L;

            for (int i = 0; i < emptyCells.Count; i++)
            {
                var (row, col) = emptyCells[i];
                long mask = masks.GetCandidateMask(row, col);

                int count = CountBits(mask);
                if (count == 0)
                {
                    // Dead end detectado aquí
                    return (-1, 0L, true);
                }

                if (count < bestCount)
                {
                    bestCount = count;
                    bestIndex = i;
                    bestMask = mask;
                    if (bestCount == 1)
                    {
                        break;
                    }
                }
            }

            return (bestIndex, bestMask, false);
        }

        private static List<int> GetNumericalCandidates(int highestCandidate, long candidateMask)
        {
            var result = new List<int>();
            for (int candidate = 1; candidate <= highestCandidate; candidate++)
            {
                if (((candidateMask >> (candidate - 1)) & 1L) == 0)
                {
                    continue;
                }

                result.Add(candidate);
            }

            return result;
        }

        private static List<(int row, int col)> GetEmptiesWithoutInitial(
            List<(int row, int col)> emptyCells,
            int initialCellIndex)
        {
            var result = new List<(int row, int col)>(emptyCells.Count - 1);
            for (int i = 0; i < emptyCells.Count; i++)
            {
                if (i == initialCellIndex)
                {
                    continue;
                }

                result.Add(emptyCells[i]);
            }

            return result;
        }

        #region Helpers

        private static long GetBit(int value)
        {
            return 1L << (value - 1);
        }

        private static int CountBits(long value)
        {
            int count = 0;
            while (value != 0)
            {
                value &= value - 1;
                count++;
            }

            return count;
        }

        private static int[,] CopyGrid(int[,] grid, int gridSize)
        {
            var newGrid = new int[gridSize, gridSize];
            for (int row = 0; row < gridSize; row++)
                for (int col = 0; col < gridSize; col++)
                    newGrid[row, col] = grid[row, col];

            return newGrid;
        }

        #endregion
    }
}
