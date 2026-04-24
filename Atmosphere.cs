using System;

namespace AutoTurel
{
    /// <summary>
    /// Класс отвечает за моделирование состояния атмосферы на различных высотах.
    /// Реализует барометрическую формулу и расчет параметров воздуха для численного интегрирования.
    /// </summary>
    public class Atmosphere
    {
        // Константы для физических расчетов
        private const double Gravity = 9.80665;       // Ускорение свободного падения (м/с^2)
        private const double MolarMass = 0.0289644;   // Молярная масса сухого воздуха (кг/моль)
        private const double UniversalGasConst = 8.31447; // Универсальная газовая постоянная (Дж/(моль*К))
        private const double AdiabaticIndex = 1.4;    // Показатель адиабаты для воздуха (gamma)
        private const double SpecificGasConst = 287.058; // Удельная газовая постоянная для воздуха (Дж/(кг*К))
        private const double TemperatureLapseRate = 0.0065; // Падение температуры на 1 метр высоты (К/м)

        private readonly double _tempBase;     // Температура у земли (Кельвины)
        private readonly double _pressureBase; // Давление у земли (Паскали)

        /// <summary>
        /// Инициализирует модель атмосферы на основе начальных данных.
        /// </summary>
        /// <param name="tempCelsius">Температура на уровне моря в градусах Цельсия.</param>
        /// <param name="pressurePa">Атмосферное давление на уровне моря в Паскалях.</param>
        public Atmosphere(double tempCelsius, double pressurePa)
        {
            // Переводим Цельсии в Кельвины для термодинамических расчетов
            _tempBase = tempCelsius + 273.15;
            _pressureBase = pressurePa;
        }

        /// <summary>
        /// Рассчитывает параметры воздуха на заданной высоте.
        /// </summary>
        /// <param name="altitude">Высота над уровнем моря в метрах.</param>
        /// <returns>
        /// Кортеж, содержащий:
        /// Density — плотность воздуха (кг/м^3),
        /// SpeedOfSound — скорость звука (м/с).
        /// </returns>
        public (double Density, double SpeedOfSound) GetStateAtAltitude(double altitude)
        {
            // 1. Рассчитываем локальную температуру на высоте (линейная модель)
            double currentTemp = _tempBase - (TemperatureLapseRate * altitude);

            // Предотвращаем уход температуры в ноль или отрицательные значения (на экстремальных высотах)
            if (currentTemp < 1.0) currentTemp = 1.0;

            // 2. Рассчитываем давление на высоте по барометрической формуле
            // P = P0 * (T / T0) ^ (g*M / R*L)
            double exponent = (Gravity * MolarMass) / (UniversalGasConst * TemperatureLapseRate);
            double currentPressure = _pressureBase * Math.Pow(currentTemp / _tempBase, exponent);

            // 3. Рассчитываем плотность воздуха через закон идеального газа: rho = P / (R_spec * T)
            double density = currentPressure / (SpecificGasConst * currentTemp);

            // 4. Рассчитываем скорость звука: a = sqrt(gamma * R_spec * T)
            double speedOfSound = Math.Sqrt(AdiabaticIndex * SpecificGasConst * currentTemp);

            return (density, speedOfSound);
        }
    }
}
