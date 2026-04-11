using System.Collections.Generic;
using System.Drawing;
using OOTPiSP_LR1.Shapes;

namespace OOTPiSP_LR1.Core
{
    /// <summary>
    /// Менеджер фигур - управляет коллекцией фигур на холсте
    /// </summary>
    public class ShapeManager
    {
        public List<ShapeBase> Shapes { get; } = new();

        public ShapeBase? SelectedShape { get; private set; }
        
        /// <summary>
        /// Список всех выделенных фигур (для множественного выделения с Ctrl)
        /// </summary>
        public List<ShapeBase> SelectedShapes { get; } = new();

        /// <summary>
        /// Добавить фигуру в коллекцию
        /// </summary>
        public void AddShape(ShapeBase shape) => Shapes.Add(shape);

        /// <summary>
        /// Удалить фигуру из коллекции
        /// </summary>
        public void RemoveShape(ShapeBase shape)
        {
            if (shape == SelectedShape)
                SelectedShape = null;
            Shapes.Remove(shape);
        }

        /// <summary>
        /// Отрисовать все фигуры
        /// </summary>
        public void DrawAll(Graphics g)
        {
            foreach (var s in Shapes)
                s.Draw(g);
        }

        /// <summary>
        /// Найти фигуру по точке (сверху вниз по z-порядку)
        /// </summary>
        public ShapeBase? HitTest(Point p)
        {
            for (int i = Shapes.Count - 1; i >= 0; i--)
            {
                if (Shapes[i].HitTest(p))
                    return Shapes[i];
            }
            return null;
        }

        /// <summary>
        /// Выбрать фигуру
        /// </summary>
        public void Select(ShapeBase? shape)
        {
            foreach (var s in Shapes)
                s.IsSelected = false;

            SelectedShape = shape;

            if (shape != null)
                shape.IsSelected = true;
        }

        /// <summary>
        /// Снять выделение со всех фигур
        /// </summary>
        public void ClearSelection()
        {
            foreach (var s in Shapes)
                s.IsSelected = false;
            SelectedShape = null;
            SelectedShapes.Clear();
        }
        
        /// <summary>
        /// Переключить выделение фигуры (для множественного выделения с Ctrl)
        /// </summary>
        /// <param name="shape">Фигура для переключения выделения</param>
        /// <returns>true если фигура стала выделенной, false если снято выделение</returns>
        public bool ToggleSelection(ShapeBase shape)
        {
            if (shape == null) return false;
            
            if (SelectedShapes.Contains(shape))
            {
                // Снимаем выделение
                shape.IsSelected = false;
                SelectedShapes.Remove(shape);
                
                // Обновляем основную выбранную фигуру
                if (SelectedShape == shape)
                {
                    SelectedShape = SelectedShapes.Count > 0 ? SelectedShapes[0] : null;
                }
                return false;
            }
            else
            {
                // Добавляем в выделенные
                shape.IsSelected = true;
                SelectedShapes.Add(shape);
                SelectedShape = shape;
                return true;
            }
        }
        
        /// <summary>
        /// Добавить фигуру к выделению (без очистки предыдущих)
        /// </summary>
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
        
        /// <summary>
        /// Очистить множественное выделение и выбрать одну фигуру
        /// </summary>
        public void SelectSingle(ShapeBase? shape)
        {
            // Снимаем выделение со всех
            foreach (var s in Shapes)
                s.IsSelected = false;
            
            SelectedShapes.Clear();
            SelectedShape = shape;
            
            if (shape != null)
            {
                shape.IsSelected = true;
                SelectedShapes.Add(shape);
            }
        }

        /// <summary>
        /// Переместить выбранную фигуру на передний план
        /// </summary>
        public void BringToFront(ShapeBase shape)
        {
            if (Shapes.Remove(shape))
            {
                Shapes.Add(shape);
            }
        }

