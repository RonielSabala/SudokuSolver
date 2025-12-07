using Xunit;
using Sudoku;

namespace Tests
{
    public class PruebasToString
    {
        [Fact]
        public void ToString_TableroVacio_ContieneSimbolosVacios() // El nombre del test describe lo que va a probar. Basícamente, chequea que el ToString de un tablero vacío contenga los símbolos esperados. Lo simbolos son los que representan una celda vacía, y las esquinas y bordes de las celdas
        {

            var board = new SudokuBoard(blockSize: 3);


            string resultado = board.ToString();


            Assert.Contains(SudokuSymbols.EmptyCell, resultado);
            Assert.Contains(SudokuSymbols.CellCorner, resultado);
            Assert.Contains(SudokuSymbols.CellVertical, resultado);
        }

        [Fact]
        public void ToString_TableroConValores_MuestraNumeros() // Este test verifica que el método ToString de un tablero de Sudoku que tiene algunos valores colocados muestre esos números correctamente en la representación de cadena.
        {

            var board = new SudokuBoard(blockSize: 3);
            board.Grid[0, 0] = 5;
            board.Grid[0, 1] = 3;


            string resultado = board.ToString();


            Assert.Contains("5", resultado);
            Assert.Contains("3", resultado);
        }

        [Fact]
        public void ToString_NoEsNuloNiVacio() // Este es para probar que el método ToString del tablero no devuelva una cadena nula o vacía.
        {

            var board = new SudokuBoard(blockSize: 3);


            string resultado = board.ToString();


            Assert.NotNull(resultado);
            Assert.NotEmpty(resultado);
        }

        [Fact]
        public void ToString_BlockSize2_FuncionaCorrectamente() // Este test verifica que el método ToString funcione correctamente para un tablero de Sudoku con un tamaño de bloque de 2x2.
        {

            var board = new SudokuBoard(blockSize: 2);


            string resultado = board.ToString();


            Assert.NotNull(resultado);
            Assert.Contains(SudokuSymbols.CellCorner, resultado);
        }

        [Fact]
        public void ToString_ContieneLineasHorizontales() // Verificación de que el tablero está representado con líneas horizontales adecuadas en su representación de cadena.
        {

            var board = new SudokuBoard(blockSize: 3);


            string resultado = board.ToString();


            Assert.Contains(SudokuSymbols.CellHorizontal.ToString(), resultado);
        }
    }
}