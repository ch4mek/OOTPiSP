using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.Json.Nodes;

namespace OOTPiSP_LR1.Shapes
{
    /// <summary>
    /// Положение точки привязки относительно фигуры
    /// </summary>
    public enum AnchorPosition
    {
        Center,      // Центр фигуры
        TopLeft,     // Верхний левый угол
        TopRight,    // Верхний правый угол
        BottomLeft,  // Нижний левый угол
        BottomRight, // Нижний правый угол
        Top,         // Верхняя сторона
        Bottom,      // Нижняя сторона
        Left,        // Левая сторона
        Right,       // Правая сторона
        Custom       // Произвольное положение
    }

    /// <summary>
    /// Стиль стороны фигуры (толщина и цвет)
    /// </summary>
    public struct SideStyle
    {
        public float Width;
        public Color Color;

        public SideStyle(float width, Color color)
        {
            Width = width;
            Color = color;
        }
    }

    /// <summary>
    /// Базовый класс для всех фигур
    /// </summary>
    public abstract class ShapeBase
    {
        private static int _nextId = 1;

        public int Id { get; set; } = _nextId++;
        public string ShapeName { get; set; } = "";

        public virtual string DefaultTypeName => "Фигура";

        public string DisplayName => string.IsNullOrEmpty(ShapeName)
            ? $"{DefaultTypeName} #{Id}"
            : ShapeName;

        /// <summary>
        /// Глобальная точка отсчёта фигуры в экранных координатах.
        /// Задаётся пользователем явно. При изменении — фигура визуально смещается.
        /// </summary>
        public Point GlobalOrigin { get; set; } = Point.Empty;

        /// <summary>
        /// Локальная точка привязки — координаты логического "центра" фигуры
        /// относительно GlobalOrigin. Это то, что пользователь задаёт как "X, Y фигуры".
        /// </summary>
        public Point LocalAnchor { get; set; } = Point.Empty;

        /// <summary>
        /// Итоговая позиция центра фигуры в экранных координатах.
        /// Используется внутри для отрисовки.
        /// </summary>
        public Point WorldPosition => new Point(
            GlobalOrigin.X + LocalAnchor.X,
            GlobalOrigin.Y + LocalAnchor.Y
        );

        // Смещение точки привязки относительно геометрического центра фигуры
        public Point AnchorOffset { get; set; }

        // Положение точки привязки
        public AnchorPosition AnchorPos { get; set; } = AnchorPosition.Center;

        // Виртуальные границы фигуры (квадрат, в котором находится фигура)
        public Rectangle VirtualBounds { get; protected set; }

        // Толщина и цвет для каждой стороны (динамически расширяются)
        private float[] _borderWidths;
        private Color[] _borderColors;

        public float[] BorderWidths
        {
            get
            {
                EnsureBorderArraysCapacity(SideCount);
                return _borderWidths;
            }
        }

        public Color[] BorderColors
        {
            get
            {
                EnsureBorderArraysCapacity(SideCount);
                return _borderColors;
            }
        }

        /// <summary>
        /// Обеспечивает достаточную ёмкость массивов для указанного количества сторон.
        /// При расширении сохраняет существующие значения и заполняет новые значениями по умолчанию.
        /// </summary>
        protected void EnsureBorderArraysCapacity(int requiredCount)
        {
            if (_borderWidths == null || _borderWidths.Length < requiredCount)
            {
                int oldLength = _borderWidths?.Length ?? 0;
                int newLength = Math.Max(requiredCount, oldLength * 2); // Удваиваем для эффективности

                var newWidths = new float[newLength];
                var newColors = new Color[newLength];

                // Копируем существующие значения
                if (_borderWidths != null)
                {
                    Array.Copy(_borderWidths, newWidths, oldLength);
                    Array.Copy(_borderColors, newColors, oldLength);
                }

                // Заполняем новые элементы значениями по умолчанию
                for (int i = oldLength; i < newLength; i++)
                {
                    newWidths[i] = 2f;
                    newColors[i] = Color.Black;
                }

                _borderWidths = newWidths;
                _borderColors = newColors;
            }
        }

        /// <summary>
        /// Удобный доступ к стилям сторон через структуру SideStyle
        /// </summary>
        public SideStyle[] Sides
        {
            get
            {
                EnsureBorderArraysCapacity(SideCount);
                var sides = new SideStyle[SideCount];
                for (int i = 0; i < SideCount; i++)
                {
                    sides[i] = new SideStyle(_borderWidths[i], _borderColors[i]);
                }
                return sides;
            }
        }

        // Цвет заливки
        public Color FillColor { get; set; }

        // Фигура выбрана?
        public bool IsSelected { get; set; }

        /// <summary>
        /// Фигура является частью составной фигуры (CompositeShape).
        /// Если true - не рисовать точку привязки и виртуальные границы.
        /// </summary>
        public bool IsChildOfComposite { get; set; }

