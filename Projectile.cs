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

        private readonly double _bc;   
        private readonly double _derK; 
        private readonly Vector3 _earthRotationVector;

        public Projectile(Vector3 startPos, Vector3 startVel, double bc, double derK, double latitude, double northAngle)
        {
            Position = startPos;
            Velocity = startVel;
            Time = 0;
            _bc = bc > 0 ? bc : 0.5;
            _derK = derK;

            double omega = 7.292115e-5;
            double latRad = latitude * Math.PI / 180.0;
            double northRad = northAngle * Math.PI / 180.0;

            Vector3 geoOmega = new Vector3(0, (float)(omega * Math.Sin(latRad)), (float)(omega * Math.Cos(latRad)));
            _earthRotationVector = Vector3.Transform(geoOmega, Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)-northRad));
        }

        public void Update(float dt, Atmosphere atmo, DragProvider drag, string modelName, Vector3 wind)
        {
            var (density, soundSpeed) = atmo.GetStateAtAltitude(Position.Y);
            Vector3 totalAcc = CalculateTotalAcceleration(density, soundSpeed, drag, modelName, wind);

            Velocity += totalAcc * dt;
            Position += Velocity * dt;
            Time += dt;
        }

        private Vector3 CalculateTotalAcceleration(double dens, double sound, DragProvider drag, string model, Vector3 wind)
        {
            const double S_std = 0.0005064; 
            const double M_std = 0.4536;

            Vector3 accGravity = new Vector3(0, -9.81f, 0);
            Vector3 relVel = Velocity - wind;
            double v = relVel.Length();

            if (v < 0.1) return accGravity;

            double cd = drag.GetDragCoefficient(model, v / sound);
            double dragMag = (0.5 * dens * v * v * cd * S_std) / (M_std * _bc);
            Vector3 accDrag = -(float)dragMag * Vector3.Normalize(relVel);

            Vector3 accCor = -2.0f * Vector3.Cross(_earthRotationVector, Velocity);
            
            Vector3 driftDir = Vector3.Cross(Velocity, accGravity);
            Vector3 accDer = Vector3.Zero;
            if (driftDir.Length() > 1e-6f)
                accDer = Vector3.Normalize(driftDir) * (float)(v * _derK);

            return accGravity + accDrag + accCor + accDer;
        }
    }
}
