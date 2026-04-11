using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace OOTPiSP_LR1.Shapes
{
    /// <summary>
    /// Равнобедренная трапеция - 4 стороны, каждая со своей толщиной и цветом
    /// </summary>
    public class TrapezoidShape : ShapeBase
    {
        // Ширина нижнего основания
        public int BottomWidth { get; set; }

        // Ширина верхнего основания
        public int TopWidth { get; set; }

        // Высота трапеции
        public int Height { get; set; }

        public override int SideCount => 4;

        public TrapezoidShape(Point anchor, int bottomWidth, int topWidth, int height)
        {
            GlobalOrigin = anchor;
            LocalAnchor = Point.Empty;
            BottomWidth = bottomWidth;
            TopWidth = topWidth;
            Height = height;
            AnchorPos = AnchorPosition.Center;
            AnchorOffset = Point.Empty;
            UpdateVirtualBounds();
        }

        protected override Point CalculateAnchorOffset(AnchorPosition position)
        {
            int halfH = Height / 2;
            int halfBottom = BottomWidth / 2;
            int halfTop = TopWidth / 2;

            return position switch
            {
                AnchorPosition.Center => new Point(0, 0),
                AnchorPosition.Top => new Point(0, -halfH),
                AnchorPosition.Bottom => new Point(0, halfH),
                AnchorPosition.Left => new Point(-halfBottom, 0),
                AnchorPosition.Right => new Point(halfBottom, 0),
                AnchorPosition.TopLeft => new Point(-halfTop, -halfH),
                AnchorPosition.TopRight => new Point(halfTop, -halfH),
                AnchorPosition.BottomLeft => new Point(-halfBottom, halfH),
                AnchorPosition.BottomRight => new Point(halfBottom, halfH),
                _ => AnchorOffset
            };
        }


        public override Point[] GetWorldPoints()
        {
            var center = GetCenter();
            int halfBottom = BottomWidth / 2;
            int halfTop = TopWidth / 2;
            int halfH = Height / 2;

            // Вершины трапеции:
            // верх-лево, верх-право, низ-право, низ-лево
            return new[]
            {
                new Point(center.X - halfTop, center.Y - halfH),  // 0 - верх-лево
                new Point(center.X + halfTop, center.Y - halfH),   // 1 - верх-право
                new Point(center.X + halfBottom, center.Y + halfH), // 2 - низ-право
                new Point(center.X - halfBottom, center.Y + halfH)  // 3 - низ-лево
            };
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
            // Проверяем попадание в трапецию через GraphicsPath
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
            // Минимальные размеры - 10 пикселей
            BottomWidth = Math.Max(10, (int)(BottomWidth * scaleFactor));
            TopWidth = Math.Max(10, (int)(TopWidth * scaleFactor));
            Height = Math.Max(10, (int)(Height * scaleFactor));
            
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
            var pts = GetWorldPoints();
            if (sideIndex < 0 || sideIndex >= 4) return 0;
            
            int next = (sideIndex + 1) % 4;
            float dx = pts[next].X - pts[sideIndex].X;
            float dy = pts[next].Y - pts[sideIndex].Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        public override void SetSideLength(int sideIndex, float length)
        {
            if (length <= 0) return;

            // Стороны: 0 - верх, 1 - право, 2 - низ, 3 - лево
            // Меняем только выбранный размер, пропорции меняются
            switch (sideIndex)
            {
                case 0: // Верхняя сторона - изменяем только верхнее основание
                    TopWidth = Math.Max(10, (int)length);
                    break;
                case 2: // Нижняя сторона - изменяем только нижнее основание
                    BottomWidth = Math.Max(10, (int)length);
                    break;
                case 1: // Правая боковая сторона
                case 3: // Левая боковая сторона
                    // Для боковых сторон изменяем высоту
                    // Боковая сторона зависит от высоты и разницы оснований
                    // Упрощённо: меняем высоту, основания остаются
                    Height = Math.Max(10, (int)length);
                    break;
            }

            // Пересчитываем смещение точки привязки
            AnchorOffset = CalculateAnchorOffset(AnchorPos);
            var center = GetCenter();
            GlobalOrigin = new Point(center.X + AnchorOffset.X - LocalAnchor.X, 
                                     center.Y + AnchorOffset.Y - LocalAnchor.Y);
            UpdateVirtualBounds();
        }
    }
}
