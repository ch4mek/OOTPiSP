using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using OOTPiSP_LR1.Shapes;

namespace OOTPiSP_LR1.Core
{
    public class ShapeManager
    {
        private ShapeBase[] _shapes = new ShapeBase[16];
        private int _count;

        public int ShapeCount => _count;

        public ShapeBase? SelectedShape { get; private set; }

        public List<ShapeBase> SelectedShapes { get; } = new();

        public ShapeBase GetShape(int index)
        {
            if (index < 0 || index >= _count)
                throw new ArgumentOutOfRangeException(nameof(index));
            return _shapes[index];
        }

        public IEnumerable<ShapeBase> GetAllShapes()
        {
            for (int i = 0; i < _count; i++)
                yield return _shapes[i];
        }

        private void EnsureCapacity(int minCapacity)
        {
            if (_shapes.Length < minCapacity)
            {
                int newCapacity = Math.Max(minCapacity, _shapes.Length * 2);
                var newArray = new ShapeBase[newCapacity];
                Array.Copy(_shapes, newArray, _count);
                _shapes = newArray;
            }
        }

        private int IndexOf(ShapeBase shape)
        {
            for (int i = 0; i < _count; i++)
            {
                if (ReferenceEquals(_shapes[i], shape))
                    return i;
            }
            return -1;
        }

        public void AddShape(ShapeBase shape)
        {
            EnsureCapacity(_count + 1);
            _shapes[_count++] = shape;
        }

        public void RemoveShape(ShapeBase shape)
        {
            int index = IndexOf(shape);
            if (index < 0) return;

            if (shape == SelectedShape)
                SelectedShape = null;

            for (int i = index; i < _count - 1; i++)
            {
                _shapes[i] = _shapes[i + 1];
            }
            _shapes[--_count] = null!;
        }

        private void InsertAt(int index, ShapeBase shape)
        {
            if (index < 0 || index > _count)
                throw new ArgumentOutOfRangeException(nameof(index));
            EnsureCapacity(_count + 1);
            for (int i = _count; i > index; i--)
            {
                _shapes[i] = _shapes[i - 1];
            }
            _shapes[index] = shape;
            _count++;
        }

        private void ClearAll()
        {
            Array.Clear(_shapes, 0, _count);
            _count = 0;
        }

        public void DrawAll(Graphics g)
        {
            for (int i = 0; i < _count; i++)
                _shapes[i].Draw(g);
        }

        public ShapeBase? HitTest(Point p)
        {
            for (int i = _count - 1; i >= 0; i--)
            {
                if (_shapes[i].HitTest(p))
                    return _shapes[i];
            }
            return null;
        }

        public void Select(ShapeBase? shape)
        {
            for (int i = 0; i < _count; i++)
                _shapes[i].IsSelected = false;

            SelectedShape = shape;

            if (shape != null)
                shape.IsSelected = true;
        }

        public void ClearSelection()
        {
            for (int i = 0; i < _count; i++)
                _shapes[i].IsSelected = false;
            SelectedShape = null;
            SelectedShapes.Clear();
        }

        public bool ToggleSelection(ShapeBase shape)
        {
            if (shape == null) return false;

            if (SelectedShapes.Contains(shape))
            {
                shape.IsSelected = false;
                SelectedShapes.Remove(shape);

                if (SelectedShape == shape)
                {
                    SelectedShape = SelectedShapes.Count > 0 ? SelectedShapes[0] : null;
                }
                return false;
            }
            else
            {
                shape.IsSelected = true;
                SelectedShapes.Add(shape);
                SelectedShape = shape;
                return true;
            }
        }

        public void AddToSelection(ShapeBase shape)
        {
            if (shape == null) return;
            if (!SelectedShapes.Contains(shape))
            {
                shape.IsSelected = true;
                SelectedShapes.Add(shape);
            }
            SelectedShape = shape;
        }

        public void SelectSingle(ShapeBase? shape)
        {
            for (int i = 0; i < _count; i++)
                _shapes[i].IsSelected = false;

            SelectedShapes.Clear();
            SelectedShape = shape;

            if (shape != null)
            {
                shape.IsSelected = true;
                SelectedShapes.Add(shape);
            }
        }

        public void BringToFront(ShapeBase shape)
        {
            int index = IndexOf(shape);
            if (index < 0) return;

            for (int i = index; i < _count - 1; i++)
            {
                _shapes[i] = _shapes[i + 1];
            }
            _shapes[_count - 1] = shape;
        }

