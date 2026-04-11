using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace OOTPiSP_LR1.Shapes
{
    /// <summary>
    /// Правильный шестиугольник - 6 сторон, каждая со своей толщиной и цветом
    /// </summary>
    public class HexagonShape : ShapeBase
    {
        // Радиус описанной окружности
        public int Radius { get; set; }

        public override int SideCount => 6;

        public HexagonShape(Point anchor, int radius)
        {
            GlobalOrigin = anchor;
            LocalAnchor = Point.Empty;
            Radius = radius;
            AnchorPos = AnchorPosition.Center;
            AnchorOffset = Point.Empty;
            UpdateVirtualBounds();
        }

        protected override Point CalculateAnchorOffset(AnchorPosition position)
        {
            return position switch
            {
                AnchorPosition.Center => new Point(0, 0),
                AnchorPosition.Top => new Point(0, -Radius),
                AnchorPosition.Bottom => new Point(0, Radius),
                AnchorPosition.Left => new Point(-Radius, 0),
                AnchorPosition.Right => new Point(Radius, 0),
                AnchorPosition.TopLeft => new Point(-Radius, -Radius),
                AnchorPosition.TopRight => new Point(Radius, -Radius),
                AnchorPosition.BottomLeft => new Point(-Radius, Radius),
                AnchorPosition.BottomRight => new Point(Radius, Radius),
                _ => AnchorOffset
            };
        }


        public override Point[] GetWorldPoints()
        {
            var center = GetCenter();
            var points = new Point[6];

            // Правильный шестиугольник, вершины на окружности
            // Старт сверху (угол -90 градусов)
            for (int i = 0; i < 6; i++)
            {
                double angle = -Math.PI / 2 + i * 2 * Math.PI / 6;
                points[i] = new Point(
                    (int)(center.X + Radius * Math.Cos(angle)),
                    (int)(center.Y + Radius * Math.Sin(angle))
                );
            }

            return points;
        }

        protected override void UpdateVirtualBounds()
        {
            var pts = GetVertices();
            VirtualBounds = CalculateBoundsWithBorderWidth(pts);
        }

        public override void Draw(Graphics g)
        {
            var pts = GetVertices();

            // Заливка фигуры
            using (var path = new GraphicsPath())
            {
                path.AddPolygon(pts);
                using (var brush = new SolidBrush(FillColor))
                {
                    g.FillPath(brush, path);
                }
            }

            // Рисуем каждую сторону отдельно со своей толщиной и цветом
            DrawSidesWithMiterClip(g, pts);

            // Виртуальные границы для выбранной фигуры
            DrawVirtualBounds(g);
        }


        public override bool HitTest(Point p)
        {
            // Проверяем попадание в шестиугольник через виртуальные границы
            // Для более точной проверки используем GraphicsPath
            using (var path = CreatePath())
            {
                return path.IsVisible(p);
            }
        }

        public override void SetAnchorPosition(AnchorPosition position)
        {
            base.SetAnchorPosition(position);
            UpdateVirtualBounds();
        }

        public override void Resize(float scaleFactor)
        {
            // Минимальный радиус - 10 пикселей
            Radius = Math.Max(10, (int)(Radius * scaleFactor));
            
            // Пересчитываем смещение точки привязки для текущей позиции
            AnchorOffset = CalculateAnchorOffset(AnchorPos);
            
            // Обновляем GlobalOrigin
            var center = GetCenter();
            GlobalOrigin = new Point(center.X + AnchorOffset.X - LocalAnchor.X, 
                                     center.Y + AnchorOffset.Y - LocalAnchor.Y);
            
            UpdateVirtualBounds();
        }

        public override void ResizeSide(int sideIndex, float scaleFactor)
        {
            // Используем алгоритм распространения смещения
            var newVertices = PropagateDisplacement(sideIndex, scaleFactor);
            
            // Применяем деформированные вершины — фигура теряет исходную форму
            ApplyDeformedVertices(newVertices);
        }

        public override float GetSideLength(int sideIndex)
        {
            // Длина стороны правильного шестиугольника = Radius
            return Radius;
        }

        public override void SetSideLength(int sideIndex, float length)
        {
            if (length <= 0) return;

            // Длина стороны правильного шестиугольника = Radius
            Radius = Math.Max(10, (int)length);

            // Пересчитываем смещение точки привязки
            AnchorOffset = CalculateAnchorOffset(AnchorPos);
            var center = GetCenter();
            GlobalOrigin = new Point(center.X + AnchorOffset.X - LocalAnchor.X, 
                                     center.Y + AnchorOffset.Y - LocalAnchor.Y);
            UpdateVirtualBounds();
        }
    }
}
