using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.Json.Nodes;

namespace OOTPiSP_LR1.Shapes
{
    /// <summary>
    /// Группа фигур, которые двигаются и трансформируются как единое целое,
    /// но сохраняют свою форму и относительное расположение.
    /// Группу можно разгруппировать, восстановив исходные фигуры.
    /// </summary>
    public class GroupShape : ShapeBase
    {
        /// <summary>
        /// Список дочерних фигур в группе
        /// </summary>
        private readonly List<ShapeBase> _children = new();

        /// <summary>
        /// Количество дочерних фигур в группе
        /// </summary>
        public int ChildCount => _children.Count;

        /// <summary>
        /// Признак того, что фигура является группой
        /// </summary>
        public bool IsGroup => true;

        /// <summary>
        /// Количество сторон — сумма сторон всех дочерних фигур
        /// </summary>
        public override int SideCount => _children.Count > 0 ? _children.Sum(s => s.SideCount) : 0;
        public override string DefaultTypeName => "Группа";

        /// <summary>
        /// Создать пустую группу
        /// </summary>
        public GroupShape()
        {
            AnchorPos = AnchorPosition.Center;
            AnchorOffset = Point.Empty;
        }

        /// <summary>
        /// Создать группу из списка фигур
        /// </summary>
        private GroupShape(IEnumerable<ShapeBase> shapes) : this()
        {
            foreach (var shape in shapes)
            {
                AddChild(shape);
            }
        }

        #region Статический фабричный метод

        /// <summary>
        /// Создать группу из списка фигур.
        /// Фигуры сохраняют свою форму, заливку и стили границ.
        /// </summary>
        /// <param name="shapes">Список фигур для группировки</param>
        /// <returns>Созданная группа или null, если список пуст</returns>
        public static GroupShape? CreateFromShapes(IEnumerable<ShapeBase> shapes)
        {
            if (shapes == null) return null;

            var shapeList = shapes.ToList();
            if (shapeList.Count == 0) return null;

            var group = new GroupShape(shapeList);
            
            // Установить GlobalOrigin в центр bounding box'а дочерних фигур,
            // чтобы точка привязки не улетала в левый верхний угол
            var bounds = group.GetGroupBounds();
            if (!bounds.IsEmpty)
            {
                group.GlobalOrigin = new Point(
                    bounds.X + bounds.Width / 2,
                    bounds.Y + bounds.Height / 2
                );
            }
            
            group.UpdateVirtualBounds();

            return group;
        }

        #endregion

        #region Управление дочерними фигурами

        /// <summary>
        /// Добавить фигуру в группу
        /// </summary>
        /// <param name="shape">Фигура для добавления</param>
        public void AddChild(ShapeBase shape)
        {
            if (shape == null) return;
            if (shape == this) return; // Предотвращаем рекурсию

            shape.IsChildOfComposite = true; // Скрываем точку привязки для дочерней фигуры
            _children.Add(shape);
            UpdateVirtualBounds();
        }

        /// <summary>
        /// Удалить фигуру из группы
        /// </summary>
        /// <param name="shape">Фигура для удаления</param>
        /// <returns>True если фигура была удалена</returns>
        public bool RemoveChild(ShapeBase shape)
        {
            if (shape == null) return false;

            bool removed = _children.Remove(shape);
            if (removed)
            {
                shape.IsChildOfComposite = false; // Восстанавливаем отображение точки привязки
                UpdateVirtualBounds();
            }
            return removed;
        }

        /// <summary>
        /// Получить список дочерних фигур (только для чтения)
        /// </summary>
        /// <returns>Список дочерних фигур</returns>
        public IReadOnlyList<ShapeBase> GetChildren()
        {
            return _children.AsReadOnly();
        }

        /// <summary>
        /// Получить дочернюю фигуру по индексу
        /// </summary>
        /// <param name="index">Индекс фигуры</param>
        /// <returns>Фигура или null, если индекс вне диапазона</returns>
        public ShapeBase? GetChild(int index)
        {
            if (index >= 0 && index < _children.Count)
                return _children[index];
            return null;
        }

        #endregion

        #region Операции группы

        /// <summary>
        /// Разгруппировать — вернуть все дочерние фигуры.
        /// После вызова группа становится пустой.
        /// </summary>
        /// <returns>Список фигур для добавления в ShapeManager</returns>
        public List<ShapeBase> Ungroup()
        {
            var children = new List<ShapeBase>(_children);

            // Восстанавливаем отображение точки привязки для всех фигур
            foreach (var child in _children)
            {
                child.IsChildOfComposite = false;
                child.IsSelected = false;
            }

            _children.Clear();
            UpdateVirtualBounds();

            return children;
        }

