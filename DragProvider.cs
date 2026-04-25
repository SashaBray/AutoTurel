using System;
using System.Collections.Generic;
using System.Text.Json;

namespace AutoTurel
{
    public class DragProvider
    {
        private Dictionary<string, double[][]> _dragTables;

        /// <summary>
        /// Встроенные таблицы G1 и G7 (Standard Drag Functions).
        /// </summary>
        private static readonly Dictionary<string, double[][]> EmbeddedTables = new()
        {
            ["G1"] = new double[][] {
                new[] { 0.00, 0.262 }, new[] { 0.50, 0.256 }, new[] { 1.00, 0.450 },
                new[] { 1.50, 0.420 }, new[] { 2.00, 0.360 }, new[] { 4.00, 0.270 }
            },
            ["G7"] = new double[][] {
                new[] { 0.00, 0.155 }, new[] { 1.00, 0.380 }, new[] { 2.00, 0.320 }, new[] { 4.00, 0.250 }
            }
        };

        public void LoadFromFile(string filePath)
        {
            // Метод остается прежним для работы с файлами
            if (System.IO.File.Exists(filePath))
            {
                string json = System.IO.File.ReadAllText(filePath);
                _dragTables = JsonSerializer.Deserialize<Dictionary<string, double[][]>>(json);
            }
        }

        /// <summary>
        /// Переключает провайдер на использование встроенных данных.
        /// </summary>
        public void UseEmbedded() => _dragTables = EmbeddedTables;

        public double GetDragCoefficient(string model, double mach)
        {
            if (_dragTables == null || !_dragTables.ContainsKey(model)) return 0.2;
            var table = _dragTables[model];
            
            if (mach <= table[0][0]) return table[0][1];
            if (mach >= table[^1][0]) return table[^1][1];

            for (int i = 0; i < table.Length - 1; i++)
            {
                if (mach >= table[i][0] && mach <= table[i + 1][0])
                {
                    double m1 = table[i][0], m2 = table[i + 1][0];
                    double cd1 = table[i][1], cd2 = table[i + 1][1];
                    return cd1 + (mach - m1) * (cd2 - cd1) / (m2 - m1);
                }
            }
            return 0.2;
        }
    }
}
