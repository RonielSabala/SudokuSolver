using Sudoku;
using System;

class Program
{
    static void Main()
    {
        // MENSAJE DE BIENVENIDA
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("===============================================");
        Console.WriteLine("     BIENVENIDO A TU GENERADOR DE SUDOKU");
        Console.WriteLine("===============================================");
        Console.ResetColor();

        // Pedir blockSize al usuario
        Console.Write("\nIngresa el tamaño del bloque (3 = 9x9, 4 = 16x16, 5 = 25x25): ");
        int blockSize;
        while (!int.TryParse(Console.ReadLine(), out blockSize) || blockSize < 2)
        {
            Console.Write("Valor inválido. Ingresa un número mayor o igual a 2: ");
        }

        // Pedir porcentaje al usuario
        Console.Write("Ingresa el porcentaje de celdas a eliminar (0 a 100): ");
        int porcentaje;
        while (!int.TryParse(Console.ReadLine(), out porcentaje) || porcentaje < 0 || porcentaje > 100)
        {
            Console.Write("Valor inválido. Ingresa un valor entre 0 y 100: ");
        }

        float removalPercentage = porcentaje / 100f;

        // Crear Sudoku
        var sudoku = new SudokuBoard(blockSize);
        var generator = new SudokuGenerator(sudoku);

        generator.GeneratePuzzle(removalPercentage);

        // Mostrar sudoku generado
        Console.WriteLine("\n===============================");
        Console.WriteLine(" SUDOKU GENERADO");
        Console.WriteLine("===============================\n");
        Console.WriteLine(sudoku.ToString());

        // Preguntar si quiere ver solución
        Console.Write("\n¿Quieres ver el sudoku resuelto? (S/N): ");
        string opcion = Console.ReadLine()!.Trim().ToUpper();

        if (opcion != "S")
        {
            Console.WriteLine("\nGracias por usar el generador");
            return;
        }

        // Resolver sudoku
        var solvedSudoku = SudokuSolver.TrySolve(sudoku, 8);

        if (solvedSudoku != null)
        {
            Console.WriteLine("\n===============================");
            Console.WriteLine(" SUDOKU RESUELTO");
            Console.WriteLine("===============================\n");
            Console.WriteLine(solvedSudoku.ToString());

            Console.WriteLine("\n===============================");
            Console.WriteLine(" ¿Es válido?");
            Console.WriteLine("===============================\n");
            Console.WriteLine(solvedSudoku.IsValid());
        }
        else
        {
            Console.WriteLine("\nNo se pudo resolver el Sudoku.");
        }
    }
}