        #endregion

        #region Переопределение методов ShapeBase

        protected override Point CalculateAnchorOffset(AnchorPosition position)
        {
            if (_children.Count == 0)
                return Point.Empty;

            var bounds = GetGroupBounds();
            int centerX = bounds.X + bounds.Width / 2;
            int centerY = bounds.Y + bounds.Height / 2;

            return position switch
            {
                AnchorPosition.Center => new Point(0, 0),
                AnchorPosition.TopLeft => new Point(bounds.X - centerX, bounds.Y - centerY),
                AnchorPosition.TopRight => new Point(bounds.Right - centerX, bounds.Y - centerY),
                AnchorPosition.BottomLeft => new Point(bounds.X - centerX, bounds.Bottom - centerY),
                AnchorPosition.BottomRight => new Point(bounds.Right - centerX, bounds.Bottom - centerY),
                AnchorPosition.Top => new Point(centerX - centerX, bounds.Y - centerY),
                AnchorPosition.Bottom => new Point(centerX - centerX, bounds.Bottom - centerY),
                AnchorPosition.Left => new Point(bounds.X - centerX, centerY - centerY),
                AnchorPosition.Right => new Point(bounds.Right - centerX, centerY - centerY),
                _ => Point.Empty
            };
        }

        /// <summary>
        /// Получить границы всех дочерних фигур
        /// </summary>
        private Rectangle GetGroupBounds()
        {
            if (_children.Count == 0)
                return Rectangle.Empty;

            int minX = int.MaxValue, minY = int.MaxValue;
            int maxX = int.MinValue, maxY = int.MinValue;

            foreach (var child in _children)
            {
                var bounds = child.VirtualBounds;
                if (bounds.X < minX) minX = bounds.X;
                if (bounds.Y < minY) minY = bounds.Y;
                if (bounds.Right > maxX) maxX = bounds.Right;
                if (bounds.Bottom > maxY) maxY = bounds.Bottom;
            }

            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }

        public override Point[] GetWorldPoints()
        {
            // Возвращаем все точки всех дочерних фигур
            var allPoints = new List<Point>();
            foreach (var child in _children)
            {
                allPoints.AddRange(child.GetWorldPoints());
            }
            return allPoints.ToArray();
        }

        protected override void UpdateVirtualBounds()
        {
            if (_children.Count == 0)
            {
                VirtualBounds = Rectangle.Empty;
                return;
            }

            // Границы охватывают все дочерние фигуры
            VirtualBounds = GetGroupBounds();

            // Обновляем позицию группы на основе центра границ
            var bounds = VirtualBounds;
            int centerX = bounds.X + bounds.Width / 2;
            int centerY = bounds.Y + bounds.Height / 2;
            LocalAnchor = new Point(centerX - GlobalOrigin.X, centerY - GlobalOrigin.Y);
        }

        public override void Draw(Graphics g)
        {
            // Рисуем все дочерние фигуры
            foreach (var child in _children)
            {
                child.Draw(g);
            }

            // Виртуальные границы для выбранной группы
            DrawVirtualBounds(g);
        }

        public override bool HitTest(Point p)
        {
            // Проверяем попадание в любую дочернюю фигуру
            foreach (var child in _children)
            {
                if (child.HitTest(p))
                    return true;
            }
            return false;
        }

        public ShapeBase? HitTestChild(Point p)
        {
            for (int i = _children.Count - 1; i >= 0; i--)
            {
                if (_children[i].HitTest(p))
                    return _children[i];
            }
            return null;
        }

        public override void Resize(float scaleFactor)
        {
            if (_children.Count == 0) return;

            // Получаем центр группы
            var bounds = GetGroupBounds();
            float centerX = bounds.X + bounds.Width / 2f;
            float centerY = bounds.Y + bounds.Height / 2f;

            // Масштабируем все дочерние фигуры относительно центра группы
            foreach (var child in _children)
            {
                // Получаем центр фигуры
                var childCenter = child.GetCenter();

                // Вычисляем вектор от центра группы до центра фигуры
                float dx = childCenter.X - centerX;
                float dy = childCenter.Y - centerY;

                // Масштабируем фигуру
                child.Resize(scaleFactor);

                // Перемещаем фигуру на новое место (масштабированный вектор)
                int newDx = (int)(dx * scaleFactor);
                int newDy = (int)(dy * scaleFactor);
                int moveDx = newDx - (int)dx;
                int moveDy = newDy - (int)dy;
                child.MoveBy(moveDx, moveDy);
            }

            UpdateVirtualBounds();
        }

