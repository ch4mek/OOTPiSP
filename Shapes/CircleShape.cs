using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.Json.Nodes;

namespace OOTPiSP_LR1.Shapes
{
    /// <summary>
    /// Окружность - особая фигура с одной толщиной и цветом линии
    /// </summary>
    public class CircleShape : ShapeBase
    {
        // Радиус окружности
        public int Radius { get; set; }

        // Толщина линии (единая для всей окружности)
        public float CircleBorderWidth
        {
            get => BorderWidths[0];
            set => BorderWidths[0] = value;
        }

        // Цвет линии (единый для всей окружности)
        public Color CircleBorderColor
        {
            get => BorderColors[0];
            set => BorderColors[0] = value;
        }

        // У окружности только одна "сторона"
        public override int SideCount => 1;
        public override string DefaultTypeName => "Окружность";

        public CircleShape(Point anchor, int radius)
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
                AnchorPosition.TopLeft => new Point(-Radius, -Radius),
                AnchorPosition.TopRight => new Point(Radius, -Radius),
                AnchorPosition.BottomLeft => new Point(-Radius, Radius),
                AnchorPosition.BottomRight => new Point(Radius, Radius),
                AnchorPosition.Top => new Point(0, -Radius),
                AnchorPosition.Bottom => new Point(0, Radius),
                AnchorPosition.Left => new Point(-Radius, 0),
                AnchorPosition.Right => new Point(Radius, 0),
                _ => AnchorOffset
            };
        }


        public override Point[] GetWorldPoints()
        {
            // Для окружности возвращаем точки на границе (для совместимости)
            var center = GetCenter();
            var points = new Point[36];
            for (int i = 0; i < 36; i++)
            {
                double angle = i * Math.PI * 2 / 36;
                points[i] = new Point(
                    (int)(center.X + Radius * Math.Cos(angle)),
                    (int)(center.Y + Radius * Math.Sin(angle))
                );
            }
            return points;
        }

        protected override void UpdateVirtualBounds()
        {
            var center = GetCenter();
            // Добавляем отступ на половину толщины линии
            float borderHalf = CircleBorderWidth / 2f;
            VirtualBounds = new Rectangle(
                (int)Math.Floor(center.X - Radius - borderHalf),
                (int)Math.Floor(center.Y - Radius - borderHalf),
                (int)Math.Ceiling(Radius * 2 + borderHalf * 2),
                (int)Math.Ceiling(Radius * 2 + borderHalf * 2)
            );
        }

        public override void Draw(Graphics g)
        {
            var center = GetCenter();
            var bounds = new Rectangle(
                center.X - Radius,
                center.Y - Radius,
                Radius * 2,
                Radius * 2
            );

            // Заливка
            using (var brush = new SolidBrush(FillColor))
            {
                g.FillEllipse(brush, bounds);
            }

            // Обводка (единая для всей окружности)
            using (var pen = new Pen(CircleBorderColor, CircleBorderWidth))
            {
                g.DrawEllipse(pen, bounds);
            }

            // Виртуальные границы для выбранной фигуры
            DrawVirtualBounds(g);
        }

        public override bool HitTest(Point p)
        {
            var center = GetCenter();
            int dx = p.X - center.X;
            int dy = p.Y - center.Y;
            return dx * dx + dy * dy <= Radius * Radius;
        }

        public override void SetAnchorPosition(AnchorPosition position)
        {
            base.SetAnchorPosition(position);
            UpdateVirtualBounds();
        }

        public override void Resize(float scaleFactor)
        {
            // Минимальный радиус - 10 пикселей
            int newRadius = Math.Max(10, (int)(Radius * scaleFactor));
            Radius = newRadius;
            
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
            // Для окружности просто меняем радиус
            Resize(scaleFactor);
        }

        public override float GetSideLength(int sideIndex)
        {
            // Длина окружности = 2 * PI * Radius
            return (float)(2 * Math.PI * Radius);
        }

        public override void SetSideLength(int sideIndex, float length)
        {
            if (length > 0)
            {
                Radius = Math.Max(10, (int)(length / (2 * Math.PI)));
                
                AnchorOffset = CalculateAnchorOffset(AnchorPos);
                var center = GetCenter();
                GlobalOrigin = new Point(center.X + AnchorOffset.X - LocalAnchor.X, 
                                         center.Y + AnchorOffset.Y - LocalAnchor.Y);
                UpdateVirtualBounds();
            }
        }

        public override JsonObject Save()
        {
            var json = base.Save();
            json["radius"] = Radius;
            return json;
        }

        public static CircleShape LoadFromJson(JsonObject json)
        {
            var shape = new CircleShape(Point.Empty, json["radius"]!.GetValue<int>());
            shape.LoadCommon(json);
            return shape;
        }
    }
}
