using Xunit;
using Sudoku;

namespace Tests
{
    public class PruebasIsValid
    {
        private SudokuBoard CrearTableroValido() // Como se ha estado haciendo antes, creamos un tablero de Sudoku 9x9 pero esta vez completamente lleno y válido.
        {
            var board = new SudokuBoard(blockSize: 3);
            int[,] valores = new int[,]
            {
                { 5, 3, 4, 6, 7, 8, 9, 1, 2 },
                { 6, 7, 2, 1, 9, 5, 3, 4, 8 },
                { 1, 9, 8, 3, 4, 2, 5, 6, 7 },
                { 8, 5, 9, 7, 6, 1, 4, 2, 3 },
                { 4, 2, 6, 8, 5, 3, 7, 9, 1 },
                { 7, 1, 3, 9, 2, 4, 8, 5, 6 },
                { 9, 6, 1, 5, 3, 7, 2, 8, 4 },
                { 2, 8, 7, 4, 1, 9, 6, 3, 5 },
                { 3, 4, 5, 2, 8, 6, 1, 7, 9 }
            };
            
            for (int row = 0; row < 9; row++)
                for (int col = 0; col < 9; col++)
                    board.Grid[row, col] = valores[row, col];
            
            return board;
        }

        private SudokuBoard CrearTableroParcialValido() // Ahora creamos un tablero parcialmente lleno pero aún así válido.
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

        // Estos son tests para verificar tableros válidos, que estén completos o parcialmente llenos.

        [Fact]
        public void IsValid_TableroCompleto_RetornaVerdadero() // Probamos un tablero completamente lleno y válido.
        {

            var board = CrearTableroValido();


            bool resultado = board.IsValid();


            Assert.True(resultado);
        }

        [Fact]
        public void IsValid_TableroParcial_RetornaVerdadero() // Probamos un tablero parcialmente lleno pero válido.
        {

            var board = CrearTableroParcialValido();


            bool resultado = board.IsValid();


            Assert.True(resultado);
        }

        [Fact]
        public void IsValid_TableroVacio_RetornaVerdadero() // Probamos un tablero completamente vacío.
        {

            var board = new SudokuBoard(blockSize: 3);


            bool resultado = board.IsValid();


            Assert.True(resultado);
        }

        // Test para verificar tableros con conflictos.

        [Fact]
        public void IsValid_ConflictoEnFila_RetornaFalso() // Probamos un tablero con un conflicto de duplicados en una fila.
        {

            var board = new SudokuBoard(blockSize: 3);
            board.Grid[0, 0] = 5;
            board.Grid[0, 5] = 5; // El conflicto será tener dos 5 en la misma fila.


            bool resultado = board.IsValid();


            Assert.False(resultado);
        }

        [Fact]
        public void IsValid_ConflictoEnColumna_RetornaFalso() // Un tablero con conflicto de duplicados en una columna.
        {

            var board = new SudokuBoard(blockSize: 3);
            board.Grid[0, 0] = 7;
            board.Grid[5, 0] = 7; // Tendremos dos 7 en la misma columna.


            bool resultado = board.IsValid();


            Assert.False(resultado);
        }

        [Fact]
        public void IsValid_ConflictoEnBloque_RetornaFalso() // Un tablero con conflicto de duplicados en un bloque 3x3.
        {

            var board = new SudokuBoard(blockSize: 3);
            board.Grid[0, 0] = 3;
            board.Grid[2, 2] = 3; // Dos 3 en el mismo bloque 3x3.


            bool resultado = board.IsValid();

  
            Assert.False(resultado);
        }

        // Tests para valores fuera de rango posibles en un tablero de Sudoku.

        [Fact]
        public void IsValid_ValorMayorQueSize_RetornaFalso() // Probamos un valor mayor que el tamaño máximo permitido (9 para un Sudoku 9x9).
        {

            var board = new SudokuBoard(blockSize: 3);
            board.Grid[0, 0] = 10; // Esta fuera del rango porque sobrepasa el máximo valor permitido, que es 9.


            bool resultado = board.IsValid();


            Assert.False(resultado);
        }

        [Fact]
        public void IsValid_ValorNegativo_RetornaFalso() // Probamos un valor negativo, que también es inválido en Sudoku.
        {

            var board = new SudokuBoard(blockSize: 3);
            board.Grid[0, 0] = -1; // -1, ni siquiera es un número válido en Sudoku y no se podrá colocar en el tablero.


            bool resultado = board.IsValid();


            Assert.False(resultado);
        }

        // Diferentes tamaños de tablero para verificar la validez.

        [Fact]
        public void IsValid_BlockSize2_TableroValido_RetornaVerdadero() // Tablero de tamaño 4x4 es válido porque es un cuadrado perfecto.
        {

            var board = new SudokuBoard(blockSize: 2);
            int[,] valores = new int[,]
            {
                { 1, 2, 3, 4 },
                { 3, 4, 1, 2 },
                { 2, 1, 4, 3 },
                { 4, 3, 2, 1 }
            };
            
            for (int row = 0; row < 4; row++)
                for (int col = 0; col < 4; col++)
                    board.Grid[row, col] = valores[row, col];


            bool resultado = board.IsValid();


            Assert.True(resultado);
        }

        [Fact]
        public void IsValid_BlockSize2_Conflicto_RetornaFalso() // Tablero de tamaño 4x4 con conflicto de duplicados.
        {

            var board = new SudokuBoard(blockSize: 2);
            board.Grid[0, 0] = 1;
            board.Grid[0, 2] = 1; // DOs 1 en la misma fila.


            bool resultado = board.IsValid();


            Assert.False(resultado);
        }

        // Tests luego de generar o modificar el tablero.

        [Fact]
        public void IsValid_DespuesDeGenerarSolucion_RetornaVerdadero() // Vamos a generar una solución completa y verificar su validez.
        {

            var board = new SudokuBoard(blockSize: 3);
            var generator = new SudokuGenerator(board, rngSeed: 12345); // Usamos una semilla fija para tener resultados ya predecibles.
            generator.GeneratePuzzle(0.0f);


            bool resultado = board.IsValid();


            Assert.True(resultado);
        }

        [Fact]
        public void IsValid_DespuesDeRemoverCeldas_RetornaVerdadero() // Generamos una solución y luego removemos algunas celdas para tener un tablero parcialmente lleno.
        {

            var board = new SudokuBoard(blockSize: 3);
            var generator = new SudokuGenerator(board, rngSeed: 12345);
            generator.GeneratePuzzle(0.5f);


            bool resultado = board.IsValid();


            Assert.True(resultado);
        }
    }
}