        public override void ResizeSide(int sideIndex, float scaleFactor)
        {
            if (scaleFactor <= 0 || _children.Count == 0) return;

            // Находим соответствующую дочернюю фигуру и сторону
            int currentIndex = 0;
            foreach (var child in _children)
            {
                if (sideIndex < currentIndex + child.SideCount)
                {
                    child.ResizeSide(sideIndex - currentIndex, scaleFactor);
                    UpdateVirtualBounds();
                    return;
                }
                currentIndex += child.SideCount;
            }
        }

        public override float GetSideLength(int sideIndex)
        {
            if (_children.Count == 0) return 0;

            // Находим соответствующую дочернюю фигуру и возвращаем длину её стороны
            int currentIndex = 0;
            foreach (var child in _children)
            {
                if (sideIndex < currentIndex + child.SideCount)
                {
                    return child.GetSideLength(sideIndex - currentIndex);
                }
                currentIndex += child.SideCount;
            }
            return 0;
        }

        public override void SetSideLength(int sideIndex, float length)
        {
            if (length <= 0 || _children.Count == 0) return;

            // Находим соответствующую дочернюю фигуру и устанавливаем длину её стороны
            int currentIndex = 0;
            foreach (var child in _children)
            {
                if (sideIndex < currentIndex + child.SideCount)
                {
                    child.SetSideLength(sideIndex - currentIndex, length);
                    UpdateVirtualBounds();
                    return;
                }
                currentIndex += child.SideCount;
            }
        }

        public override void MoveBy(int dx, int dy)
        {
            // Перемещаем все дочерние фигуры
            foreach (var child in _children)
            {
                child.MoveBy(dx, dy);
            }

            // Обновляем LocalAnchor для самой группы
            LocalAnchor = new Point(LocalAnchor.X + dx, LocalAnchor.Y + dy);

            UpdateVirtualBounds();
        }

        public override void MoveTo(Point location)
        {
            if (_children.Count == 0)
            {
                LocalAnchor = new Point(location.X - GlobalOrigin.X, location.Y - GlobalOrigin.Y);
                return;
            }

            // Вычисляем текущий центр всех фигур
            var bounds = GetGroupBounds();
            int currentCenterX = bounds.X + bounds.Width / 2;
            int currentCenterY = bounds.Y + bounds.Height / 2;

            // Вычисляем смещение
            int dx = location.X - currentCenterX;
            int dy = location.Y - currentCenterY;

            // Перемещаем все дочерние фигуры
            foreach (var child in _children)
            {
                child.MoveBy(dx, dy);
            }

            LocalAnchor = new Point(location.X - GlobalOrigin.X, location.Y - GlobalOrigin.Y);
            UpdateVirtualBounds();
        }

        public override void ResetDeformation()
        {
            base.ResetDeformation();

            // Сбрасываем деформацию всех дочерних фигур
            foreach (var child in _children)
            {
                child.ResetDeformation();
            }

            UpdateVirtualBounds();
        }
        #endregion

        #region Сохранение/Загрузка

        public override JsonObject Save()
        {
            var json = base.Save();

            var children = new JsonArray();
            foreach (var child in _children)
            {
                children.Add(child.Save());
            }
            json["children"] = children;
            return json;
        }

        public static GroupShape LoadFromJson(JsonObject json)
        {
            var shape = new GroupShape();

            if (json.ContainsKey("children"))
            {
                var childrenArray = json["children"]!.AsArray();
                foreach (var childJson in childrenArray)
                {
                    if (childJson != null)
                    {
                        var child = ShapeBase.CreateFromJson(childJson.AsObject());
                        shape.AddChild(child);
                    }
                }
            }

            shape.LoadCommon(json);

            var bounds = shape.GetGroupBounds();
            if (!bounds.IsEmpty)
            {
                shape.GlobalOrigin = new Point(
                    bounds.X + bounds.Width / 2,
                    bounds.Y + bounds.Height / 2
                );
            }

            return shape;
        }

        #endregion

    }
}
