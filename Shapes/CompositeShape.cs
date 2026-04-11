using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace OOTPiSP_LR1.Shapes
{
    /// <summary>
    /// Составная фигура - группирует несколько фигур в одну с объединением контуров.
    /// При наложении фигур внутренние линии удаляются, создается единый контур и заливка.
    /// </summary>
    public class CompositeShape : ShapeBase
    {
        /// <summary>
        /// Список дочерних фигур
        /// </summary>
        private readonly List<ShapeBase> _childShapes = new();

        /// <summary>
        /// Показывать отдельные фигуры или объединение
        /// </summary>
        private bool _isExpanded = true;

        /// <summary>
        /// Кэшированный объединённый путь
        /// </summary>
        private GraphicsPath? _cachedPath;

        /// <summary>
        /// Флаг необходимости пересчёта пути
        /// </summary>
        private bool _needsRecalculation = true;

        /// <summary>
        /// Кэшированные вершины выпуклой оболочки
        /// </summary>
        private PointF[]? _cachedHullVertices;

        /// <summary>
        /// Количество сторон (динамически вычисляется как сумма сторон всех дочерних фигур)
        /// Для объединённого контура - количество вершин выпуклой оболочки
        /// </summary>
        public override int SideCount
        {
            get
            {
                if (_isExpanded)
                {
                    int total = 0;
                    foreach (var child in _childShapes)
                    {
                        total += child.SideCount;
                    }
                    return Math.Max(1, total);
                }
                else
                {
                    var hull = GetUnionVertices();
                    return Math.Max(1, hull.Length);
                }
            }
        }

        /// <summary>
        /// Режим отображения: true - показывать отдельные фигуры, false - объединённый контур
        /// </summary>
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;
                _needsRecalculation = true;
                UpdateVirtualBounds();
            }
        }

        /// <summary>
        /// Количество дочерних фигур
        /// </summary>
        public int ChildCount => _childShapes.Count;

        /// <summary>
        /// Создать пустую составную фигуру
        /// </summary>
        public CompositeShape()
        {
            AnchorPos = AnchorPosition.Center;
            AnchorOffset = Point.Empty;
        }

        /// <summary>
        /// Создать составную фигуру с начальным списком фигур
        /// </summary>
        public CompositeShape(IEnumerable<ShapeBase> shapes) : this()
        {
            foreach (var shape in shapes)
            {
                AddChild(shape);
            }
        }

        /// <summary>
        /// Добавить дочернюю фигуру
        /// </summary>
        public void AddChild(ShapeBase shape)
        {
            if (shape == null) return;
            if (shape == this) return; // Предотвращаем рекурсию
            
            shape.IsChildOfComposite = true; // Скрываем точку привязки для дочерней фигуры
            _childShapes.Add(shape);
            _needsRecalculation = true;
            UpdateVirtualBounds();
        }

        /// <summary>
        /// Удалить дочернюю фигуру
        /// </summary>
        public bool RemoveChild(ShapeBase shape)
        {
            if (shape == null) return false;
            
            bool removed = _childShapes.Remove(shape);
            if (removed)
            {
                shape.IsChildOfComposite = false; // Восстанавливаем отображение точки привязки
                _needsRecalculation = true;
                UpdateVirtualBounds();
            }
            return removed;
        }

        /// <summary>
        /// Удалить дочернюю фигуру по индексу
        /// </summary>
        public void RemoveChildAt(int index)
        {
            if (index >= 0 && index < _childShapes.Count)
            {
                _childShapes[index].IsChildOfComposite = false; // Восстанавливаем отображение точки привязки
                _childShapes.RemoveAt(index);
                _needsRecalculation = true;
                UpdateVirtualBounds();
            }
        }

        /// <summary>
        /// Получить список дочерних фигур (только для чтения)
        /// </summary>
        public IReadOnlyList<ShapeBase> GetChildren()
        {
            return _childShapes.AsReadOnly();
        }

        /// <summary>
        /// Получить дочернюю фигуру по индексу
        /// </summary>
        public ShapeBase? GetChild(int index)
        {
            if (index >= 0 && index < _childShapes.Count)
                return _childShapes[index];
            return null;
        }

        /// <summary>
        /// Очистить список дочерних фигур
        /// </summary>
        public void ClearChildren()
        {
            // Восстанавливаем отображение точки привязки для всех дочерних фигур
            foreach (var child in _childShapes)
            {
                child.IsChildOfComposite = false;
            }
            _childShapes.Clear();
            _needsRecalculation = true;
            UpdateVirtualBounds();
        }

        /// <summary>
        /// Получить объединённый контур всех дочерних фигур
        /// </summary>
        public GraphicsPath GetCombinedPath()
        {
            if (_needsRecalculation || _cachedPath == null)
            {
                RecalculateCombinedPath();
            }
            return _cachedPath ?? new GraphicsPath();
        }

        /// <summary>
        /// Получить вершины внешнего контура (выпуклая оболочка)
        /// </summary>
        public PointF[] GetUnionVertices()
        {
            if (_needsRecalculation || _cachedHullVertices == null)
            {
                _cachedHullVertices = ComputeUnionVertices();
            }
            return _cachedHullVertices ?? Array.Empty<PointF>();
        }

        /// <summary>
        /// Пересчитать объединённый путь
        /// </summary>
        private void RecalculateCombinedPath()
        {
            _cachedPath?.Dispose();
            _cachedPath = new GraphicsPath();

            if (_childShapes.Count == 0)
            {
                _needsRecalculation = false;
                return;
            }

            // Получаем объединённый контур через Clipper2
            var unionVertices = ComputeUnionVertices();
            
            if (unionVertices.Length >= 3)
            {
                _cachedPath.AddPolygon(unionVertices);
            }

            _needsRecalculation = false;
        }

        /// <summary>
        /// Вычислить вершины объединённого контура с помощью Clipper2
        /// Поддерживает вогнутые формы, отверстия и сложные пересечения
        /// </summary>
        private PointF[] ComputeUnionVertices()
        {
            if (_childShapes.Count == 0)
                return Array.Empty<PointF>();

            // Собираем полигоны каждой дочерней фигуры
            var polygons = new List<PointF[]>();
            foreach (var child in _childShapes)
            {
                var childVertices = child.GetVertices();
                if (childVertices != null && childVertices.Length >= 3)
                {
                    polygons.Add(childVertices);
                }
            }

            if (polygons.Count == 0)
                return Array.Empty<PointF>();

            if (polygons.Count == 1)
                return polygons[0];

            // Используем Clipper2 для точного объединения полигонов
            return PolygonConverter.UnionPolygons(polygons);
        }

        #region Переопределение методов ShapeBase

        protected override Point CalculateAnchorOffset(AnchorPosition position)
        {
            if (_childShapes.Count == 0)
                return Point.Empty;

            var bounds = GetCompositeBounds();
            int centerX = bounds.X + bounds.Width / 2;
            int centerY = bounds.Y + bounds.Height / 2;

            return position switch
            {
                AnchorPosition.Center => new Point(0, 0),
                AnchorPosition.TopLeft => new Point(bounds.X - centerX, bounds.Y - centerY),
                AnchorPosition.TopRight => new Point(bounds.Right - centerX, bounds.Y - centerY),
                AnchorPosition.BottomLeft => new Point(bounds.X - centerX, bounds.Bottom - centerY),
                AnchorPosition.BottomRight => new Point(bounds.Right - centerX, bounds.Bottom - centerY),
                AnchorPosition.Top => new Point(centerX - centerX, bounds.Y - centerY),
                AnchorPosition.Bottom => new Point(centerX - centerX, bounds.Bottom - centerY),
                AnchorPosition.Left => new Point(bounds.X - centerX, centerY - centerY),
                AnchorPosition.Right => new Point(bounds.Right - centerX, centerY - centerY),
                _ => Point.Empty
            };
        }

        /// <summary>
        /// Получить границы всех дочерних фигур
        /// </summary>
        private Rectangle GetCompositeBounds()
        {
            if (_childShapes.Count == 0)
                return Rectangle.Empty;

            int minX = int.MaxValue, minY = int.MaxValue;
            int maxX = int.MinValue, maxY = int.MinValue;

            foreach (var child in _childShapes)
            {
                var bounds = child.VirtualBounds;
                if (bounds.X < minX) minX = bounds.X;
                if (bounds.Y < minY) minY = bounds.Y;
                if (bounds.Right > maxX) maxX = bounds.Right;
                if (bounds.Bottom > maxY) maxY = bounds.Bottom;
            }

            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }

        public override Point[] GetWorldPoints()
        {
            if (_isExpanded)
            {
                // В раскрытом режиме возвращаем все точки всех фигур
                var allPoints = new List<Point>();
                foreach (var child in _childShapes)
                {
                    allPoints.AddRange(child.GetWorldPoints());
                }
                return allPoints.ToArray();
            }
            else
            {
                // В объединённом режиме возвращаем вершины выпуклой оболочки
                var hullVertices = GetUnionVertices();
                return Array.ConvertAll(hullVertices, p => Point.Round(p));
            }
        }

        protected override void UpdateVirtualBounds()
        {
            if (_childShapes.Count == 0)
            {
                VirtualBounds = Rectangle.Empty;
                return;
            }

            if (_isExpanded)
            {
                // Границы охватывают все дочерние фигуры
                VirtualBounds = GetCompositeBounds();
            }
            else
            {
                // Границы по выпуклой оболочке
                var path = GetCombinedPath();
                if (path.PointCount > 0)
                {
                    VirtualBounds = Rectangle.Round(path.GetBounds());
                }
                else
                {
                    VirtualBounds = GetCompositeBounds();
                }
            }
        }

        public override void Draw(Graphics g)
        {
            if (_childShapes.Count == 0)
                return;

            if (_isExpanded)
            {
                // Режим "раскрыто" - рисуем дочерние фигуры отдельно
                foreach (var child in _childShapes)
                {
                    child.Draw(g);
                }
            }
            else
            {
                // Режим "объединено" - рисуем единый контур
                var hullVertices = GetUnionVertices();
                
                if (hullVertices.Length >= 3)
                {
                    // Заливка
                    using (var path = new GraphicsPath())
                    {
                        path.AddPolygon(hullVertices);
                        using (var brush = new SolidBrush(FillColor))
                        {
                            g.FillPath(brush, path);
                        }
                    }

                    // Обводка (используем стиль первой стороны для всего контура)
                    DrawSidesWithMiterClip(g, hullVertices);
                }
            }

            // Виртуальные границы для выбранной фигуры
            DrawVirtualBounds(g);
        }

        public override bool HitTest(Point p)
        {
            if (_childShapes.Count == 0)
                return false;

            if (_isExpanded)
            {
                // Проверяем попадание в любую дочернюю фигуру
                foreach (var child in _childShapes)
                {
                    if (child.HitTest(p))
                        return true;
                }
                return false;
            }
            else
            {
                // Проверяем попадание в объединённый контур
                var hullVertices = GetUnionVertices();
                if (hullVertices.Length < 3)
                    return false;

                return IsPointInPolygon(p, hullVertices);
            }
        }

        /// <summary>
        /// Проверка попадания точки в многоугольник (ray casting algorithm)
        /// </summary>
        private static bool IsPointInPolygon(Point p, PointF[] polygon)
        {
            bool inside = false;
            int n = polygon.Length;

            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                if (((polygon[i].Y > p.Y) != (polygon[j].Y > p.Y)) &&
                    (p.X < (polygon[j].X - polygon[i].X) * (p.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) + polygon[i].X))
                {
                    inside = !inside;
                }
            }

            return inside;
        }

        public override void Resize(float scaleFactor)
        {
            // Масштабируем все дочерние фигуры
            foreach (var child in _childShapes)
            {
                child.Resize(scaleFactor);
            }

            _needsRecalculation = true;
            UpdateVirtualBounds();
        }

        public override void ResizeSide(int sideIndex, float scaleFactor)
        {
            if (_isExpanded)
            {
                // В раскрытом режиме масштабируем соответствующую сторону соответствующей фигуры
                int currentIndex = 0;
                foreach (var child in _childShapes)
                {
                    if (sideIndex < currentIndex + child.SideCount)
                    {
                        child.ResizeSide(sideIndex - currentIndex, scaleFactor);
                        break;
                    }
                    currentIndex += child.SideCount;
                }
            }
            else
            {
                // В объединённом режиме используем алгоритм распространения смещения
                var hullVertices = GetUnionVertices();
                if (sideIndex >= 0 && sideIndex < hullVertices.Length)
                {
                    // Деформируем оболочку
                    var newVertices = PropagateDisplacementOnHull(sideIndex, scaleFactor, hullVertices);
                    _cachedHullVertices = newVertices;
                    DeformedVertices = newVertices;
                }
            }

            _needsRecalculation = true;
            UpdateVirtualBounds();
        }

        /// <summary>
        /// Алгоритм распространения смещения для выпуклой оболочки
        /// </summary>
        private PointF[] PropagateDisplacementOnHull(int sideIndex, float scaleFactor, PointF[] vertices)
        {
            int n = vertices.Length;
            if (n < 3) return vertices;

            var result = new PointF[n];
            Array.Copy(vertices, result, n);

            int edgeIndex1 = sideIndex;
            int edgeIndex2 = (sideIndex + 1) % n;

            // Центр ребра
            PointF center = new PointF(
                (result[edgeIndex1].X + result[edgeIndex2].X) / 2,
                (result[edgeIndex1].Y + result[edgeIndex2].Y) / 2
            );

            // Масштабируем вершины ребра относительно центра
            result[edgeIndex1] = new PointF(
                center.X + (result[edgeIndex1].X - center.X) * scaleFactor,
                center.Y + (result[edgeIndex1].Y - center.Y) * scaleFactor
            );
            result[edgeIndex2] = new PointF(
                center.X + (result[edgeIndex2].X - center.X) * scaleFactor,
                center.Y + (result[edgeIndex2].Y - center.Y) * scaleFactor
            );

            return result;
        }

        public override float GetSideLength(int sideIndex)
        {
            if (_isExpanded)
            {
                // В раскрытом режиме возвращаем длину стороны соответствующей фигуры
                int currentIndex = 0;
                foreach (var child in _childShapes)
                {
                    if (sideIndex < currentIndex + child.SideCount)
                    {
                        return child.GetSideLength(sideIndex - currentIndex);
                    }
                    currentIndex += child.SideCount;
                }
                return 0;
            }
            else
            {
                // В объединённом режиме возвращаем длину ребра выпуклой оболочки
                var hullVertices = GetUnionVertices();
                if (sideIndex >= 0 && sideIndex < hullVertices.Length)
                {
                    int next = (sideIndex + 1) % hullVertices.Length;
                    float dx = hullVertices[next].X - hullVertices[sideIndex].X;
                    float dy = hullVertices[next].Y - hullVertices[sideIndex].Y;
                    return (float)Math.Sqrt(dx * dx + dy * dy);
                }
                return 0;
            }
        }

        public override void SetSideLength(int sideIndex, float length)
        {
            if (length <= 0) return;

            float currentLength = GetSideLength(sideIndex);
            if (currentLength <= 0) return;

            float scaleFactor = length / currentLength;
            ResizeSide(sideIndex, scaleFactor);
        }

        public override void MoveBy(int dx, int dy)
        {
            // Перемещаем все дочерние фигуры
            foreach (var child in _childShapes)
            {
                child.MoveBy(dx, dy);
            }

            // Обновляем LocalAnchor для самой составной фигуры
            LocalAnchor = new Point(LocalAnchor.X + dx, LocalAnchor.Y + dy);

            _needsRecalculation = true;
            UpdateVirtualBounds();
        }

        public override void MoveTo(Point location)
        {
            if (_childShapes.Count == 0)
            {
                LocalAnchor = new Point(location.X - GlobalOrigin.X, location.Y - GlobalOrigin.Y);
                return;
            }

            // Вычисляем текущий центр всех фигур
            var bounds = GetCompositeBounds();
            int currentCenterX = bounds.X + bounds.Width / 2;
            int currentCenterY = bounds.Y + bounds.Height / 2;

            // Вычисляем смещение
            int dx = location.X - currentCenterX;
            int dy = location.Y - currentCenterY;

            // Перемещаем все дочерние фигуры
            foreach (var child in _childShapes)
            {
                child.MoveBy(dx, dy);
            }

            LocalAnchor = new Point(location.X - GlobalOrigin.X, location.Y - GlobalOrigin.Y);
            _needsRecalculation = true;
            UpdateVirtualBounds();
        }

        /// <summary>
        /// Создать глубокую копию составной фигуры
        /// </summary>
        public CompositeShape DeepClone()
        {
            var clone = new CompositeShape
            {
                GlobalOrigin = GlobalOrigin,
                LocalAnchor = LocalAnchor,
                AnchorOffset = AnchorOffset,
                AnchorPos = AnchorPos,
                FillColor = FillColor,
                IsSelected = false,
                _isExpanded = _isExpanded
            };

            // Копируем стили границ
            for (int i = 0; i < BorderWidths.Length; i++)
            {
                clone.BorderWidths[i] = BorderWidths[i];
                clone.BorderColors[i] = BorderColors[i];
            }

            // Клонируем дочерние фигуры
            foreach (var child in _childShapes)
            {
                var childClone = CloneShape(child);
                if (childClone != null)
                {
                    clone.AddChild(childClone);
                }
            }

            clone.UpdateVirtualBounds();
            return clone;
        }

        /// <summary>
        /// Клонировать фигуру по типу
        /// </summary>
        private static ShapeBase? CloneShape(ShapeBase shape)
        {
            return shape switch
            {
                TriangleShape t => new TriangleShape(t.GlobalOrigin, t.Radius)
                {
                    LocalAnchor = t.LocalAnchor,
                    AnchorOffset = t.AnchorOffset,
                    AnchorPos = t.AnchorPos,
                    FillColor = t.FillColor,
                    IsSelected = false
                },
                CircleShape c => new CircleShape(c.GlobalOrigin, c.Radius)
                {
                    LocalAnchor = c.LocalAnchor,
                    AnchorOffset = c.AnchorOffset,
                    AnchorPos = c.AnchorPos,
                    FillColor = c.FillColor,
                    IsSelected = false
                },
                RectangleShape r => new RectangleShape(r.GlobalOrigin, r.Width, r.Height)
                {
                    LocalAnchor = r.LocalAnchor,
                    AnchorOffset = r.AnchorOffset,
                    AnchorPos = r.AnchorPos,
                    FillColor = r.FillColor,
                    IsSelected = false
                },
                HexagonShape h => new HexagonShape(h.GlobalOrigin, h.Radius)
                {
                    LocalAnchor = h.LocalAnchor,
                    AnchorOffset = h.AnchorOffset,
                    AnchorPos = h.AnchorPos,
                    FillColor = h.FillColor,
                    IsSelected = false
                },
                TrapezoidShape tr => new TrapezoidShape(tr.GlobalOrigin, tr.TopWidth, tr.BottomWidth, tr.Height)
                {
                    LocalAnchor = tr.LocalAnchor,
                    AnchorOffset = tr.AnchorOffset,
                    AnchorPos = tr.AnchorPos,
                    FillColor = tr.FillColor,
                    IsSelected = false
                },
                PolygonShape p => p.ClonePolygon(),
                CompositeShape cs => cs.DeepClone(),
                _ => null
            };
        }

        public override void ResetDeformation()
        {
            base.ResetDeformation();
            _cachedHullVertices = null;
            _needsRecalculation = true;

            // Сбрасываем деформацию всех дочерних фигур
            foreach (var child in _childShapes)
            {
                child.ResetDeformation();
            }

            UpdateVirtualBounds();
        }

        #endregion
    }
}
