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
            _atmo = atmo;
            _dragProvider = drag;
            _env = env;
            _config = config;
        }

        public (double Pitch, double Yaw, double Miss, double AngMiss, long TotalMs, double AvgIterMs) Solve(
            Vector3 tPos, Vector3 tVel, Vector3 tAcc, float v0, int iterations)
        {
            Stopwatch totalSw = Stopwatch.StartNew();
            Vector3 aimPoint = tPos;
            float finalPitch = 0, finalYaw = 0, finalMiss = 0;
            double finalAngMiss = 0;

            for (int i = 0; i < iterations; i++)
            {
                Vector3 dir = aimPoint - _env.GunPosition;
                float dist = Math.Max(dir.Length(), 0.1f);
                float ratio = Math.Clamp(dir.Y / dist, -1.0f, 1.0f);
                float pitch = (float)Math.Asin(ratio);
                float yaw = (float)Math.Atan2(dir.X, dir.Z);

                bool isLast = (i == iterations - 1);
                var res = SimulateShot(CalculateVel(pitch, yaw, v0), tPos, tVel, tAcc, isLast);

                Vector3 missVec = res.TargetPos - res.ProjPos;
                finalMiss = missVec.Length();
                finalPitch = pitch;
                finalYaw = yaw;

                Vector3 vT = Vector3.Normalize(res.TargetPos - _env.GunPosition);
                Vector3 vP = Vector3.Normalize(res.ProjPos - _env.GunPosition);
                finalAngMiss = Math.Acos(Math.Clamp(Vector3.Dot(vT, vP), -1, 1)) * (180.0 / Math.PI);

                aimPoint += missVec * _env.ConvergenceFactor;
            }

            totalSw.Stop();
            double avgIter = (double)totalSw.ElapsedMilliseconds / iterations;
            return (finalPitch * 180 / Math.PI, finalYaw * 180 / Math.PI, finalMiss, finalAngMiss, totalSw.ElapsedMilliseconds, avgIter);
        }

        private (Vector3 ProjPos, Vector3 TargetPos) SimulateShot(Vector3 v0, Vector3 tp, Vector3 tv, Vector3 ta, bool isLast)
        {
            var p = new Projectile(_env.GunPosition, v0, _env.BallisticCoefficient, _env.DerivationCoefficient, _env.Latitude, _env.NorthDirectionAngle);
            var t = new Target(tp, tv, ta);
            float dt = 0.005f;
            float lastDist = float.MaxValue;
            StringBuilder csv = (isLast && _config.SaveTrajectoryCsv) ? new StringBuilder("Time,PX,PY,PZ,TX,TY,TZ\n") : null;

            while (true)
            {
                p.Update(dt, _atmo, _dragProvider, _env.DefaultDragModel, _env.Wind);
                t.Update(dt);

                if (csv != null) 
                    csv.AppendLine($"{p.Time:F3},{p.Position.X:F2},{p.Position.Y:F2},{p.Position.Z:F2},{t.Position.X:F2},{t.Position.Y:F2},{t.Position.Z:F2}");

                float d = Vector3.Distance(p.Position, t.Position);
                if (d > lastDist || p.Position.Y < -500 || p.Time > 20) break;
                lastDist = d;
            }

            if (csv != null) SaveCsv(csv.ToString());
            return (p.Position, t.Position);
        }

        private void SaveCsv(string data)
        {
            if (!Directory.Exists(_config.ReportsDirectory)) Directory.CreateDirectory(_config.ReportsDirectory);
            string name = Path.Combine(_config.ReportsDirectory, string.Format(_config.CsvFileNameTemplate, "Shot", DateTime.Now.ToString("HHmmss")));
            File.WriteAllText(name, data);
            Console.WriteLine($"[LOG] Траектория сохранена: {name}");
        }

        private Vector3 CalculateVel(float p, float y, float v0) => 
            new Vector3((float)(v0 * Math.Cos(p) * Math.Sin(y)), (float)(v0 * Math.Sin(p)), (float)(v0 * Math.Cos(p) * Math.Cos(y)));
    }
}
