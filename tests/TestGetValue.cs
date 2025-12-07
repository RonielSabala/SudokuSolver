using Xunit;
using Sudoku;

namespace Tests
{
    public class PruebasGetValue
    {
        private SudokuBoard CrearTableroEstandar() // Crearemos un tablero de Sudoku 9x9 con algunos valores predefinidos.
        {
            var board = new SudokuBoard(blockSize: 3);
            int[,] valores = new int[,]
            {
                { 5, 3, 0, 0, 7, 0, 0, 0, 0 },
                { 6, 0, 0, 1, 9, 5, 0, 0, 0 },
                { 0, 9, 8, 0, 0, 0, 0, 6, 0 },
                { 8, 0, 0, 0, 6, 0, 0, 0, 3 },
                { 4, 0, 0, 8, 0, 3, 0, 0, 1 },
                { 7, 0, 0, 0, 2, 0, 0, 0, 6 },
                { 0, 6, 0, 0, 0, 0, 2, 8, 0 },
                { 0, 0, 0, 4, 1, 9, 0, 0, 5 },
                { 0, 0, 0, 0, 8, 0, 0, 7, 9 }
            };
            
            for (int row = 0; row < 9; row++)
                for (int col = 0; col < 9; col++)
                    board.Grid[row, col] = valores[row, col];
            
            return board;
        }

        [Fact]
        public void GetValue_PosicionConValor_RetornaValorCorrecto() // Probamos una posición que tiene un valor asignado y verificamos que se retorna correctamente.
        {

            var board = CrearTableroEstandar();


            int valor = board.GetValue(0, 0); // Es la primera posición fila 0, columna 0.


            Assert.Equal(5, valor);
        }

        [Fact]
        public void GetValue_CeldaVacia_RetornaCero() // Probamos una posición que está vacía, osea tiene valor de 0 y retorna 0.
        {
    
            var board = CrearTableroEstandar();


            int valor = board.GetValue(0, 2);


            Assert.Equal(0, valor);
        }

        [Theory]
        [InlineData(0, 0, 5)]
        [InlineData(0, 1, 3)]
        [InlineData(1, 0, 6)]
        [InlineData(0, 4, 7)]
        [InlineData(1, 3, 1)]
        [InlineData(8, 8, 9)] // Dicho anteriormente, esto es para probar múltiples posiciones en una sola prueba.
        public void GetValue_DiferentesPosiciones_RetornaValoresCorrectos(int row, int col, int esperado) // Probamos varias posiciones con valores conocidos.
        {

            var board = CrearTableroEstandar();


            int valor = board.GetValue(row, col); // Obtenemos el valor de las posiciones dadas.

  
            Assert.Equal(esperado, valor);
        }

        [Fact]
        public void GetValue_TableroVacio_RetornaCero() // Probamos un tablero completamente vacío.
        {

            var board = new SudokuBoard(blockSize: 3);


            int valor = board.GetValue(4, 4); // Cualquier posición en un tablero vacío debería retornar 0.


            Assert.Equal(0, valor);
        }

        [Fact]
        public void GetValue_TodasLasCeldasVacias_RetornanCero() // Probamos todas las celdas de un tablero vacío para asegurarnos de que todas retornan 0.
        {

            var board = new SudokuBoard(blockSize: 3);


            for (int row = 0; row < 9; row++)
                for (int col = 0; col < 9; col++)
                {
                    Assert.Equal(0, board.GetValue(row, col));
                }
        }

        [Fact]
        public void GetValue_PrimeraYUltimaPosicion_RetornaValoresCorrectos() // Probamos las posiciones límite: la primera y la última del tablero.
        {

            var board = CrearTableroEstandar();


            int primera = board.GetValue(0, 0); // Primera posición fila 0, columna 0.
            int ultima = board.GetValue(8, 8); // Última posición fila 8, columna 8.


            Assert.Equal(5, primera); // Valor esperado en la primera posición.
            Assert.Equal(9, ultima); // Valor esperado en la última posición. (Fijarse en el tablero creado arriba)
        }

        [Fact]
        public void GetValue_DespuesDeModificarGrid_RetornaValorActualizado() // Probamos que después de modificar directamente el grid, GetValue retorna el valor actualizado.
        {

            var board = new SudokuBoard(blockSize: 3);
            board.Grid[5, 5] = 7;


            int valor = board.GetValue(5, 5);


            Assert.Equal(7, valor);
        }

        [Fact]
        public void GetValue_BlockSize2_FuncionaCorrectamente() // Probamos un tablero de Sudoku 4x4 para verificar que GetValue funciona correctamente en tableros de diferentes tamaños.
        {

            var board = new SudokuBoard(blockSize: 2);
            board.Grid[0, 0] = 1;
            board.Grid[3, 3] = 4;


            int valor1 = board.GetValue(0, 0);
            int valor2 = board.GetValue(3, 3);


            Assert.Equal(1, valor1);
            Assert.Equal(4, valor2);
        }
    }
}