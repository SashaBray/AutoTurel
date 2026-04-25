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

---

## 🚀 Новые возможности / New Features
- **Dual Mode**: 
  - *Low Angle* (Настильная): Классическая траектория, поиск минимума дистанции.
  - *High Angle* (Навесная): Минометный режим, использование эмпирических формул для коррекции Pitch и Yaw.
- **Spherical Earth**: Учет кривизны планеты и динамическое изменение вектора гравитации.
- **Smart Time Limits**: Раздельные лимиты `MaxFlightTime` для обычного режима и `MaxFlightTimeHighAngle` для навесного.
- **Unified Output**: Вывод всегда возвращает массив данных для легкой интеграции.

---

## 💻 Примеры вызова / Usage Examples

### 1. Настильный огонь (Default)
```bash
dotnet run -- --target-pos 1000,0,0
```

### 2. Навесной огонь (High Angle)
```bash
dotnet run -- --target-pos 1000,0,0 --high-angle
```

### 3. Быстрый режим (Fast Mode)
[RU] Игнорирует JSON, использует встроенные таблицы G1/G7:
[EN] Ignores JSON, uses embedded G1/G7 tables:
```bash
dotnet run -- --target-pos 1000,0,0 --fast
```

---

## 📊 Формат вывода / Output Format
[RU] Программа всегда выводит результат последней строкой в формате:
[EN] The program always outputs the result in the following format:

`Pitch;Yaw;Miss;AngMiss;TimeMs`

- **Pitch**: [RU] Тангаж (°) / [EN] Elevation angle.
- **Yaw**: [RU] Азимут (°) / [EN] Azimuth angle.
- **Miss**: [RU] Линейный промах (м) / [EN] Linear miss distance.
- **AngMiss**: [RU] Угловой промах (°) / [EN] Angular miss.
- **TimeMs**: [RU] Общее время расчета (мс) / [EN] Total computation time.

---

## ⚙️ Настройки (env_defaults.json)

| Параметр / Parameter | Описание / Description |
| :--- | :--- |
| `UseHighAngle` | [RU] Включить навесной режим / [EN] Enable high-angle mode |
| `MaxFlightTimeHighAngle` | [RU] Лимит времени для навеса (с) / [EN] High-angle time limit (s) |
| `PlanetRadius` | [RU] Радиус планеты (м) / [EN] Planet radius (m) |
| `ConvergenceFactor` | [RU] Фактор сходимости / [EN] Algorithm convergence factor |

---

## 🛠 Технологии / Tech Stack
- **Language**: C# 12 (.NET 8 SDK)
- **Physics**: Dynamic Drag (G1/G7), Coriolis, Derivation, Gravity, Spherical Geometry.