        // Количество сторон фигуры
        public abstract int SideCount { get; }

        /// <summary>
        /// Деформированные вершины фигуры (после изменения отдельных сторон).
        /// Если null — используются базовые вершины из GetWorldPoints().
        /// </summary>
        protected PointF[]? DeformedVertices { get; set; }

        /// <summary>
        /// Фигура была деформирована (изменены пропорции)
        /// </summary>
        public bool IsDeformed => DeformedVertices != null;

        protected ShapeBase()
        {
            // Инициализируем массивы минимальным размером
            // Они будут автоматически расширены при необходимости через EnsureBorderArraysCapacity
            _borderWidths = new float[6];
            _borderColors = new Color[6];

            for (int i = 0; i < 6; i++)
            {
                _borderWidths[i] = 2f;
                _borderColors[i] = Color.Black;
            }

            FillColor = Color.LightGray;
        }

        /// <summary>
        /// Установить толщину и цвет для указанной стороны
        /// </summary>
        public void SetBorder(int sideIndex, float width, Color color)
        {
            if (sideIndex < 0 || sideIndex >= SideCount)
                throw new ArgumentOutOfRangeException(nameof(sideIndex));

            BorderWidths[sideIndex] = width;
            BorderColors[sideIndex] = color;
        }

        /// <summary>
        /// Установить толщину для указанной стороны
        /// </summary>
        public void SetBorderWidth(int sideIndex, float width)
        {
            if (sideIndex < 0 || sideIndex >= SideCount)
                throw new ArgumentOutOfRangeException(nameof(sideIndex));
            BorderWidths[sideIndex] = width;
        }

        /// <summary>
        /// Установить цвет для указанной стороны
        /// </summary>
        public void SetBorderColor(int sideIndex, Color color)
        {
            if (sideIndex < 0 || sideIndex >= SideCount)
                throw new ArgumentOutOfRangeException(nameof(sideIndex));
            BorderColors[sideIndex] = color;
        }

        /// <summary>
        /// Установить положение точки привязки относительно фигуры
        /// </summary>
        public virtual void SetAnchorPosition(AnchorPosition position)
        {
            AnchorPos = position;
            AnchorOffset = CalculateAnchorOffset(position);
            // Обновляем LocalAnchor чтобы геометрический центр остался на месте
            var center = GetCenter();
            LocalAnchor = new Point(center.X - GlobalOrigin.X, center.Y - GlobalOrigin.Y);
        }

        /// <summary>
        /// Рассчитать смещение точки привязки для указанного положения
        /// </summary>
        protected abstract Point CalculateAnchorOffset(AnchorPosition position);

        /// <summary>
        /// Получить геометрический центр фигуры (в абсолютных координатах)
        /// </summary>
        public Point GetCenter()
        {
            return new Point(WorldPosition.X - AnchorOffset.X, WorldPosition.Y - AnchorOffset.Y);
        }

        /// <summary>
        /// Обновить виртуальные границы фигуры
        /// </summary>
        protected abstract void UpdateVirtualBounds();

        /// <summary>
        /// Публичный метод для обновления виртуальных границ
        /// </summary>
        public void RefreshBounds()
        {
            UpdateVirtualBounds();
        }

        /// <summary>
        /// Получить максимальную толщину ребра фигуры
        /// </summary>
        protected float GetMaxBorderWidth()
        {
            float max = 0;
            for (int i = 0; i < SideCount; i++)
            {
                if (BorderWidths[i] > max)
                    max = BorderWidths[i];
            }
            return max;
        }

