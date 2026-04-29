using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text.Json.Nodes;

namespace OOTPiSP_LR1.Shapes
{
    /// <summary>
    /// Вспомогательный класс для представления отрезка линии
    /// </summary>
    public class LineSegment
    {
        /// <summary>
        /// Длина отрезка в пикселях
        /// </summary>
        public float Length { get; set; }

        /// <summary>
        /// Угол в градусах относительно предыдущего отрезка
        /// Для первого отрезка - угол относительно горизонтали вправо
        /// </summary>
        public float AngleDegrees { get; set; }

        /// <summary>
        /// Создаёт новый отрезок с указанной длиной и углом
        /// </summary>
        public LineSegment(float length, float angleDegrees)
        {
            Length = length;
            AngleDegrees = angleDegrees;
        }

        /// <summary>
        /// Создаёт копию отрезка
        /// </summary>
        public LineSegment Clone()
        {
            return new LineSegment(Length, AngleDegrees);
        }

        public override string ToString()
        {
            return $"L={Length:F1}, A={AngleDegrees:F1}°";
        }
    }

    /// <summary>
    /// Произвольный многоугольник, задаваемый отрезками с длиной и углом
    /// </summary>
    public class PolygonShape : ShapeBase
    {
        /// <summary>
        /// Список отрезков, образующих многоугольник
        /// </summary>
        public List<LineSegment> Segments { get; private set; }

        /// <summary>
        /// Признак замкнутости многоугольника
        /// </summary>
        public bool IsClosed { get; set; }

        /// <summary>
        /// Начальная точка относительно WorldPosition
        /// </summary>
        public PointF OriginPoint { get; set; }

        /// <summary>
        /// Кэшированные вершины многоугольника
        /// </summary>
        private PointF[]? _cachedVertices;

        /// <summary>
        /// Количество сторон равно количеству отрезков
        /// </summary>
        public override int SideCount => Segments.Count;
        public override string DefaultTypeName => "Многоугольник";

        /// <summary>
        /// Создаёт пустой многоугольник в указанной точке
        /// </summary>
        public PolygonShape(Point anchor, PointF originPoint)
        {
            GlobalOrigin = anchor;
            LocalAnchor = Point.Empty;
            OriginPoint = originPoint;
            Segments = new List<LineSegment>();
            IsClosed = true;
            AnchorPos = AnchorPosition.Center;
            AnchorOffset = Point.Empty;
            UpdateVirtualBounds();
        }

        /// <summary>
        /// Создаёт многоугольник с предустановленными отрезками
        /// </summary>
        public PolygonShape(Point anchor, PointF originPoint, IEnumerable<LineSegment> segments, bool isClosed = true)
        {
            GlobalOrigin = anchor;
            LocalAnchor = Point.Empty;
            OriginPoint = originPoint;
            Segments = segments.Select(s => s.Clone()).ToList();
            IsClosed = isClosed;
            AnchorPos = AnchorPosition.Center;
            AnchorOffset = Point.Empty;
            UpdateVirtualBounds();
        }

        /// <summary>
        /// Добавить отрезок по длине и углу
        /// </summary>
        /// <param name="length">Длина отрезка</param>
        /// <param name="angleDegrees">Угол относительно предыдущего отрезка (для первого - относительно горизонтали)</param>
        public void AddSegmentByLengthAngle(float length, float angleDegrees)
        {
            Segments.Add(new LineSegment(length, angleDegrees));
            _cachedVertices = null;
            UpdateVirtualBounds();
        }

        /// <summary>
        /// Удалить отрезок по индексу
        /// </summary>
        public void RemoveSegment(int index)
        {
            if (index >= 0 && index < Segments.Count)
            {
                Segments.RemoveAt(index);
                _cachedVertices = null;
                UpdateVirtualBounds();
            }
        }

