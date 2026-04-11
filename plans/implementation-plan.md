# План реализации изменений

## Обзор проекта

Проект представляет собой WinForms приложение для рисования фигур с возможностью настройки параметров каждой фигуры через панель свойств (F4).

**Ключевые файлы:**
- [`ShapeBase.cs`](Shapes/ShapeBase.cs) - Базовый класс фигуры
- [`PropertiesForm.cs`](PropertiesForm.cs) - Панель свойств
- [`PropertiesForm.Designer.cs`](PropertiesForm.Designer.cs) - Дизайн панели свойств
- [`MainForm.cs`](MainForm.cs) - Главная форма
- Фигуры: [`CircleShape.cs`](Shapes/CircleShape.cs), [`RectangleShape.cs`](Shapes/RectangleShape.cs), [`TriangleShape.cs`](Shapes/TriangleShape.cs), [`HexagonShape.cs`](Shapes/HexagonShape.cs), [`TrapezoidShape.cs`](Shapes/TrapezoidShape.cs)

---

## Задача 1: Изменение размера фигур

### Проблема
В текущей реализации можно изменять только толщину линий граней, но не размер самих фигур.

### Анализ текущего состояния

Каждая фигура имеет свои параметры размера:
| Фигура | Параметры размера |
|--------|-------------------|
| `CircleShape` | `Radius` (int) |
| `RectangleShape` | `Width`, `Height` (int) |
| `TriangleShape` | `Radius` (int) - радиус описанной окружности |
| `HexagonShape` | `Radius` (int) - радиус описанной окружности |
| `TrapezoidShape` | `BottomWidth`, `TopWidth`, `Height` (int) |

### Необходимые изменения

#### 1. ShapeBase.cs
Добавить абстрактный метод для изменения размера:
```csharp
/// <summary>
/// Изменить размер фигуры
/// </summary>
public abstract void Resize(float scaleFactor);
```

#### 2. CircleShape.cs
```csharp
public override void Resize(float scaleFactor)
{
    Radius = (int)(Radius * scaleFactor);
    UpdateVirtualBounds();
}
```

#### 3. RectangleShape.cs
```csharp
public override void Resize(float scaleFactor)
{
    Width = (int)(Width * scaleFactor);
    Height = (int)(Height * scaleFactor);
    UpdateVirtualBounds();
}
```

#### 4. TriangleShape.cs
```csharp
public override void Resize(float scaleFactor)
{
    Radius = (int)(Radius * scaleFactor);
    UpdateVirtualBounds();
}
```

#### 5. HexagonShape.cs
```csharp
public override void Resize(float scaleFactor)
{
    Radius = (int)(Radius * scaleFactor);
    UpdateVirtualBounds();
}
```

#### 6. TrapezoidShape.cs
```csharp
public override void Resize(float scaleFactor)
{
    BottomWidth = (int)(BottomWidth * scaleFactor);
    TopWidth = (int)(TopWidth * scaleFactor);
    Height = (int)(Height * scaleFactor);
    UpdateVirtualBounds();
}
```

#### 7. PropertiesForm.Designer.cs
Добавить новые контролы:
- `labelSize` - заголовок секции "Размер"
- `textSize` - текстовое поле для ввода размера (или масштаба)
- `buttonSizeUp` - кнопка увеличения размера
- `buttonSizeDown` - кнопка уменьшения размера

#### 8. PropertiesForm.cs
Добавить логику:
- Отображение текущего размера фигуры
- Обработка изменения размера через текстовое поле или кнопки
- Вызов `shape.Resize(scaleFactor)` при изменении

### Возможные проблемы
- При уменьшении размера фигура может стать слишком маленькой
- Нужно добавить минимальный размер (например, 10 пикселей)
- При изменении размера нужно сохранить положение точки привязки

---

## Задача 2: Русский перевод для Anchor Position

### Проблема
ComboBox `comboAnchorPosition` содержит английские значения: "Center", "TopLeft", "TopRight" и т.д.

### Анализ текущего состояния

В [`PropertiesForm.Designer.cs`](PropertiesForm.Designer.cs:304-315):
```csharp
this.comboAnchorPosition.Items.AddRange(new object[] {
    "Center",
    "TopLeft",
    "TopRight",
    "BottomLeft",
    "BottomRight",
    "Top",
    "Bottom",
    "Left",
    "Right",
    "Custom"
});
```

### Необходимые изменения

