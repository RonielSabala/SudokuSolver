using Sudoku;

class Program
{
    static void Main()
    {
        var sudoku = new SudokuBoard(blockSize: 3);
        var sudokuGenerator = new SudokuGenerator(sudoku);

        sudokuGenerator.GenerateSolution();
        Console.WriteLine(sudoku.ToString());
        Console.WriteLine();

        sudokuGenerator.RemoveRandomCells(removalPercentage: 0.5f);
        Console.WriteLine(sudoku.ToString());

        var solvedSudoku = SudokuSolver.TrySolve(sudoku, maxDegreeOfParallelism: 8);
        if (solvedSudoku != null)
        {
            Console.WriteLine(solvedSudoku.ToString());
            Console.WriteLine(solvedSudoku.IsValid());
        }
    }
}