        /// <summary>
        /// Вставить отрезок в указанную позицию
        /// </summary>
        public void InsertSegment(int index, float length, float angleDegrees)
        {
            if (index >= 0 && index <= Segments.Count)
            {
                Segments.Insert(index, new LineSegment(length, angleDegrees));
                _cachedVertices = null;
                UpdateVirtualBounds();
            }
        }

        /// <summary>
        /// Обновить отрезок по индексу
        /// </summary>
        public void UpdateSegment(int index, float length, float angleDegrees)
        {
            if (index >= 0 && index < Segments.Count)
            {
                Segments[index].Length = length;
                Segments[index].AngleDegrees = angleDegrees;
                _cachedVertices = null;
                UpdateVirtualBounds();
            }
        }

        /// <summary>
        /// Очистить все отрезки
        /// </summary>
        public void ClearSegments()
        {
            Segments.Clear();
            _cachedVertices = null;
            UpdateVirtualBounds();
        }

        /// <summary>
        /// Вычислить вершины многоугольника на основе отрезков
        /// </summary>
        public PointF[] CalculateVertices()
        {
            if (Segments.Count == 0)
            {
                _cachedVertices = Array.Empty<PointF>();
                return _cachedVertices;
            }

            var vertices = new List<PointF>();
            var center = WorldPosition;

            // Начальная точка (смещение от WorldPosition)
            float currentX = center.X + OriginPoint.X;
            float currentY = center.Y + OriginPoint.Y;
            vertices.Add(new PointF(currentX, currentY));

            // Текущий угол наклона (в радианах)
            float currentAngle = 0;

            // Вычисляем все вершины кроме последней (для замкнутого многоугольника)
            // Последняя вершина совпадает с первой, поэтому не добавляем её
            int segmentsToProcess = IsClosed ? Segments.Count - 1 : Segments.Count;

            for (int i = 0; i < segmentsToProcess; i++)
            {
                var segment = Segments[i];
                
                // Добавляем угол текущего отрезка
                currentAngle += segment.AngleDegrees * (float)Math.PI / 180;

                // Вычисляем новую точку
                currentX += segment.Length * (float)Math.Cos(currentAngle);
                currentY += segment.Length * (float)Math.Sin(currentAngle);

                vertices.Add(new PointF(currentX, currentY));
            }

            _cachedVertices = vertices.ToArray();
            return _cachedVertices;
        }

        /// <summary>
        /// Получить кэшированные или вычислить вершины
        /// </summary>
        private PointF[] GetOrCreateVertices()
        {
            if (_cachedVertices == null)
            {
                CalculateVertices();
            }
            return _cachedVertices ?? Array.Empty<PointF>();
        }

        protected override Point CalculateAnchorOffset(AnchorPosition position)
        {
            var bounds = GetPolygonBounds();
            if (bounds.IsEmpty) return Point.Empty;

            float centerX = bounds.X + bounds.Width / 2f;
            float centerY = bounds.Y + bounds.Height / 2f;
            var worldPos = WorldPosition;

            return position switch
            {
                AnchorPosition.Center => new Point(0, 0),
                AnchorPosition.Top => new Point((int)(centerX - worldPos.X), (int)(bounds.Top - worldPos.Y)),
                AnchorPosition.Bottom => new Point((int)(centerX - worldPos.X), (int)(bounds.Bottom - worldPos.Y)),
                AnchorPosition.Left => new Point((int)(bounds.Left - worldPos.X), (int)(centerY - worldPos.Y)),
                AnchorPosition.Right => new Point((int)(bounds.Right - worldPos.X), (int)(centerY - worldPos.Y)),
                AnchorPosition.TopLeft => new Point((int)(bounds.Left - worldPos.X), (int)(bounds.Top - worldPos.Y)),
                AnchorPosition.TopRight => new Point((int)(bounds.Right - worldPos.X), (int)(bounds.Top - worldPos.Y)),
                AnchorPosition.BottomLeft => new Point((int)(bounds.Left - worldPos.X), (int)(bounds.Bottom - worldPos.Y)),
                AnchorPosition.BottomRight => new Point((int)(bounds.Right - worldPos.X), (int)(bounds.Bottom - worldPos.Y)),
                _ => AnchorOffset
            };
        }

