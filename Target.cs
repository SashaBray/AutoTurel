using System;
using System.Numerics;

namespace AutoTurel
{
    /// <summary>
    /// Представляет маневрирующую цель, вектор ускорения которой привязан к вектору её скорости.
    /// </summary>
    public class Target
    {
        public Vector3 Position { get; private set; }
        public Vector3 Velocity { get; private set; }
        
        // Ускорение в локальных координатах цели (куда она "давит" двигателями/рулями)
        private Vector3 _acceleration; 

        /// <summary>
        /// Создает цель с заданными начальными параметрами.
        /// </summary>
        public Target(Vector3 startPos, Vector3 startVel, Vector3 startAcc)
        {
            Position = startPos;
            Velocity = startVel;
            _acceleration = startAcc;
        }

        /// <summary>
        /// Выполняет один шаг движения цели с обновлением вектора ускорения.
        /// </summary>
        /// <param name="dt">Шаг времени в секундах.</param>
        public void Update(float dt)
        {
            // ЗАЩИТА: Если скорость нулевая или очень маленькая, не пытаемся её нормализовать
            if (Velocity.Length() < 1e-6f)
            {
                // Если есть ускорение, обновляем скорость, но не поворачиваем его
                Velocity += _acceleration * dt;
                Position += Velocity * dt;
                return; 
            }

            Vector3 oldVelocityDir = Vector3.Normalize(Velocity);

            Velocity += _acceleration * dt;
            Position += Velocity * dt;

            Vector3 newVelocityDir = Vector3.Normalize(Velocity);

            // Еще одна защита: если после обновления скорость стала нулевой
            if (newVelocityDir.Length() < 1e-6f) return;

            Quaternion rotation = CreateRotationBetweenVectors(oldVelocityDir, newVelocityDir);
            _acceleration = Vector3.Transform(_acceleration, rotation);
        }

        /// <summary>
        /// Вспомогательный метод для создания кватерниона поворота между двумя направлениями.
        /// </summary>
        private Quaternion CreateRotationBetweenVectors(Vector3 start, Vector3 end)
        {
            float dot = Vector3.Dot(start, end);
            
            // Если векторы почти параллельны, поворот не требуется
            if (dot > 0.9999f) return Quaternion.Identity;

            Vector3 axis = Vector3.Cross(start, end);
            float s = (float)Math.Sqrt((1 + dot) * 2);
            float invS = 1 / s;

            return new Quaternion(axis.X * invS, axis.Y * invS, axis.Z * invS, s * 0.5f);
        }
    }
}
