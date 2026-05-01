using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.Json.Nodes;

namespace OOTPiSP_LR1.Shapes
{
    public class EllipseShape : ShapeBase
    {
        public int RadiusX { get; set; }
        public int RadiusY { get; set; }

        public float EllipseBorderWidth
        {
            get => BorderWidths[0];
            set => BorderWidths[0] = value;
        }

        public Color EllipseBorderColor
        {
            get => BorderColors[0];
            set => BorderColors[0] = value;
        }

        public override int SideCount => 1;
        public override string DefaultTypeName => "Эллипс";

        public EllipseShape(Point anchor, int radiusX, int radiusY)
        {
            GlobalOrigin = anchor;
            LocalAnchor = Point.Empty;
            RadiusX = radiusX;
            RadiusY = radiusY;
            AnchorPos = AnchorPosition.Center;
            AnchorOffset = Point.Empty;
            UpdateVirtualBounds();
        }

        protected override Point CalculateAnchorOffset(AnchorPosition position)
        {
            return position switch
            {
                AnchorPosition.Center => new Point(0, 0),
                AnchorPosition.TopLeft => new Point(-RadiusX, -RadiusY),
                AnchorPosition.TopRight => new Point(RadiusX, -RadiusY),
                AnchorPosition.BottomLeft => new Point(-RadiusX, RadiusY),
                AnchorPosition.BottomRight => new Point(RadiusX, RadiusY),
                AnchorPosition.Top => new Point(0, -RadiusY),
                AnchorPosition.Bottom => new Point(0, RadiusY),
                AnchorPosition.Left => new Point(-RadiusX, 0),
                AnchorPosition.Right => new Point(RadiusX, 0),
                _ => AnchorOffset
            };
        }

        public override Point[] GetWorldPoints()
        {
            var center = GetCenter();
            var points = new Point[36];
            for (int i = 0; i < 36; i++)
            {
                double angle = i * Math.PI * 2 / 36;
                points[i] = new Point(
                    (int)(center.X + RadiusX * Math.Cos(angle)),
                    (int)(center.Y + RadiusY * Math.Sin(angle))
                );
            }
            return points;
        }

        protected override void UpdateVirtualBounds()
        {
            var center = GetCenter();
            float borderHalf = EllipseBorderWidth / 2f;
            VirtualBounds = new Rectangle(
                (int)Math.Floor(center.X - RadiusX - borderHalf),
                (int)Math.Floor(center.Y - RadiusY - borderHalf),
                (int)Math.Ceiling(RadiusX * 2 + borderHalf * 2),
                (int)Math.Ceiling(RadiusY * 2 + borderHalf * 2)
            );
        }

        public override void Draw(Graphics g)
        {
            var center = GetCenter();
            var bounds = new Rectangle(
                center.X - RadiusX,
                center.Y - RadiusY,
                RadiusX * 2,
                RadiusY * 2
            );

            using (var brush = new SolidBrush(FillColor))
            {
                g.FillEllipse(brush, bounds);
            }

            using (var pen = new Pen(EllipseBorderColor, EllipseBorderWidth))
            {
                g.DrawEllipse(pen, bounds);
            }

            DrawVirtualBounds(g);
        }

        public override bool HitTest(Point p)
        {
            var center = GetCenter();
            if (RadiusX == 0 || RadiusY == 0) return false;
            double dx = (double)(p.X - center.X) / RadiusX;
            double dy = (double)(p.Y - center.Y) / RadiusY;
            return dx * dx + dy * dy <= 1.0;
        }

        public override void SetAnchorPosition(AnchorPosition position)
        {
            base.SetAnchorPosition(position);
            UpdateVirtualBounds();
        }

        public override void Resize(float scaleFactor)
        {
            RadiusX = Math.Max(10, (int)(RadiusX * scaleFactor));
            RadiusY = Math.Max(10, (int)(RadiusY * scaleFactor));

            AnchorOffset = CalculateAnchorOffset(AnchorPos);
            var center = GetCenter();
            GlobalOrigin = new Point(center.X + AnchorOffset.X - LocalAnchor.X,
                                     center.Y + AnchorOffset.Y - LocalAnchor.Y);
            UpdateVirtualBounds();
        }

        public override void ResizeSide(int sideIndex, float scaleFactor)
        {
            Resize(scaleFactor);
        }

        public override float GetSideLength(int sideIndex)
        {
            return (float)(Math.PI * (3 * (RadiusX + RadiusY) - Math.Sqrt((3 * RadiusX + RadiusY) * (RadiusX + 3 * RadiusY))));
        }

        public override void SetSideLength(int sideIndex, float length)
        {
            if (length > 0)
            {
                float ratio = RadiusY > 0 ? (float)RadiusX / RadiusY : 1f;
                double approxPerimeter = Math.PI * (3 * (RadiusX + RadiusY) - Math.Sqrt((3 * RadiusX + RadiusY) * (RadiusX + 3 * RadiusY)));
                double scale = length / approxPerimeter;
                RadiusX = Math.Max(10, (int)(RadiusX * scale));
                RadiusY = Math.Max(10, (int)(RadiusY * scale));

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
            json["radiusX"] = RadiusX;
            json["radiusY"] = RadiusY;
            return json;
        }

        public static EllipseShape LoadFromJson(JsonObject json)
        {
            var shape = new EllipseShape(
                Point.Empty,
                json["radiusX"]!.GetValue<int>(),
                json["radiusY"]!.GetValue<int>()
            );
            shape.LoadCommon(json);
            return shape;
        }
    }
}