        public void CreateInitialShapes(int canvasWidth, int canvasHeight, int margin = 50)
        {
            ClearAll();
            SelectedShape = null;
            SelectedShapes.Clear();

            int spacing = canvasWidth / 5;

            var circle = new CircleShape(
                new Point(margin + spacing / 2, margin + canvasHeight / 2),
                120
            )
            {
                FillColor = Color.LightBlue
            };
            circle.SetBorder(0, 15f, Color.DarkBlue);
            AddShape(circle);

            var rect = new RectangleShape(
                new Point(margin + spacing + spacing / 2, margin + canvasHeight / 2),
                240, 160
            )
            {
                FillColor = Color.LightGreen
            };
            rect.SetBorder(0, 8f, Color.DarkGreen);
            rect.SetBorder(1, 25f, Color.Blue);
            rect.SetBorder(2, 15f, Color.Red);
            rect.SetBorder(3, 35f, Color.Purple);
            AddShape(rect);

            var triangle = new TriangleShape(
                new Point(margin + 2 * spacing + spacing / 2, margin + canvasHeight / 2),
                140
            )
            {
                FillColor = Color.LightYellow
            };
            triangle.SetBorder(0, 10f, Color.Orange);
            triangle.SetBorder(1, 28f, Color.DarkBlue);
            triangle.SetBorder(2, 45f, Color.Crimson);
            AddShape(triangle);

            var hexagon = new HexagonShape(
                new Point(margin + 3 * spacing + spacing / 2, margin + canvasHeight / 2),
                120
            )
            {
                FillColor = Color.LightPink
            };
            hexagon.SetBorder(0, 8f, Color.Purple);
            hexagon.SetBorder(1, 18f, Color.Teal);
            hexagon.SetBorder(2, 30f, Color.Navy);
            hexagon.SetBorder(3, 12f, Color.Maroon);
            hexagon.SetBorder(4, 40f, Color.DarkGreen);
            hexagon.SetBorder(5, 22f, Color.SaddleBrown);
            AddShape(hexagon);

            var trapezoid = new TrapezoidShape(
                new Point(margin + 4 * spacing + spacing / 2, margin + canvasHeight / 2),
                280, 160, 160
            )
            {
                FillColor = Color.LightCoral
            };
            trapezoid.SetBorder(0, 12f, Color.DarkRed);
            trapezoid.SetBorder(1, 40f, Color.Blue);
            trapezoid.SetBorder(2, 20f, Color.DarkGreen);
            trapezoid.SetBorder(3, 6f, Color.Purple);
            AddShape(trapezoid);

            var pentagon = new PolygonShape(
                new Point(margin + 5 * spacing / 2, margin + canvasHeight / 2 + 200),
                new PointF(0, -80)
            )
            {
                FillColor = Color.Lavender,
                IsClosed = true
            };
            pentagon.AddSegmentByLengthAngle(95f, 54f);
            pentagon.AddSegmentByLengthAngle(95f, 72f);
            pentagon.AddSegmentByLengthAngle(95f, 72f);
            pentagon.AddSegmentByLengthAngle(95f, 72f);
            pentagon.AddSegmentByLengthAngle(95f, 72f);
            pentagon.SetBorder(0, 10f, Color.Indigo);
            pentagon.SetBorder(1, 15f, Color.Violet);
            pentagon.SetBorder(2, 8f, Color.Plum);
            pentagon.SetBorder(3, 20f, Color.MediumPurple);
            pentagon.SetBorder(4, 12f, Color.DarkViolet);
            AddShape(pentagon);
        }

        #region Операции группировки

        public List<ShapeBase> GetSelectedShapes()
        {
            return new List<ShapeBase>(SelectedShapes);
        }

        public GroupShape? GroupSelectedShapes()
        {
            var selectedShapes = GetSelectedShapes();

            if (selectedShapes.Count < 2)
                return null;

            var group = GroupShape.CreateFromShapes(selectedShapes);
            if (group == null)
                return null;

            foreach (var shape in selectedShapes)
            {
                RemoveShapeInternal(shape);
            }

            AddShape(group);

            ClearSelection();
            SelectSingle(group);

            return group;
        }

        public List<ShapeBase>? UngroupSelectedShape()
        {
            if (SelectedShape == null)
                return null;

            if (SelectedShape is not GroupShape group)
                return null;

            int groupIndex = IndexOf(group);

            var children = group.Ungroup();

            RemoveShapeInternal(group);

            if (groupIndex >= 0)
            {
                foreach (var child in children)
                {
                    InsertAt(groupIndex, child);
                    groupIndex++;
                }
            }
            else
            {
                foreach (var child in children)
                {
                    AddShape(child);
                }
            }

            ClearSelection();

            foreach (var child in children)
            {
                AddToSelection(child);
            }

            return children;
        }

        public bool IsSelectedShapeGroup()
        {
            return SelectedShape is GroupShape;
        }

