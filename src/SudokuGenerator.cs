namespace Sudoku
{
    internal static class Utils
    {
        public static void Shuffle<T>(T[] array, Random rng)
        {
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (array[i], array[j]) = (array[j], array[i]);
            }
        }

        public static int[] GeneratePermutedIndex(int blockSize, Random rng)
        {
            var result = new List<int>(blockSize * blockSize);
            var bandIndices = Enumerable.Range(0, blockSize).ToArray();
            Shuffle(bandIndices, rng);

            foreach (var band in bandIndices)
            {
                var local = Enumerable.Range(band * blockSize, blockSize).ToArray();
                Shuffle(local, rng);
                result.AddRange(local);
            }

            return result.ToArray();
        }
    }

    public class SudokuGenerator
    {
        private SudokuBoard _sudoku;
        private readonly Random _rng;

        public SudokuGenerator(SudokuBoard sudoku, int? rngSeed = null)
        {
            _sudoku = sudoku;
            _rng = new Random(rngSeed ?? Random.Shared.Next());
        }

        public void GeneratePuzzle(float removalPercentage)
        {
            int size = _sudoku.Size;
            int blockSize = _sudoku.BlockSize;
            int[,] grid = _sudoku.Grid;

            // Crear grid base siguiendo el patrón matemático del Sudoku
            var values = Enumerable.Range(1, size).ToArray();
            Utils.Shuffle(values, _rng);
            var mapped = new int[size + 1];
            for (int i = 1; i <= size; i++)
                mapped[i] = values[i - 1];

            var newRowOrder = Utils.GeneratePermutedIndex(blockSize, _rng);
            var newColOrder = Utils.GeneratePermutedIndex(blockSize, _rng);

            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    int baseValue =
                        (((row * blockSize) + (row / blockSize) + col) % size) + 1;

                    int mappedValue = mapped[baseValue];

                    int finalRow = newRowOrder[row];
                    int finalCol = newColOrder[col];

                    grid[row, col] = mappedValue; 

                    //reacomodo según el orden permutado
                    grid[row, col] = mappedValue;
                }
            }

            // permutaciones reales sobre un grid temporal
            var tempGrid = new int[size, size];
            for (int r = 0; r < size; r++)
                for (int c = 0; c < size; c++)
                    tempGrid[r, c] = grid[newRowOrder[r], newColOrder[c]];

            // Copiar resultado final
            Array.Copy(tempGrid, grid, tempGrid.Length);

            // Calcular cuántas celdas remover
            int totalCells = size * size;
            int toRemove = Math.Clamp((int)Math.Round(removalPercentage * totalCells), 0, totalCells);

            var positions = Enumerable.Range(0, totalCells).ToArray();
            Utils.Shuffle(positions, _rng);

            int removed = 0;
            for (int i = 0; i < positions.Length && removed < toRemove; i++)
            {
                int idx = positions[i];
                int r = idx / size;
                int c = idx % size;

                if (grid[r, c] != 0)
                {
                    grid[r, c] = 0;
                    removed++;
                }
            }
        }


    }
}