#### 1. PropertiesForm.Designer.cs
Заменить английские значения на русские:
```csharp
this.comboAnchorPosition.Items.AddRange(new object[] {
    "Центр",
    "Верхний левый",
    "Верхний правый",
    "Нижний левый",
    "Нижний правый",
    "Верх",
    "Низ",
    "Лево",
    "Право",
    "Произвольно"
});
```

#### 2. PropertiesForm.cs
Добавить словарь для маппинга русских названий на enum:

```csharp
private static readonly Dictionary<string, AnchorPosition> AnchorPositionMap = new()
{
    { "Центр", AnchorPosition.Center },
    { "Верхний левый", AnchorPosition.TopLeft },
    { "Верхний правый", AnchorPosition.TopRight },
    { "Нижний левый", AnchorPosition.BottomLeft },
    { "Нижний правый", AnchorPosition.BottomRight },
    { "Верх", AnchorPosition.Top },
    { "Низ", AnchorPosition.Bottom },
    { "Лево", AnchorPosition.Left },
    { "Право", AnchorPosition.Right },
    { "Произвольно", AnchorPosition.Custom }
};

private static readonly Dictionary<AnchorPosition, string> AnchorPositionReverseMap = new()
{
    { AnchorPosition.Center, "Центр" },
    { AnchorPosition.TopLeft, "Верхний левый" },
    // ... и т.д.
};
```

#### 3. Обновить UpdateProperties()
```csharp
// Вместо:
comboAnchorPosition.SelectedItem = _shape.AnchorPos.ToString();

// Использовать:
comboAnchorPosition.SelectedItem = AnchorPositionReverseMap[_shape.AnchorPos];
```

#### 4. Обновить buttonApply_Click()
```csharp
// Вместо:
if (Enum.TryParse<AnchorPosition>(comboAnchorPosition.SelectedItem.ToString(), out var pos))

// Использовать:
if (comboAnchorPosition.SelectedItem != null && 
    AnchorPositionMap.TryGetValue(comboAnchorPosition.SelectedItem.ToString(), out var pos))
```

### Возможные проблемы
- Нужно убедиться, что все значения enum покрыты в словарях
- При добавлении новых значений в enum нужно обновить словари

---

## Задача 3: Реструктуризация контекстного меню F4

### Проблема
Панель свойств содержит много информации, которая отображается неструктурированно.

### Анализ текущего состояния

Текущая структура панели:
1. Абсолютная точка привязки (X, Y)
2. Локальная точка привязки (X, Y)
3. Виртуальные границы (4 поля)
4. Положение точки привязки (ComboBox)
5. Цвет заливки
6. Грани (динамически создаваемые контролы)
7. Кнопка "Применить"

### Предлагаемая реструктуризация

#### Вариант: Использование GroupBox для группировки

```
┌─── Положение ──────────────────────────┐
│ Точка привязки: X: [    ] Y: [    ]    │
│ Смещение:       X: [    ] Y: [    ]    │
│ Положение:      [Центр          ▼]     │
└─────────────────────────────────────────┘

┌─── Размер ─────────────────────────────┐
│ Размер: [    ] [+] [-]                 │
│ Границы: Лево: [  ] Верх: [  ]         │
│          Право: [  ] Низ:  [  ]        │
└─────────────────────────────────────────┘

┌─── Оформление ─────────────────────────┐
│ Заливка: [████████]                    │
│                                         │
│ Грани:                                  │
│ ┌─────────────────────────────────────┐│
│ │ Верх:  [толщина] [цвет]             ││
│ │ Право: [толщина] [цвет]             ││
│ │ ...                                 ││
│ └─────────────────────────────────────┘│
└─────────────────────────────────────────┘

[Применить изменения]
```

### Необходимые изменения

#### 1. PropertiesForm.Designer.cs
- Добавить `GroupBox` элементы для каждой секции
- Перераспределить существующие контролы по GroupBox
- Добавить отступы между секциями

#### 2. PropertiesForm.cs
- Обновить метод `UpdateLayout()` для учёта новых GroupBox
- Скорректировать позиции контролов

### Возможные проблемы
- Нужно пересчитать все позиции элементов
- Может потребоваться увеличение высоты панели

---

## Задача 4: Triangle/Hypotenuse Edge Connection

### Проблема
При соединении граней разной толщины или цвета текущая реализация просто продлевает линии, что создаёт некрасивые стыки.

