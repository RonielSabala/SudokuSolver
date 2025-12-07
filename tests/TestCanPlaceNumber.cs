using Xunit;
using Sudoku;

namespace Tests
{
    public class PruebasCanPlaceNumber
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
                { 4, 0, 0, 8, 0, 3, 0, 0, 0 },
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

        // Tests de conflictos a la hora de colocar un número.

        [Fact]
        public void CanPlaceNumber_ConflictoEnFila_RetornaFalso() // Vamos a probar que no se puede colocar un número que ya existe en la misma fila.
        {

            var board = CrearTableroEstandar();

            // Como se vió en la creación del tablero, ya hay un 5 en la fila 0.
            bool resultado = board.CanPlaceNumber(0, 2, 5);


            Assert.False(resultado);
        }

        [Fact]
        public void CanPlaceNumber_ConflictoEnColumna_RetornaFalso() // Lo mismo que se hizo anteriormente pero para columnas.
        {

            var board = CrearTableroEstandar();

            // Ya tenemos un 6 en la columna 0 y fila 1.
            bool resultado = board.CanPlaceNumber(2, 0, 6);


            Assert.False(resultado);
        }

        [Fact]
        public void CanPlaceNumber_ConflictoEnBloque_RetornaFalso() // Y ahora probamos que no se puede colocar un número que ya existe en el mismo bloque 3x3.
        {

            var board = CrearTableroEstandar();

            // Ya tenemos un 5 en el bloque del medio superior que son la fila numero 1 y columna número 2.
            bool resultado = board.CanPlaceNumber(1, 2, 5);


            Assert.False(resultado);
        }

        // Tests para casos válidos de colocación de números.

        [Fact]
        public void CanPlaceNumber_NumeroValido_RetornaVerdadero() // Probamos colocar un número que no genera conflictos en fila, columna o bloque.
        {

            var board = CrearTableroEstandar();

            // Colocar el número 4 en la fila 0, columna 2 no nos generará conflictos.
            bool resultado = board.CanPlaceNumber(0, 2, 4);


            Assert.True(resultado);
        }

        [Fact]
        public void CanPlaceNumber_TableroVacio_PermiteCualquierNumero() // Probamos en un tablero vacío que se pueda colocar cualquier número.
        {

            var board = new SudokuBoard(blockSize: 3);


            bool resultado = board.CanPlaceNumber(0, 0, 5); // vamos a colocar un 5 en la posición: fila 0, columna 0. (No confundir con el test anterior que tiene valores predefinidos)


            Assert.True(resultado);
        }

        [Theory] // Esto más que nada es para optimizar la escritura de tests similares. Osea en vez de escribir varios tests parecidos, usamos Theory e InlineData para probar varios casos en un solo método.
        [InlineData(0, 2, 1, true)]
        [InlineData(0, 2, 2, true)]
        [InlineData(0, 2, 4, true)]
        [InlineData(0, 2, 5, false)]  // Ya existe en la posición fila 0, columna 0.
        [InlineData(0, 2, 7, false)]  // Ya existe en la posición fila 0, columna 4.
        [InlineData(0, 2, 3, false)]  // Ya existe en la posición fila 1, columna 1.
        public void CanPlaceNumber_VariosNumeros_RetornaResultadoCorrecto(int row, int col, int value, bool esperado) // Vamos a probar varios numeros en la misma posición fila 0, columna 2.
        {

            var board = CrearTableroEstandar();


            bool resultado = board.CanPlaceNumber(row, col, value); // Probamos colocar diferentes números en la posición fila 0, columna 2. Para que así nos devuelva un valor boolean correcto o falso dependiendo del número que intentemos colocar.


            Assert.Equal(esperado, resultado);
        }

        // Test más específicos para validar bloques. Osea, que no se pueda colocar un número que ya existe en el mismo bloque 3x3.

