# Дизайн новых типов фигур: CompositeShape и PolygonShape

## Содержание

1. [Обзор архитектуры](#обзор-архитектуры)
2. [Диаграмма классов](#диаграмма-классов)
3. [CompositeShape - Составная фигура](#compositeshape---составная-фигура)
4. [PolygonShape - Фигура из линий](#polygonshape---фигура-из-линий)
5. [Псевдокод ключевых алгоритмов](#псевдокод-ключевых-алгоритмов)
6. [План интеграции](#план-интеграции)

---

## Обзор архитектуры

### Существующая система

Проект использует следующую архитектуру:

- **ShapeBase** - абстрактный базовый класс для всех фигур
  - Позиционирование: `GlobalOrigin`, `LocalAnchor`, `AnchorOffset`
  - Стилизация: `BorderWidths[6]`, `BorderColors[6]`, `FillColor`
  - Абстрактные методы: `Draw()`, `HitTest()`, `GetWorldPoints()`, `Resize()`, `ResizeSide()`
  - Визуализация сторон: `DrawSidesWithMiterClip()`

- **ShapeManager** - управление коллекцией фигур
  - Хранит `List<ShapeBase> Shapes`
  - Методы: `AddShape()`, `RemoveShape()`, `DrawAll()`, `HitTest()`

- **PropertiesPanel** - UI панель свойств
  - Привязывается к `ShapeBase` через `SetShape()`
  - Динамически создаёт контролы на основе `SideCount`

---

## Диаграмма классов

```mermaid
classDiagram
    ShapeBase <|-- CircleShape
    ShapeBase <|-- RectangleShape
    ShapeBase <|-- TriangleShape
    ShapeBase <|-- HexagonShape
    ShapeBase <|-- TrapezoidShape
    ShapeBase <|-- CompositeShape
    ShapeBase <|-- PolygonShape
    
    class ShapeBase {
        +Point GlobalOrigin
        +Point LocalAnchor
        +Point AnchorOffset
        +AnchorPosition AnchorPos
        +Rectangle VirtualBounds
        +float[] BorderWidths
        +Color[] BorderColors
        +Color FillColor
        +bool IsSelected
        +int SideCount {abstract}
        +PointF[] DeformedVertices
        ---
        +GetWorldPoints() Point[] {abstract}
        +GetVertices() PointF[]
        +Draw(Graphics) {abstract}
        +HitTest(Point) bool {abstract}
        +Resize(float) {abstract}
        +ResizeSide(int, float) {abstract}
        +GetSideLength(int) float {abstract}
        +SetSideLength(int, float) {abstract}
        +DrawSidesWithMiterClip(Graphics, PointF[])
        +ApplyDeformedVertices(PointF[])
    }
    
    class CompositeShape {
        +List~ShapeBase~ ChildShapes
        +bool IsExpanded
        ---
        +AddChild(ShapeBase)
        +RemoveChild(ShapeBase)
        +GetCombinedPath() GraphicsPath
        +GetUnionPoints() PointF[]
        +RecalculateBounds()
    }
    
    class PolygonShape {
        +List~LineSegment~ Segments
        +PointF[] CachedVertices
        ---
        +AddSegment(LineSegment)
        +RemoveSegment(int index)
        +CalculateVertices() PointF[]
        +AddSegmentByLengthAngle(float length, float angleDegrees)
        +ClosePolygon()
        +bool IsClosed
    }
    
    class LineSegment {
        +float Length
        +float AngleDegrees
        +PointF StartPoint
        +PointF EndPoint
    }
```

---

## CompositeShape - Составная фигура

### Назначение

`CompositeShape` позволяет группировать несколько фигур в одну с объединением контуров:
- При наложении фигур внутренние линии удаляются
- Создаётся единый внешний контур
- Единая заливка для всей группы

### Поля

```csharp
// Список дочерних фигур
public List<ShapeBase> ChildShapes { get; }

// Флаг режима отображения (раскрыто/свёрнуто)
public bool IsExpanded { get; set; }

// Кэшированный объединённый путь (для оптимизации)
private GraphicsPath? _cachedPath;

// Флаг необходимости пересчёта
private bool _needsRecalculation = true;
```

### Ключевые методы

```csharp
// Добавление дочерней фигуры
public void AddChild(ShapeBase shape)
{
    ChildShapes.Add(shape);
    _needsRecalculation = true;
}

// Удаление дочерней фигуры
public void RemoveChild(ShapeBase shape)
{
    ChildShapes.Remove(shape);
    _needsRecalculation = true;
}

// Получение объединённого контура (ключевой алгоритм)
public GraphicsPath GetCombinedPath()
{
    if (_needsRecalculation)
    {
        RecalculateCombinedPath();
    }
    return _cachedPath;
}

// Обновление виртуальных границ
protected override void UpdateVirtualBounds()
{
    var path = GetCombinedPath();
    VirtualBounds = Rectangle.Round(path.GetBounds());
}
```

### Особенности реализации

1. **SideCount** - динамически вычисляется как сумма сторон всех дочерних фигур
2. **BorderWidths/BorderColors** - используются только от первой дочерней фигуры
3. **MoveBy/MoveTo** - перемещает все дочерние фигуры относительно их текущих позиций
4. **IsExpanded** - если true, рисуются все дочерние фигуры отдельно (как сейчас)

---

## PolygonShape - Фигура из линий

### Назначение

`PolygonShape` представляет произвольный многоугольник, задаваемый отрезками:
- Каждый отрезок характеризуется длиной и углом
- Углы задаются относительно предыдущего отрезка
- Может быть как замкнутым, так и разомкнутым (ломаная линия)

### Вспомогательный класс LineSegment

```csharp
public struct LineSegment
{
    public float Length;           // Длина отрезка в пикселях
    public float AngleDegrees;     // Угол в градусах (относительно предыдущего)
    public PointF StartPoint;     // Начальная точка (вычисляется)
    public PointF EndPoint;       // Конечная точка (вычисляется)
    
    // Конструктор
    public LineSegment(float length, float angleDegrees)
    {
        Length = length;
        AngleDegrees = angleDegrees;
        StartPoint = PointF.Empty;
        EndPoint = PointF.Empty;
    }
}
```

### Поля

```csharp
// Список отрезков
public List<LineSegment> Segments { get; }

// Кэшированные вершины
private PointF[]? _cachedVertices;

// Признак замкнутости многоугольника
public bool IsClosed { get; set; }

// Точка начала (относительно GlobalOrigin + LocalAnchor)
public PointF OriginPoint { get; set; }
```

### Ключевые методы

```csharp
// Вычисление вершин на основе отрезков
public PointF[] CalculateVertices()
{
    if (Segments.Count == 0)
        return Array.Empty<PointF>();
    
    var vertices = new List<PointF> { OriginPoint };
    float currentAngle = 0; // Начальный угол - горизонтально вправо
    
    foreach (var segment in Segments)
    {
        currentAngle += segment.AngleDegrees * (float)Math.PI / 180;
        
        var lastPoint = vertices[vertices.Count - 1];
        var newPoint = new PointF(
            lastPoint.X + segment.Length * (float)Math.Cos(currentAngle),
            lastPoint.Y + segment.Length * (float)Math.Sin(currentAngle)
        );
        vertices.Add(newPoint);
    }
    
    // Если замкнутый - добавляем первую точку в конец
    if (IsClosed && vertices.Count > 2)
    {
        vertices.Add(vertices[0]);
    }
    
    _cachedVertices = vertices.ToArray();
    return _cachedVertices;
}

// Добавить отрезок по длине и углу
public void AddSegmentByLengthAngle(float length, float angleDegrees)
{
    Segments.Add(new LineSegment(length, angleDegrees));
    _cachedVertices = null;
    UpdateVirtualBounds();
}
```

### Конструктор

```csharp
public PolygonShape(Point anchor, PointF originPoint)
{
    GlobalOrigin = anchor;
    LocalAnchor = Point.Empty;
    OriginPoint = originPoint;
    Segments = new List<LineSegment>();
    IsClosed = false;
    AnchorPos = AnchorPosition.Center;
    AnchorOffset = Point.Empty;
    UpdateVirtualBounds();
}
```

---

## Псевдокод ключевых алгоритмов

### Алгоритм объединения контуров (CompositeShape)

```csharp
// Псевдокод объединения GraphicsPath нескольких фигур
private GraphicsPath CreateUnionPath()
{
    // 1. Создаём пустую область
    Region resultRegion = new Region();
    
    // 2. Для каждой дочерней фигуры
    foreach (var child in ChildShapes)
    {
        // Получаем GraphicsPath дочерней фигуры
        var childPath = CreateChildPath(child);
        
        // Создаём регион и объединяем с результатом
        Region childRegion = new Region(childPath);
        resultRegion.Union(childRegion);
    }
    
    // 3. Преобразуем регион обратно в GraphicsPath
    // GDI+ не имеет прямого метода, поэтому используем обход границ
    GraphicsPath unionPath = new GraphicsPath();
    RectangleF bounds = resultRegion.GetBounds();
    
    // Создаём путь по границе региона
    using (var pen = new Pen(Color.Black, 1))
    {
        // Используем GetRegionScans для получения границ
        // Или применяем альтернативный алгоритм
    }
    
    return unionPath;
}

// Альтернативный алгоритм: построение объединённого контура через вершины
private PointF[] GetUnionVertices()
{
    // Собираем все вершины дочерних фигур
    var allVertices = new List<PointF>();
    
    foreach (var child in ChildShapes)
    {
        var childVertices = child.GetVertices();
        // Трансформируем в мировые координаты
        var center = child.GetCenter();
        for (int i = 0; i < childVertices.Length; i++)
        {
            allVertices.Add(new PointF(
                center.X + childVertices[i].X,
                center.Y + childVertices[i].Y
            ));
        }
    }
    
    // Применяем алгоритм выпуклой оболочки (Convex Hull)
    // Или алгоритм Грэхема для построения внешнего контура
    return ComputeConvexHull(allVertices);
}

// Алгоритм Грэхема для выпуклой оболочки
private PointF[] ComputeConvexHull(List<PointF> points)
{
    if (points.Count < 3) return points.ToArray();
    
    // Находим точку с минимальным Y (и X при равных Y)
    var startPoint = points.OrderBy(p => p.Y).ThenBy(p => p.X).First();
    
    // Сортируем остальные точки по полярному углу
    var sorted = points
        .Where(p => p != startPoint)
        .OrderBy(p => Math.Atan2(p.Y - startPoint.Y, p.X - startPoint.X))
        .ToList();
    
    // Строим выпуклую оболочку
    var hull = new Stack<PointF>();
    hull.Push(startPoint);
    
    foreach (var p in sorted)
    {
        while (hull.Count > 1 && CrossProduct(hull.ElementAt(1), hull.Peek(), p) <= 0)
        {
            hull.Pop();
        }
        hull.Push(p);
    }
    
    return hull.ToArray();
}

// Вычисление векторного произведения
private float CrossProduct(PointF O, PointF A, PointF B)
{
    return (A.X - O.X) * (B.Y - O.Y) - (A.Y - O.Y) * (B.X - O.X);
}
```

### Алгоритм отрисовки CompositeShape

```csharp
public override void Draw(Graphics g)
{
    if (IsExpanded)
    {
        // Режим "раскрыто" - рисуем дочерние фигуры отдельно
        foreach (var child in ChildShapes)
        {
            child.Draw(g);
        }
    }
    else
    {
        // Режим "объединено" - рисуем единый контур
        var path = GetCombinedPath();
        
        // Заливка
        using (var brush = new SolidBrush(FillColor))
        {
            g.FillPath(brush, path);
        }
        
        // Обводка (единая для всех сторон)
        DrawSidesWithMiterClip(g, GetUnionVertices());
    }
}
```

---

## План интеграции

### Этап 1: Создание базовой структуры

1. **Создать класс `LineSegment`** в файле `Shapes/LineSegment.cs`
   - Структура с Length, AngleDegrees, StartPoint, EndPoint

2. **Создать класс `PolygonShape`** в файле `Shapes/PolygonShape.cs`
   - Наследовать от `ShapeBase`
   - Реализовать все абстрактные методы
   - Реализовать `CalculateVertices()`

3. **Создать класс `CompositeShape`** в файле `Shapes/CompositeShape.cs`
   - Наследовать от `ShapeBase`
   - Реализовать управление дочерними фигурами
   - Реализовать `GetCombinedPath()`

### Этап 2: Интеграция с ShapeManager

1. **Добавить методы создания** в `ShapeManager`:
   ```csharp
   public CompositeShape CreateCompositeShape(Point anchor, List<ShapeBase> children)
   public PolygonShape CreatePolygonShape(Point anchor, PointF origin)
   ```

2. **Зарегистрировать новые типы** в `MainForm` (кнопки создания)

### Этап 3: Интеграция с PropertiesPanel

1. **Добавить обработку PolygonShape**:
   - Динамические контролы для редактирования сегментов
   - Добавление/удаление сегментов
   - Редактирование длины и угла

2. **Добавить обработку CompositeShape**:
   - Список дочерних фигур
   - Кнопки добавления/удаления
   - Переключатель Expanded/Collapsed

### Этап 4: Дополнительные функции

1. **Для CompositeShape**:
   - Поддержка вложенных CompositeShape
   - Сохранение относительных позиций при перемещении
   - Анимация раскрытия/свёртывания

2. **Для PolygonShape**:
   - Инструмент "лассо" для выделения нескольких фигур
   - Конвертация выделенных фигур в PolygonShape
   - Шаблоны (звезда, стрелка и т.д.)

### Файлы для создания/изменения

| Файл | Действие |
|------|----------|
| `Shapes/LineSegment.cs` | Создать |
| `Shapes/PolygonShape.cs` | Создать |
| `Shapes/CompositeShape.cs` | Создать |
| `Core/ShapeManager.cs` | Добавить методы создания |
| `MainForm.cs` | Добавить кнопки создания |
| `PropertiesForm.cs` | Добавить панели редактирования |
| `PropertiesForm.Designer.cs` | Добавить контролы UI |

---

## Рекомендации по реализации

1. **Оптимизация**: Кэшировать результаты вычислений, пересчитывать только при изменении
2. **UI**: Использовать существующие паттерны из PropertiesPanel для новых контролов
3. **Тестирование**: Начинать с простых случаев (2-3 фигуры в CompositeShape)
4. **Совместимость**: Сохранять обратную совместимость с существующим кодом