        /// <summary>
        /// Вычислить границы фигуры с учётом толщины рёбер, используя алгоритм пересечения кромок.
        /// Этот метод использует тот же алгоритм, что и DrawSidesWithMiterClip.
        /// </summary>
        protected Rectangle CalculateBoundsWithBorderWidth(PointF[] pts)
        {
            if (pts == null || pts.Length == 0)
                return Rectangle.Empty;

            int n = pts.Length;
            if (n < 2)
            {
                // Для одной точки просто возвращаем точку
                return new Rectangle((int)pts[0].X, (int)pts[0].Y, 0, 0);
            }

            // Обеспечиваем достаточный размер массивов границ для n вершин
            EnsureBorderArraysCapacity(n);

            // Массивы для хранения кромок каждого ребра
            var topStarts = new PointF[n];
            var topEnds = new PointF[n];
            var bottomStarts = new PointF[n];
            var bottomEnds = new PointF[n];

            // Вычисляем кромки для каждого ребра (тот же алгоритм, что и в DrawSidesWithMiterClip)
            for (int i = 0; i < n; i++)
            {
                int next = (i + 1) % n;
                PointF p1 = pts[i];
                PointF p2 = pts[next];
                float w = BorderWidths[i];

                // Направление ребра
                float dx = p2.X - p1.X;
                float dy = p2.Y - p1.Y;
                float len = (float)Math.Sqrt(dx * dx + dy * dy);
                if (len < 0.001f) len = 0.001f;

                // Единичный вектор направления
                float dirX = dx / len;
                float dirY = dy / len;

                // Нормаль (перпендикуляр) - поворот на 90° против часовой стрелки
                float normalX = -dirY;
                float normalY = dirX;

                // Сдвиг по нормали на половину толщины
                float offset = w / 2f;

                // Верхняя кромка (по нормали)
                topStarts[i] = new PointF(p1.X + normalX * offset, p1.Y + normalY * offset);
                topEnds[i] = new PointF(p2.X + normalX * offset, p2.Y + normalY * offset);

                // Нижняя кромка (против нормали)
                bottomStarts[i] = new PointF(p1.X - normalX * offset, p1.Y - normalY * offset);
                bottomEnds[i] = new PointF(p2.X - normalX * offset, p2.Y - normalY * offset);
            }

            // Для каждой вершины находим точки пересечения кромок
            var intersectionTop = new PointF[n];
            var intersectionBottom = new PointF[n];

            for (int i = 0; i < n; i++)
            {
                int prev = (i - 1 + n) % n;

                // Пересечение верхних кромок: prev -> i
                intersectionTop[i] = IntersectLines(
                    topStarts[prev], topEnds[prev],
                    topStarts[i], topEnds[i]
                );

                // Пересечение нижних кромок: prev -> i
                intersectionBottom[i] = IntersectLines(
                    bottomStarts[prev], bottomEnds[prev],
                    bottomStarts[i], bottomEnds[i]
                );
            }

            // Находим границы среди всех точек пересечения
            float minX = intersectionTop[0].X, maxX = intersectionTop[0].X;
            float minY = intersectionTop[0].Y, maxY = intersectionTop[0].Y;

            for (int i = 0; i < n; i++)
            {
                // Проверяем верхние точки пересечения
                if (intersectionTop[i].X < minX) minX = intersectionTop[i].X;
                if (intersectionTop[i].X > maxX) maxX = intersectionTop[i].X;
                if (intersectionTop[i].Y < minY) minY = intersectionTop[i].Y;
                if (intersectionTop[i].Y > maxY) maxY = intersectionTop[i].Y;

                // Проверяем нижние точки пересечения
                if (intersectionBottom[i].X < minX) minX = intersectionBottom[i].X;
                if (intersectionBottom[i].X > maxX) maxX = intersectionBottom[i].X;
                if (intersectionBottom[i].Y < minY) minY = intersectionBottom[i].Y;
                if (intersectionBottom[i].Y > maxY) maxY = intersectionBottom[i].Y;
            }

            return new Rectangle(
                (int)Math.Floor(minX),
                (int)Math.Floor(minY),
                (int)Math.Ceiling(maxX - minX),
                (int)Math.Ceiling(maxY - minY)
            );
        }

        /// <summary>
        /// Получить мировые координаты вершин фигуры (базовые, без деформации)
        /// </summary>
        public abstract Point[] GetWorldPoints();

        /// <summary>
        /// Получить вершины фигуры с учётом деформации.
        /// Если фигура была деформирована — возвращает деформированные вершины,
        /// иначе — базовые вершины конвертированные в PointF.
        /// </summary>
        public PointF[] GetVertices()
        {
            if (DeformedVertices != null)
            {
                return DeformedVertices;
            }
            
            var pts = GetWorldPoints();
            var result = new PointF[pts.Length];
            for (int i = 0; i < pts.Length; i++)
            {
                result[i] = new PointF(pts[i].X, pts[i].Y);
            }
            return result;
        }

        /// <summary>
        /// Применить деформированные вершины к фигуре
        /// </summary>
        protected void ApplyDeformedVertices(PointF[] vertices)
        {
            DeformedVertices = vertices;
            UpdateVirtualBounds();
        }

        /// <summary>
        /// Сбросить деформацию — вернуть фигуру к исходной форме
        /// </summary>
        public virtual void ResetDeformation()
        {
            DeformedVertices = null;
            UpdateVirtualBounds();
        }

        /// <summary>
        /// Отрисовать фигуру
        /// </summary>
        public abstract void Draw(Graphics g);

        /// <summary>
        /// Проверка попадания точки в фигуру
        /// </summary>
        public abstract bool HitTest(Point p);

        /// <summary>
        /// Изменить размер фигуры с заданным масштабом
        /// </summary>
        /// <param name="scaleFactor">Коэффициент масштабирования (1.0 = без изменений)</param>
        public abstract void Resize(float scaleFactor);

