using System.Diagnostics;
using System.Globalization;
using System.Text;
using Sudoku;

class PerformanceMetrics
{
    const int TamañoMaximoBloque = 12; // Este es el tamaño máximo de bloque que se probará, osea un Sudoku de 12x12.
    const int Timeout_Segundos = 30; // Tiempo máximo permitido por intento de solución.
    const int Iteraciones_Por_Config = 3; // Número de iteraciones para promediar.
    
    static readonly string OutputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "performance_metrics_output");
    static readonly int[] RecuentosHilos = new int[] { 1, 2, 4, 8 };
    static readonly int[] blockSizes = new int[] { 4, 6, 8, 9 };
    static readonly double PorcentajeRemovalCeldas = 0.30; // 30% de celdas removidas, porque este es un valor común en sudokus.

    public static void Run()
    {
        Directory.CreateDirectory(OutputDirectory);

        Console.WriteLine("  SUDOKU SOLVER - Métricas de rendimiento (Speedup y eficiencia)");
        Console.WriteLine();
        Console.WriteLine($"Configuración:");
        Console.WriteLine($"  Block Sizes: {string.Join(", ", blockSizes)}");
        Console.WriteLine($"  Conteo de Hilos: {string.Join(", ", RecuentosHilos)}");
        Console.WriteLine($"  Porcentaje de Remoción: {PorcentajeRemovalCeldas:P0}");
        Console.WriteLine($"  Iteraciones por configuración: {Iteraciones_Por_Config}");
        Console.WriteLine($"  Timeout por intento: {Timeout_Segundos}s");
        Console.WriteLine($"  Directorio de salida: {OutputDirectory}");
        Console.WriteLine();

        // Diccionario para almacenar tiempos: [tamaño_bloque][hilos] = tiempo promedio en ms.
        var tiempos_ejecucion = new Dictionary<int, Dictionary<int, double>>();
        var desv_estandar_tiempos = new Dictionary<int, Dictionary<int, double>>();

        // Ejecutará los benchamarks de rendimiento para comparar speedup y eficiencia.
        Console.WriteLine("Starting benchmark runs...");
        Console.WriteLine();

        foreach (var blockSize in blockSizes)
        {
            tiempos_ejecucion[blockSize] = new Dictionary<int, double>();
            desv_estandar_tiempos[blockSize] = new Dictionary<int, double>();

            foreach (var hilos in RecuentosHilos)
            {
                var tiempos_iteracion = new List<double>();

                Console.Write($"BlockSize: {blockSize} | Hilos: {hilos} ... ");

                for (int iteracion = 0; iteracion < Iteraciones_Por_Config; iteracion++)
                {
                    double tiempo_transcurrido_ms = MeasureExecutionTime(blockSize, hilos);

                    if (tiempo_transcurrido_ms > 0)
                    {
                        tiempos_iteracion.Add(tiempo_transcurrido_ms);
                        Console.Write($"[OK]");
                    }
                    else
                    {
                        Console.Write($"[FAIL]");
                    }
                }

                Console.WriteLine();

                if (tiempos_iteracion.Count > 0)
                {
                    double promedio = tiempos_iteracion.Average();
                    double desv_estandar = Math.Sqrt(tiempos_iteracion.Sum(t => Math.Pow(t - promedio, 2)) / tiempos_iteracion.Count);
                    tiempos_ejecucion[blockSize][hilos] = promedio;
                    desv_estandar_tiempos[blockSize][hilos] = desv_estandar;
                    Console.WriteLine($"  Average: {promedio:F3} ms (StdDev: {desv_estandar:F3}ms)");
                }
                else
                {
                    tiempos_ejecucion[blockSize][hilos] = double.NaN;
                    desv_estandar_tiempos[blockSize][hilos] = double.NaN;
                    Console.WriteLine($"  Error: All iterations failed");
                }

                Console.WriteLine();
            }
        }

        // Calcular speedup y eficiencia para los reportes.
        Console.WriteLine();
        Console.WriteLine("Análisis del speedup (vs Secuencial - 1 thread)");
        Console.WriteLine();

        var reporte_speedup = new StringBuilder();
        var reporte_eficiencia = new StringBuilder();

        reporte_speedup.AppendLine("Reporte de speedup");
        reporte_speedup.AppendLine("----------------------");
        reporte_speedup.AppendLine($"Removal: {PorcentajeRemovalCeldas:P0} | Iteraciones: {Iteraciones_Por_Config}");
        reporte_speedup.AppendLine();

        reporte_eficiencia.AppendLine("Reporte de eficiencia");
        reporte_eficiencia.AppendLine("----------------------");
        reporte_eficiencia.AppendLine($"Removal: {PorcentajeRemovalCeldas:P0} | Iteraciones: {Iteraciones_Por_Config}");
        reporte_eficiencia.AppendLine();

        foreach (var blockSize in blockSizes)
        {
            Console.WriteLine($"BlockSize: {blockSize}");
            reporte_speedup.AppendLine($"BlockSize: {blockSize}");
            reporte_eficiencia.AppendLine($"BlockSize: {blockSize}");

            if (!tiempos_ejecucion[blockSize].ContainsKey(1) || double.IsNaN(tiempos_ejecucion[blockSize][1]))
            {
                Console.WriteLine("  Error: Sequential execution (1 thread) failed - skipping");
                continue;
            }

            double tiempo_secuencial = tiempos_ejecucion[blockSize][1];

            foreach (var hilos in RecuentosHilos)
            {
                if (!tiempos_ejecucion[blockSize].ContainsKey(hilos) || double.IsNaN(tiempos_ejecucion[blockSize][hilos]))
                {
                    Console.WriteLine($"  Hilos={hilos}: N/A");
                    reporte_speedup.AppendLine($"  Hilos={hilos}: N/A");
                    reporte_eficiencia.AppendLine($"  Hilos={hilos}: N/A");
                    continue;
                }

                double tiempo_paralelo = tiempos_ejecucion[blockSize][hilos];
                double speedup = tiempo_secuencial / tiempo_paralelo;
                double eficiencia = (speedup / hilos) * 100;

                Console.WriteLine($"  Hilos={hilos}: Time={tiempo_paralelo:F3}ms, Speedup={speedup:F3}x, Eficiencia={eficiencia:F2}%");

                reporte_speedup.AppendLine($"  Hilos={hilos}: Speedup = {speedup:F3}x (ideal: {hilos:F1}x)");
                reporte_eficiencia.AppendLine($"  Hilos={hilos}: Eficiencia = {eficiencia:F2}%");
            }

            Console.WriteLine();
            reporte_speedup.AppendLine();
            reporte_eficiencia.AppendLine();
        }

        // Va a generar la tabla resumen que se mostrará en consola y se guardará en el HTML.
        Console.WriteLine();
        Console.WriteLine("Resumen de tiempos de ejecución (ms)");
        Console.WriteLine();
        PrintExecutionTimeTable(tiempos_ejecucion);

        Console.WriteLine();
        Console.WriteLine("Resumen de speedup");
        Console.WriteLine();
        PrintSpeedupTable(tiempos_ejecucion);

        Console.WriteLine();
        Console.WriteLine("Resumen de eficiencia (%)");
        Console.WriteLine();

        PrintEfficiencyTable(tiempos_ejecucion);

        // Directorio para los reportes, quería guardarlo en imágenes pero es más sencillo en texto.
        SaveReport(Path.Combine(OutputDirectory, "speedup_report.txt"), reporte_speedup.ToString());
        SaveReport(Path.Combine(OutputDirectory, "efficiency_report.txt"), reporte_eficiencia.ToString());
        
        // Generación del resumen HTML.
        SaveHtmlSummary(tiempos_ejecucion, reporte_speedup.ToString(), reporte_eficiencia.ToString());

        Console.WriteLine();
        Console.WriteLine("Reportes guardados en:");
        Console.WriteLine($"  {Path.Combine(OutputDirectory, "speedup_report.txt")}");
        Console.WriteLine($"  {Path.Combine(OutputDirectory, "efficiency_report.txt")}");
        Console.WriteLine();
        Console.WriteLine($"Directorio de salida: {OutputDirectory}");
    }

    static double MeasureExecutionTime(int blockSize, int hilos)
    {
        try
        {
            SudokuBoard tablero = new SudokuBoard(blockSize);
            var generador = new SudokuGenerator(tablero, 10);
            generador.GeneratePuzzle((float)PorcentajeRemovalCeldas);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var cronometro = Stopwatch.StartNew();
            SudokuBoard? solucion = null;
            Exception? excepcion_tarea = null;

            var tarea = System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    solucion = SudokuSolver.TrySolve(tablero, hilos);
                }
                catch (Exception e)
                {
                    excepcion_tarea = e;
                }
            });

            bool finalizado = tarea.Wait(TimeSpan.FromSeconds(Timeout_Segundos));
            cronometro.Stop();

            if (!finalizado || excepcion_tarea != null)
            {
                return -1; // Error o timeout durante la solución del Sudoku.
            }

            return cronometro.Elapsed.TotalMilliseconds;
        }
        catch
        {
            return -1; // Error en la generación de tablero o en la configuración.
        }
    }

    static void PrintExecutionTimeTable(Dictionary<int, Dictionary<int, double>> tiempos_ejecucion)
    {
        Console.Write("BlockSize");
        foreach (var hilos in RecuentosHilos)
        {
            Console.Write($"  |  {hilos}T (ms)");
        }
        Console.WriteLine();

        // Filas de las tablas de tiempos de ejecución (Será lo mismo por las siguientes tablas hasta la línea 324).
        foreach (var blockSize in blockSizes)
        {
            Console.Write($"    {blockSize}");
            foreach (var hilos in RecuentosHilos)
            {
                if (tiempos_ejecucion[blockSize].ContainsKey(hilos))
                {
                    double t = tiempos_ejecucion[blockSize][hilos];
                    string valor = double.IsNaN(t) ? "N/A" : t.ToString("F3", CultureInfo.InvariantCulture);
                    Console.Write($"  |  {valor,10}");
                }
                else
                {
                    Console.Write($"  |      N/A");
                }
            }
            Console.WriteLine();
        }
    }

    static void PrintSpeedupTable(Dictionary<int, Dictionary<int, double>> tiempos_ejecucion)
    {

        Console.Write("BlockSize");
        foreach (var hilos in RecuentosHilos)
        {
            Console.Write($"  |  {hilos}T Speedup");
        }
        Console.WriteLine();

        foreach (var blockSize in blockSizes)
        {
            Console.Write($"    {blockSize}");

            if (!tiempos_ejecucion[blockSize].ContainsKey(1) || double.IsNaN(tiempos_ejecucion[blockSize][1]))
            {
                // If sequential is missing, print N/A for all thread counts
                foreach (var hilos in RecuentosHilos)
                {
                    Console.Write($"  |    N/A");
                }
                Console.WriteLine();
                continue;
            }

            double tiempo_secuencial = tiempos_ejecucion[blockSize][1];

            foreach (var hilos in RecuentosHilos)
            {
                if (tiempos_ejecucion[blockSize].ContainsKey(hilos) && !double.IsNaN(tiempos_ejecucion[blockSize][hilos]))
                {
                    double speedup = tiempo_secuencial / tiempos_ejecucion[blockSize][hilos];
                    Console.Write($"  |  {speedup:F3}x");
                }
                else
                {
                    Console.Write($"  |    N/A");
                }
            }
            Console.WriteLine();
        }
    }

    static void PrintEfficiencyTable(Dictionary<int, Dictionary<int, double>> tiempos_ejecucion)
    {
        Console.Write("BlockSize");
        foreach (var hilos in RecuentosHilos)
        {
            Console.Write($"  |  {hilos}T Eff(%)");
        }
        Console.WriteLine();

        foreach (var blockSize in blockSizes)
        {
            Console.Write($"    {blockSize}");

            if (!tiempos_ejecucion[blockSize].ContainsKey(1) || double.IsNaN(tiempos_ejecucion[blockSize][1]))
            {
                // If sequential is missing, print N/A for all thread counts
                foreach (var hilos in RecuentosHilos)
                {
                    Console.Write($"  |  N/A");
                }
                Console.WriteLine();
                continue;
            }

            double tiempo_secuencial = tiempos_ejecucion[blockSize][1];

            foreach (var hilos in RecuentosHilos)
            {
                if (tiempos_ejecucion[blockSize].ContainsKey(hilos) && !double.IsNaN(tiempos_ejecucion[blockSize][hilos]))
                {
                    double speedup = tiempo_secuencial / tiempos_ejecucion[blockSize][hilos];
                    double eficiencia = (speedup / hilos) * 100;
                    Console.Write($"  |  {eficiencia:F2}%");
                }
                else
                {
                    Console.Write($"  |  N/A");
                }
            }
            Console.WriteLine();
        }
    }

    static void SaveReport(string filePath, string content)
    {
        try
        {
            File.WriteAllText(filePath, content, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving report to {filePath}: {ex.Message}");
        }
    }

    static void SaveHtmlSummary(Dictionary<int, Dictionary<int, double>> tiempos_ejecucion, string reporteSpeedupTexto, string reporteEficienciaTexto)
    {
        try
        {
            var constructorHTML = new StringBuilder();
            constructorHTML.AppendLine("<!doctype html>");
            constructorHTML.AppendLine("<html lang=\"es\"> ");
            constructorHTML.AppendLine("<head>");
            constructorHTML.AppendLine("  <meta charset=\"utf-8\"> ");
            constructorHTML.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\"> ");
            constructorHTML.AppendLine("  <title>Resumen de rendimiento - Sudoku Solver</title>");
            constructorHTML.AppendLine("  <style>");
            constructorHTML.AppendLine("    body { font-family: Arial, sans-serif; margin: 20px; color: #222; }");
            constructorHTML.AppendLine("    table { border-collapse: collapse; width: 100%; margin-bottom: 20px; }");
            constructorHTML.AppendLine("    th, td { border: 1px solid #ccc; padding: 8px; text-align: right; }");
            constructorHTML.AppendLine("    th { background: #f0f0f0; text-align: center; }");
            constructorHTML.AppendLine("    caption { text-align: left; font-weight: bold; margin-bottom: 8px; }");
            constructorHTML.AppendLine("  </style>");
            constructorHTML.AppendLine("</head>");
            constructorHTML.AppendLine("<body>");

            constructorHTML.AppendLine($"<h1>Resumen de rendimiento</h1>");
            constructorHTML.AppendLine($"<p>Porcentaje de celdas removidas: {PorcentajeRemovalCeldas:P0}. Iteraciones por configuración: {Iteraciones_Por_Config}.</p>");

            // Tabla de tiempos de ejecución.
            constructorHTML.AppendLine("<table>");
            constructorHTML.AppendLine("<caption>Resumen de tiempos de ejecución (ms)</caption>");
            constructorHTML.AppendLine("<tr><th>BlockSize</th>");
            foreach (var hilos in RecuentosHilos)
                constructorHTML.AppendLine($"<th>Hilos {hilos}</th>");
            constructorHTML.AppendLine("</tr>");

            foreach (var blockSize in blockSizes)
            {
                constructorHTML.AppendLine($"<tr><td style='text-align:center'>{blockSize}</td>");
                foreach (var hilos in RecuentosHilos)
                {
                    string valor = "N/A";
                    if (tiempos_ejecucion.ContainsKey(blockSize) && tiempos_ejecucion[blockSize].ContainsKey(hilos) && !double.IsNaN(tiempos_ejecucion[blockSize][hilos]))
                        valor = tiempos_ejecucion[blockSize][hilos].ToString("F3", CultureInfo.InvariantCulture);
                    constructorHTML.AppendLine($"<td>{valor}</td>");
                }
                constructorHTML.AppendLine("</tr>");
            }
            constructorHTML.AppendLine("</table>");

            // Tabla de speedup.
            constructorHTML.AppendLine("<table>");
            constructorHTML.AppendLine("<caption>Resumen de speedup</caption>");
            constructorHTML.AppendLine("<tr><th>BlockSize</th>");
            foreach (var hilos in RecuentosHilos)
                constructorHTML.AppendLine($"<th>Hilos {hilos}</th>");
            constructorHTML.AppendLine("</tr>");

            foreach (var blockSize in blockSizes)
            {
                constructorHTML.AppendLine($"<tr><td style='text-align:center'>{blockSize}</td>");
                if (!tiempos_ejecucion.ContainsKey(blockSize) || !tiempos_ejecucion[blockSize].ContainsKey(1) || double.IsNaN(tiempos_ejecucion[blockSize][1]))
                {
                    // sequential missing -> N/A for all thread columns
                    for (int i = 0; i < RecuentosHilos.Length; i++)
                        constructorHTML.AppendLine("<td>N/A</td>");
                }
                else
                {
                    double tiempo_secuencial = tiempos_ejecucion[blockSize][1];
                    foreach (var hilos in RecuentosHilos)
                    {
                        string valor = "N/A";
                        if (tiempos_ejecucion[blockSize].ContainsKey(hilos) && !double.IsNaN(tiempos_ejecucion[blockSize][hilos]))
                        {
                            double speedup = tiempo_secuencial / tiempos_ejecucion[blockSize][hilos];
                            valor = speedup.ToString("F3", CultureInfo.InvariantCulture) + "x";
                        }
                        constructorHTML.AppendLine($"<td>{valor}</td>");
                    }
                }
                constructorHTML.AppendLine("</tr>");
            }
            constructorHTML.AppendLine("</table>");

            // Tabla de eficiencia.
            constructorHTML.AppendLine("<table>");
            constructorHTML.AppendLine("<caption>Resumen de eficiencia (%)</caption>");
            constructorHTML.AppendLine("<tr><th>BlockSize</th>");
            foreach (var hilos in RecuentosHilos)
                constructorHTML.AppendLine($"<th>Hilos {hilos}</th>");
            constructorHTML.AppendLine("</tr>");

            foreach (var blockSize in blockSizes)
            {
                constructorHTML.AppendLine($"<tr><td style='text-align:center'>{blockSize}</td>");
                if (!tiempos_ejecucion.ContainsKey(blockSize) || !tiempos_ejecucion[blockSize].ContainsKey(1) || double.IsNaN(tiempos_ejecucion[blockSize][1]))
                {
                    // sequential missing -> N/A for all thread columns
                    for (int i = 0; i < RecuentosHilos.Length; i++)
                        constructorHTML.AppendLine("<td>N/A</td>");
                }
                else
                {
                    double tiempo_secuencial = tiempos_ejecucion[blockSize][1];
                    foreach (var hilos in RecuentosHilos)
                    {
                        string valor = "N/A";
                        if (tiempos_ejecucion[blockSize].ContainsKey(hilos) && !double.IsNaN(tiempos_ejecucion[blockSize][hilos]))
                        {
                            double speedup = tiempo_secuencial / tiempos_ejecucion[blockSize][hilos];
                            double eficiencia = (speedup / hilos) * 100.0;
                            valor = eficiencia.ToString("F2", CultureInfo.InvariantCulture) + "%";
                        }
                        constructorHTML.AppendLine($"<td>{valor}</td>");
                    }
                }
                constructorHTML.AppendLine("</tr>");
            }
            constructorHTML.AppendLine("</table>");

            constructorHTML.AppendLine("</body>");
            constructorHTML.AppendLine("</html>");

            string rutaSalidaHTML = Path.Combine(OutputDirectory, "performance_summary.html");
            File.WriteAllText(rutaSalidaHTML, constructorHTML.ToString(), Encoding.UTF8);
            Console.WriteLine($"HTML generado en: {rutaSalidaHTML}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generando HTML: {ex.Message}");
        }
    }
}