        /// <summary>
        /// Создать начальный набор из 5 фигур с учётом отступов
        /// </summary>
        public void CreateInitialShapes(int canvasWidth, int canvasHeight, int margin = 50)
        {
            Shapes.Clear();
            SelectedShape = null;

            int spacing = canvasWidth / 5;

            // 1. Окружность
            var circle = new CircleShape(
                new Point(margin + spacing / 2, margin + canvasHeight / 2),
                120
            )
            {
                FillColor = Color.LightBlue
            };
            circle.SetBorder(0, 15f, Color.DarkBlue);
            Shapes.Add(circle);

            // 2. Прямоугольник
            var rect = new RectangleShape(
                new Point(margin + spacing + spacing / 2, margin + canvasHeight / 2),
                240, 160
            )
            {
                FillColor = Color.LightGreen
            };
            rect.SetBorder(0, 8f, Color.DarkGreen);    // Верх - тонкая
            rect.SetBorder(1, 25f, Color.Blue);        // Право - очень толстая
            rect.SetBorder(2, 15f, Color.Red);         // Низ - средняя
            rect.SetBorder(3, 35f, Color.Purple);      // Лево - супер толстая
            Shapes.Add(rect);

            // 3. Треугольник
            var triangle = new TriangleShape(
                new Point(margin + 2 * spacing + spacing / 2, margin + canvasHeight / 2),
                140
            )
            {
                FillColor = Color.LightYellow
            };
            triangle.SetBorder(0, 10f, Color.Orange);      // Сторона 1 - тонкая
            triangle.SetBorder(1, 28f, Color.DarkBlue);    // Сторона 2 - толстая
            triangle.SetBorder(2, 45f, Color.Crimson);     // Сторона 3 - супер толстая
            Shapes.Add(triangle);

            // 4. Шестиугольник
            var hexagon = new HexagonShape(
                new Point(margin + 3 * spacing + spacing / 2, margin + canvasHeight / 2),
                120
            )
            {
                FillColor = Color.LightPink
            };
            hexagon.SetBorder(0, 8f, Color.Purple);        // Сторона 0 - тонкая
            hexagon.SetBorder(1, 18f, Color.Teal);         // Сторона 1 - средняя
            hexagon.SetBorder(2, 30f, Color.Navy);         // Сторона 2 - толстая
            hexagon.SetBorder(3, 12f, Color.Maroon);       // Сторона 3 - средне-тонкая
            hexagon.SetBorder(4, 40f, Color.DarkGreen);    // Сторона 4 - супер толстая
            hexagon.SetBorder(5, 22f, Color.SaddleBrown);  // Сторона 5 - толстая
            Shapes.Add(hexagon);

            // 5. Трапеция
            var trapezoid = new TrapezoidShape(
                new Point(margin + 4 * spacing + spacing / 2, margin + canvasHeight / 2),
                280, 160, 160
            )
            {
                FillColor = Color.LightCoral
            };
            trapezoid.SetBorder(0, 12f, Color.DarkRed);     // Верх - тонкая
            trapezoid.SetBorder(1, 40f, Color.Blue);        // Право - супер толстая
            trapezoid.SetBorder(2, 20f, Color.DarkGreen);   // Низ - средняя
            trapezoid.SetBorder(3, 6f, Color.Purple);       // Лево - самая тонкая
            Shapes.Add(trapezoid);

            // 6. Произвольный многоугольник (PolygonShape) - пятиугольник
            var pentagon = new PolygonShape(
                new Point(margin + 5 * spacing / 2, margin + canvasHeight / 2 + 200),
                new PointF(0, -80)
            )
            {
                FillColor = Color.Lavender,
                IsClosed = true
            };
            // Добавляем 5 отрезков для пятиугольника
            pentagon.AddSegmentByLengthAngle(95f, 54f);     // Первый отрезок
            pentagon.AddSegmentByLengthAngle(95f, 72f);     // Второй отрезок
            pentagon.AddSegmentByLengthAngle(95f, 72f);     // Третий отрезок
            pentagon.AddSegmentByLengthAngle(95f, 72f);     // Четвёртый отрезок
            pentagon.AddSegmentByLengthAngle(95f, 72f);     // Пятый отрезок
            pentagon.SetBorder(0, 10f, Color.Indigo);
            pentagon.SetBorder(1, 15f, Color.Violet);
            pentagon.SetBorder(2, 8f, Color.Plum);
            pentagon.SetBorder(3, 20f, Color.MediumPurple);
            pentagon.SetBorder(4, 12f, Color.DarkViolet);
            Shapes.Add(pentagon);
        }

        #region Операции группировки

        /// <summary>
        /// Получить список выбранных фигур
        /// </summary>
        /// <returns>Список выбранных фигур</returns>
        public List<ShapeBase> GetSelectedShapes()
        {
            return new List<ShapeBase>(SelectedShapes);
        }

        /// <summary>
        /// Сгруппировать выбранные фигуры в GroupShape.
        /// Заменяет выбранные фигуры на группу в списке Shapes.
        /// </summary>
        /// <returns>Созданная группа или null, если меньше 2 фигур выбрано</returns>
        public GroupShape? GroupSelectedShapes()
        {
            // 1. Получить выбранные фигуры
            var selectedShapes = GetSelectedShapes();

            // 2. Если меньше 2 фигур - вернуть null
            if (selectedShapes.Count < 2)
                return null;

            // 3. Создать GroupShape через GroupShape.CreateFromShapes()
            var group = GroupShape.CreateFromShapes(selectedShapes);
            if (group == null)
                return null;

            // 4. Удалить выбранные фигуры из Shapes
            foreach (var shape in selectedShapes)
            {
                Shapes.Remove(shape);
            }

            // 5. Добавить группу в Shapes
            Shapes.Add(group);

            // 6. Очистить выбор
            ClearSelection();

            // 7. Выбрать группу
            SelectSingle(group);

            // 8. Вернуть группу
            return group;
        }

        /// <summary>
        /// Разгруппировать выбранную фигуру (если это GroupShape).
        /// Заменяет группу на отдельные фигуры в списке Shapes.
        /// </summary>
        /// <returns>Список разгруппированных фигур или null, если выбранная фигура не группа</returns>
        public List<ShapeBase>? UngroupSelectedShape()
        {
            // 1. Получить первую выбранную фигуру
            if (SelectedShape == null)
                return null;

            // 2. Проверить, является ли она GroupShape
            if (SelectedShape is not GroupShape group)
                return null;

            // 3. Найти позицию группы в списке
            int groupIndex = Shapes.IndexOf(group);

            // 4. Вызвать group.Ungroup() для получения детей
            var children = group.Ungroup();

            // 5. Удалить группу из Shapes
            Shapes.Remove(group);

            // 6. Добавить детей в Shapes (в ту же позицию или в конец)
            if (groupIndex >= 0)
            {
                // Вставляем в ту же позицию, где была группа
                foreach (var child in children)
                {
                    Shapes.Insert(groupIndex, child);
                    groupIndex++;
                }
            }
            else
            {
                // Если позиция не найдена, добавляем в конец
                Shapes.AddRange(children);
            }

            // 7. Очистить выбор
            ClearSelection();

            // 8. Выбрать разгруппированные фигуры
            foreach (var child in children)
            {
                AddToSelection(child);
            }

            // 9. Вернуть список детей
            return children;
        }

        /// <summary>
        /// Проверить, является ли выбранная фигура группой.
        /// </summary>
        /// <returns>true если первая выбранная фигура - GroupShape</returns>
        public bool IsSelectedShapeGroup()
        {
            return SelectedShape is GroupShape;
        }

        #endregion
    }
}