        /// <summary>
        /// Изменить размер указанной стороны с пропорциональным изменением остальных
        /// </summary>
        /// <param name="sideIndex">Индекс стороны</param>
        /// <param name="scaleFactor">Коэффициент масштабирования</param>
        public abstract void ResizeSide(int sideIndex, float scaleFactor);

        /// <summary>
        /// Получить длину указанной стороны
        /// </summary>
        public abstract float GetSideLength(int sideIndex);

        /// <summary>
        /// Установить длину указанной стороны с пропорциональным изменением остальных
        /// </summary>
        /// <param name="sideIndex">Индекс стороны</param>
        /// <param name="length">Новая длина стороны</param>
        public abstract void SetSideLength(int sideIndex, float length);

        /// <summary>
        /// Получить угол в вершине (в градусах)
        /// </summary>
        /// <param name="vertexIndex">Индекс вершины</param>
        /// <returns>Угол в градусах</returns>
        public virtual float GetAngle(int vertexIndex)
        {
            var pts = GetVertices();
            int n = pts.Length;
            if (n < 3 || vertexIndex < 0 || vertexIndex >= n) return 0;

            int prev = (vertexIndex - 1 + n) % n;
            int next = (vertexIndex + 1) % n;

            // Векторы от вершины к соседям
            PointF v1 = new PointF(pts[prev].X - pts[vertexIndex].X, pts[prev].Y - pts[vertexIndex].Y);
            PointF v2 = new PointF(pts[next].X - pts[vertexIndex].X, pts[next].Y - pts[vertexIndex].Y);

            // Вычисляем угол между векторами
            float dot = v1.X * v2.X + v1.Y * v2.Y;
            float len1 = (float)Math.Sqrt(v1.X * v1.X + v1.Y * v1.Y);
            float len2 = (float)Math.Sqrt(v2.X * v2.X + v2.Y * v2.Y);

            if (len1 < 0.001f || len2 < 0.001f) return 0;

            float cosAngle = dot / (len1 * len2);
            cosAngle = Math.Max(-1, Math.Min(1, cosAngle)); // Clamp для точности

            return (float)(Math.Acos(cosAngle) * 180 / Math.PI);
        }

        /// <summary>
        /// Установить угол в вершине (в градусах)
        /// </summary>
        /// <param name="vertexIndex">Индекс вершины</param>
        /// <param name="angleDegrees">Новый угол в градусах</param>
        public virtual void SetAngle(int vertexIndex, float angleDegrees)
        {
            var pts = GetVertices();
            int n = pts.Length;
            if (n < 3 || vertexIndex < 0 || vertexIndex >= n) return;

            int prev = (vertexIndex - 1 + n) % n;
            int next = (vertexIndex + 1) % n;

            // Текущие векторы
            PointF v1 = new PointF(pts[prev].X - pts[vertexIndex].X, pts[prev].Y - pts[vertexIndex].Y);
            PointF v2 = new PointF(pts[next].X - pts[vertexIndex].X, pts[next].Y - pts[vertexIndex].Y);

            float len1 = (float)Math.Sqrt(v1.X * v1.X + v1.Y * v1.Y);
            float len2 = (float)Math.Sqrt(v2.X * v2.X + v2.Y * v2.Y);

            if (len1 < 0.001f || len2 < 0.001f) return;

            // Текущий угол v2 относительно v1
            float angle1 = (float)Math.Atan2(v1.Y, v1.X);
            float angle2 = (float)Math.Atan2(v2.Y, v2.X);
            float currentAngle = angle2 - angle1;

            // Нормализуем к диапазону [-PI, PI]
            while (currentAngle > Math.PI) currentAngle -= (float)(2 * Math.PI);
            while (currentAngle < -Math.PI) currentAngle += (float)(2 * Math.PI);

            // Новый угол для v2
            float newAngle = angleDegrees * (float)Math.PI / 180;
            float targetAngle = angle1 + newAngle;

            // Новая позиция для next вершины (сохраняем длину ребра)
            PointF newNext = new PointF(
                pts[vertexIndex].X + len2 * (float)Math.Cos(targetAngle),
                pts[vertexIndex].Y + len2 * (float)Math.Sin(targetAngle)
            );

            // Создаём новый массив вершин
            var newVertices = new PointF[n];
            for (int i = 0; i < n; i++)
            {
                newVertices[i] = pts[i];
            }
            newVertices[next] = newNext;

            // Применяем деформированные вершины
            ApplyDeformedVertices(newVertices);
        }

        /// <summary>
        /// Переместить фигуру на указанное смещение
        /// </summary>
        public virtual void MoveBy(int dx, int dy)
        {
            LocalAnchor = new Point(LocalAnchor.X + dx, LocalAnchor.Y + dy);
            UpdateVirtualBounds();
        }

