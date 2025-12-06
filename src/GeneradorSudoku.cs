using System.Text;

class GeneradorSudoku
{
    static void Main()
    {
        var sudoku = new Sudoku(blockSize: 3);

        sudoku.GenerateSolution();
        sudoku.RemoveRandomCells(removalPercentage: 0.5f);
        Console.WriteLine(sudoku.ToString());
        Console.WriteLine(sudoku.IsValid());
    }
}

public static class SudokuSymbols
{
    public const string EmptyCell = ".";
    public const string CellCorner = "+";
    public const string CellVertical = "|";
    public const char CellHorizontal = '-';
}

public static class SudokuUtils
{
    public static void Shuffle<T>(T[] array, Random rng)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }

    public static int[] GeneratePermutedIndex(int BlockSize, Random rng)
    {
        var result = new List<int>(BlockSize * BlockSize);
        var bandIndices = Enumerable.Range(0, BlockSize).ToArray();
        Shuffle(bandIndices, rng);

        foreach (var band in bandIndices)
        {
            var local = Enumerable.Range(band * BlockSize, BlockSize).ToArray();
            Shuffle(local, rng);
            result.AddRange(local);
        }

        return result.ToArray();
    }
}

public class Sudoku
{
    private readonly int _blockSize;
    public int Size { get; }
    private int[,] _grid;
    private readonly Random _rng;

    public Sudoku(int blockSize, int? rngSeed = null)
    {
        if (blockSize < 2)
            throw new ArgumentException("blockSize debe ser >= 2", nameof(blockSize));

        _blockSize = blockSize;
        Size = blockSize * blockSize;
        _grid = new int[Size, Size];
        _rng = new Random(rngSeed ?? Random.Shared.Next());
    }

    public int GetValue(int row, int col) => _grid[row, col];

    public bool CanPlaceNumber(int row, int col, int value)
    {
        // Validar fila y columna
        for (int i = 0; i < Size; i++)
            if (_grid[row, i] == value || _grid[i, col] == value)
            {
                return false;
            }

        // Validar subcuadrado
        int rowStart = row - (row % _blockSize);
        int colStart = col - (col % _blockSize);
        for (int r = 0; r < _blockSize; r++)
            for (int c = 0; c < _blockSize; c++)
                if (_grid[rowStart + r, colStart + c] == value)
                {
                    return false;
                }

        return true;
    }

    public bool IsValid()
    {
        for (int row = 0; row < Size; row++)
        {
            for (int col = 0; col < Size; col++)
            {
                int cellValue = _grid[row, col];
                if (cellValue == 0)
                {
                    continue;
                }

                _grid[row, col] = 0;
                bool canPlace = CanPlaceNumber(row, col, cellValue);
                _grid[row, col] = cellValue;

                if (!canPlace)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public void GenerateSolution()
    {
        var baseGrid = new int[Size, Size];
        for (int row = 0; row < Size; row++)
            for (int col = 0; col < Size; col++)
                baseGrid[row, col] = (((row * _blockSize) + (row / _blockSize) + col) % Size) + 1;

        var values = Enumerable.Range(1, Size).ToArray();
        SudokuUtils.Shuffle(values, _rng);

        var mapped = new int[Size + 1];
        for (int i = 1; i <= Size; i++)
        {
            mapped[i] = values[i - 1];
        }

        var mappedGrid = new int[Size, Size];
        for (int row = 0; row < Size; row++)
            for (int col = 0; col < Size; col++)
                mappedGrid[row, col] = mapped[baseGrid[row, col]];

        var newRowOrder = SudokuUtils.GeneratePermutedIndex(_blockSize, _rng);
        var newColOrder = SudokuUtils.GeneratePermutedIndex(_blockSize, _rng);
        for (int row = 0; row < Size; row++)
            for (int col = 0; col < Size; col++)
                _grid[row, col] = mappedGrid[newRowOrder[row], newColOrder[col]];
    }

    public void RemoveRandomCells(float removalPercentage)
    {
        int totalCellsCount = Size * Size;
        int toRemove = (int)Math.Round(removalPercentage * totalCellsCount);
        toRemove = Math.Clamp(toRemove, 0, totalCellsCount);

        var positions = Enumerable.Range(0, totalCellsCount).ToArray();
        SudokuUtils.Shuffle(positions, _rng);

        int removed = 0;
        for (int i = 0; i < positions.Length && removed < toRemove; i++)
        {
            int idx = positions[i];
            int row = idx / Size;
            int col = idx % Size;
            if (_grid[row, col] != 0)
            {
                _grid[row, col] = 0;
                removed++;
            }
        }
    }

    override
    public string ToString()
    {
        var sb = new StringBuilder();
        int cellWidth = (Size <= 9) ? 1 : 2;
        string sepSegment = new string(SudokuSymbols.CellHorizontal, cellWidth + 2);

        var horizontal = new StringBuilder();
        horizontal.Append(SudokuSymbols.CellCorner);

        for (int i = 0; i < Size; i++)
        {
            horizontal.Append(sepSegment);
            if ((i + 1) % _blockSize == 0)
            {
                horizontal.Append(SudokuSymbols.CellCorner);
            }
        }

        for (int row = 0; row < Size; row++)
        {
            if (row % _blockSize == 0)
            {
                sb.AppendLine(horizontal.ToString());
            }

            for (int col = 0; col < Size; col++)
            {
                if (col % _blockSize == 0)
                {
                    sb.Append(SudokuSymbols.CellVertical);
                }

                int cellValue = _grid[row, col];
                string cellText = cellValue == 0 ? SudokuSymbols.EmptyCell : cellValue.ToString();
                sb.Append(" " + cellText.PadLeft(cellWidth) + " ");
            }

            sb.AppendLine(SudokuSymbols.CellVertical);
        }

        sb.AppendLine(horizontal.ToString());
        return sb.ToString();
    }
}
