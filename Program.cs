using System;
using System.Diagnostics;
using System.Globalization;
using System.Numerics;

namespace AutoTurel
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch appWatch = Stopwatch.StartNew();
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

            Console.WriteLine("=== AutoTurel Professional Edition ===");

            // Замер времени работы с JSON / Measure JSON IO time
            Stopwatch ioWatch = Stopwatch.StartNew();
            var config = ConfigLoader.Load<AppConfig>("config.json");
            var env = ConfigLoader.Load<EnvironmentDefaults>(config.EnvironmentDefaultsPath);
            var drag = new DragProvider();
            drag.LoadFromFile(config.DragTablesPath);
            ioWatch.Stop();

            // Параметры по умолчанию
            Vector3 tp = new Vector3(1000, 0, 0), tv = Vector3.Zero, ta = Vector3.Zero;
            int iters = env.Iterations;

            // CLI Parsing
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--target-pos") tp = ParseV(args[++i]);
                if (args[i] == "--target-vel") tv = ParseV(args[++i]);
                if (args[i] == "--target-acc") ta = ParseV(args[++i]);
                if (args[i] == "--iters") iters = int.Parse(args[++i]);
                if (args[i] == "--csv") config.SaveTrajectoryCsv = true;
            }

            var atmosphere = new Atmosphere(env.TemperatureBase, env.PressureBase);
            var solver = new BallisticSolver(atmosphere, drag, env, config);
            
            var res = solver.Solve(tp, tv, ta, env.MuzzleVelocity, iters);

            appWatch.Stop();

            Console.WriteLine("\n--- РЕЗУЛЬТАТЫ ---");
            Console.WriteLine($"Углы: Pitch {res.Pitch:F3}°, Yaw {res.Yaw:F3}°");
            Console.WriteLine($"Промах: Линейный {res.Miss:F3}м, Угловой {res.AngMiss:F4}°");
            
            Console.WriteLine("\n--- ТАЙМИНГИ ---");
            Console.WriteLine($"Работа с файлами (JSON): {ioWatch.Elapsed.TotalMilliseconds:F2} мс");
            Console.WriteLine($"Время всех итераций:    {res.TotalMs} мс");
            Console.WriteLine($"Среднее на итерацию:    {res.AvgIterMs:F2} мс");
            Console.WriteLine($"Общее время программы:  {appWatch.Elapsed.TotalMilliseconds:F2} мс");
        }

        static Vector3 ParseV(string s) {
            var p = s.Split(',');
            return new Vector3(float.Parse(p[0]), float.Parse(p[1]), float.Parse(p[2]));
        }
    }
}