        /// <summary>
        /// Переместить фигуру в указанную точку (экранные координаты)
        /// Учитывает GlobalOrigin для корректного позиционирования
        /// </summary>
        public virtual void MoveTo(Point location)
        {
            // location - это желаемая позиция WorldPosition
            // WorldPosition = GlobalOrigin + LocalAnchor
            // Значит LocalAnchor = location - GlobalOrigin
            LocalAnchor = new Point(location.X - GlobalOrigin.X, location.Y - GlobalOrigin.Y);
            UpdateVirtualBounds();
        }

        /// <summary>
        /// Переместить GlobalOrigin — фигура визуально сдвигается
        /// </summary>
        public void SetGlobalOrigin(Point origin)
        {
            GlobalOrigin = origin;
            UpdateVirtualBounds();
        }

        /// <summary>
        /// Ограничить позицию фигуры в заданных границах
        /// </summary>
        /// <param name="bounds">Прямоугольник границ</param>
        /// <returns>True если позиция была изменена</returns>
        public bool ClampToBounds(Rectangle bounds)
        {
            bool wasClamped = false;
            
            // Получаем текущие виртуальные границы
            var vb = VirtualBounds;
            
            // Проверяем, выходит ли фигура за границы
            int newWorldX = WorldPosition.X;
            int newWorldY = WorldPosition.Y;
            
            // Ограничиваем по X
            if (vb.Left < bounds.Left)
            {
                newWorldX = WorldPosition.X + (bounds.Left - vb.Left);
                wasClamped = true;
            }
            else if (vb.Right > bounds.Right)
            {
                newWorldX = WorldPosition.X - (vb.Right - bounds.Right);
                wasClamped = true;
            }
            
            // Ограничиваем по Y
            if (vb.Top < bounds.Top)
            {
                newWorldY = WorldPosition.Y + (bounds.Top - vb.Top);
                wasClamped = true;
            }
            else if (vb.Bottom > bounds.Bottom)
            {
                newWorldY = WorldPosition.Y - (vb.Bottom - bounds.Bottom);
                wasClamped = true;
            }
            
            // Применяем ограниченную позицию
            if (wasClamped)
            {
                LocalAnchor = new Point(newWorldX - GlobalOrigin.X, newWorldY - GlobalOrigin.Y);
                UpdateVirtualBounds();
            }
            
            return wasClamped;
        }

        /// <summary>
        /// Получить верхний левый угол виртуальных границ
        /// </summary>
        public Point GetVirtualTopLeft()
        {
            return new Point(VirtualBounds.X, VirtualBounds.Y);
        }

        /// <summary>
        /// Получить правый нижний угол виртуальных границ
        /// </summary>
        public Point GetVirtualBottomRight()
        {
            return new Point(VirtualBounds.Right, VirtualBounds.Bottom);
        }

        /// <summary>
        /// Создать GraphicsPath для фигуры (используется для заливки и обводки)
        /// </summary>
        protected virtual GraphicsPath CreatePath()
        {
            var path = new GraphicsPath();
            var points = GetWorldPoints();
            if (points.Length > 2)
            {
                path.AddPolygon(points);
            }
            return path;
        }

        /// <summary>
        /// Отрисовать виртуальные границы (для выбранной фигуры)
        /// </summary>
        protected void DrawVirtualBounds(Graphics g)
        {
            // Не рисуем виртуальные границы для дочерних фигур составной фигуры
            if (IsChildOfComposite) return;

            if (!IsSelected) return;

            // Bounding box
            using (var pen = new Pen(Color.Red, 1) { DashStyle = DashStyle.Dot })
            {
                g.DrawRectangle(pen, VirtualBounds);
            }

            // GlobalOrigin — оранжевый крестик с подписью
            DrawCross(g, GlobalOrigin, Color.OrangeRed, 10);
            using (var font = new Font("Arial", 7f))
            using (var orangeBrush = new SolidBrush(Color.OrangeRed))
            {
                g.DrawString($"G({GlobalOrigin.X},{GlobalOrigin.Y})", font, orangeBrush,
                    GlobalOrigin.X + 8, GlobalOrigin.Y - 14);
            }

            // Линия от GlobalOrigin до LocalAnchor (WorldPosition)
            using (var linkPen = new Pen(Color.OrangeRed, 1) { DashStyle = DashStyle.Dash })
            {
                g.DrawLine(linkPen, GlobalOrigin, WorldPosition);
            }

            // WorldPosition (LocalAnchor) — синяя точка
            using (var blueBrush = new SolidBrush(Color.Blue))
            {
                g.FillEllipse(blueBrush, WorldPosition.X - 5, WorldPosition.Y - 5, 10, 10);
            }
            using (var font = new Font("Arial", 7f))
            using (var whiteBrush = new SolidBrush(Color.White))
            {
                g.DrawString($"L({LocalAnchor.X},{LocalAnchor.Y})", font, whiteBrush,
                    WorldPosition.X + 6, WorldPosition.Y - 14);
            }

            // Геометрический центр — зелёный крестик
            DrawCross(g, GetCenter(), Color.LimeGreen, 6);
        }

