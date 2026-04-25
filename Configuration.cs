using System;
using System.Numerics;
using System.IO;
using System.Text.Json;

namespace AutoTurel
{
    /// <summary>
    /// Конфигурация путей и системных настроек. 
    /// Configuration of file paths and system settings.
    /// </summary>
    public class AppConfig
    {
        public string DragTablesPath { get; set; } = "drags.json";
        public string EnvironmentDefaultsPath { get; set; } = "env_defaults.json";
        public bool EnableDebugOutput { get; set; } = false;
        public bool SaveTrajectoryCsv { get; set; } = false;
        public string ReportsDirectory { get; set; } = "Reports";
        public string CsvFileNameTemplate { get; set; } = "traj_{0}_{1}.csv";
    }

    /// <summary>
    /// Физические параметры и лимиты симуляции. 
    /// Physical parameters and simulation limits.
    /// </summary>
    public class EnvironmentDefaults
    {
        public double TemperatureBase { get; set; } = 15.0;
        public double PressureBase { get; set; } = 101325.0;
        public double Latitude { get; set; } = 55.75;
        public double NorthDirectionAngle { get; set; } = 0.0;
        public Vector3 Wind { get; set; } = Vector3.Zero;
        public Vector3 GunPosition { get; set; } = Vector3.Zero;
        public float MuzzleVelocity { get; set; } = 800.0f;
        public string DefaultDragModel { get; set; } = "G1";
        public double BallisticCoefficient { get; set; } = 0.5;
        public double DerivationCoefficient { get; set; } = 0.0001;
        public int Iterations { get; set; } = 15;
        public float ConvergenceFactor { get; set; } = 0.8f;
        public double PlanetRadius { get; set; } = 6371000.0;
        public bool UseHighAngle { get; set; } = false;

        /// <summary> Лимит времени для настильной стрельбы. / Time limit for low-angle fire. </summary>
        public float MaxFlightTime { get; set; } = 25.0f;

        /// <summary> Лимит времени для навесной стрельбы. / Time limit for high-angle fire. </summary>
        public float MaxFlightTimeHighAngle { get; set; } = 150.0f;
    }

    public static class ConfigLoader
    {
        public static T Load<T>(string path) where T : new()
        {
            if (!File.Exists(path)) return new T();
            try {
                string json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<T>(json) ?? new T();
            } catch { return new T(); }
        }
    }
}