        [Fact]
        public void CanPlaceNumber_ValidaBloqueSupIzq_Correctamente()
        {

            var board = CrearTableroEstandar();
            // Bloque superior izquierdo ya contiene lo numeros: 5, 3, 6, 9, 8.

            // Act & Assert
            Assert.False(board.CanPlaceNumber(2, 2, 5)); // El 5 ya está en el bloque, posición de la fila 0, columna 0.
            Assert.False(board.CanPlaceNumber(2, 2, 3)); // El 3 ya está en el bloque, posición de la fila 0, columna 1.
            Assert.True(board.CanPlaceNumber(2, 2, 1));  // El 1 no está en el bloque.
        }

        [Fact]
        public void CanPlaceNumber_ValidaBloqueCentral_Correctamente() // Vamos a probar la validación del bloque central 3x3.
        {

            var board = CrearTableroEstandar();
            // El bloque central ya contiene los números: 1, 9, 5, 6, 8, 3, 2.


            Assert.False(board.CanPlaceNumber(4, 4, 6)); // El numero 6 ya está en el bloque, posición fila 3, columna 4.
            Assert.True(board.CanPlaceNumber(4, 4, 5));  // 5 no está en el bloque.
        }

        // Tests para tableros de diferentes tamaños.

        [Fact]
        public void CanPlaceNumber_BlockSize2_FuncionaCorrectamente() // Probamos un tablero de Sudoku 4x4 (blockSize 2).
        {

            var board = new SudokuBoard(blockSize: 2);
            board.Grid[0, 0] = 1;
            board.Grid[0, 1] = 2;

            Assert.False(board.CanPlaceNumber(0, 2, 1)); // El número 1 ya está en la posición fila 0, columna 0.
            Assert.False(board.CanPlaceNumber(1, 1, 2)); // El número 2 ya está en la posición fila 0, columna 1.
            Assert.False(board.CanPlaceNumber(1, 1, 1)); // El número 1 ya está en la posición fila 0, columna 0 (mismo bloque).
            Assert.True(board.CanPlaceNumber(0, 2, 3));  // El número 3 no está en fila, columna o bloque. Por lo que es válido de colocar.
        }

        // Tests para casos límite.

        [Fact]
        public void CanPlaceNumber_UltimaFila_ValidaCorrectamente() // Probamos la última fila y columna del tablero.
        {

            var board = CrearTableroEstandar();


            Assert.False(board.CanPlaceNumber(8, 0, 9)); // El numero 9 ya está en la posición fila 8, columna 8.
            Assert.True(board.CanPlaceNumber(8, 0, 3));  // Podemos colocar un 3 en la posición fila 8, columna 0. Porque no genera conflictos.
        }

        [Fact]
        public void CanPlaceNumber_UltimaColumna_ValidaCorrectamente() // Probamos la última columna del tablero.
        {

            var board = CrearTableroEstandar();


            Assert.False(board.CanPlaceNumber(0, 8, 9)); // El numero 9 ya está en la posición fila 8, columna 8.
            Assert.True(board.CanPlaceNumber(0, 8, 1));  // Podemos colocar un 1 en la posición fila 0, columna 8. Porque no genera conflictos.
        }

        [Fact]
        public void CanPlaceNumber_EsquinaSuperiorIzquierda_ValidaCorrectamente() // Probamos la esquina superior izquierda del tablero.
        {

            var board = new SudokuBoard(blockSize: 3);


            bool resultado = board.CanPlaceNumber(0, 0, 1); // Colocamos un 1 en la posición fila 0, columna 0.

            Assert.True(resultado);
        }

        [Fact]
        public void CanPlaceNumber_EsquinaInferiorDerecha_ValidaCorrectamente() // Probamos la esquina inferior derecha del tablero.
        {

            var board = new SudokuBoard(blockSize: 3);


            bool resultado = board.CanPlaceNumber(8, 8, 9); // Colocamos un 9 en la posición fila 8, columna 8. Y no debería haber conflictos.


            Assert.True(resultado);
        }
    }
}