### Анализ текущего состояния

В [`RectangleShape.cs`](Shapes/RectangleShape.cs:109-153) метод `DrawSidesWithDifferentWidths`:
```csharp
// Продлеваем линию на половину толщины предыдущей и следующей линии
float extendStart = BorderWidths[prev] / 2f;
float extendEnd = BorderWidths[next] / 2f;
```

Аналогичная реализация в TriangleShape, HexagonShape, TrapezoidShape.

### Предлагаемое решение

При соединении двух граней разной толщины формировать треугольный переход (гипотенузу):

```
Текущая реализация:
┌─────────────────────┐
│ ═══════════════════ │ - грань 1 (толщина 4)
│ ════════════════    │ - грань 2 (толщина 2)
└─────────────────────┘

Предлагаемая реализация:
┌─────────────────────┐
│ ═══════════════════ │ - грань 1 (толщина 4)
│ ═════════════╱      │ - переход (гипотенуза)
│ ════════════        │ - грань 2 (толщина 2)
└─────────────────────┘
```

### Необходимые изменения

#### 1. Создать вспомогательный метод в ShapeBase.cs

```csharp
/// <summary>
/// Рисует треугольный переход между двумя гранями разной толщины
/// </summary>
protected void DrawCornerTransition(Graphics g, Point corner, 
    float width1, Color color1, 
    float width2, Color color2,
    Point prevPoint, Point nextPoint)
{
    // Вычисляем направление граней
    // Рисуем треугольник перехода
}
```

#### 2. Обновить DrawSidesWithDifferentWidths во всех фигурах

Для каждой фигуры (RectangleShape, TriangleShape, HexagonShape, TrapezoidShape):

```csharp
private void DrawSidesWithDifferentWidths(Graphics g, Point[] pts)
{
    // Сначала рисуем все стороны
    for (int i = 0; i < SideCount; i++)
    {
        int next = (i + 1) % SideCount;
        using (var pen = new Pen(BorderColors[i], BorderWidths[i]))
        {
            g.DrawLine(pen, pts[i], pts[next]);
        }
    }
    
    // Затем рисуем переходы в углах
    for (int i = 0; i < SideCount; i++)
    {
        int prev = (i + SideCount - 1) % SideCount;
        int next = (i + 1) % SideCount;
        
        // Если толщины или цвета разные - рисуем переход
        if (BorderWidths[prev] != BorderWidths[i] || 
            BorderColors[prev] != BorderColors[i])
        {
            DrawCornerTransition(g, pts[i], 
                BorderWidths[prev], BorderColors[prev],
                BorderWidths[i], BorderColors[i],
                pts[prev], pts[next]);
        }
    }
}
```

### Алгоритм DrawCornerTransition

1. Вычислить направление входящей грани (от prevPoint к corner)
2. Вычислить направление исходящей грани (от corner к nextPoint)
3. Вычислить биссектрису угла
4. Построить треугольник перехода:
   - Вершина 1: угол входящей грани
   - Вершина 2: угол исходящей грани
   - Вершина 3: точка на биссектрисе (гипотенуза)

### Возможные проблемы
- Сложность вычисления правильной формы перехода
- Нужно учитывать цвет заливки фигуры
- Возможны артефакты при очень большой разнице толщин

---

## Порядок реализации

Рекомендуется реализовывать задачи в следующем порядке:

1. **Задача 2** (Русский перевод) - самая простая, не требует изменений в логике
2. **Задача 1** (Изменение размера) - добавляет новую функциональность
3. **Задача 3** (Реструктуризация меню) - требует переработки UI
4. **Задача 4** (Гипотенуза) - самая сложная, требует математических вычислений

---

## Файлы для изменения

| Файл | Задача 1 | Задача 2 | Задача 3 | Задача 4 |
|------|----------|----------|----------|----------|
| `ShapeBase.cs` | ✅ | - | - | ✅ |
| `CircleShape.cs` | ✅ | - | - | - |
| `RectangleShape.cs` | ✅ | - | - | ✅ |
| `TriangleShape.cs` | ✅ | - | - | ✅ |
| `HexagonShape.cs` | ✅ | - | - | ✅ |
| `TrapezoidShape.cs` | ✅ | - | - | ✅ |
| `PropertiesForm.cs` | ✅ | ✅ | ✅ | - |
| `PropertiesForm.Designer.cs` | ✅ | ✅ | ✅ | - |
