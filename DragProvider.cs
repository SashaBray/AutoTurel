using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace AutoTurel
{
    /// <summary>
    /// Класс отвечает за загрузку, хранение и интерполяцию данных 
    /// аэродинамического сопротивления (Drag functions).
    /// </summary>
    public class DragProvider
    {
        /// <summary>
        /// Словарь, где ключ — название функции (напр. "G1"), 
        /// а значение — массив пар [Mach, Cd].
        /// </summary>
        private Dictionary<string, double[][]> _dragTables;

        /// <summary>
        /// Загружает библиотеку драг-функций из JSON файла.
        /// </summary>
        /// <param name="filePath">Путь к файлу в формате { "Name": [[M, Cd], ...] }</param>
        /// <exception cref="FileNotFoundException">Если файл не найден.</exception>
        public void LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Файл драг-функций не найден: {filePath}");
            }

            string jsonContent = File.ReadAllText(filePath);
            
            // Десериализуем JSON напрямую в наш словарь.
            // C# автоматически поймет структуру [[double, double]].
            _dragTables = JsonSerializer.Deserialize<Dictionary<string, double[][]>>(jsonContent);
        }

        /// <summary>
        /// Рассчитывает коэффициент сопротивления (Cd) для заданного числа Маха 
        /// методом линейной интерполяции.
        /// </summary>
        /// <param name="modelName">Название модели (напр. "G1").</param>
        /// <param name="mach">Текущее число Маха снаряда.</param>
        /// <returns>Интерполированный коэффициент Cd.</returns>
        public double GetDragCoefficient(string modelName, double mach)
        {
            // Если таблицы нет или в ней меньше 2 точек — возвращаем среднее Cd
            if (_dragTables == null || !_dragTables.ContainsKey(modelName) || _dragTables[modelName].Length < 2)
                return 0.3; 

            double[][] table = _dragTables[modelName];

            // Крайне важно: если Mach выходит за пределы таблицы, возвращаем крайние значения
            if (mach <= table[0][0]) return table[0][1];
            if (mach >= table[table.Length - 1][0]) return table[table.Length - 1][1];

            for (int i = 0; i < table.Length - 1; i++)
            {
                double m1 = table[i][0];
                double m2 = table[i + 1][0];
                
                if (mach >= m1 && mach <= m2)
                {
                    double cd1 = table[i][1];
                    double cd2 = table[i + 1][1];
                    
                    // Защита от деления на ноль, если в таблице две одинаковые точки по Маху
                    if (Math.Abs(m2 - m1) < 1e-6) return cd1;

                    return cd1 + (mach - m1) * (cd2 - cd1) / (m2 - m1);
                }
            }
            return 0.3;
        }
    }
}
