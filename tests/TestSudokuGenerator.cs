using Xunit;
using Sudoku;

namespace Tests
{
    public class PruebasSudokuGenerator
    {
        // Test para la clase SudokuGenerator.

        [Fact]
        public void GenerateSolution_GeneraTableroCompleto()
        {

            var board = new SudokuBoard(blockSize: 3);
            var generator = new SudokuGenerator(board);


            generator.GenerateSolution();

            // Esto es para verificar que el tablero está completamente lleno.
            for (int row = 0; row < 9; row++)
                for (int col = 0; col < 9; col++)
                {
                    Assert.NotEqual(0, board.Grid[row, col]);
                    Assert.InRange(board.Grid[row, col], 1, 9);
                }
        }

        [Fact]
        public void GenerateSolution_TableroEsValido()
        {

            var board = new SudokuBoard(blockSize: 3);
            var generator = new SudokuGenerator(board);


            generator.GenerateSolution();


            Assert.True(board.IsValid());
        }

        [Fact]
        public void GenerateSolution_ConSemilla_GeneraMismoTablero()
        {

            var board1 = new SudokuBoard(blockSize: 3);
            var board2 = new SudokuBoard(blockSize: 3);
            var generator1 = new SudokuGenerator(board1, rngSeed: 12345);
            var generator2 = new SudokuGenerator(board2, rngSeed: 12345);


            generator1.GenerateSolution();
            generator2.GenerateSolution();

            // Deben ser iguales ambos tableros para la misma semilla.
            for (int row = 0; row < 9; row++)
                for (int col = 0; col < 9; col++)
                {
                    Assert.Equal(board1.Grid[row, col], board2.Grid[row, col]);
                }
        }

        [Fact]
        public void GenerateSolution_SinSemilla_GeneraTablerosDiferentes()
        {

            var board1 = new SudokuBoard(blockSize: 3);
            var board2 = new SudokuBoard(blockSize: 3);
            var generator1 = new SudokuGenerator(board1);
            var generator2 = new SudokuGenerator(board2);


            generator1.GenerateSolution();
            generator2.GenerateSolution();

            // Esto verfica que al menos una celda es diferente entre ambos tableros.
            bool sonDiferentes = false;
            for (int row = 0; row < 9 && !sonDiferentes; row++)
                for (int col = 0; col < 9 && !sonDiferentes; col++)
                {
                    if (board1.Grid[row, col] != board2.Grid[row, col])
                        sonDiferentes = true;
                }

            Assert.True(sonDiferentes);
        }

        [Fact]
        public void GenerateSolution_BlockSize2_FuncionaCorrectamente()
        {

            var board = new SudokuBoard(blockSize: 2);
            var generator = new SudokuGenerator(board);

            generator.GenerateSolution();


            Assert.True(board.IsValid());
            for (int row = 0; row < 4; row++)
                for (int col = 0; col < 4; col++)
                {
                    Assert.InRange(board.Grid[row, col], 1, 4);
                }
        }

        // Tests para el método RemoveRandomCells.

        [Fact]
        public void RemoveRandomCells_EliminaCeldasCorrectamente()
        {

            var board = new SudokuBoard(blockSize: 3);
            var generator = new SudokuGenerator(board, rngSeed: 12345);
            generator.GenerateSolution();

            generator.RemoveRandomCells(0.5f); // Va a remover el 50% de las celdas este código.

            // Contar cuántas celdas están vacías.
            int celdasVacias = 0;
            for (int row = 0; row < 9; row++)
                for (int col = 0; col < 9; col++)
                {
                    if (board.Grid[row, col] == 0)
                        celdasVacias++;
                }

            Assert.InRange(celdasVacias, 35, 45); // Básicamente alrededor de un 50% debe estar vacío.
        }

        [Fact]
        public void RemoveRandomCells_TableroSigueValido()
        {

            var board = new SudokuBoard(blockSize: 3);
            var generator = new SudokuGenerator(board);
            generator.GenerateSolution();


            generator.RemoveRandomCells(0.6f);


            Assert.True(board.IsValid());
        }

        [Theory]
        [InlineData(0.0f)]
        [InlineData(0.25f)]
        [InlineData(0.5f)]
        [InlineData(0.75f)]
        [InlineData(1.0f)]
        public void RemoveRandomCells_DiferentesPorcentajes_Funciona(float porcentaje)
        {

            var board = new SudokuBoard(blockSize: 3);
            var generator = new SudokuGenerator(board);
            generator.GenerateSolution();


            generator.RemoveRandomCells(porcentaje);


            Assert.True(board.IsValid());
        }

        [Fact]
        public void RemoveRandomCells_Porcentaje0_NoEliminaNada()
        {

            var board = new SudokuBoard(blockSize: 3);
            var generator = new SudokuGenerator(board, rngSeed: 12345);
            generator.GenerateSolution();


            generator.RemoveRandomCells(0.0f);

            // Ninguna celda debe estar vacía para el caso de esta prueba porque el porcentaje es 0.
            for (int row = 0; row < 9; row++)
                for (int col = 0; col < 9; col++)
                {
                    Assert.NotEqual(0, board.Grid[row, col]);
                }
        }

        [Fact]
        public void RemoveRandomCells_Porcentaje1_EliminaTodas()
        {

            var board = new SudokuBoard(blockSize: 3);
            var generator = new SudokuGenerator(board, rngSeed: 12345);
            generator.GenerateSolution();


            generator.RemoveRandomCells(1.0f);

            // Contrario al caso anterior, todas las celdas deben estar vacías.
            for (int row = 0; row < 9; row++)
                for (int col = 0; col < 9; col++)
                {
                    Assert.Equal(0, board.Grid[row, col]);
                }
        }
    }
}