        /// <summary>
        /// Получить границы многоугольника
        /// </summary>
        private RectangleF GetPolygonBounds()
        {
            var vertices = GetOrCreateVertices();
            if (vertices.Length == 0) return RectangleF.Empty;

            float minX = vertices[0].X;
            float maxX = vertices[0].X;
            float minY = vertices[0].Y;
            float maxY = vertices[0].Y;

            foreach (var v in vertices)
            {
                if (v.X < minX) minX = v.X;
                if (v.X > maxX) maxX = v.X;
                if (v.Y < minY) minY = v.Y;
                if (v.Y > maxY) maxY = v.Y;
            }

            return new RectangleF(minX, minY, maxX - minX, maxY - minY);
        }

        public override Point[] GetWorldPoints()
        {
            var vertices = GetOrCreateVertices();
            var points = new Point[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                points[i] = Point.Round(vertices[i]);
            }
            return points;
        }

        protected override void UpdateVirtualBounds()
        {
            var vertices = GetOrCreateVertices();
            if (vertices.Length < 2)
            {
                VirtualBounds = Rectangle.Empty;
                return;
            }

            // Для замкнутых фигур используем стандартный метод
            if (IsClosed && vertices.Length >= 3)
            {
                VirtualBounds = CalculateBoundsWithBorderWidth(vertices);
            }
            else
            {
                // Для незамкнутых - просто границы всех точек
                VirtualBounds = Rectangle.Round(GetPolygonBounds());
            }
        }

        public override void Draw(Graphics g)
        {
            var vertices = GetOrCreateVertices();
            if (vertices.Length < 2) return;

            // Заливка фигуры (только для замкнутых)
            if (IsClosed && vertices.Length >= 3)
            {
                using (var path = new GraphicsPath())
                {
                    path.AddPolygon(vertices);
                    using (var brush = new SolidBrush(FillColor))
                    {
                        g.FillPath(brush, path);
                    }
                }
            }

            // Рисуем стороны
            if (IsClosed && vertices.Length >= 3)
            {
                // Для замкнутого многоугольника - используем стандартный метод
                DrawSidesWithMiterClip(g, vertices);
            }
            else
            {
                // Для незамкнутой ломаной - рисуем отрезки отдельно
                DrawOpenPolyline(g, vertices);
            }

            // Виртуальные границы для выбранной фигуры
            DrawVirtualBounds(g);
        }

        /// <summary>
        /// Отрисовка незамкнутой ломаной линии
        /// </summary>
        private void DrawOpenPolyline(Graphics g, PointF[] vertices)
        {
            int n = vertices.Length - 1; // Количество отрезков
            for (int i = 0; i < n; i++)
            {
                int sideIndex = Math.Min(i, BorderWidths.Length - 1);
                using (var pen = new Pen(BorderColors[sideIndex], BorderWidths[sideIndex]))
                {
                    g.DrawLine(pen, vertices[i], vertices[i + 1]);
                }
            }
        }

