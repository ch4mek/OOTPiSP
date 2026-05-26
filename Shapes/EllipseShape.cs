using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.Json.Nodes;

namespace OOTPiSP_LR1.Shapes
{
    public class EllipseShape : ShapeBase
    {
        private int _semiMajor;
        private int _semiMinor;
        private float _rotationDegrees;

        public int SemiMajor
        {
            get => _semiMajor;
            set => _semiMajor = Math.Max(10, value);
        }

        public int SemiMinor
        {
            get => _semiMinor;
            set => _semiMinor = Math.Max(10, value);
        }

        public float RotationDegrees
        {
            get => _rotationDegrees;
            set => _rotationDegrees = value % 360f;
        }

        public double FocalDistance
        {
            get
            {
                double a = SemiMajor;
                double b = SemiMinor;
                return Math.Sqrt(Math.Max(0, a * a - b * b));
            }
            set
            {
                double c = Math.Max(0, value);
                if (c >= SemiMajor)
                    c = SemiMajor - 1;
                double b = Math.Sqrt(Math.Max(0, SemiMajor * SemiMajor - c * c));
                SemiMinor = Math.Max(10, (int)Math.Round(b));
            }
        }

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

        public EllipseShape(Point anchor, int semiMajor, int semiMinor)
        {
            GlobalOrigin = anchor;
            LocalAnchor = Point.Empty;
            _semiMajor = Math.Max(10, semiMajor);
            _semiMinor = Math.Max(10, semiMinor);
            if (_semiMinor > _semiMajor)
                (_semiMajor, _semiMinor) = (_semiMinor, _semiMajor);
            _rotationDegrees = 0;
            AnchorPos = AnchorPosition.Center;
            AnchorOffset = Point.Empty;
            UpdateVirtualBounds();
        }

        protected override Point CalculateAnchorOffset(AnchorPosition position)
        {
            return position switch
            {
                AnchorPosition.Center => new Point(0, 0),
                AnchorPosition.TopLeft => new Point(-SemiMajor, -SemiMinor),
                AnchorPosition.TopRight => new Point(SemiMajor, -SemiMinor),
                AnchorPosition.BottomLeft => new Point(-SemiMajor, SemiMinor),
                AnchorPosition.BottomRight => new Point(SemiMajor, SemiMinor),
                AnchorPosition.Top => new Point(0, -SemiMinor),
                AnchorPosition.Bottom => new Point(0, SemiMinor),
                AnchorPosition.Left => new Point(-SemiMajor, 0),
                AnchorPosition.Right => new Point(SemiMajor, 0),
                _ => AnchorOffset
            };
        }

        public PointF GetFocus1()
        {
            var center = GetCenter();
            double rad = RotationDegrees * Math.PI / 180.0;
            double c = FocalDistance;
            return new PointF(
                (float)(center.X + c * Math.Cos(rad)),
                (float)(center.Y + c * Math.Sin(rad))
            );
        }

        public PointF GetFocus2()
        {
            var center = GetCenter();
            double rad = RotationDegrees * Math.PI / 180.0;
            double c = FocalDistance;
            return new PointF(
                (float)(center.X - c * Math.Cos(rad)),
                (float)(center.Y - c * Math.Sin(rad))
            );
        }

        public override Point[] GetWorldPoints()
        {
            var center = GetCenter();
            double rad = RotationDegrees * Math.PI / 180.0;
            double cosA = Math.Cos(rad);
            double sinA = Math.Sin(rad);
            var points = new Point[72];
            for (int i = 0; i < 72; i++)
            {
                double angle = i * Math.PI * 2 / 72;
                double lx = SemiMajor * Math.Cos(angle);
                double ly = SemiMinor * Math.Sin(angle);
                double wx = lx * cosA - ly * sinA + center.X;
                double wy = lx * sinA + ly * cosA + center.Y;
                points[i] = new Point((int)Math.Round(wx), (int)Math.Round(wy));
            }
            return points;
        }

        protected override void UpdateVirtualBounds()
        {
            var center = GetCenter();
            double rad = RotationDegrees * Math.PI / 180.0;
            double cosA = Math.Abs(Math.Cos(rad));
            double sinA = Math.Abs(Math.Sin(rad));
            double a = SemiMajor;
            double b = SemiMinor;
            double halfW = Math.Sqrt(a * a * cosA * cosA + b * b * sinA * sinA);
            double halfH = Math.Sqrt(a * a * sinA * sinA + b * b * cosA * cosA);
            float borderHalf = EllipseBorderWidth / 2f;
            VirtualBounds = new Rectangle(
                (int)Math.Floor(center.X - halfW - borderHalf),
                (int)Math.Floor(center.Y - halfH - borderHalf),
                (int)Math.Ceiling(halfW * 2 + borderHalf * 2),
                (int)Math.Ceiling(halfH * 2 + borderHalf * 2)
            );
        }

        public override void Draw(Graphics g)
        {
            var center = GetCenter();
            double rad = RotationDegrees * Math.PI / 180.0;

            g.TranslateTransform(center.X, center.Y);
            g.RotateTransform((float)RotationDegrees);

            var bounds = new Rectangle(-SemiMajor, -SemiMinor, SemiMajor * 2, SemiMinor * 2);

            using (var brush = new SolidBrush(FillColor))
            {
                g.FillEllipse(brush, bounds);
            }

            using (var pen = new Pen(EllipseBorderColor, EllipseBorderWidth))
            {
                g.DrawEllipse(pen, bounds);
            }

            g.ResetTransform();

            DrawFoci(g);
            DrawVirtualBounds(g);
        }

