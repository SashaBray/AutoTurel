# AutoTurel Professional Edition 🎯

[RU] Высокопроизводительное баллистическое ядро на C# для систем автоматического наведения.
[EN] High-performance C# ballistic engine for automated guidance systems.

---

## 📐 Система координат / Coordinate System
[RU] В данной версии используется инженерный стандарт:
[EN] This version uses the engineering standard:

*   **X+** : Вперед (Основная дистанция) / Forward (Main distance)
*   **Y+** : Влево (Поперечное смещение) / Left (Lateral offset)
*   **Z+** : Вверх (Высота) / Up (Altitude)

> [RU] При Yaw = 0° ствол смотрит строго вдоль оси X.
> [EN] When Yaw = 0°, the gun points strictly along the X axis.

---

## 🚀 Возможности / Features
- **Complex Physics**: Drag (G1/G7), Coriolis, Derivation, Gravity.
- **Dynamic Atmosphere**: Real-time recalculation of air density and speed of sound based on altitude (Z).
- **Iterative Solver**: Shooting method to minimize miss distance with adjustable convergence.
- **Performance Profiling**: High-precision measurement of IO and Compute time.
- **Fast Mode**: Non-disk execution for integration.

---

## 💻 Примеры вызова / Usage Examples

### 1. Полный расчет / Full Simulation
[RU] С записью траектории в CSV и отладкой физики:
[EN] With CSV trajectory logging and physics debugging:
```bash
dotnet run -- --target-pos 1500,0,50 --target-vel 0,25,0 --v0 820 --csv --debug
```

### 2. Быстрый режим / Fast Mode
[RU] Мгновенный ответ без чтения JSON (используются встроенные таблицы G1/G7):
[EN] Instant response without JSON IO (uses embedded G1/G7 tables):
```bash
dotnet run -- --target-pos 1000,0,0 --fast --silent
```
**Output:** `Pitch;Yaw;Miss;AngMiss;TimeMs`

---

## ⚙️ Настройки по умолчанию / Default Settings
[RU] Если параметры не указаны, используются значения из `env_defaults.json` или жестко заданные константы:
[EN] If parameters are not specified, values from `env_defaults.json` or hardcoded constants are used:


| Параметр / Parameter | Дефолт / Default | Описание / Description |
| :--- | :--- | :--- |
| `MuzzleVelocity` | 800.0 | [RU] Нач. скорость (м/с) / [EN] Muzzle velocity (m/s) |
| `BallisticCoefficient` | 0.5 | [RU] Баллистический коэф. / [EN] Ballistic coefficient |
| `Iterations` | 15 | [RU] Циклы пристрелки / [EN] Correction iterations |
| `MaxFlightTime` | 15.0 | [RU] Лимит полета (с) / [EN] Max flight time (s) |
| `ConvergenceFactor` | 0.8 | [RU] Коэф. сходимости / [EN] Convergence factor |
| `ReportsDirectory` | "Reports" | [RU] Папка для CSV / [EN] Folder for CSV reports |

---

## 🛠 Технологии / Tech Stack
- **Language**: C# 12 (.NET 8 SDK)
- **Math**: System.Numerics (SIMD accelerated)
- **Serialization**: System.Text.Json
- **Logging**: CSV, High-precision Stopwatch