        /// <summary>
        /// Рисует крестик в указанной точке
        /// </summary>
        private void DrawCross(Graphics g, Point center, Color color, int size)
        {
            using (var pen = new Pen(color, 2))
            {
                g.DrawLine(pen, center.X - size / 2, center.Y, center.X + size / 2, center.Y);
                g.DrawLine(pen, center.X, center.Y - size / 2, center.X, center.Y + size / 2);
            }
        }

        /// <summary>
        /// Рисует стороны фигуры с разной толщиной и цветом, используя пересечение кромок.
        /// Для каждого ребра считаем 2 линии: верхнюю и нижнюю кромку (offset на ±t/2 по нормали).
        /// В вершине пересекаем соседние кромки для получения точек bevel-угла.
        /// </summary>
        protected void DrawSidesWithMiterClip(Graphics g, PointF[] pts)
        {
            int n = pts.Length;
            if (n < 2) return;

            // Обеспечиваем достаточный размер массивов границ для n вершин
            EnsureBorderArraysCapacity(n);

            // Массивы для хранения кромок каждого ребра
            // Верхняя кромка: startTop -> endTop
            // Нижняя кромка: startBottom -> endBottom
            var topStarts = new PointF[n];
            var topEnds = new PointF[n];
            var bottomStarts = new PointF[n];
            var bottomEnds = new PointF[n];

            // Вычисляем кромки для каждого ребра
            for (int i = 0; i < n; i++)
            {
                int next = (i + 1) % n;
                PointF p1 = pts[i];
                PointF p2 = pts[next];
                float w = BorderWidths[i];

                // Направление ребра
                float dx = p2.X - p1.X;
                float dy = p2.Y - p1.Y;
                float len = (float)Math.Sqrt(dx * dx + dy * dy);
                if (len < 0.001f) len = 0.001f;

                // Единичный вектор направления
                float dirX = dx / len;
                float dirY = dy / len;

                // Нормаль (перпендикуляр) - поворот на 90° против часовой стрелки
                float normalX = -dirY;
                float normalY = dirX;

                // Сдвиг по нормали на половину толщины
                float offset = w / 2f;

                // Верхняя кромка (по нормали)
                topStarts[i] = new PointF(p1.X + normalX * offset, p1.Y + normalY * offset);
                topEnds[i] = new PointF(p2.X + normalX * offset, p2.Y + normalY * offset);

                // Нижняя кромка (против нормали)
                bottomStarts[i] = new PointF(p1.X - normalX * offset, p1.Y - normalY * offset);
                bottomEnds[i] = new PointF(p2.X - normalX * offset, p2.Y - normalY * offset);
            }

            // Для каждой вершины находим точки пересечения кромок
            // intersectionTop[i] = пересечение верхней кромки (i-1) с верхней кромкой (i)
            // intersectionBottom[i] = пересечение нижней кромки (i-1) с нижней кромкой (i)
            var intersectionTop = new PointF[n];
            var intersectionBottom = new PointF[n];

            for (int i = 0; i < n; i++)
            {
                int prev = (i - 1 + n) % n;

                // Пересечение верхних кромок: prev -> i
                intersectionTop[i] = IntersectLines(
                    topStarts[prev], topEnds[prev],
                    topStarts[i], topEnds[i]
                );

                // Пересечение нижних кромок: prev -> i
                intersectionBottom[i] = IntersectLines(
                    bottomStarts[prev], bottomEnds[prev],
                    bottomStarts[i], bottomEnds[i]
                );
            }

            // Рисуем каждое ребро как четырёхугольник между точками пересечения
            for (int i = 0; i < n; i++)
            {
                int next = (i + 1) % n;

                // Четыре угла четырёхугольника для ребра i:
                // - начало: intersectionTop[i] и intersectionBottom[i] (в вершине i)
                // - конец: intersectionTop[next] и intersectionBottom[next] (в вершине i+1)
                PointF[] quad = new PointF[]
                {
                    intersectionTop[i],
                    intersectionTop[next],
                    intersectionBottom[next],
                    intersectionBottom[i]
                };

                // Рисуем закрашенный четырёхугольник
                using (var brush = new SolidBrush(BorderColors[i]))
                {
                    g.FillPolygon(brush, quad);
                }
            }
        }