        private void DrawFoci(Graphics g)
        {
            var f1 = GetFocus1();
            var f2 = GetFocus2();
            float r = 5f;

            using (var brush = new SolidBrush(Color.Red))
            {
                g.FillEllipse(brush, f1.X - r, f1.Y - r, r * 2, r * 2);
                g.FillEllipse(brush, f2.X - r, f2.Y - r, r * 2, r * 2);
            }

            using (var pen = new Pen(Color.DarkRed, 1.5f))
            {
                g.DrawEllipse(pen, f1.X - r, f1.Y - r, r * 2, r * 2);
                g.DrawEllipse(pen, f2.X - r, f2.Y - r, r * 2, r * 2);
            }
        }

        public override bool HitTest(Point p)
        {
            var center = GetCenter();
            double rad = -RotationDegrees * Math.PI / 180.0;
            double cosA = Math.Cos(rad);
            double sinA = Math.Sin(rad);
            double dx = p.X - center.X;
            double dy = p.Y - center.Y;
            double lx = dx * cosA - dy * sinA;
            double ly = dx * sinA + dy * cosA;
            if (SemiMajor == 0 || SemiMinor == 0) return false;
            double nx = lx / SemiMajor;
            double ny = ly / SemiMinor;
            return nx * nx + ny * ny <= 1.0;
        }

        public void SetFromFoci(PointF f1, PointF f2)
        {
            double newCenterX = (f1.X + f2.X) / 2.0;
            double newCenterY = (f1.Y + f2.Y) / 2.0;

            double dx = f1.X - f2.X;
            double dy = f1.Y - f2.Y;
            double newC = Math.Sqrt(dx * dx + dy * dy) / 2.0;

            double newRotation = Math.Atan2(f1.Y - newCenterY, f1.X - newCenterX) * 180.0 / Math.PI;

            if (newC >= SemiMajor)
                SemiMajor = (int)Math.Ceiling(newC) + 1;

            RotationDegrees = (float)newRotation;

            double newB = Math.Sqrt(Math.Max(0, (double)SemiMajor * SemiMajor - newC * newC));
            SemiMinor = Math.Max(10, (int)Math.Round(newB));

            AnchorOffset = CalculateAnchorOffset(AnchorPos);
            GlobalOrigin = new Point(
                (int)Math.Round(newCenterX) + AnchorOffset.X - LocalAnchor.X,
                (int)Math.Round(newCenterY) + AnchorOffset.Y - LocalAnchor.Y);
            UpdateVirtualBounds();
        }

        public override void SetAnchorPosition(AnchorPosition position)
        {
            var center = GetCenter();
            AnchorPos = position;
            AnchorOffset = CalculateAnchorOffset(position);
            GlobalOrigin = new Point(center.X + AnchorOffset.X - LocalAnchor.X,
                                     center.Y + AnchorOffset.Y - LocalAnchor.Y);
            UpdateVirtualBounds();
        }

        public override void Resize(float scaleFactor)
        {
            var center = GetCenter();

            SemiMajor = Math.Max(10, (int)(SemiMajor * scaleFactor));
            SemiMinor = Math.Max(10, (int)(SemiMinor * scaleFactor));

            AnchorOffset = CalculateAnchorOffset(AnchorPos);
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
            double a = SemiMajor;
            double b = SemiMinor;
            return (float)(Math.PI * (3 * (a + b) - Math.Sqrt((3 * a + b) * (a + 3 * b))));
        }

        public override void SetSideLength(int sideIndex, float length)
        {
            if (length > 0)
            {
                var center = GetCenter();

                double a = SemiMajor;
                double b = SemiMinor;
                double approxPerimeter = Math.PI * (3 * (a + b) - Math.Sqrt((3 * a + b) * (a + 3 * b)));
                double scale = length / approxPerimeter;
                SemiMajor = Math.Max(10, (int)(a * scale));
                SemiMinor = Math.Max(10, (int)(b * scale));

                AnchorOffset = CalculateAnchorOffset(AnchorPos);
                GlobalOrigin = new Point(center.X + AnchorOffset.X - LocalAnchor.X,
                                         center.Y + AnchorOffset.Y - LocalAnchor.Y);
                UpdateVirtualBounds();
            }
        }

        public override JsonObject Save()
        {
            var json = base.Save();
            json["semiMajor"] = SemiMajor;
            json["semiMinor"] = SemiMinor;
            json["rotation"] = RotationDegrees;
            return json;
        }

        public static EllipseShape LoadFromJson(JsonObject json)
        {
            var shape = new EllipseShape(
                Point.Empty,
                json["semiMajor"]!.GetValue<int>(),
                json["semiMinor"]!.GetValue<int>()
            );
            shape.LoadCommon(json);
            if (json.ContainsKey("rotation"))
                shape.RotationDegrees = json["rotation"]!.GetValue<float>();
            return shape;
        }
    }
}
