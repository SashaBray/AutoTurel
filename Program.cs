using System;
using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Linq;

namespace AutoTurel
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch appSw = Stopwatch.StartNew();
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

            bool fast = args.Contains("--fast");
            bool silent = args.Contains("--silent") || !args.Contains("--debug"); // По умолчанию тихий режим / Default to silent

            AppConfig cfg; EnvironmentDefaults env; DragProvider drag = new DragProvider();
            Stopwatch ioSw = new Stopwatch();

            if (fast) { cfg = new AppConfig(); env = new EnvironmentDefaults(); drag.UseEmbedded(); }
            else { 
                ioSw.Start();
                cfg = ConfigLoader.Load<AppConfig>("config.json");
                env = ConfigLoader.Load<EnvironmentDefaults>(cfg.EnvironmentDefaultsPath);
                drag.LoadFromFile(cfg.DragTablesPath);
                ioSw.Stop();
            }

            // Параметры цели / Target parameters
            Vector3 tp = new Vector3(1000, 0, 0); Vector3 tv = Vector3.Zero; Vector3 ta = Vector3.Zero;
            int iters = env.Iterations; float? mTime = null; bool high = env.UseHighAngle;

            for (int i = 0; i < args.Length; i++) {
                switch (args[i]) {
                    case "--target-pos": tp = ParseV(args[++i]); break;
                    case "--target-vel": tv = ParseV(args[++i]); break;
                    case "--target-acc": ta = ParseV(args[++i]); break;
                    case "--iters": iters = int.Parse(args[++i]); break;
                    case "--max-time": mTime = float.Parse(args[++i]); break;
                    case "--v0": env.MuzzleVelocity = float.Parse(args[++i]); break;
                    case "--high-angle": high = true; break;
                    case "--csv": cfg.SaveTrajectoryCsv = true; break;
                    case "--debug": cfg.EnableDebugOutput = true; silent = false; break;
                }
            }

            var solver = new BallisticSolver(new Atmosphere(env.TemperatureBase, env.PressureBase), drag, env, cfg);
            var res = solver.Solve(tp, tv, ta, env.MuzzleVelocity, iters, mTime, high);

            appSw.Stop();

            // Единый формат вывода: Pitch;Yaw;Miss;AngMiss;TotalTimeMs
            // Unified output format
            Console.WriteLine($"{res.Pitch:F4};{res.Yaw:F4};{res.Miss:F4};{res.AngMiss:F4};{appSw.ElapsedMilliseconds}");
        }

        static Vector3 ParseV(string s) {
            try {
                var p = s.Split(',');
                return new Vector3(float.Parse(p[0]), float.Parse(p[1]), float.Parse(p[2]));
            } catch { return Vector3.Zero; }
        }
    }
}
