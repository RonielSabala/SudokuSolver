using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Sudoku;

class Program
{
    const int maxBlockSize = 12;
    const int TIMEOUT_SECONDS = 2;
    static readonly string OutputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "sudoku_bench_output_htmls");
    static readonly int[] threadCounts = new int[] { 1, 2, 4, 8 };
    static readonly double[] RemovalValues = new double[] { 0.10, 0.20, 0.30, 0.40, 0.50 };
    const double removalPercentage = 0.30;

    static void Main()
    {
        Console.WriteLine("Sudoku Solver - Benchmark y análisis de rendimiento");
        Console.WriteLine();
        Console.WriteLine("Elige una opción:");
        Console.WriteLine("  1. Benchmark de Tiempo (Benchmarks.cs)");
        Console.WriteLine("  2. Análisis de speedup y eficiencia (PerformanceMetrics.cs)");
        Console.WriteLine("  3. Benchmark 3D y generar HTML (Benchmarks.cs)");
        Console.WriteLine();
        Console.Write("Opción: ");
        
        string? choice = Console.ReadLine();
        
        if (choice == "2")
        {
            PerformanceMetrics.Run();
        }
        else if (choice == "3")
        {
            Run3DBenchmark();
        }
        else
        {
            RunBenchmark();
        }
    }

    static void RunBenchmark()
    {
        Console.WriteLine("\n=== Benchmark de Tiempo de Ejecución ===");
        Console.WriteLine($"BlockSize range: 4..{maxBlockSize}");
        Console.WriteLine($"Removal: {(removalPercentage * 100):F1}%");
        Console.WriteLine($"Timeout por intento: {TIMEOUT_SECONDS}s");
        Console.WriteLine();

        var blockSizes = Enumerable.Range(4, maxBlockSize - 3).ToArray(); // 4..maxBlockSize
        
        // Tabla de resultados.
        Console.WriteLine("Tiempo de ejecución (ms) por BlockSize y Cantidad de Hilos:\n");
        
        Console.WriteLine("┌──────────┬────────────┬────────────┬────────────┬────────────┐");
        Console.WriteLine("│ BlockSz  │  1 Hilo    │  2 Hilos   │  4 Hilos   │  8 Hilos   │");
        Console.WriteLine("├──────────┼────────────┼────────────┼────────────┼────────────┤");

        foreach (int blockSize in blockSizes)
        {
            Console.Write($"│ {blockSize:D2}       │");
            
            foreach (int threads in threadCounts)
            {
                double elapsedMs = MeasureExecutionTime(blockSize, threads);
                
                if (elapsedMs < 0)
                {
                    Console.Write("  TIMEOUT   │");
                }
                else
                {
                    Console.Write($"  {elapsedMs,7:F2}   │");
                }
            }
            Console.WriteLine();
        }

        Console.WriteLine("└──────────┴────────────┴────────────┴────────────┴────────────┘");
                Console.WriteLine();
                Console.WriteLine("Benchmark completado.");
        }

        static void Run3DBenchmark()
        {
                Directory.CreateDirectory(OutputDirectory);

                Console.WriteLine("\nSudoku solver - Benchmark 3D (superficie)");
                Console.WriteLine($"BlockSize range: 4..{maxBlockSize}, timeout por intento: {TIMEOUT_SECONDS}s");
                Console.WriteLine($"Output directory: {OutputDirectory}");
                Console.WriteLine($"Removals to run: {string.Join(", ", RemovalValues.Select(r => r.ToString("F2", CultureInfo.InvariantCulture)))}");
                Console.WriteLine();

                var blockSizes = Enumerable.Range(4, Math.Max(0, maxBlockSize - 3)).ToArray(); // El rango de tamaños de bloques.
                var threadsList = threadCounts.Distinct().OrderBy(n => n).ToArray();

                double timeoutMs = TIMEOUT_SECONDS * 1000.0;

                foreach (var removal in RemovalValues)
                {
                        Console.WriteLine($"\nGenerando HTML para removal = {removal:F2}");

                        // Matrices Z: filas = threadsList.Length, columnas = blockSizes.Length
                        int rows = threadsList.Length;
                        int cols = blockSizes.Length;
                        double[][] zSolved = new double[rows][];
                        double[][] zUnsolved = new double[rows][];
                        for (int r = 0; r < rows; r++)
                        {
                                zSolved[r] = new double[cols];
                                zUnsolved[r] = new double[cols];
                                for (int c = 0; c < cols; c++)
                                {
                                        zSolved[r][c] = double.NaN;
                                        zUnsolved[r][c] = double.NaN;
                                }
                        }

                        var timeoutXs = new List<int>();
                        var timeoutYs = new List<double>();
                        var timeoutZs = new List<double>();
                        var timeoutTexts = new List<string>();

                        // Bucle principal de benchmark, que recorre los números de hilos y tamaños de bloque.
                        for (int ti = 0; ti < threadsList.Length; ti++)
                        {
                                int threads = threadsList[ti];
                                Console.WriteLine($"\n=== Ejecutando para {threads} hilo(s) ({ti + 1}/{threadsList.Length}) ===");

                                for (int ci = 0; ci < blockSizes.Length; ci++)
                                {
                                        int blockSize = blockSizes[ci];
                                        Console.Write($"  blockSize={blockSize} ... ");

                                        double elapsedMs = -1.0;

                                        // Generar puzzle para este blockSize y removal.
                                        SudokuBoard board;
                                        try
                                        {
                                                board = new SudokuBoard(blockSize);
                                        }
                                        catch (Exception ex)
                                        {
                                                Console.WriteLine($"\n    ERROR al crear SudokuBoard (blockSize={blockSize}): {ex.Message}");
                                                zSolved[ti][ci] = double.NaN;
                                                zUnsolved[ti][ci] = double.NaN;
                                                continue;
                                        }

                                        var generator = new SudokuGenerator(board, 10);
                                        try
                                        {
                                                generator.GeneratePuzzle((float)removal);
                                        }
                                        catch (Exception ex)
                                        {
                                                Console.WriteLine($"\n    ERROR al generar/rajar: {ex.Message}");
                                                zSolved[ti][ci] = double.NaN;
                                                zUnsolved[ti][ci] = double.NaN;
                                                continue;
                                        }

                                        GC.Collect();
                                        GC.WaitForPendingFinalizers();
                                        GC.Collect();

                                        var sw = Stopwatch.StartNew();
                                        SudokuBoard? solution = null;
                                        Exception? taskEx = null;

                                        var task = System.Threading.Tasks.Task.Run(() =>
                                        {
                                                try
                                                {
                                                        solution = SudokuSolver.TrySolve(board, threads);
                                                }
                                                catch (Exception e)
                                                {
                                                        taskEx = e;
                                                }
                                        });

                                        bool finished = task.Wait(TimeSpan.FromSeconds(TIMEOUT_SECONDS));
                                        sw.Stop();

                                        if (!finished)
                                        {
                                                Console.WriteLine($"(timeout {TIMEOUT_SECONDS}s)");
                                                elapsedMs = -1;
                                                zSolved[ti][ci] = double.NaN;
                                                zUnsolved[ti][ci] = double.NaN;

                                                timeoutXs.Add(blockSize);
                                                timeoutYs.Add(removal);
                                                timeoutZs.Add(timeoutMs);
                                                timeoutTexts.Add($"blockSize={blockSize}, removal={removal:F2}\\nTimeout");
                                                continue;
                                        }

                                        if (taskEx != null)
                                        {
                                                Console.WriteLine($"\n    EXCEPCIÓN al resolver: {taskEx.Message} (intento único)");
                                                elapsedMs = -1;
                                                zSolved[ti][ci] = double.NaN;
                                                zUnsolved[ti][ci] = double.NaN;
                                                continue;
                                        }

                                        elapsedMs = sw.Elapsed.TotalMilliseconds;

                                        if (solution != null)
                                        {
                                                Console.WriteLine($"OK (t={elapsedMs.ToString("F3", CultureInfo.InvariantCulture)} ms)");
                                                zSolved[ti][ci] = elapsedMs;
                                                zUnsolved[ti][ci] = double.NaN;
                                        }
                                        else
                                        {
                                                Console.WriteLine($"No resuelto (t={elapsedMs.ToString("F3", CultureInfo.InvariantCulture)} ms)");
                                                zSolved[ti][ci] = double.NaN;
                                                zUnsolved[ti][ci] = elapsedMs; // Usamos el tiempo medido para la superficie roja.
                                        }
                                }
                        }

                        // Serializar a JSON para Plotly.
                        var jsonOptions = new JsonSerializerOptions
                        {
                                NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
                        };

                        string xJson = JsonSerializer.Serialize(blockSizes, jsonOptions);
                        string yJson = JsonSerializer.Serialize(threadsList, jsonOptions);
                        string zSolvedJson = JsonSerializer.Serialize(zSolved, jsonOptions);
                        string zUnsolvedJson = JsonSerializer.Serialize(zUnsolved, jsonOptions);
                        string toXJson = JsonSerializer.Serialize(timeoutXs, jsonOptions);
                        string toYJson = JsonSerializer.Serialize(timeoutYs, jsonOptions);
                        string toZJson = JsonSerializer.Serialize(timeoutZs, jsonOptions);
                        string toTextJson = JsonSerializer.Serialize(timeoutTexts, jsonOptions);

                        // Generación del HTML.
                        string title = $"Removal = {removal:F3}";
                        string htmlPath = Path.Combine(OutputDirectory, $"sudoku_3d_plot_removal_{removal.ToString("F3", CultureInfo.InvariantCulture)}.html");

                        var html = $@"<!doctype html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>Sudoku Solver 3D Surface Plot (removal={removal:F3})</title>
    <script src=""https://cdn.plot.ly/plotly-latest.min.js""></script>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        h2 {{ margin-bottom: 8px; }}
    </style>
</head>
<body>
        <h2>Sudoku solver benchmark — surface (X:blockSize, Y:hilos, Z:tiempo ms)</h2>
    <p>RemovalPercentage = {removal:F3}. Timeout por intento: {TIMEOUT_SECONDS} s.</p>

    <div id=""plot_surface"" style=""width:100%;height:720px;""></div>
    <script>
        (function() {{
            var solvedSurface = {{
                z: {zSolvedJson},
                x: {xJson},
                y: {yJson},
                type: 'surface',
                colorscale: 'Viridis',
                showscale: true,
                name: 'Tiempo (ms)'
            }};

            var unsolvedSurface = {{
                z: {zUnsolvedJson},
                x: {xJson},
                y: {yJson},
                type: 'surface',
                colorscale: [['0','rgb(255,200,200)'],['1','rgb(200,0,0)']],
                opacity: 0.45,
                showscale: false,
                name: 'No resuelto (superficie)'
            }};

            var timeouts = {{
                x: {toXJson},
                y: {toYJson},
                z: {toZJson},
                mode: 'markers',
                type: 'scatter3d',
                marker: {{
                    color: 'orange',
                    size: 6,
                    symbol: 'diamond'
                }},
                text: {toTextJson},
                hoverinfo: 'text',
                name: 'Timeout'
            }};

            var data = [solvedSurface, unsolvedSurface, timeouts];

            var layout = {{
                title: '{title}',
                autosize: true,
                scene: {{
                    xaxis: {{title: 'blockSize (sub-square size)'}},
                    yaxis: {{title: 'hilos'}},
                    zaxis: {{title: 'tiempo (ms)'}}
                }},
                legend: {{orientation: 'h'}}
            }};

            Plotly.newPlot('plot_surface', data, layout);
        }})();
    </script>

    <hr/>
    <p>Generado: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>
</body>
</html>";

                        File.WriteAllText(htmlPath, html);
                        Console.WriteLine($"\nHTML generado en: {htmlPath}");
                } 

                Console.WriteLine("\nHTML generados");
                Console.WriteLine($"Revisa la carpeta: {OutputDirectory}");
        }

        static double MeasureExecutionTime(int blockSize, int threads)
    {
        try
        {
            // Crear el tablero de sudoku.
            SudokuBoard board = new SudokuBoard(blockSize);
            
            // Generará un sudoku con el porcentaje de celdas removidas.
            var generator = new SudokuGenerator(board, 10);
            generator.GeneratePuzzle((float)removalPercentage);

            // Limpiar la memoria.
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // Ejecutará el solver.
            var sw = Stopwatch.StartNew();
            SudokuBoard? solution = null;
            Exception? taskEx = null;

            var task = System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    solution = SudokuSolver.TrySolve(board, threads);
                }
                catch (Exception e)
                {
                    taskEx = e;
                }
            });

            bool finished = task.Wait(TimeSpan.FromSeconds(TIMEOUT_SECONDS));
            sw.Stop();

            if (!finished)
            {
                return -1; // Es el timeout para la medida de tiempo.
            }

            if (taskEx != null)
            {
                return -2; // Indica error en la tarea.
            }

            return sw.Elapsed.TotalMilliseconds;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nError en blockSize={blockSize}: {ex.Message}");
            return -3;
        }
    }
}
