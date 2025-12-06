using System.Text;

namespace Sudoku
{
    public static class SudokuSymbols
    {
        public const string EmptyCell = ".";
        public const string CellCorner = "+";
        public const string CellVertical = "|";
        public const char CellHorizontal = '-';
    }

    public class SudokuBoard
    {
        public readonly int BlockSize;
        public readonly int Size;
        public int[,] Grid;

        public SudokuBoard(int blockSize)
        {
            if (blockSize < 2)
                throw new ArgumentException("blockSize debe ser >= 2", nameof(blockSize));

            BlockSize = blockSize;
            Size = blockSize * blockSize;
            Grid = new int[Size, Size];
        }

        public int GetValue(int row, int col) => Grid[row, col];

        public bool CanPlaceNumber(int row, int col, int value)
        {
            // Validar fila y columna
            for (int i = 0; i < Size; i++)
                if (Grid[row, i] == value || Grid[i, col] == value)
                {
                    return false;
                }

            // Validar subcuadrado
            int rowStart = row - (row % BlockSize);
            int colStart = col - (col % BlockSize);
            for (int r = 0; r < BlockSize; r++)
                for (int c = 0; c < BlockSize; c++)
                    if (Grid[rowStart + r, colStart + c] == value)
                    {
                        return false;
                    }

            return true;
        }

        public bool IsValid()
        {
            for (int row = 0; row < Size; row++)
                for (int col = 0; col < Size; col++)
                {
                    int cellValue = Grid[row, col];
                    if (cellValue == 0)
                    {
                        continue;
                    }

                    // Valor fuera de rango
                    if (cellValue < 1 || cellValue > Size)
                    {
                        return false;
                    }

                    Grid[row, col] = 0;
                    bool canPlace = CanPlaceNumber(row, col, cellValue);
                    Grid[row, col] = cellValue;

                    if (!canPlace)
                    {
                        return false;
                    }
                }

            return true;
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
                if ((i + 1) % BlockSize == 0)
                {
                    horizontal.Append(SudokuSymbols.CellCorner);
                }
            }

            for (int row = 0; row < Size; row++)
            {
                if (row % BlockSize == 0)
                {
                    sb.AppendLine(horizontal.ToString());
                }

                for (int col = 0; col < Size; col++)
                {
                    if (col % BlockSize == 0)
                    {
                        sb.Append(SudokuSymbols.CellVertical);
                    }

                    int cellValue = Grid[row, col];
                    string cellText = cellValue == 0 ? SudokuSymbols.EmptyCell : cellValue.ToString();
                    sb.Append(" " + cellText.PadLeft(cellWidth) + " ");
                }

                sb.AppendLine(SudokuSymbols.CellVertical);
            }

            sb.AppendLine(horizontal.ToString());
            return sb.ToString();
        }
    }
}
