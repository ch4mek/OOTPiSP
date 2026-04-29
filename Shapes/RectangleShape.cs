using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.Json.Nodes;

namespace OOTPiSP_LR1.Shapes
{
    /// <summary>
    /// Прямоугольник - 4 стороны, каждая со своей толщиной и цветом
    /// </summary>
    public class RectangleShape : ShapeBase
    {
        // Ширина прямоугольника
        public int Width { get; set; }

        // Высота прямоугольника
        public int Height { get; set; }

        public override int SideCount => 4;
        public override string DefaultTypeName => "Прямоугольник";

        public RectangleShape(Point anchor, int width, int height)
        {
            GlobalOrigin = anchor;
            LocalAnchor = Point.Empty;
            Width = width;
            Height = height;
            AnchorPos = AnchorPosition.Center;
            AnchorOffset = Point.Empty;
            UpdateVirtualBounds();
        }

        protected override Point CalculateAnchorOffset(AnchorPosition position)
        {
            int halfW = Width / 2;
            int halfH = Height / 2;

            return position switch
            {
                AnchorPosition.Center => new Point(0, 0),
                AnchorPosition.TopLeft => new Point(-halfW, -halfH),
                AnchorPosition.TopRight => new Point(halfW, -halfH),
                AnchorPosition.BottomLeft => new Point(-halfW, halfH),
                AnchorPosition.BottomRight => new Point(halfW, halfH),
                AnchorPosition.Top => new Point(0, -halfH),
                AnchorPosition.Bottom => new Point(0, halfH),
                AnchorPosition.Left => new Point(-halfW, 0),
                AnchorPosition.Right => new Point(halfW, 0),
                _ => AnchorOffset
            };
        }


        public override Point[] GetWorldPoints()
        {
            var center = GetCenter();
            int halfW = Width / 2;
            int halfH = Height / 2;

            // Вершины прямоугольника: верх-лево, верх-право, низ-право, низ-лево
            return new[]
            {
                new Point(center.X - halfW, center.Y - halfH), // 0 - верх-лево
                new Point(center.X + halfW, center.Y - halfH), // 1 - верх-право
                new Point(center.X + halfW, center.Y + halfH), // 2 - низ-право
                new Point(center.X - halfW, center.Y + halfH)  // 3 - низ-лево
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
        }


        public override bool HitTest(Point p)
        {
            // Проверяем попадание в прямоугольник
            var center = GetCenter();
            int halfW = Width / 2;
            int halfH = Height / 2;

            return p.X >= center.X - halfW && p.X <= center.X + halfW &&
                   p.Y >= center.Y - halfH && p.Y <= center.Y + halfH;
        }

        public override void SetAnchorPosition(AnchorPosition position)
        {
            base.SetAnchorPosition(position);
            UpdateVirtualBounds();
        }

        public override void Resize(float scaleFactor)
        {
            // Минимальные размеры - 10 пикселей
            Width = Math.Max(10, (int)(Width * scaleFactor));
            Height = Math.Max(10, (int)(Height * scaleFactor));
            
            // Пересчитываем смещение точки привязки для текущей позиции
            AnchorOffset = CalculateAnchorOffset(AnchorPos);
            
            // Обновляем GlobalOrigin чтобы сохранить позицию
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
            return sideIndex switch
            {
                0 or 2 => Width,  // Верх и низ - ширина
                1 or 3 => Height, // Право и лево - высота
                _ => 0
            };
        }

        public override void SetSideLength(int sideIndex, float length)
        {
            if (length <= 0) return;

            // Стороны: 0 - верх, 1 - право, 2 - низ, 3 - лево
            // Верх и низ - ширина, право и лево - высота
            // Меняем только выбранную сторону, пропорции меняются
            switch (sideIndex)
            {
                case 0: // Верхняя сторона - изменяем только ширину
                case 2: // Нижняя сторона - изменяем только ширину
                    Width = Math.Max(10, (int)length);
                    break;
                case 1: // Правая сторона - изменяем только высоту
                case 3: // Левая сторона - изменяем только высоту
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

        public override JsonObject Save()
        {
            var json = base.Save();
            json["width"] = Width;
            json["height"] = Height;
            return json;
        }

        public static RectangleShape LoadFromJson(JsonObject json)
        {
            var shape = new RectangleShape(Point.Empty, json["width"]!.GetValue<int>(), json["height"]!.GetValue<int>());
            shape.LoadCommon(json);
            return shape;
        }
    }
}