        /// <summary>
        /// Находит точку пересечения двух линий (не отрезков).
        /// Линия 1: p1 -> p2, Линия 2: p3 -> p4
        /// </summary>
        private static PointF IntersectLines(PointF p1, PointF p2, PointF p3, PointF p4)
        {
            // Параметрическое представление:
            // L1: p1 + t * (p2 - p1)
            // L2: p3 + s * (p4 - p3)
            // Решаем систему уравнений

            float dx1 = p2.X - p1.X;
            float dy1 = p2.Y - p1.Y;
            float dx2 = p4.X - p3.X;
            float dy2 = p4.Y - p3.Y;

            float denom = dx1 * dy2 - dy1 * dx2;

            // Если линии параллельны, возвращаем среднюю точку
            if (Math.Abs(denom) < 0.0001f)
            {
                return new PointF((p1.X + p3.X) / 2, (p1.Y + p3.Y) / 2);
            }

            float t = ((p3.X - p1.X) * dy2 - (p3.Y - p1.Y) * dx2) / denom;

            return new PointF(p1.X + t * dx1, p1.Y + t * dy1);
        }

        private static PointF Normalize(float dx, float dy)
        {
            float len = (float)Math.Sqrt(dx * dx + dy * dy);
            return len < 0.001f ? PointF.Empty : new PointF(dx / len, dy / len);
        }

        #region Алгоритм распространения смещения для изменения стороны

        /// <summary>
        /// Изменяет длину указанного ребра, искажая форму фигуры.
        /// Вершины изменяемого ребра сдвигаются, остальные остаются на месте.
        /// Это позволяет превратить равнобедренную трапецию в произвольный четырёхугольник.
        /// </summary>
        /// <param name="sideIndex">Индекс стороны для изменения</param>
        /// <param name="scaleFactor">Коэффициент масштабирования стороны</param>
        /// <returns>Новые координаты вершин</returns>
        protected PointF[] PropagateDisplacement(int sideIndex, float scaleFactor)
        {
            var pts = GetWorldPoints();
            int n = pts.Length;
            if (n < 3) return Array.ConvertAll(pts, p => new PointF(p.X, p.Y));

            // Конвертируем в PointF
            var V = new PointF[n];
            for (int k = 0; k < n; k++)
            {
                V[k] = new PointF(pts[k].X, pts[k].Y);
            }

            // Индексы вершин изменяемого ребра
            int edgeIndex1 = sideIndex;
            int edgeIndex2 = (sideIndex + 1) % n;

            // Центр ребра
            PointF center = new PointF(
                (V[edgeIndex1].X + V[edgeIndex2].X) / 2,
                (V[edgeIndex1].Y + V[edgeIndex2].Y) / 2
            );

            // Масштабируем вершины ребра относительно центра
            V[edgeIndex1] = new PointF(
                center.X + (V[edgeIndex1].X - center.X) * scaleFactor,
                center.Y + (V[edgeIndex1].Y - center.Y) * scaleFactor
            );
            V[edgeIndex2] = new PointF(
                center.X + (V[edgeIndex2].X - center.X) * scaleFactor,
                center.Y + (V[edgeIndex2].Y - center.Y) * scaleFactor
            );

            return V;
        }