        public bool AddShapeToGroup(ShapeBase shape, GroupShape group)
        {
            if (shape == null || group == null) return false;
            if (shape == group) return false;
            if (IndexOf(shape) < 0) return false;

            group.AddChild(shape);
            RemoveShapeInternal(shape);
            return true;
        }

        public ShapeBase? RemoveShapeFromGroup(ShapeBase child, GroupShape group)
        {
            if (child == null || group == null) return null;
            if (!group.RemoveChild(child)) return null;

            int groupIndex = IndexOf(group);
            if (groupIndex >= 0)
                InsertAt(groupIndex + 1, child);
            else
                AddShape(child);

            if (group.ChildCount == 0)
            {
                var children = group.Ungroup();
                RemoveShapeInternal(group);
            }

            return child;
        }

        private void RemoveShapeInternal(ShapeBase shape)
        {
            int index = IndexOf(shape);
            if (index < 0) return;

            for (int i = index; i < _count - 1; i++)
            {
                _shapes[i] = _shapes[i + 1];
            }
            _shapes[--_count] = null!;
        }

        #endregion

        #region Сохранение/Загрузка

        public void SaveToFile(string path)
        {
            SaveShapesToFile(path, GetAllShapes());
        }

        public void SaveShapesToFile(string path, IEnumerable<ShapeBase> shapes)
        {
            var root = new JsonObject();
            var shapesArray = new JsonArray();
            foreach (var shape in shapes)
            {
                shapesArray.Add(shape.Save());
            }
            root["shapes"] = shapesArray;

            var options = new JsonSerializerOptions { WriteIndented = true };
            options.TypeInfoResolver = new DefaultJsonTypeInfoResolver();
            var jsonString = root.ToJsonString(options);
            File.WriteAllText(path, jsonString);
        }

        public List<ShapeBase> AddFromFile(string path, Point targetCenter)
        {
            string text = File.ReadAllText(path);
            var node = JsonNode.Parse(text);
            if (node == null)
                throw new InvalidDataException("Файл пуст или содержит невалидный JSON");

            var json = node.AsObject();
            if (!json.ContainsKey("shapes"))
                throw new InvalidDataException("Файл не содержит данных фигр (отсутствует ключ 'shapes')");

            var shapesNode = json["shapes"];
            if (shapesNode == null)
                throw new InvalidDataException("Данные фигр отсутствуют");

            var shapesArray = shapesNode.AsArray();

            var tempShapes = new List<ShapeBase>();
            foreach (var shapeNode in shapesArray)
            {
                if (shapeNode != null)
                {
                    var shape = ShapeBase.CreateFromJson(shapeNode.AsObject());
                    shape.IsSelected = false;
                    shape.IsChildOfComposite = false;
                    tempShapes.Add(shape);
                }
            }

            int minX = int.MaxValue, minY = int.MaxValue;
            int maxX = int.MinValue, maxY = int.MinValue;
            foreach (var s in tempShapes)
            {
                var tl = s.GetVirtualTopLeft();
                var br = s.GetVirtualBottomRight();
                if (tl.X < minX) minX = tl.X;
                if (tl.Y < minY) minY = tl.Y;
                if (br.X > maxX) maxX = br.X;
                if (br.Y > maxY) maxY = br.Y;
            }

            int centerX = (minX + maxX) / 2;
            int centerY = (minY + maxY) / 2;
            int dx = targetCenter.X - centerX;
            int dy = targetCenter.Y - centerY;

            var loadedShapes = new List<ShapeBase>();
            foreach (var shape in tempShapes)
            {
                shape.MoveBy(dx, dy);
                AddShape(shape);
                loadedShapes.Add(shape);
            }

            ShapeBase.SyncNextId(GetAllShapes());
            return loadedShapes;
        }

        public void LoadFromFile(string path)
        {
            string text = File.ReadAllText(path);
            var node = JsonNode.Parse(text);
            if (node == null)
                throw new InvalidDataException("Файл пуст или содержит невалидный JSON");

            var json = node.AsObject();
            if (!json.ContainsKey("shapes"))
                throw new InvalidDataException("Файл не содержит данных фигур (отсутствует ключ 'shapes')");

            var shapesNode = json["shapes"];
            if (shapesNode == null)
                throw new InvalidDataException("Данные фигр отсутствуют");

            var shapesArray = shapesNode.AsArray();

            ClearAll();
            ClearSelection();

            foreach (var shapeNode in shapesArray)
            {
                if (shapeNode != null)
                {
                    var shape = ShapeBase.CreateFromJson(shapeNode.AsObject());
                    shape.IsSelected = false;
                    shape.IsChildOfComposite = false;
                    AddShape(shape);
                }
            }

            ShapeBase.SyncNextId(GetAllShapes());
        }

        #endregion
    }
}
