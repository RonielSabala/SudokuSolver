using Xunit;
using Sudoku;

namespace Tests
{
    public class PruebasSudokuBoardConstructor
    {
        // Tests para el constructor de la clase SudokuBoard.

        [Fact]
        public void Constructor_BlockSize2_Valido_CreaExitosamente()
        {
            // Crear el tablero con un blockSize de 2.
            var board = new SudokuBoard(blockSize: 2);

            // Assert para verificar que se creó correctamente.
            Assert.NotNull(board);
            Assert.Equal(2, board.BlockSize);
            Assert.Equal(4, board.Size);
        }

        [Fact]
        public void Constructor_BlockSize3_Valido_CreaExitosamente()
        {
            // Creación del tablero con un blockSize de 3.
            var board = new SudokuBoard(blockSize: 3);

            // Verificar que se creó correctamente.
            Assert.NotNull(board);
            Assert.Equal(3, board.BlockSize);
            Assert.Equal(9, board.Size);
        }

        [Fact]
        public void Constructor_BlockSize4_Valido_CreaExitosamente()
        {

            var board = new SudokuBoard(blockSize: 4);


            Assert.NotNull(board);
            Assert.Equal(4, board.BlockSize);
            Assert.Equal(16, board.Size);
        }

        [Fact]
        public void Constructor_BlockSize1_Invalido_LanzaExcepcion()
        {

            var excepcion = Assert.Throws<ArgumentException>(() => new SudokuBoard(blockSize: 1));
            Assert.NotNull(excepcion);
        }

        [Fact]
        public void Constructor_BlockSize0_Invalido_LanzaExcepcion()
        {

            var excepcion = Assert.Throws<ArgumentException>(() => new SudokuBoard(blockSize: 0));
            Assert.NotNull(excepcion);
        }

        [Fact]
        public void Constructor_BlockSizeMenos1_Invalido_LanzaExcepcion()
        {

            var excepcion = Assert.Throws<ArgumentException>(() => new SudokuBoard(blockSize: -1));
            Assert.NotNull(excepcion);
        }

        [Fact]
        public void Constructor_BlockSizeMenos100_Invalido_LanzaExcepcion()
        {

            var excepcion = Assert.Throws<ArgumentException>(() => new SudokuBoard(blockSize: -100));
            Assert.NotNull(excepcion);
        }

        // Tests adicionales para verificar propiedades del constructor.

        [Fact]
        public void Constructor_VerificaBlockSize_SeAsignaCorrectamente()
        {

            var board = new SudokuBoard(blockSize: 3);


            Assert.Equal(3, board.BlockSize);
        }

        [Fact]
        public void Constructor_VerificaSize_EsBlockSizeAlCuadrado()
        {

            var board2 = new SudokuBoard(blockSize: 2);
            var board3 = new SudokuBoard(blockSize: 3);
            var board4 = new SudokuBoard(blockSize: 4);


            Assert.Equal(4, board2.Size);   // 2 * 2 = 4
            Assert.Equal(9, board3.Size);   // 3 * 3 = 9
            Assert.Equal(16, board4.Size);  // 4 * 4 = 16 básicamente tienen que ser cuadrados perfectos.
        }

        [Theory]
        [InlineData(2, 4)]
        [InlineData(3, 9)]
        [InlineData(4, 16)]
        [InlineData(5, 25)]
        public void Constructor_DiferentesBlockSizes_CalculaSizeCorrectamente(int blockSize, int sizeEsperado)
        {

            var board = new SudokuBoard(blockSize);


            Assert.Equal(sizeEsperado, board.Size);
        }

        [Fact]
        public void Constructor_GridSeInicializa_ConTamanioCorrecto()
        {

            var board = new SudokuBoard(blockSize: 3);


            Assert.NotNull(board.Grid);
            Assert.Equal(9, board.Grid.GetLength(0)); // filas
            Assert.Equal(9, board.Grid.GetLength(1)); // columnas
        }

        [Fact]
        public void Constructor_TodosLosValoresDelGrid_InicianEnCero()
        {

            var board = new SudokuBoard(blockSize: 3);

            // Verificar que todos los valores son cero.
            for (int row = 0; row < board.Size; row++)
                for (int col = 0; col < board.Size; col++)
                {
                    int valor = board.GetValue(row, col);
                    Assert.Equal(0, valor);
                }
        }

        [Fact]
        public void Constructor_TodosLosValoresDelGrid_InicianEnCero_BlockSize2()
        {

            var board = new SudokuBoard(blockSize: 2);


            for (int row = 0; row < 4; row++)
                for (int col = 0; col < 4; col++)
                {
                    Assert.Equal(0, board.GetValue(row, col));
                }
        }

        [Fact]
        public void Constructor_GridNoEsNull_DespuesDeInicializar()
        {

            var board = new SudokuBoard(blockSize: 3);


            Assert.NotNull(board.Grid);
        }

        // Tests para verificar mensajes y nombres de parámetros en excepciones.

        [Fact]
        public void Constructor_BlockSizeInvalido_MensajeContieneTextoEsperado()
        {

            var excepcion = Assert.Throws<ArgumentException>(() => new SudokuBoard(blockSize: 1));
            Assert.Contains("blockSize debe ser >= 2", excepcion.Message);
        }

        [Fact]
        public void Constructor_BlockSizeInvalido_MensajeContieneNombreParametro()
        {

            var excepcion = Assert.Throws<ArgumentException>(() => new SudokuBoard(blockSize: 0));
            Assert.Equal("blockSize", excepcion.ParamName);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Constructor_ValoresInvalidos_MensajeContieneTextoYParametro(int blockSizeInvalido)
        {

            var excepcion = Assert.Throws<ArgumentException>(() => new SudokuBoard(blockSize: blockSizeInvalido));
            Assert.Contains("blockSize debe ser >= 2", excepcion.Message);
            Assert.Equal("blockSize", excepcion.ParamName);
        }

        // Tests adicionales solo para asegurar el correcto funcionamiento del constructor.

        [Fact]
        public void Constructor_BlockSizeGrande_FuncionaCorrectamente()
        {

            var board = new SudokuBoard(blockSize: 10);


            Assert.Equal(100, board.Size);
            Assert.Equal(10, board.BlockSize);
        }

        [Fact]
        public void Constructor_MultipleInstancias_SonIndependientes()
        {

            var board1 = new SudokuBoard(blockSize: 2);
            var board2 = new SudokuBoard(blockSize: 3);


            Assert.Equal(4, board1.Size);
            Assert.Equal(9, board2.Size);
            Assert.NotEqual(board1.Size, board2.Size);
        }

        [Fact]
        public void Constructor_GridsDeInstanciasDiferentes_SonIndependientes()
        {

            var board1 = new SudokuBoard(blockSize: 3);
            var board2 = new SudokuBoard(blockSize: 3);
            
            board1.Grid[0, 0] = 5;


            Assert.Equal(5, board1.Grid[0, 0]);
            Assert.Equal(0, board2.Grid[0, 0]);
        }
    }
}