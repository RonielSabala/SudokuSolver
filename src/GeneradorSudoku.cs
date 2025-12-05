using System.Text;

class GeneradorSudoku
{
    static void Main()
    {
        var sudoku = new Sudoku(blockSize: 5);

        sudoku.GenerateSolution();
        sudoku.RemoveRandomCells(removalPercentage: 0.5f);
        sudoku.Print();
    }
}

public class Sudoku
{
    private readonly int _blockSize;
    public int Size { get; }
    private readonly int[,] _grid;
    private readonly Random _rng;
    private const string _emptySymbol = ".";
    private const string _cellCornerSymbol = "+";
    private const string _cellVerticalSymbol = "|";
    private const char _cellHorizontalSymbol = '-';

    public Sudoku(int blockSize)
    {
        if (blockSize < 2)
            throw new ArgumentException("blockSize debe ser >= 2", nameof(blockSize));

        _blockSize = blockSize;
        Size = blockSize * blockSize;
        _grid = new int[Size, Size];
        _rng = new Random();
    }

    public int GetValue(int row, int col) => _grid[row, col];

    public void Print()
    {
        Console.WriteLine(BuildBoardString());
    }

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

    private void Shuffle<T>(T[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = _rng.Next(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }

    public void GenerateSolution()
    {
        var baseGrid = new int[Size, Size];
        for (int row = 0; row < Size; row++)
            for (int col = 0; col < Size; col++)
                baseGrid[row, col] = (((row * _blockSize) + (row / _blockSize) + col) % Size) + 1;

        var values = Enumerable.Range(1, Size).ToArray();
        Shuffle(values);

        var mapped = new int[Size + 1];
        for (int i = 1; i <= Size; i++)
        {
            mapped[i] = values[i - 1];
        }

        var mappedGrid = new int[Size, Size];
        for (int row = 0; row < Size; row++)
            for (int col = 0; col < Size; col++)
                mappedGrid[row, col] = mapped[baseGrid[row, col]];

        var newRowOrder = GeneratePermutedIndex();
        var newColOrder = GeneratePermutedIndex();
        for (int row = 0; row < Size; row++)
            for (int col = 0; col < Size; col++)
                _grid[row, col] = mappedGrid[newRowOrder[row], newColOrder[col]];
    }

    private int[] GeneratePermutedIndex()
    {
        var result = new List<int>(Size);
        var bandIndices = Enumerable.Range(0, _blockSize).ToArray();
        Shuffle(bandIndices);

        foreach (var band in bandIndices)
        {
            var local = Enumerable.Range(band * _blockSize, _blockSize).ToArray();
            Shuffle(local);
            result.AddRange(local);
        }

        return result.ToArray();
    }

    public void RemoveRandomCells(float removalPercentage)
    {
        int totalCellsCount = Size * Size;
        int toRemove = (int)Math.Round(removalPercentage * totalCellsCount);
        toRemove = Math.Clamp(toRemove, 0, totalCellsCount);

        var positions = Enumerable.Range(0, totalCellsCount).ToArray();
        Shuffle(positions);

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

    private string BuildBoardString()
    {
        var sb = new StringBuilder();
        int cellWidth = (Size <= 9) ? 1 : 2;
        string sepSegment = new string(_cellHorizontalSymbol, cellWidth + 2);

        var horizontal = new StringBuilder();
        horizontal.Append(_cellCornerSymbol);
        for (int i = 0; i < Size; i++)
        {
            horizontal.Append(sepSegment);
            if ((i + 1) % _blockSize == 0) horizontal.Append(_cellCornerSymbol);
        }

        for (int r = 0; r < Size; r++)
        {
            if (r % _blockSize == 0) sb.AppendLine(horizontal.ToString());

            for (int c = 0; c < Size; c++)
            {
                if (c % _blockSize == 0) sb.Append(_cellVerticalSymbol);

                string cellText = _grid[r, c] == 0 ? _emptySymbol : _grid[r, c].ToString();
                sb.Append(" ");
                sb.Append(cellText.PadLeft(cellWidth));
                sb.Append(" ");
            }

            sb.AppendLine(_cellVerticalSymbol);
        }

        sb.AppendLine(horizontal.ToString());
        return sb.ToString();
    }
}
