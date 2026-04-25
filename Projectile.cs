using System;
using System.Numerics;

namespace AutoTurel
{
    /// <summary>
    /// Математическая модель снаряда. / Mathematical model of a projectile.
    /// </summary>
    public class Projectile
    {
        public Vector3 Position { get; private set; }
        public Vector3 Velocity { get; private set; }
        public double Time { get; private set; }

        private readonly double _bc, _derK, _pRadius;
        private readonly Vector3 _earthRotationVector;

        public Projectile(Vector3 startPos, Vector3 startVel, double bc, double derK, double lat, double northAngle, double planetRadius)
        {
            Position = startPos; Velocity = startVel; Time = 0;
            _bc = bc > 0 ? bc : 0.5; _derK = derK; _pRadius = planetRadius;

            double omega = 7.292115e-5;
            double latRad = lat * Math.PI / 180.0;
            double nRad = northAngle * Math.PI / 180.0;

            Vector3 geoOmega = new Vector3((float)(omega * Math.Cos(latRad)), 0, (float)(omega * Math.Sin(latRad)));
            _earthRotationVector = Vector3.Transform(geoOmega, Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)-nRad));
        }

        public void Update(float dt, Atmosphere atmo, DragProvider drag, string model, Vector3 wind, double alt)
        {
            var (dens, sound) = atmo.GetStateAtAltitude(alt);
            const double S_std = 0.0005064; const double M_std = 0.4536;

            Vector3 pCenter = new Vector3(0, 0, -(float)_pRadius);
            Vector3 accGravity = Vector3.Normalize(pCenter - Position) * 9.81f;

            Vector3 relV = Velocity - wind;
            double v = relV.Length();
            Vector3 accDrag = Vector3.Zero;

            if (v > 0.1) {
                double cd = drag.GetDragCoefficient(model, v / sound);
                double dMag = (0.5 * dens * v * v * cd * S_std) / (M_std * _bc);
                accDrag = -(float)dMag * Vector3.Normalize(relV);
            }

            Vector3 accCor = -2.0f * Vector3.Cross(_earthRotationVector, Velocity);
            Vector3 driftDir = Vector3.Cross(Velocity, accGravity);
            Vector3 accDer = (driftDir.Length() > 1e-6f) ? Vector3.Normalize(driftDir) * (float)(v * _derK) : Vector3.Zero;

            Velocity += (accGravity + accDrag + accCor + accDer) * dt;
            Position += Velocity * dt;
            Time += dt;
        }
    }
}