        public override bool HitTest(Point p)
        {
            var vertices = GetOrCreateVertices();

            if (vertices.Length < 2) return false;

            if (IsClosed && vertices.Length >= 3)
            {
                // Для замкнутого многоугольника - используем математический алгоритм
                // (Ray Casting Algorithm - проверка пересечения луча с рёбрами)
                return IsPointInPolygon(p, vertices);
            }
            else
            {
                // Для незамкнутой ломаной - проверяем расстояние до отрезков
                const float tolerance = 5f;
                for (int i = 0; i < vertices.Length - 1; i++)
                {
                    if (DistancePointToSegment(p, vertices[i], vertices[i + 1]) < tolerance)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Проверка попадания точки в полигон (Ray Casting Algorithm)
        /// </summary>
        private static bool IsPointInPolygon(Point p, PointF[] vertices)
        {
            int n = vertices.Length;
            bool inside = false;

            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                float xi = vertices[i].X, yi = vertices[i].Y;
                float xj = vertices[j].X, yj = vertices[j].Y;

                if (((yi > p.Y) != (yj > p.Y)) &&
                    (p.X < (xj - xi) * (p.Y - yi) / (yj - yi) + xi))
                {
                    inside = !inside;
                }
            }

            return inside;
        }

        /// <summary>
        /// Расстояние от точки до отрезка
        /// </summary>
        private static float DistancePointToSegment(PointF p, PointF a, PointF b)
        {
            float dx = b.X - a.X;
            float dy = b.Y - a.Y;
            float lengthSq = dx * dx + dy * dy;

            if (lengthSq < 0.0001f)
            {
                // Точки a и b совпадают
                return (float)Math.Sqrt((p.X - a.X) * (p.X - a.X) + (p.Y - a.Y) * (p.Y - a.Y));
            }

            // Проекция точки p на линию ab
            float t = ((p.X - a.X) * dx + (p.Y - a.Y) * dy) / lengthSq;
            t = Math.Max(0, Math.Min(1, t));

            PointF projection = new PointF(
                a.X + t * dx,
                a.Y + t * dy
            );

            return (float)Math.Sqrt(
                (p.X - projection.X) * (p.X - projection.X) +
                (p.Y - projection.Y) * (p.Y - projection.Y)
            );
        }

        public override void Resize(float scaleFactor)
        {
            if (scaleFactor <= 0) return;

            // Масштабируем длины всех отрезков
            foreach (var segment in Segments)
            {
                segment.Length *= scaleFactor;
            }

            // Масштабируем OriginPoint
            OriginPoint = new PointF(
                OriginPoint.X * scaleFactor,
                OriginPoint.Y * scaleFactor
            );

            _cachedVertices = null;
            AnchorOffset = CalculateAnchorOffset(AnchorPos);
            UpdateVirtualBounds();
        }

        public override void ResizeSide(int sideIndex, float scaleFactor)
        {
            if (sideIndex < 0 || sideIndex >= Segments.Count) return;
            if (scaleFactor <= 0) return;

            // Изменяем длину указанного отрезка
            Segments[sideIndex].Length *= scaleFactor;

            _cachedVertices = null;
            UpdateVirtualBounds();
        }

        public override float GetSideLength(int sideIndex)
        {
            if (sideIndex < 0 || sideIndex >= Segments.Count) return 0;
            return Segments[sideIndex].Length;
        }

        public override void SetSideLength(int sideIndex, float length)
        {
            if (sideIndex < 0 || sideIndex >= Segments.Count) return;
            if (length <= 0) return;

            Segments[sideIndex].Length = length;

            _cachedVertices = null;
            UpdateVirtualBounds();
        }

        public override void SetAnchorPosition(AnchorPosition position)
        {
            base.SetAnchorPosition(position);
            UpdateVirtualBounds();
        }

        /// <summary>
        /// Переместить фигуру в указанную точку (экранные координаты)
        /// Переопределено для сброса кэша вершин при перемещении
        /// </summary>
        public override void MoveTo(Point location)
        {
            _cachedVertices = null; // Сбрасываем кэш при перемещении
            base.MoveTo(location);
        }

        /// <summary>
        /// Переместить фигуру на указанное смещение
        /// Переопределено для сброса кэша вершин при перемещении
        /// </summary>
        public override void MoveBy(int dx, int dy)
        {
            _cachedVertices = null; // Сбрасываем кэш при перемещении
            base.MoveBy(dx, dy);
        }

        /// <summary>
        /// Создать клон многоугольника
        /// </summary>
        public PolygonShape ClonePolygon()
        {
            var clone = new PolygonShape(GlobalOrigin, OriginPoint, Segments, IsClosed)
            {
                LocalAnchor = LocalAnchor,
                AnchorPos = AnchorPos,
                AnchorOffset = AnchorOffset,
                FillColor = FillColor,
                IsSelected = IsSelected
            };

            // Копируем стили границ
            for (int i = 0; i < Math.Min(Segments.Count, 6); i++)
            {
                clone.BorderWidths[i] = BorderWidths[i];
                clone.BorderColors[i] = BorderColors[i];
            }

            return clone;
        }

        /// <summary>
        /// Сбросить деформацию - пересчитать вершины
        /// </summary>
        public override void ResetDeformation()
        {
            _cachedVertices = null;
            base.ResetDeformation();
        }

        /// <summary>
        /// Получить суммарную длину всех отрезков
        /// </summary>
        public float GetTotalLength()
        {
            float total = 0;
            foreach (var segment in Segments)
            {
                total += segment.Length;
            }
            return total;
        }

        /// <summary>
        /// Создать многоугольник из списка вершин
        /// </summary>
        /// <param name="vertices">Вершины многоугольника (в мировых координатах)</param>
        /// <param name="isClosed">Замкнутый ли многоугольник</param>
        /// <returns>Новый многоугольник</returns>
        public static PolygonShape CreateFromVertices(PointF[] vertices, bool isClosed = true)
        {
            if (vertices == null || vertices.Length < 2)
                return null!;

            // Находим центральную точку (будущий GlobalOrigin)
            float centerX = 0, centerY = 0;
            foreach (var v in vertices)
            {
                centerX += v.X;
                centerY += v.Y;
            }
            centerX /= vertices.Length;
            centerY /= vertices.Length;

            var anchor = new Point((int)centerX, (int)centerY);
            var polygon = new PolygonShape(anchor, PointF.Empty)
            {
                IsClosed = isClosed
            };

            // Первая вершина относительно центра
            var firstVertex = vertices[0];
            polygon.OriginPoint = new PointF(firstVertex.X - centerX, firstVertex.Y - centerY);

            // Вычисляем отрезки между вершинами
            for (int i = 0; i < vertices.Length - 1; i++)
            {
                var current = vertices[i];
                var next = vertices[i + 1];

                // Длина отрезка
                float dx = next.X - current.X;
                float dy = next.Y - current.Y;
                float length = (float)Math.Sqrt(dx * dx + dy * dy);

                // Угол отрезка относительно горизонтали
                float absoluteAngle = (float)(Math.Atan2(dy, dx) * 180 / Math.PI);

                // Для первого отрезка угол абсолютный
                // Для последующих - угол относительно предыдущего
                float relativeAngle;
                if (i == 0)
                {
                    relativeAngle = absoluteAngle;
                }
                else
                {
                    // Угол относительно предыдущего отрезка
                    var prev = vertices[i - 1];
                    float prevDx = current.X - prev.X;
                    float prevDy = current.Y - prev.Y;
                    float prevAngle = (float)(Math.Atan2(prevDy, prevDx) * 180 / Math.PI);
                    relativeAngle = absoluteAngle - prevAngle;
                }

                polygon.AddSegmentByLengthAngle(length, relativeAngle);
            }

            // Для замкнутого многоугольника добавляем последний отрезок
            if (isClosed && vertices.Length >= 3)
            {
                var last = vertices[vertices.Length - 1];
                var first = vertices[0];

                float dx = first.X - last.X;
                float dy = first.Y - last.Y;
                float length = (float)Math.Sqrt(dx * dx + dy * dy);

                // Угол относительно предыдущего отрезка
                var prev = vertices[vertices.Length - 2];
                float prevDx = last.X - prev.X;
                float prevDy = last.Y - prev.Y;
                float prevAngle = (float)(Math.Atan2(prevDy, prevDx) * 180 / Math.PI);
                float absoluteAngle = (float)(Math.Atan2(dy, dx) * 180 / Math.PI);
                float relativeAngle = absoluteAngle - prevAngle;

                polygon.AddSegmentByLengthAngle(length, relativeAngle);
            }

            return polygon;
        }

        /// <summary>
        /// Создать правильный многоугольник с n сторонами
        /// </summary>
        public static PolygonShape CreateRegularPolygon(Point anchor, int sides, float radius, bool isClosed = true)
        {
            if (sides < 3) sides = 3;

            var polygon = new PolygonShape(anchor, PointF.Empty)
            {
                IsClosed = isClosed
            };

            // Угол между сторонами в правильном многоугольнике
            float interiorAngle = 180 - 360f / sides;

            // Длина стороны правильного многоугольника
            float sideLength = 2 * radius * (float)Math.Sin(Math.PI / sides);

            // Начальная точка - верхняя вершина
            float startX = 0;
            float startY = -radius;

            polygon.OriginPoint = new PointF(startX, startY);

            // Первый отрезок идёт под углом 180/sides градусов (для правильного многоугольника с верхней вершиной)
            float firstAngle = 180f / sides;
            polygon.AddSegmentByLengthAngle(sideLength, firstAngle);

            // Остальные отрезки
            for (int i = 1; i < sides; i++)
            {
                polygon.AddSegmentByLengthAngle(sideLength, 180 - interiorAngle);
            }

            return polygon;
        }

        /// <summary>
        /// Создать звезду с указанным количеством лучей
        /// </summary>
        public static PolygonShape CreateStar(Point anchor, int rays, float outerRadius, float innerRadius)
        {
            if (rays < 3) rays = 3;

            var polygon = new PolygonShape(anchor, PointF.Empty)
            {
                IsClosed = true
            };

            // Начальная точка - верхняя вершина
            polygon.OriginPoint = new PointF(0, -outerRadius);

            // Угол между лучами
            float angleStep = 360f / (rays * 2);

            // Первый отрезок - от внешнего радиуса к внутреннему
            float currentAngle = 180 - 90 - angleStep; // Начальный угол для первого отрезка
            bool isOuter = true;

            for (int i = 0; i < rays * 2; i++)
            {
                float length = isOuter ? (float)Math.Sqrt(
                    outerRadius * outerRadius + innerRadius * innerRadius -
                    2 * outerRadius * innerRadius * Math.Cos(angleStep * Math.PI / 180)) : (float)Math.Sqrt(
                    outerRadius * outerRadius + innerRadius * innerRadius -
                    2 * outerRadius * innerRadius * Math.Cos(angleStep * Math.PI / 180));

                // Угол поворота
                float turnAngle = 180 - angleStep;

                if (i == 0)
                {
                    polygon.AddSegmentByLengthAngle(length, currentAngle);
                }
                else
                {
                    polygon.AddSegmentByLengthAngle(length, isOuter ? turnAngle : -turnAngle);
                }

                isOuter = !isOuter;
            }

            return polygon;
        }

        public override JsonObject Save()
        {
            var json = base.Save();
            json["isClosed"] = IsClosed;
            json["originPoint"] = SavePointF(OriginPoint);

            var segs = new JsonArray();
            foreach (var seg in Segments)
            {
                segs.Add(new JsonObject { ["length"] = seg.Length, ["angleDegrees"] = seg.AngleDegrees });
            }
            json["segments"] = segs;
            return json;
        }

        public static PolygonShape LoadFromJson(JsonObject json)
        {
            var originPt = json.ContainsKey("originPoint") ? LoadPointF(json["originPoint"]!.AsObject()) : PointF.Empty;
            var shape = new PolygonShape(Point.Empty, originPt);

            if (json.ContainsKey("segments"))
            {
                var segs = json["segments"]!.AsArray();
                foreach (var segNode in segs)
                {
                    var segObj = segNode!.AsObject();
                    shape.AddSegmentByLengthAngle(
                        (float)segObj["length"]!.GetValue<double>(),
                        (float)segObj["angleDegrees"]!.GetValue<double>()
                    );
                }
            }

            if (json.ContainsKey("isClosed"))
                shape.IsClosed = json["isClosed"]!.GetValue<bool>();

            shape.LoadCommon(json);
            return shape;
        }
    }
}