        /// <summary>
        /// Вычисляет расстояние между двумя точками
        /// </summary>
        private static float Distance(PointF p1, PointF p2)
        {
            float dx = p2.X - p1.X;
            float dy = p2.Y - p1.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// Расстояние от точки P до отрезка AB
        /// </summary>
        private static float DistancePointToSegment(PointF P, PointF A, PointF B)
        {
            float ABx = B.X - A.X;
            float ABy = B.Y - A.Y;
            float APx = P.X - A.X;
            float APy = P.Y - A.Y;

            float ab2 = ABx * ABx + ABy * ABy;
            if (ab2 < 0.0001f)
            {
                // A и B совпадают
                return (float)Math.Sqrt(APx * APx + APy * APy);
            }

            float t = (APx * ABx + APy * ABy) / ab2;
            t = Math.Max(0, Math.Min(1, t));

            PointF proj = new PointF(A.X + t * ABx, A.Y + t * ABy);
            float dx = P.X - proj.X;
            float dy = P.Y - proj.Y;

            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// Вес влияния в зависимости от расстояния.
        /// Чем ближе к ребру — тем сильнее влияние.
        /// </summary>
        private static float Weight(float distance)
        {
            return 1.0f / (1.0f + distance * 0.01f);
        }

        #endregion

        #region Сохранение/Загрузка

        public virtual JsonObject Save()
        {
            var json = new JsonObject();
            json["id"] = Id;
            json["shapeName"] = ShapeName;
            json["shapeType"] = GetType().Name;
            json["globalOrigin"] = SavePoint(GlobalOrigin);
            json["localAnchor"] = SavePoint(LocalAnchor);
            json["anchorOffset"] = SavePoint(AnchorOffset);
            json["anchorPos"] = AnchorPos.ToString();
            json["fillColor"] = SaveColor(FillColor);
            json["isChildOfComposite"] = IsChildOfComposite;

            var bw = new JsonArray();
            var bc = new JsonArray();
            for (int i = 0; i < SideCount; i++)
            {
                bw.Add(BorderWidths[i]);
                bc.Add(SaveColor(BorderColors[i]));
            }
            json["borderWidths"] = bw;
            json["borderColors"] = bc;

            if (DeformedVertices != null)
            {
                var dv = new JsonArray();
                foreach (var v in DeformedVertices)
                {
                    dv.Add(SavePointF(v));
                }
                json["deformedVertices"] = dv;
            }

            return json;
        }

        protected void LoadCommon(JsonObject json)
        {
            if (json.ContainsKey("id"))
                Id = json["id"]!.GetValue<int>();

            if (json.ContainsKey("shapeName"))
                ShapeName = json["shapeName"]!.GetValue<string>() ?? "";

            GlobalOrigin = LoadPoint(json["globalOrigin"]!.AsObject());
            LocalAnchor = LoadPoint(json["localAnchor"]!.AsObject());
            AnchorOffset = LoadPoint(json["anchorOffset"]!.AsObject());
            AnchorPos = Enum.Parse<AnchorPosition>(json["anchorPos"]!.GetValue<string>());
            FillColor = LoadColor(json["fillColor"]!.GetValue<string>());
            IsChildOfComposite = json["isChildOfComposite"]?.GetValue<bool>() ?? false;

            if (json.ContainsKey("borderWidths") && json.ContainsKey("borderColors"))
            {
                var bw = json["borderWidths"]!.AsArray();
                var bc = json["borderColors"]!.AsArray();
                int borderCount = Math.Min(SideCount, Math.Min(bw.Count, bc.Count));
                for (int i = 0; i < borderCount; i++)
                {
                    BorderWidths[i] = (float)bw[i]!.GetValue<double>();
                    BorderColors[i] = LoadColor(bc[i]!.GetValue<string>());
                }
            }

            if (json.ContainsKey("deformedVertices"))
            {
                var dv = json["deformedVertices"]!.AsArray();
                var vertices = new PointF[dv.Count];
                for (int i = 0; i < dv.Count; i++)
                {
                    vertices[i] = LoadPointF(dv[i]!.AsObject());
                }
                DeformedVertices = vertices;
            }

            UpdateVirtualBounds();
        }

        public static ShapeBase CreateFromJson(JsonObject json)
        {
            string? type = json["shapeType"]?.GetValue<string>();
            return type switch
            {
                "CircleShape" => CircleShape.LoadFromJson(json),
                "RectangleShape" => RectangleShape.LoadFromJson(json),
                "TriangleShape" => TriangleShape.LoadFromJson(json),
                "HexagonShape" => HexagonShape.LoadFromJson(json),
                "TrapezoidShape" => TrapezoidShape.LoadFromJson(json),
                "PolygonShape" => PolygonShape.LoadFromJson(json),
                "CompositeShape" => CompositeShape.LoadFromJson(json),
                "GroupShape" => GroupShape.LoadFromJson(json),
                _ => throw new InvalidOperationException($"Unknown shape type: {type}")
            };
        }

        protected static JsonObject SavePoint(Point p) => new() { ["x"] = p.X, ["y"] = p.Y };
        protected static Point LoadPoint(JsonObject json) => new(json["x"]!.GetValue<int>(), json["y"]!.GetValue<int>());
        protected static JsonObject SavePointF(PointF p) => new() { ["x"] = p.X, ["y"] = p.Y };
        protected static PointF LoadPointF(JsonObject json) => new(json["x"]!.GetValue<float>(), json["y"]!.GetValue<float>());
        protected static string SaveColor(Color c) => $"#{c.A:X2}{c.R:X2}{c.G:X2}{c.B:X2}";
        protected static Color LoadColor(string hex)
        {
            if (hex.StartsWith("#")) hex = hex.Substring(1);
            return Color.FromArgb(
                byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber),
                byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber),
                byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber),
                byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber)
            );
        }

        public static void SyncNextId(IEnumerable<ShapeBase> shapes)
        {
            int maxId = 0;
            foreach (var shape in shapes)
            {
                CheckShapeId(shape, ref maxId);
            }
            _nextId = maxId + 1;
        }

        private static void CheckShapeId(ShapeBase shape, ref int maxId)
        {
            if (shape.Id > maxId) maxId = shape.Id;

            if (shape is CompositeShape composite)
            {
                foreach (var child in composite.GetChildren())
                    CheckShapeId(child, ref maxId);
            }
            else if (shape is GroupShape group)
            {
                foreach (var child in group.GetChildren())
                    CheckShapeId(child, ref maxId);
            }
        }

        #endregion

    }
}
