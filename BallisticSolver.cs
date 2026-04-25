using System;
using System.Numerics;
using System.Diagnostics;
using System.Text;
using System.IO;

namespace AutoTurel
{
    public class BallisticSolver
    {
        private readonly Atmosphere _atmo;
        private readonly DragProvider _dragProvider;
        private readonly EnvironmentDefaults _env;
        private readonly AppConfig _config;

        public BallisticSolver(Atmosphere atmo, DragProvider drag, EnvironmentDefaults env, AppConfig config)
        {
            _atmo = atmo; _dragProvider = drag; _env = env; _config = config;
        }

        public (double Pitch, double Yaw, double Miss, double AngMiss, long TotalMs) Solve(
            Vector3 tPos, Vector3 tVel, Vector3 tAcc, float v0, int iterations, float? manualMaxTime, bool useHighAngle)
        {
            Stopwatch sw = Stopwatch.StartNew();
            
            // Выбор лимита времени / Choose time limit
            float limit = manualMaxTime ?? (useHighAngle ? _env.MaxFlightTimeHighAngle : _env.MaxFlightTime);

            float currentPitch = useHighAngle ? (float)(45 * Math.PI / 180.0) : (float)Math.Asin(Math.Clamp((tPos.Z - _env.GunPosition.Z) / Vector3.Distance(tPos, _env.GunPosition), -1, 1));
            float currentYaw = (float)Math.Atan2(tPos.Y - _env.GunPosition.Y, tPos.X - _env.GunPosition.X);

            Vector3 aimPoint = tPos;
            float finalMiss = 0; double finalAngMiss = 0;

            for (int i = 0; i < iterations; i++)
            {
                bool isLast = (i == iterations - 1);
                var res = SimulateShot(CalculateVel(currentPitch, currentYaw, v0), tPos, tVel, tAcc, isLast, limit, useHighAngle);

                Vector3 missVec = res.TargetPos - res.ProjPos;
                finalMiss = missVec.Length();

                double d_act = Vector2.Distance(new Vector2(res.ProjPos.X, res.ProjPos.Y), new Vector2(_env.GunPosition.X, _env.GunPosition.Y));
                double d_tar = Vector2.Distance(new Vector2(res.TargetPos.X, res.TargetPos.Y), new Vector2(_env.GunPosition.X, _env.GunPosition.Y));
                double alphaDeg = currentPitch * 180.0 / Math.PI;

                // Отладка выводится только при включенном флаге / Debug output only if enabled
                if (_config.EnableDebugOutput)
                    Console.WriteLine($"Iter {i+1:D2} | Pitch: {alphaDeg:F3}° | ProjPos: <{res.ProjPos.X:F1}, {res.ProjPos.Y:F1}, {res.ProjPos.Z:F1}> | Miss: {finalMiss:F2}m");

                if (useHighAngle)
                {
                    double div = (1.0425 - 1.0426 * Math.Pow(alphaDeg / 90.0, 3));
                    double D = d_act / (Math.Abs(div) > 1e-9 ? div : 1.0);
                    double nTerm = (1.0426 * D - d_tar) * Math.Pow(90, 3) / (Math.Abs(1.0426 * D) > 1e-9 ? 1.0426 * D : 1.0);
                    double nextAlphaDeg = Math.Clamp((nTerm > 0) ? Math.Pow(nTerm, 1.0/3.0) : 45.0, 45.1, 89.9);

                    currentPitch += (float)(((nextAlphaDeg * Math.PI / 180.0) - currentPitch) * _env.ConvergenceFactor);
                    double yP = Math.Atan2(res.ProjPos.Y - _env.GunPosition.Y, res.ProjPos.X - _env.GunPosition.X);
                    double yT = Math.Atan2(res.TargetPos.Y - _env.GunPosition.Y, res.TargetPos.X - _env.GunPosition.X);
                    currentYaw += (float)((yT - yP) * _env.ConvergenceFactor); 
                }
                else
                {
                    aimPoint += missVec * _env.ConvergenceFactor;
                    Vector3 dir = aimPoint - _env.GunPosition;
                    currentPitch = (float)Math.Asin(Math.Clamp(dir.Z / Math.Max(dir.Length(), 0.1f), -1.0f, 1.0f));
                    currentYaw = (float)Math.Atan2(dir.Y, dir.X);
                }

                if (i == iterations - 1) // Финальный угловой промах / Final angular miss
                {
                    Vector3 vT = Vector3.Normalize(res.TargetPos - _env.GunPosition);
                    Vector3 vP = Vector3.Normalize(res.ProjPos - _env.GunPosition);
                    finalAngMiss = Math.Acos(Math.Clamp(Vector3.Dot(vT, vP), -1, 1)) * (180.0 / Math.PI);
                }

                if (float.IsNaN(finalMiss)) break;
            }

            sw.Stop();
            return (currentPitch * 180 / Math.PI, currentYaw * 180 / Math.PI, finalMiss, finalAngMiss, sw.ElapsedMilliseconds);
        }

        private (Vector3 ProjPos, Vector3 TargetPos) SimulateShot(Vector3 v0, Vector3 tp, Vector3 tv, Vector3 ta, bool isLast, float limit, bool highAngle)
        {
            var p = new Projectile(_env.GunPosition, v0, _env.BallisticCoefficient, _env.DerivationCoefficient, _env.Latitude, _env.NorthDirectionAngle, _env.PlanetRadius);
            var t = new Target(tp, tv, ta);
            float dt = 0.005f; float lastDist = float.MaxValue;
            Vector3 pCenter = new Vector3(0, 0, -(float)_env.PlanetRadius);
            StringBuilder csv = (isLast && _config.SaveTrajectoryCsv) ? new StringBuilder("Time,PX,PY,PZ,TX,TY,TZ,AltP\n") : null;

            while (true)
            {
                double altP = Vector3.Distance(p.Position, pCenter) - _env.PlanetRadius;
                double altT = Vector3.Distance(t.Position, pCenter) - _env.PlanetRadius;

                p.Update(dt, _atmo, _dragProvider, _env.DefaultDragModel, _env.Wind, altP);
                t.Update(dt);
                if (csv != null) csv.AppendLine($"{p.Time:F3},{p.Position.X:F1},{p.Position.Y:F1},{p.Position.Z:F1},{t.Position.X:F1},{t.Position.Y:F1},{t.Position.Z:F1},{altP:F1}");

                float d = Vector3.Distance(p.Position, t.Position);
                if (highAngle) { if (p.Velocity.Z < 0 && altP <= altT) break; }
                else { if (d > lastDist) break; }

                if (altP < -500 || p.Time > limit || float.IsNaN(d)) break;
                lastDist = d;
            }
            if (csv != null) {
                if (!Directory.Exists(_config.ReportsDirectory)) Directory.CreateDirectory(_config.ReportsDirectory);
                File.WriteAllText(Path.Combine(_config.ReportsDirectory, string.Format(_config.CsvFileNameTemplate, "Shot", DateTime.Now.ToString("HHmmss"))), csv.ToString());
            }
            return (p.Position, t.Position);
        }

        private Vector3 CalculateVel(float p, float y, float v0) => new Vector3((float)(v0 * Math.Cos(p) * Math.Cos(y)), (float)(v0 * Math.Cos(p) * Math.Sin(y)), (float)(v0 * Math.Sin(p)));
    }
}
