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

        public void GenerateSolution()
        {
            int size = _sudoku.Size;
            int blockSize = _sudoku.BlockSize;
            int[,] grid = _sudoku.Grid;

            var baseGrid = new int[size, size];
            for (int row = 0; row < size; row++)
                for (int col = 0; col < size; col++)
                    baseGrid[row, col] = (((row * blockSize) + (row / blockSize) + col) % size) + 1;

            var values = Enumerable.Range(1, size).ToArray();
            Utils.Shuffle(values, _rng);

            var mapped = new int[size + 1];
            for (int i = 1; i <= size; i++)
            {
                mapped[i] = values[i - 1];
            }

            var mappedGrid = new int[size, size];
            for (int row = 0; row < size; row++)
                for (int col = 0; col < size; col++)
                    mappedGrid[row, col] = mapped[baseGrid[row, col]];

            var newRowOrder = Utils.GeneratePermutedIndex(blockSize, _rng);
            var newColOrder = Utils.GeneratePermutedIndex(blockSize, _rng);
            for (int row = 0; row < size; row++)
                for (int col = 0; col < size; col++)
                    grid[row, col] = mappedGrid[newRowOrder[row], newColOrder[col]];
        }

        public void RemoveRandomCells(float removalPercentage)
        {
            int size = _sudoku.Size;
            int[,] grid = _sudoku.Grid;

            int totalCellsCount = size * size;
            int toRemove = (int)Math.Round(removalPercentage * totalCellsCount);
            toRemove = Math.Clamp(toRemove, 0, totalCellsCount);

            var positions = Enumerable.Range(0, totalCellsCount).ToArray();
            Utils.Shuffle(positions, _rng);

            int removed = 0;
            for (int i = 0; i < positions.Length && removed < toRemove; i++)
            {
                int idx = positions[i];
                int row = idx / size;
                int col = idx % size;
                if (grid[row, col] != 0)
                {
                    grid[row, col] = 0;
                    removed++;
                }
            }
        }
    }
}
