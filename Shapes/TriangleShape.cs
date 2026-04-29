using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.Json.Nodes;

namespace OOTPiSP_LR1.Shapes
{
    /// <summary>
    /// Равносторонний треугольник - 3 стороны, каждая со своей толщиной и цветом
    /// </summary>
    public class TriangleShape : ShapeBase
    {
        // Радиус описанной окружности вокруг треугольника
        public int Radius { get; set; }

        public override int SideCount => 3;
        public override string DefaultTypeName => "Треугольник";

        public TriangleShape(Point anchor, int radius)
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
            // Высота равностороннего треугольника
            double height = Radius * 1.5; // Приблизительно

            return position switch
            {
                AnchorPosition.Center => new Point(0, 0),
                AnchorPosition.Top => new Point(0, -(int)(Radius * 0.866)), // Вершина
                AnchorPosition.Bottom => new Point(0, (int)(Radius * 0.433)), // Центр основания
                AnchorPosition.TopLeft => new Point(-Radius / 2, (int)(Radius * 0.433)),
                AnchorPosition.TopRight => new Point(Radius / 2, (int)(Radius * 0.433)),
                AnchorPosition.BottomLeft => new Point(-Radius / 2, (int)(Radius * 0.433)),
                AnchorPosition.BottomRight => new Point(Radius / 2, (int)(Radius * 0.433)),
                AnchorPosition.Left => new Point(-Radius / 2, (int)(Radius * 0.433)),
                AnchorPosition.Right => new Point(Radius / 2, (int)(Radius * 0.433)),
                _ => AnchorOffset
            };
        }


        public override Point[] GetWorldPoints()
        {
            var center = GetCenter();
            var points = new Point[3];

            // Равносторонний треугольник, вершины на окружности
            // Старт сверху (угол -90 градусов)
            for (int i = 0; i < 3; i++)
            {
                double angle = -Math.PI / 2 + i * 2 * Math.PI / 3;
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
            // Проверяем попадание в треугольник через барицентрические координаты
            var pts = GetWorldPoints();

            float Area(Point a, Point b, Point c)
            {
                return Math.Abs((b.X - a.X) * (c.Y - a.Y) - (c.X - a.X) * (b.Y - a.Y)) / 2f;
            }

            float area = Area(pts[0], pts[1], pts[2]);
            float area1 = Area(p, pts[1], pts[2]);
            float area2 = Area(pts[0], p, pts[2]);
            float area3 = Area(pts[0], pts[1], p);

            return Math.Abs(area - (area1 + area2 + area3)) < 0.1f;
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
            // Длина стороны равностороннего треугольника = Radius * sqrt(3)
            return (float)(Radius * Math.Sqrt(3));
        }

        public override void SetSideLength(int sideIndex, float length)
        {
            if (length <= 0) return;

            // Длина стороны равностороннего треугольника = Radius * sqrt(3)
            // => Radius = length / sqrt(3)
            Radius = Math.Max(10, (int)(length / Math.Sqrt(3)));

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
            json["radius"] = Radius;
            return json;
        }

        public static TriangleShape LoadFromJson(JsonObject json)
        {
            var shape = new TriangleShape(Point.Empty, json["radius"]!.GetValue<int>());
            shape.LoadCommon(json);
            return shape;
        }
    }
}
