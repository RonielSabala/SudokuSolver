using Xunit;
using Sudoku;

namespace Tests
{
    public class PruebasSudokuSolver
    {
        private SudokuBoard CrearTableroParcial() // Similar a CrearTableroEstandar pero con más celdas vacías.
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

        // Estos son tests básicos para verificar que el SudokuSolver funciona correctamente.

        [Fact]
        public void TrySolve_TableroParcial_EncuentraSolucion()
        {

            var board = CrearTableroParcial();


            var solucion = SudokuSolver.TrySolve(board, maxDegreeOfParallelism: 1);


            Assert.NotNull(solucion);
            Assert.True(solucion.IsValid());
        }

        [Fact]
        public void TrySolve_SolucionCompleta_NoHayCeldasVacias()
        {

            var board = CrearTableroParcial();


            var solucion = SudokuSolver.TrySolve(board, maxDegreeOfParallelism: 1);


            Assert.NotNull(solucion);
            for (int row = 0; row < 9; row++)
                for (int col = 0; col < 9; col++)
                {
                    Assert.NotEqual(0, solucion.Grid[row, col]);
                }
        }

        [Fact]
        public void TrySolve_TableroInvalido_RetornaNull()
        {
            // Simulación de un tablero inválido con conflictos.
            var board = new SudokuBoard(blockSize: 3);
            board.Grid[0, 0] = 5;
            board.Grid[0, 5] = 5; // Esto va a crear un conflicto en una de las filas.


            var solucion = SudokuSolver.TrySolve(board, maxDegreeOfParallelism: 1);


            Assert.Null(solucion);
        }

        [Fact]
        public void TrySolve_TableroYaResuelto_RetornaMismoTablero()
        {

            var board = new SudokuBoard(blockSize: 3);
            var generator = new SudokuGenerator(board, rngSeed: 12345);
            generator.GenerateSolution();


            var solucion = SudokuSolver.TrySolve(board, maxDegreeOfParallelism: 1);


            Assert.NotNull(solucion);
            Assert.True(solucion.IsValid());
        }

        // Estos tests verifican el comportamiento con diferentes grados de paralelismo.

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(4)]
        public void TrySolve_DiferentesGradosParalelismo_EncuentraSolucion(int grado)
        {

            var board = CrearTableroParcial();


            var solucion = SudokuSolver.TrySolve(board, maxDegreeOfParallelism: grado);


            Assert.NotNull(solucion);
            Assert.True(solucion.IsValid());
        }

        [Fact]
        public void TrySolve_ParalelismoCero_UsaUno()
        {

            var board = CrearTableroParcial();


            var solucion = SudokuSolver.TrySolve(board, maxDegreeOfParallelism: 0);


            Assert.NotNull(solucion);
        }

        [Fact]
        public void TrySolve_ParalelismoNegativo_UsaUno() // Utilizará el valor por defecto del sistema para el paralelismo. (El valor por defecto significa que se usarán todos los núcleos disponibles)
        {

            var board = CrearTableroParcial();


            var solucion = SudokuSolver.TrySolve(board, maxDegreeOfParallelism: -1);


            Assert.NotNull(solucion);
        }

        // Diferentes tamaños de bloques.

        [Fact]
        public void TrySolve_BlockSize2_FuncionaCorrectamente()
        {

            var board = new SudokuBoard(blockSize: 2);
            board.Grid[0, 0] = 1;
            board.Grid[0, 1] = 2;


            var solucion = SudokuSolver.TrySolve(board, maxDegreeOfParallelism: 1);


            Assert.NotNull(solucion);
            Assert.True(solucion.IsValid());
        }

        // Tests de casos límite.

        [Fact]
        public void TrySolve_TableroVacio_EncuentraSolucion()
        {

            var board = new SudokuBoard(blockSize: 3);


            var solucion = SudokuSolver.TrySolve(board, maxDegreeOfParallelism: 1);


            Assert.NotNull(solucion);
            Assert.True(solucion.IsValid());
        }

        [Fact]
        public void TrySolve_TableroSinSolucion_RetornaNull()
        {
            // Este tablero está diseñado para no tener solución.
            var board = new SudokuBoard(blockSize: 3);
            // Va a llenar la primera fila y la segunda fila con los mismos números para crear conflictos.
            for (int i = 0; i < 9; i++)
            {
                board.Grid[0, i] = i + 1;
                board.Grid[1, i] = i + 1; // Esto crea conflictos en columnas.
            }


            var solucion = SudokuSolver.TrySolve(board, maxDegreeOfParallelism: 1);


            Assert.Null(solucion);
        }
    }
}