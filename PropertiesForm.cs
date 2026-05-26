using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using OOTPiSP_LR1.Shapes;

namespace OOTPiSP_LR1
{
    /// <summary>
    /// Панель свойств для редактирования параметров фигуры
    /// </summary>
    public partial class PropertiesPanel : UserControl
    {
        private ShapeBase? _shape;
        
        public event EventHandler? ShapeChanged;
        public event EventHandler<AnchorPosition>? AnchorPositionChanged;
        
        /// <summary>
        /// Границы холста для ограничения позиции фигуры
        /// </summary>
        public Rectangle CanvasBounds { get; set; } = Rectangle.Empty;

        // Словари для маппинга русских названий на enum и обратно
        private static readonly Dictionary<string, AnchorPosition> AnchorPositionMap = new()
        {
            { "Центр", AnchorPosition.Center },
            { "Верхний левый", AnchorPosition.TopLeft },
            { "Верхний правый", AnchorPosition.TopRight },
            { "Нижний левый", AnchorPosition.BottomLeft },
            { "Нижний правый", AnchorPosition.BottomRight },
            { "Верх", AnchorPosition.Top },
            { "Низ", AnchorPosition.Bottom },
            { "Лево", AnchorPosition.Left },
            { "Право", AnchorPosition.Right },
            { "Произвольно", AnchorPosition.Custom }
        };

        private static readonly Dictionary<AnchorPosition, string> AnchorPositionReverseMap = new()
        {
            { AnchorPosition.Center, "Центр" },
            { AnchorPosition.TopLeft, "Верхний левый" },
            { AnchorPosition.TopRight, "Верхний правый" },
            { AnchorPosition.BottomLeft, "Нижний левый" },
            { AnchorPosition.BottomRight, "Нижний правый" },
            { AnchorPosition.Top, "Верх" },
            { AnchorPosition.Bottom, "Низ" },
            { AnchorPosition.Left, "Лево" },
            { AnchorPosition.Right, "Право" },
            { AnchorPosition.Custom, "Произвольно" }
        };

        public PropertiesPanel()
        {
            InitializeComponent();

            int shiftY = 35;
            foreach (Control c in Controls)
            {
                if (c != labelShapeId && c != labelShapeName && c != textShapeName)
                {
                    c.Location = new Point(c.Location.X, c.Location.Y + shiftY);
                }
            }
        }

        /// <summary>
        /// Обновить расположение элементов в зависимости от размера панели
        /// </summary>
        public void UpdateLayout(int width, int height)
        {
            // Обновляем ширину текстовых полей
            int smallTextBoxWidth = Math.Max(60, (width - 150) / 4);
            
            // Абсолютная точка привязки
            textAnchorX.Width = smallTextBoxWidth;
            textAnchorY.Width = smallTextBoxWidth;
            textAnchorY.Location = new Point(120 + smallTextBoxWidth, textAnchorY.Location.Y);
            labelAnchorY.Location = new Point(100 + smallTextBoxWidth, labelAnchorY.Location.Y);
            
            // Локальная точка привязки
            textLocalAnchorX.Width = smallTextBoxWidth;
            textLocalAnchorY.Width = smallTextBoxWidth;
            textLocalAnchorY.Location = new Point(120 + smallTextBoxWidth, textLocalAnchorY.Location.Y);
            labelLocalAnchorY.Location = new Point(100 + smallTextBoxWidth, labelLocalAnchorY.Location.Y);
            
            // Виртуальные границы
            textBoundsLeft.Width = smallTextBoxWidth;
            textBoundsTop.Width = smallTextBoxWidth;
            textBoundsRight.Width = smallTextBoxWidth;
            textBoundsBottom.Width = smallTextBoxWidth;
            
            // Обновляем расположение правых полей виртуальных границ
            textBoundsTop.Location = new Point(150 + smallTextBoxWidth, textBoundsTop.Location.Y);
            labelBoundsTop.Location = new Point(100 + smallTextBoxWidth, labelBoundsTop.Location.Y);
            textBoundsBottom.Location = new Point(150 + smallTextBoxWidth, textBoundsBottom.Location.Y);
            labelBoundsBottom.Location = new Point(100 + smallTextBoxWidth, labelBoundsBottom.Location.Y);
            
            // ComboBox положения точки привязки
            comboAnchorPosition.Width = Math.Max(180, width - 50);
            
            // Панель цвета заливки
            panelFillColor.Location = new Point(width - 100, panelFillColor.Location.Y);
            labelFillColor.Location = new Point(width - 200, labelFillColor.Location.Y);
            
            // Разделители
            separator1.Width = width - 20;
            separator2.Width = width - 20;
            separator3.Width = width - 20;
            
            // Кнопки размера
            buttonSizeUp.Location = new Point(100, 412);
            buttonSizeDown.Location = new Point(150, 412);
            
            // Контролы длины стороны
            labelSideLength.Location = new Point(10, 455);
            comboSideSelect.Location = new Point(10, 488);
            comboSideSelect.Width = Math.Max(100, (width - 200) / 2);
            textSideLength.Location = new Point(20 + comboSideSelect.Width, 491);
            buttonSetSideLength.Location = new Point(120 + comboSideSelect.Width, 488);
            
            // Панель граней (с учётом новых контролов длины стороны)
            panelBorders.Location = new Point(10, 568);
            panelBorders.Size = new Size(width - 20, height - 635);
            
            // Кнопка применить
            buttonApply.Location = new Point(10, height - 70);
            buttonApply.Size = new Size(width - 20, 50);
        }

        /// <summary>
        /// Установить фигуру для редактирования
        /// </summary>
        public void SetShape(ShapeBase? shape)
        {
            _shape = shape;
            UpdateProperties();
            UpdateBorderControls();
            UpdateSideSelection();
            UpdateAngleSelection();
            UpdatePolygonSegmentPanel();
            UpdateCompositePanel();
            UpdateGroupPanel();
            UpdateEllipsePanel();
        }

        /// <summary>
        /// Обновить отображаемые свойства
        /// </summary>
        public void UpdateProperties()
        {
            if (_shape == null)
            {
                ClearProperties();
                return;
            }

            labelShapeId.Text = $"ID: {_shape.Id}  ({_shape.GetType().Name})";

            textShapeName.TextChanged -= textShapeName_TextChanged;
            textShapeName.Text = _shape.ShapeName;
            textShapeName.TextChanged += textShapeName_TextChanged;

            // Глобальная точка отсчёта фигуры
            textAnchorX.Text = _shape.GlobalOrigin.X.ToString();
            textAnchorY.Text = _shape.GlobalOrigin.Y.ToString();

            // Локальная точка привязки — координаты центра фигуры относительно GlobalOrigin
            textLocalAnchorX.Text = _shape.LocalAnchor.X.ToString();
            textLocalAnchorY.Text = _shape.LocalAnchor.Y.ToString();

            // Виртуальные границы
            var topLeft = _shape.GetVirtualTopLeft();
            var bottomRight = _shape.GetVirtualBottomRight();
            
            textBoundsLeft.Text = topLeft.X.ToString();
            textBoundsTop.Text = topLeft.Y.ToString();
            textBoundsRight.Text = bottomRight.X.ToString();
            textBoundsBottom.Text = bottomRight.Y.ToString();

            // Положение точки привязки
            if (AnchorPositionReverseMap.TryGetValue(_shape.AnchorPos, out string russianName))
            {
                comboAnchorPosition.SelectedItem = russianName;
            }
            else
            {
                comboAnchorPosition.SelectedIndex = -1;
            }

            // Цвет заливки
            panelFillColor.BackColor = _shape.FillColor;

            // Обновляем контролы для граней
            UpdateBorderControls();
        }

        private void ClearProperties()
        {
            labelShapeId.Text = "ID: -";
            textShapeName.TextChanged -= textShapeName_TextChanged;
            textShapeName.Text = "";
            textShapeName.TextChanged += textShapeName_TextChanged;
            textAnchorX.Text = "";
            textAnchorY.Text = "";
            textLocalAnchorX.Text = "";
            textLocalAnchorY.Text = "";
            textBoundsLeft.Text = "";
            textBoundsTop.Text = "";
            textBoundsRight.Text = "";
            textBoundsBottom.Text = "";
            comboAnchorPosition.SelectedIndex = -1;
            panelFillColor.BackColor = Color.White;
        }

        private void UpdateBorderControls()
        {
            // Очищаем старые контролы для граней
            ClearBorderControls();

            if (_shape == null) return;

            // Для окружности - только одна "грань"
            if (_shape is CircleShape circle)
            {
                AddBorderControl(0, "Линия", circle.CircleBorderWidth, circle.CircleBorderColor);
            }
            else if (_shape is EllipseShape ellipse)
            {
                AddBorderControl(0, "Линия", ellipse.EllipseBorderWidth, ellipse.EllipseBorderColor);
            }
            else
            {
                // Для остальных фигур - по количеству сторон
                string[] sideNames = GetSideNames(_shape);
                
                for (int i = 0; i < _shape.SideCount; i++)
                {
                    AddBorderControl(i, sideNames[i], _shape.BorderWidths[i], _shape.BorderColors[i]);
                }
            }
        }

        private string[] GetSideNames(ShapeBase shape)
        {
            return shape switch
            {
                RectangleShape => new[] { "Верх", "Право", "Низ", "Лево" },
                TriangleShape => new[] { "Сторона 1", "Сторона 2", "Сторона 3" },
                HexagonShape => new[] { "Сторона 1", "Сторона 2", "Сторона 3", "Сторона 4", "Сторона 5", "Сторона 6" },
                TrapezoidShape => new[] { "Верх", "Право", "Низ", "Лево" },
                PolygonShape polygon => GetPolygonSideNames(polygon),
                CompositeShape composite => GetCompositeSideNames(composite),
                GroupShape group => GetGroupSideNames(group),
                _ => Array.Empty<string>()
            };
        }

        private string[] GetPolygonSideNames(PolygonShape polygon)
        {
            var names = new string[polygon.SideCount];
            for (int i = 0; i < polygon.SideCount; i++)
            {
                names[i] = $"Отрезок {i + 1}";
            }
            return names;
        }

        private string[] GetCompositeSideNames(CompositeShape composite)
        {
            var names = new string[composite.SideCount];
            for (int i = 0; i < composite.SideCount; i++)
            {
                names[i] = $"Сторона {i + 1}";
            }
            return names;
        }

        private string[] GetGroupSideNames(GroupShape group)
        {
            var names = new List<string>();
            var children = group.GetChildren();
            int sideIndex = 0;
            
            foreach (var child in children)
            {
                string childTypeName = child.GetType().Name;
                for (int i = 0; i < child.SideCount; i++)
                {
                    names.Add($"{childTypeName} [{sideIndex + 1}]");
                    sideIndex++;
                }
            }
            
            return names.ToArray();
        }

        private void AddBorderControl(int index, string name, float width, Color color)
        {
            int yPos = 10 + index * 45;

            // Метка названия стороны
            var label = new Label
            {
                Text = name + ":",
                Location = new Point(10, yPos),
                Size = new Size(100, 28),
                Font = new Font("Segoe UI", 12F)
            };
            panelBorders.Controls.Add(label);

            // Поле толщины
            var textWidth = new TextBox
            {
                Text = width.ToString(),
                Location = new Point(120, yPos - 3),
                Size = new Size(60, 34),
                Tag = index,
                Font = new Font("Segoe UI", 12F)
            };
            textWidth.TextChanged += BorderWidthChanged;
            panelBorders.Controls.Add(textWidth);

            // Кнопка выбора цвета
            var colorBtn = new Button
            {
                BackColor = color,
                Location = new Point(190, yPos - 3),
                Size = new Size(40, 34),
                Tag = index
            };
            colorBtn.Click += BorderColorClick;
            panelBorders.Controls.Add(colorBtn);
        }

        private void ClearBorderControls()
        {
            panelBorders.Controls.Clear();
        }

        private void BorderWidthChanged(object? sender, EventArgs e)
        {
            if (_shape == null) return;
            
            if (sender is not TextBox textBox) return;
            
            int index = (int)textBox.Tag!;
            
            if (float.TryParse(textBox.Text, out float width) && width > 0)
            {
                if (_shape is CircleShape circle)
                {
                    circle.CircleBorderWidth = width;
                }
                else if (_shape is EllipseShape ellipse)
                {
                    ellipse.EllipseBorderWidth = width;
                }
                else
                {
                    _shape.SetBorderWidth(index, width);
                }
                ShapeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void BorderColorClick(object? sender, EventArgs e)
        {
            if (_shape == null) return;
            
            if (sender is not Button btn) return;
            
            int index = (int)btn.Tag!;
            
            using (var dialog = new ColorDialog())
            {
                dialog.Color = btn.BackColor;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    btn.BackColor = dialog.Color;
                    
                    if (_shape is CircleShape circle)
                    {
                        circle.CircleBorderColor = dialog.Color;
                    }
                    else if (_shape is EllipseShape ellipse)
                    {
                        ellipse.EllipseBorderColor = dialog.Color;
                    }
                    else
                    {
                        _shape.SetBorderColor(index, dialog.Color);
                    }
                    ShapeChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            if (_shape == null) return;

            bool anchorChanged = false;
            bool localAnchorChanged = false;
            int absX = 0, absY = 0, localX = 0, localY = 0;

            // Проверяем, изменились ли глобальные координаты
            if (int.TryParse(textAnchorX.Text, out absX) && 
                int.TryParse(textAnchorY.Text, out absY))
            {
                if (_shape.GlobalOrigin.X != absX || _shape.GlobalOrigin.Y != absY)
                {
                    anchorChanged = true;
                }
            }

            // Проверяем, изменились ли локальные координаты
            if (int.TryParse(textLocalAnchorX.Text, out localX) && 
                int.TryParse(textLocalAnchorY.Text, out localY))
            {
                if (_shape.LocalAnchor.X != localX || _shape.LocalAnchor.Y != localY)
                {
                    localAnchorChanged = true;
                }
            }

            // Если изменились глобальные координаты - меняем GlobalOrigin
            if (anchorChanged)
            {
                _shape.SetGlobalOrigin(new Point(absX, absY));
            }
            // Если изменились локальные координаты - меняем LocalAnchor
            else if (localAnchorChanged)
            {
                _shape.LocalAnchor = new Point(localX, localY);
                _shape.AnchorPos = AnchorPosition.Custom;
                _shape.RefreshBounds();
            }

            // Применяем положение точки привязки из ComboBox
            if (comboAnchorPosition.SelectedItem != null)
            {
                if (AnchorPositionMap.TryGetValue(comboAnchorPosition.SelectedItem.ToString(), out var pos))
                {
                    _shape.SetAnchorPosition(pos);
                    AnchorPositionChanged?.Invoke(this, pos);
                }
            }

            // Ограничиваем позицию фигуры в пределах холста
            if (CanvasBounds != Rectangle.Empty)
            {
                _shape.ClampToBounds(CanvasBounds);
            }

            // Обновляем отображение
            UpdateProperties();
            ShapeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void textShapeName_TextChanged(object? sender, EventArgs e)
        {
            if (_shape != null)
            {
                _shape.ShapeName = textShapeName.Text;
                ShapeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void panelFillColor_Click(object sender, EventArgs e)
        {
            if (_shape == null) return;
            
            using (var dialog = new ColorDialog())
            {
                dialog.Color = _shape.FillColor;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _shape.FillColor = dialog.Color;
                    panelFillColor.BackColor = dialog.Color;
                    ShapeChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private void comboAnchorPosition_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Событие обрабатывается в buttonApply_Click
        }

        private void buttonSizeUp_Click(object sender, EventArgs e)
        {
            if (_shape == null) return;
            
            // Увеличиваем размер на 10%
            _shape.Resize(1.1f);
            UpdateProperties();
            ShapeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void buttonSizeDown_Click(object sender, EventArgs e)
        {
            if (_shape == null) return;
            
            // Уменьшаем размер на 10%
            _shape.Resize(0.9f);
            UpdateProperties();
            UpdateSideLengthDisplay();
            ShapeChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Обновить выпадающий список выбора стороны в зависимости от типа фигуры
        /// </summary>
        private void UpdateSideSelection()
        {
            comboSideSelect.Items.Clear();
            
            if (_shape == null) return;

            string[] sideNames = GetSideNames(_shape);
            foreach (var name in sideNames)
            {
                comboSideSelect.Items.Add(name);
            }

            if (comboSideSelect.Items.Count > 0)
            {
                comboSideSelect.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Обновить отображение длины выбранной стороны
        /// </summary>
        private void UpdateSideLengthDisplay()
        {
            if (_shape == null || comboSideSelect.SelectedIndex < 0)
            {
                textSideLength.Text = "";
                return;
            }

            int sideIndex = comboSideSelect.SelectedIndex;
            float length = _shape.GetSideLength(sideIndex);
            textSideLength.Text = ((int)length).ToString();
        }

        private void comboSideSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSideLengthDisplay();
        }

        private void buttonSetSideLength_Click(object sender, EventArgs e)
        {
            if (_shape == null) return;
            if (comboSideSelect.SelectedIndex < 0) return;
            
            if (float.TryParse(textSideLength.Text, out float length) && length > 0)
            {
                int sideIndex = comboSideSelect.SelectedIndex;
                _shape.SetSideLength(sideIndex, length);
                UpdateProperties();
                UpdateSideLengthDisplay();
                ShapeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Обновить выпадающий список выбора вершины для углов
        /// </summary>
        private void UpdateAngleSelection()
        {
            comboAngleSelect.Items.Clear();
            
            if (_shape == null) return;

            // Углы доступны только для треугольника и трапеции
            if (_shape is TriangleShape or TrapezoidShape)
            {
                string[] vertexNames = GetVertexNames(_shape);
                foreach (var name in vertexNames)
                {
                    comboAngleSelect.Items.Add(name);
                }

                if (comboAngleSelect.Items.Count > 0)
                {
                    comboAngleSelect.SelectedIndex = 0;
                }
            }
        }

        private string[] GetVertexNames(ShapeBase shape)
        {
            return shape switch
            {
                TriangleShape => new[] { "Вершина 1", "Вершина 2", "Вершина 3" },
                TrapezoidShape => new[] { "Верх-лево", "Верх-право", "Низ-право", "Низ-лево" },
                _ => Array.Empty<string>()
            };
        }

        /// <summary>
        /// Обновить отображение угла выбранной вершины
        /// </summary>
        private void UpdateAngleDisplay()
        {
            if (_shape == null || comboAngleSelect.SelectedIndex < 0)
            {
                textAngleValue.Text = "";
                return;
            }

            int vertexIndex = comboAngleSelect.SelectedIndex;
            float angle = _shape.GetAngle(vertexIndex);
            textAngleValue.Text = ((int)angle).ToString();
        }

        private void comboAngleSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateAngleDisplay();
        }

        private void buttonSetAngle_Click(object sender, EventArgs e)
        {
            if (_shape == null) return;
            if (comboAngleSelect.SelectedIndex < 0) return;
            
            // Углы доступны только для треугольника и трапеции
            if (_shape is not (TriangleShape or TrapezoidShape)) return;

            if (float.TryParse(textAngleValue.Text, out float angle) && angle > 0 && angle < 180)
            {
                int vertexIndex = comboAngleSelect.SelectedIndex;
                _shape.SetAngle(vertexIndex, angle);
                UpdateProperties();
                UpdateAngleDisplay();
                ShapeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        #region PolygonShape Segment Editing

        private Panel? _polygonSegmentPanel;
        private ListBox? _segmentListBox;
        private TextBox? _segmentLengthText;
        private TextBox? _segmentAngleText;
        private Button? _addSegmentButton;
        private Button? _removeSegmentButton;
        private Button? _updateSegmentButton;
        private CheckBox? _isClosedCheckBox;

        /// <summary>
        /// Создать панель редактирования отрезков для PolygonShape
        /// </summary>
        private void CreatePolygonSegmentPanel()
        {
            if (_polygonSegmentPanel != null) return;

            _polygonSegmentPanel = new Panel
            {
                Location = new Point(10, 560),
                Size = new Size(260, 200),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Метка заголовка
            var titleLabel = new Label
            {
                Text = "Отрезки многоугольника:",
                Location = new Point(5, 5),
                Size = new Size(200, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            _polygonSegmentPanel.Controls.Add(titleLabel);

            // Список отрезков
            _segmentListBox = new ListBox
            {
                Location = new Point(5, 30),
                Size = new Size(250, 80),
                Font = new Font("Segoe UI", 9F)
            };
            _segmentListBox.SelectedIndexChanged += SegmentListBox_SelectedIndexChanged;
            _polygonSegmentPanel.Controls.Add(_segmentListBox);

            // Поля редактирования
            var lengthLabel = new Label
            {
                Text = "Длина:",
                Location = new Point(5, 115),
                Size = new Size(50, 20)
            };
            _polygonSegmentPanel.Controls.Add(lengthLabel);

            _segmentLengthText = new TextBox
            {
                Location = new Point(55, 115),
                Size = new Size(60, 20)
            };
            _polygonSegmentPanel.Controls.Add(_segmentLengthText);

            var angleLabel = new Label
            {
                Text = "Угол:",
                Location = new Point(125, 115),
                Size = new Size(45, 20)
            };
            _polygonSegmentPanel.Controls.Add(angleLabel);

            _segmentAngleText = new TextBox
            {
                Location = new Point(170, 115),
                Size = new Size(60, 20)
            };
            _polygonSegmentPanel.Controls.Add(_segmentAngleText);

            // Кнопки
            _addSegmentButton = new Button
            {
                Text = "+",
                Location = new Point(5, 145),
                Size = new Size(30, 25)
            };
            _addSegmentButton.Click += AddSegmentButton_Click;
            _polygonSegmentPanel.Controls.Add(_addSegmentButton);

            _removeSegmentButton = new Button
            {
                Text = "-",
                Location = new Point(40, 145),
                Size = new Size(30, 25)
            };
            _removeSegmentButton.Click += RemoveSegmentButton_Click;
            _polygonSegmentPanel.Controls.Add(_removeSegmentButton);

            _updateSegmentButton = new Button
            {
                Text = "Обновить",
                Location = new Point(75, 145),
                Size = new Size(75, 25)
            };
            _updateSegmentButton.Click += UpdateSegmentButton_Click;
            _polygonSegmentPanel.Controls.Add(_updateSegmentButton);

            // Чекбокс замкнутости
            _isClosedCheckBox = new CheckBox
            {
                Text = "Замкнутый",
                Location = new Point(160, 147),
                Size = new Size(100, 20)
            };
            _isClosedCheckBox.CheckedChanged += IsClosedCheckBox_CheckedChanged;
            _polygonSegmentPanel.Controls.Add(_isClosedCheckBox);

            Controls.Add(_polygonSegmentPanel);
        }

        private void UpdatePolygonSegmentPanel()
        {
            if (_shape is not PolygonShape polygon)
            {
                if (_polygonSegmentPanel != null)
                {
                    _polygonSegmentPanel.Visible = false;
                }
                return;
            }

            CreatePolygonSegmentPanel();
            _polygonSegmentPanel!.Visible = true;

            // Обновляем список отрезков
            _segmentListBox!.Items.Clear();
            for (int i = 0; i < polygon.Segments.Count; i++)
            {
                _segmentListBox.Items.Add($"[{i + 1}] {polygon.Segments[i]}");
            }

            // Обновляем чекбокс замкнутости
            _isClosedCheckBox!.Checked = polygon.IsClosed;

            // Выбираем первый элемент если есть
            if (_segmentListBox.Items.Count > 0 && _segmentListBox.SelectedIndex < 0)
            {
                _segmentListBox.SelectedIndex = 0;
            }
        }

        private void SegmentListBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_shape is not PolygonShape polygon) return;
            if (_segmentListBox!.SelectedIndex < 0) return;

            int index = _segmentListBox.SelectedIndex;
            if (index < polygon.Segments.Count)
            {
                _segmentLengthText!.Text = polygon.Segments[index].Length.ToString("F1");
                _segmentAngleText!.Text = polygon.Segments[index].AngleDegrees.ToString("F1");
            }
        }

        private void AddSegmentButton_Click(object? sender, EventArgs e)
        {
            if (_shape is not PolygonShape polygon) return;

            if (float.TryParse(_segmentLengthText!.Text, out float length) && length > 0 &&
                float.TryParse(_segmentAngleText!.Text, out float angle))
            {
                polygon.AddSegmentByLengthAngle(length, angle);
                UpdatePolygonSegmentPanel();
                UpdateBorderControls();
                UpdateSideSelection();
                ShapeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void RemoveSegmentButton_Click(object? sender, EventArgs e)
        {
            if (_shape is not PolygonShape polygon) return;
            if (_segmentListBox!.SelectedIndex < 0) return;

            polygon.RemoveSegment(_segmentListBox.SelectedIndex);
            UpdatePolygonSegmentPanel();
            UpdateBorderControls();
            UpdateSideSelection();
            ShapeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void UpdateSegmentButton_Click(object? sender, EventArgs e)
        {
            if (_shape is not PolygonShape polygon) return;
            if (_segmentListBox!.SelectedIndex < 0) return;

            if (float.TryParse(_segmentLengthText!.Text, out float length) && length > 0 &&
                float.TryParse(_segmentAngleText!.Text, out float angle))
            {
                polygon.UpdateSegment(_segmentListBox.SelectedIndex, length, angle);
                UpdatePolygonSegmentPanel();
                UpdateBorderControls();
                ShapeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void IsClosedCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            if (_shape is not PolygonShape polygon) return;
            
            polygon.IsClosed = _isClosedCheckBox!.Checked;
            polygon.RefreshBounds();
            ShapeChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region CompositeShape Editing

        private Panel? _compositePanel;
        private CheckBox? _isExpandedCheckBox;
        private ListBox? _childShapesListBox;
        private Button? _addChildButton;
        private Button? _removeChildButton;
        private Label? _childCountLabel;

        /// <summary>
        /// Создать панель редактирования для CompositeShape
        /// </summary>
        private void CreateCompositePanel()
        {
            if (_compositePanel != null) return;

            _compositePanel = new Panel
            {
                Location = new Point(10, 560),
                Size = new Size(260, 200),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Метка заголовка
            var titleLabel = new Label
            {
                Text = "Составная фигура:",
                Location = new Point(5, 5),
                Size = new Size(200, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            _compositePanel.Controls.Add(titleLabel);

            // Чекбокс режима отображения
            _isExpandedCheckBox = new CheckBox
            {
                Text = "Показать объединение (IsExpanded)",
                Location = new Point(5, 28),
                Size = new Size(250, 20),
                Font = new Font("Segoe UI", 9F)
            };
            _isExpandedCheckBox.CheckedChanged += IsExpandedCheckBox_CheckedChanged;
            _compositePanel.Controls.Add(_isExpandedCheckBox);

            // Метка количества дочерних фигур
            _childCountLabel = new Label
            {
                Text = "Дочерние фигуры: 0",
                Location = new Point(5, 52),
                Size = new Size(200, 18),
                Font = new Font("Segoe UI", 8F)
            };
            _compositePanel.Controls.Add(_childCountLabel);

            // Список дочерних фигур
            _childShapesListBox = new ListBox
            {
                Location = new Point(5, 72),
                Size = new Size(250, 90),
                Font = new Font("Segoe UI", 9F)
            };
            _childShapesListBox.SelectedIndexChanged += ChildShapesListBox_SelectedIndexChanged;
            _compositePanel.Controls.Add(_childShapesListBox);

            // Кнопки
            _addChildButton = new Button
            {
                Text = "+ Добавить",
                Location = new Point(5, 168),
                Size = new Size(85, 25)
            };
            _addChildButton.Click += AddChildButton_Click;
            _compositePanel.Controls.Add(_addChildButton);

            _removeChildButton = new Button
            {
                Text = "- Удалить",
                Location = new Point(95, 168),
                Size = new Size(75, 25)
            };
            _removeChildButton.Click += RemoveChildButton_Click;
            _compositePanel.Controls.Add(_removeChildButton);

            Controls.Add(_compositePanel);
        }

        /// <summary>
        /// Обновить панель редактирования CompositeShape
        /// </summary>
        private void UpdateCompositePanel()
        {
            if (_shape is not CompositeShape composite)
            {
                if (_compositePanel != null)
                {
                    _compositePanel.Visible = false;
                }
                return;
            }

            CreateCompositePanel();
            _compositePanel!.Visible = true;

            // Обновляем чекбокс режима отображения
            _isExpandedCheckBox!.Checked = composite.IsExpanded;

            // Обновляем метку количества
            _childCountLabel!.Text = $"Дочерние фигуры: {composite.ChildCount}";

            // Обновляем список дочерних фигур
            _childShapesListBox!.Items.Clear();
            var children = composite.GetChildren();
            for (int i = 0; i < children.Count; i++)
            {
                string shapeType = children[i].GetType().Name;
                _childShapesListBox.Items.Add($"[{i + 1}] {shapeType}");
            }

            // Выбираем первый элемент если есть
            if (_childShapesListBox.Items.Count > 0 && _childShapesListBox.SelectedIndex < 0)
            {
                _childShapesListBox.SelectedIndex = 0;
            }
        }

        private void IsExpandedCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            if (_shape is not CompositeShape composite) return;
            
            composite.IsExpanded = _isExpandedCheckBox!.Checked;
            composite.RefreshBounds();
            UpdateBorderControls();
            ShapeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ChildShapesListBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            // Можно добавить отображение свойств выбранной дочерней фигуры
        }

        private void AddChildButton_Click(object? sender, EventArgs e)
        {
            if (_shape is not CompositeShape composite) return;

            // Показываем меню для выбора типа добавляемой фигуры
            var menu = new ContextMenuStrip();
            menu.Items.Add("Окружность", null, (s, args) => AddChildShape("Circle"));
            menu.Items.Add("Прямоугольник", null, (s, args) => AddChildShape("Rectangle"));
            menu.Items.Add("Треугольник", null, (s, args) => AddChildShape("Triangle"));
            menu.Items.Add("Шестиугольник", null, (s, args) => AddChildShape("Hexagon"));
            menu.Items.Add("Трапеция", null, (s, args) => AddChildShape("Trapezoid"));
            
            menu.Show(_addChildButton!, new Point(0, _addChildButton!.Height));
        }

        private void AddChildShape(string shapeType)
        {
            if (_shape is not CompositeShape composite) return;

            // Создаём фигуру в центре составной фигуры
            var center = _shape.GetCenter();
            ShapeBase? newChild = shapeType switch
            {
                "Circle" => new CircleShape(center, 50) { FillColor = Color.LightBlue },
                "Rectangle" => new RectangleShape(center, 80, 60) { FillColor = Color.LightGreen },
                "Triangle" => new TriangleShape(center, 60) { FillColor = Color.LightYellow },
                "Hexagon" => new HexagonShape(center, 50) { FillColor = Color.LightPink },
                "Trapezoid" => new TrapezoidShape(center, 100, 60, 70) { FillColor = Color.LightCoral },
                _ => null
            };

            if (newChild != null)
            {
                composite.AddChild(newChild);
                UpdateCompositePanel();
                UpdateBorderControls();
                UpdateSideSelection();
                ShapeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void RemoveChildButton_Click(object? sender, EventArgs e)
        {
            if (_shape is not CompositeShape composite) return;
            if (_childShapesListBox!.SelectedIndex < 0) return;

            composite.RemoveChildAt(_childShapesListBox.SelectedIndex);
            UpdateCompositePanel();
            UpdateBorderControls();
            UpdateSideSelection();
            ShapeChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region GroupShape Editing

        private Panel? _groupPanel;
        private Label? _groupInfoLabel;
        private Label? _groupChildCountLabel;
        private ListBox? _groupChildrenListBox;
        private Button? _ungroupButton;

        /// <summary>
        /// Событие при запросе разгруппировки
        /// </summary>
        public event EventHandler? UngroupRequested;

        /// <summary>
        /// Создать панель редактирования для GroupShape
        /// </summary>
        private void CreateGroupPanel()
        {
            if (_groupPanel != null) return;

            _groupPanel = new Panel
            {
                Location = new Point(10, 560),
                Size = new Size(260, 200),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Метка заголовка
            var titleLabel = new Label
            {
                Text = "Группа фигур:",
                Location = new Point(5, 5),
                Size = new Size(200, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            _groupPanel.Controls.Add(titleLabel);

            // Метка информации о группе
            _groupInfoLabel = new Label
            {
                Text = "Тип: Группа",
                Location = new Point(5, 28),
                Size = new Size(250, 18),
                Font = new Font("Segoe UI", 8F)
            };
            _groupPanel.Controls.Add(_groupInfoLabel);

            // Метка количества дочерних фигур
            _groupChildCountLabel = new Label
            {
                Text = "Фигур в группе: 0",
                Location = new Point(5, 48),
                Size = new Size(200, 18),
                Font = new Font("Segoe UI", 8F)
            };
            _groupPanel.Controls.Add(_groupChildCountLabel);

            // Список дочерних фигур
            _groupChildrenListBox = new ListBox
            {
                Location = new Point(5, 70),
                Size = new Size(250, 90),
                Font = new Font("Segoe UI", 9F)
            };
            _groupPanel.Controls.Add(_groupChildrenListBox);

            // Кнопка разгруппирования
            _ungroupButton = new Button
            {
                Text = "Разгруппировать",
                Location = new Point(5, 168),
                Size = new Size(120, 25),
                BackColor = Color.LightCoral
            };
            _ungroupButton.Click += UngroupButton_Click;
            _groupPanel.Controls.Add(_ungroupButton);

            Controls.Add(_groupPanel);
        }

        /// <summary>
        /// Обновить панель редактирования GroupShape
        /// </summary>
        private void UpdateGroupPanel()
        {
            if (_shape is not GroupShape group)
            {
                if (_groupPanel != null)
                {
                    _groupPanel.Visible = false;
                }
                return;
            }

            CreateGroupPanel();
            _groupPanel!.Visible = true;

            // Обновляем информацию о типах фигур в группе
            var children = group.GetChildren();
            var types = children.Select(c => c.GetType().Name).Distinct();
            string typesInfo = string.Join(", ", types);
            _groupInfoLabel!.Text = $"Содержит: {typesInfo}";

            // Обновляем метку количества
            _groupChildCountLabel!.Text = $"Фигур в группе: {group.ChildCount}";

            // Обновляем список дочерних фигур
            _groupChildrenListBox!.Items.Clear();
            for (int i = 0; i < children.Count; i++)
            {
                string shapeType = children[i].GetType().Name;
                _groupChildrenListBox.Items.Add($"[{i + 1}] {shapeType}");
            }

            // Выбираем первый элемент если есть
            if (_groupChildrenListBox.Items.Count > 0 && _groupChildrenListBox.SelectedIndex < 0)
            {
                _groupChildrenListBox.SelectedIndex = 0;
            }
        }

        private void UngroupButton_Click(object? sender, EventArgs e)
        {
            if (_shape is not GroupShape) return;
            
            UngroupRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region EllipseShape Editing

        private Panel? _ellipsePanel;
        private TextBox? _ellipseSemiMajorText;
        private TextBox? _ellipseSemiMinorText;
        private TextBox? _ellipseF1XText;
        private TextBox? _ellipseF1YText;
        private TextBox? _ellipseF2XText;
        private TextBox? _ellipseF2YText;
        private TextBox? _ellipseRotationText;
        private TextBox? _ellipseBorderWidthText;
        private Button? _ellipseBorderColorBtn;
        private Button? _ellipseApplyButton;
        private TextBox? _ellipseHintAngleText;
        private ComboBox? _ellipseHintPivotCombo;
        private Button? _ellipseHintButton;
        private Label? _ellipseHintLabel;

        private void CreateEllipsePanel()
        {
            if (_ellipsePanel != null) return;

            _ellipsePanel = new Panel
            {
                Location = new Point(10, 560),
                Size = new Size(260, 405),
                BorderStyle = BorderStyle.FixedSingle
            };

            var titleLabel = new Label
            {
                Text = "Параметры эллипса:",
                Location = new Point(5, 5),
                Size = new Size(200, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            _ellipsePanel.Controls.Add(titleLabel);

            var labelA = new Label { Text = "Полуось a:", Location = new Point(5, 30), Size = new Size(80, 20), Font = new Font("Segoe UI", 9F) };
            _ellipsePanel.Controls.Add(labelA);
            _ellipseSemiMajorText = new TextBox { Location = new Point(90, 28), Size = new Size(60, 22), Font = new Font("Segoe UI", 9F) };
            _ellipsePanel.Controls.Add(_ellipseSemiMajorText);

            var labelB = new Label { Text = "Полуось b:", Location = new Point(5, 55), Size = new Size(80, 20), Font = new Font("Segoe UI", 9F) };
            _ellipsePanel.Controls.Add(labelB);
            _ellipseSemiMinorText = new TextBox { Location = new Point(90, 53), Size = new Size(60, 22), Font = new Font("Segoe UI", 9F) };
            _ellipsePanel.Controls.Add(_ellipseSemiMinorText);

            var labelF1 = new Label { Text = "Фокус 1", Location = new Point(5, 80), Size = new Size(55, 20), Font = new Font("Segoe UI", 9F) };
            _ellipsePanel.Controls.Add(labelF1);
            var labelF1X = new Label { Text = "X:", Location = new Point(60, 80), Size = new Size(18, 20), Font = new Font("Segoe UI", 9F) };
            _ellipsePanel.Controls.Add(labelF1X);
            _ellipseF1XText = new TextBox { Location = new Point(78, 78), Size = new Size(50, 22), Font = new Font("Segoe UI", 9F) };
            _ellipsePanel.Controls.Add(_ellipseF1XText);
            var labelF1Y = new Label { Text = "Y:", Location = new Point(133, 80), Size = new Size(18, 20), Font = new Font("Segoe UI", 9F) };
            _ellipsePanel.Controls.Add(labelF1Y);
            _ellipseF1YText = new TextBox { Location = new Point(151, 78), Size = new Size(50, 22), Font = new Font("Segoe UI", 9F) };
            _ellipsePanel.Controls.Add(_ellipseF1YText);

            var labelF2 = new Label { Text = "Фокус 2", Location = new Point(5, 105), Size = new Size(55, 20), Font = new Font("Segoe UI", 9F) };
            _ellipsePanel.Controls.Add(labelF2);
            var labelF2X = new Label { Text = "X:", Location = new Point(60, 105), Size = new Size(18, 20), Font = new Font("Segoe UI", 9F) };
            _ellipsePanel.Controls.Add(labelF2X);
            _ellipseF2XText = new TextBox { Location = new Point(78, 103), Size = new Size(50, 22), Font = new Font("Segoe UI", 9F) };
            _ellipsePanel.Controls.Add(_ellipseF2XText);
            var labelF2Y = new Label { Text = "Y:", Location = new Point(133, 105), Size = new Size(18, 20), Font = new Font("Segoe UI", 9F) };
            _ellipsePanel.Controls.Add(labelF2Y);
            _ellipseF2YText = new TextBox { Location = new Point(151, 103), Size = new Size(50, 22), Font = new Font("Segoe UI", 9F) };
            _ellipsePanel.Controls.Add(_ellipseF2YText);

            var labelRot = new Label { Text = "Поворот (°):", Location = new Point(5, 130), Size = new Size(80, 20), Font = new Font("Segoe UI", 9F) };
            _ellipsePanel.Controls.Add(labelRot);
            _ellipseRotationText = new TextBox { Location = new Point(90, 128), Size = new Size(60, 22), Font = new Font("Segoe UI", 9F) };
            _ellipsePanel.Controls.Add(_ellipseRotationText);

            var labelBW = new Label { Text = "Толщина:", Location = new Point(5, 155), Size = new Size(80, 20), Font = new Font("Segoe UI", 9F) };
            _ellipsePanel.Controls.Add(labelBW);
            _ellipseBorderWidthText = new TextBox { Location = new Point(90, 153), Size = new Size(60, 22), Font = new Font("Segoe UI", 9F) };
            _ellipseBorderWidthText.TextChanged += EllipseBorderWidth_TextChanged;
            _ellipsePanel.Controls.Add(_ellipseBorderWidthText);

            var labelBC = new Label { Text = "Цвет обводки:", Location = new Point(5, 180), Size = new Size(80, 20), Font = new Font("Segoe UI", 9F) };
            _ellipsePanel.Controls.Add(labelBC);
            _ellipseBorderColorBtn = new Button { Location = new Point(90, 178), Size = new Size(60, 22) };
            _ellipseBorderColorBtn.Click += EllipseBorderColor_Click;
            _ellipsePanel.Controls.Add(_ellipseBorderColorBtn);

            _ellipseApplyButton = new Button
            {
                Text = "Применить",
                Location = new Point(5, 210),
                Size = new Size(145, 28),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            _ellipseApplyButton.Click += EllipseApplyButton_Click;
            _ellipsePanel.Controls.Add(_ellipseApplyButton);

            var hintTitleLabel = new Label
            {
                Text = "Подсказка поворота:",
                Location = new Point(5, 248),
                Size = new Size(200, 18),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            _ellipsePanel.Controls.Add(hintTitleLabel);

            var hintAngleLabel = new Label
            {
                Text = "Угол Δ (°):",
                Location = new Point(5, 270),
                Size = new Size(70, 18),
                Font = new Font("Segoe UI", 9F)
            };
            _ellipsePanel.Controls.Add(hintAngleLabel);

            _ellipseHintAngleText = new TextBox
            {
                Location = new Point(78, 268),
                Size = new Size(50, 22),
                Font = new Font("Segoe UI", 9F),
                Text = "45"
            };
            _ellipsePanel.Controls.Add(_ellipseHintAngleText);

            var hintPivotLabel = new Label
            {
                Text = "Вращать отн.:",
                Location = new Point(5, 293),
                Size = new Size(75, 18),
                Font = new Font("Segoe UI", 9F)
            };
            _ellipsePanel.Controls.Add(hintPivotLabel);

            _ellipseHintPivotCombo = new ComboBox
            {
                Location = new Point(83, 291),
                Size = new Size(120, 22),
                Font = new Font("Segoe UI", 9F),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Items = { "Фокус 1", "Фокус 2", "Центр" },
                SelectedIndex = 2
            };
            _ellipsePanel.Controls.Add(_ellipseHintPivotCombo);

            _ellipseHintButton = new Button
            {
                Text = "Рассчитать",
                Location = new Point(135, 316),
                Size = new Size(80, 26),
                Font = new Font("Segoe UI", 9F)
            };
            _ellipseHintButton.Click += EllipseHintButton_Click;
            _ellipsePanel.Controls.Add(_ellipseHintButton);

            _ellipseHintLabel = new Label
            {
                Location = new Point(5, 348),
                Size = new Size(248, 50),
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(30, 30, 120)
            };
            _ellipsePanel.Controls.Add(_ellipseHintLabel);

            Controls.Add(_ellipsePanel);
        }

        private void UpdateEllipsePanel()
        {
            if (_shape is not EllipseShape ellipse)
            {
                if (_ellipsePanel != null)
                    _ellipsePanel.Visible = false;
                panelBorders.Visible = true;
                labelBorders.Visible = true;
                return;
            }

            CreateEllipsePanel();
            _ellipsePanel!.Visible = true;
            _ellipsePanel.BringToFront();
            panelBorders.Visible = false;
            labelBorders.Visible = false;

            _ellipseSemiMajorText!.Text = ellipse.SemiMajor.ToString();
            _ellipseSemiMinorText!.Text = ellipse.SemiMinor.ToString();

            var f1 = ellipse.GetFocus1();
            var f2 = ellipse.GetFocus2();
            _ellipseF1XText!.Text = ((int)Math.Round(f1.X)).ToString();
            _ellipseF1YText!.Text = ((int)Math.Round(f1.Y)).ToString();
            _ellipseF2XText!.Text = ((int)Math.Round(f2.X)).ToString();
            _ellipseF2YText!.Text = ((int)Math.Round(f2.Y)).ToString();

            _ellipseRotationText!.Text = ((int)ellipse.RotationDegrees).ToString();

            _ellipseBorderWidthText!.TextChanged -= EllipseBorderWidth_TextChanged;
            _ellipseBorderWidthText.Text = ellipse.EllipseBorderWidth.ToString();
            _ellipseBorderWidthText.TextChanged += EllipseBorderWidth_TextChanged;

            _ellipseBorderColorBtn!.BackColor = ellipse.EllipseBorderColor;

            if (_ellipseHintLabel != null)
                _ellipseHintLabel.Text = "";
        }

        private void EllipseBorderWidth_TextChanged(object? sender, EventArgs e)
        {
            if (_shape is not EllipseShape ellipse) return;
            if (float.TryParse(_ellipseBorderWidthText!.Text, out float w) && w > 0)
            {
                ellipse.EllipseBorderWidth = w;
                ellipse.RefreshBounds();
                ShapeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void EllipseBorderColor_Click(object? sender, EventArgs e)
        {
            if (_shape is not EllipseShape ellipse) return;
            using (var dialog = new ColorDialog())
            {
                dialog.Color = ellipse.EllipseBorderColor;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    ellipse.EllipseBorderColor = dialog.Color;
                    _ellipseBorderColorBtn!.BackColor = dialog.Color;
                    ShapeChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private void EllipseApplyButton_Click(object? sender, EventArgs e)
        {
            if (_shape is not EllipseShape ellipse) return;

            bool changed = false;

            var curF1 = ellipse.GetFocus1();
            var curF2 = ellipse.GetFocus2();

            bool fociChanged = false;
            float f1X = curF1.X, f1Y = curF1.Y, f2X = curF2.X, f2Y = curF2.Y;

            if (float.TryParse(_ellipseF1XText!.Text, out float parsedF1X))
                f1X = parsedF1X;
            if (float.TryParse(_ellipseF1YText!.Text, out float parsedF1Y))
                f1Y = parsedF1Y;
            if (float.TryParse(_ellipseF2XText!.Text, out float parsedF2X))
                f2X = parsedF2X;
            if (float.TryParse(_ellipseF2YText!.Text, out float parsedF2Y))
                f2Y = parsedF2Y;

            double eps = 0.5;
            if (Math.Abs(f1X - curF1.X) > eps || Math.Abs(f1Y - curF1.Y) > eps ||
                Math.Abs(f2X - curF2.X) > eps || Math.Abs(f2Y - curF2.Y) > eps)
            {
                fociChanged = true;
            }

            if (fociChanged)
            {
                ellipse.SetFromFoci(new PointF(f1X, f1Y), new PointF(f2X, f2Y));
                changed = true;
            }
            else
            {
                int newA = ellipse.SemiMajor;
                int newB = ellipse.SemiMinor;

                if (int.TryParse(_ellipseSemiMajorText!.Text, out int a) && a >= 10)
                    newA = a;

                if (int.TryParse(_ellipseSemiMinorText!.Text, out int b) && b >= 10)
                    newB = b;

                if (newB > newA)
                    (newA, newB) = (newB, newA);

                if (newA != ellipse.SemiMajor || newB != ellipse.SemiMinor)
                {
                    ellipse.SemiMajor = newA;
                    ellipse.SemiMinor = newB;
                    changed = true;
                }

                if (float.TryParse(_ellipseRotationText!.Text, out float rot))
                {
                    if (Math.Abs(rot - ellipse.RotationDegrees) > 0.01f)
                    {
                        ellipse.RotationDegrees = rot;
                        changed = true;
                    }
                }
            }

            if (changed)
            {
                ellipse.SetAnchorPosition(ellipse.AnchorPos);
            }

            UpdateEllipsePanel();
            UpdateProperties();
            ShapeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void EllipseHintButton_Click(object? sender, EventArgs e)
        {
            if (_shape is not EllipseShape ellipse) return;
            if (!float.TryParse(_ellipseHintAngleText!.Text, out float deltaAngle)) return;

            double c = ellipse.FocalDistance;
            double newRad = (ellipse.RotationDegrees + deltaAngle) * Math.PI / 180.0;

            var curF1 = ellipse.GetFocus1();
            var curF2 = ellipse.GetFocus2();
            var center = ellipse.GetCenter();

            string pivotName = _ellipseHintPivotCombo!.SelectedItem?.ToString() ?? "Центр";

            double newF1X, newF1Y, newF2X, newF2Y;
            string f1Status, f2Status;

            if (pivotName == "Фокус 1")
            {
                newF1X = curF1.X;
                newF1Y = curF1.Y;
                newF2X = curF1.X - 2 * c * Math.Cos(newRad);
                newF2Y = curF1.Y - 2 * c * Math.Sin(newRad);
                f1Status = $"без изменений ({(int)Math.Round(curF1.X)}, {(int)Math.Round(curF1.Y)})";
                f2Status = FormatFocusResult("Фокус 2", curF2, newF2X, newF2Y);
            }
            else if (pivotName == "Фокус 2")
            {
                newF2X = curF2.X;
                newF2Y = curF2.Y;
                newF1X = curF2.X + 2 * c * Math.Cos(newRad);
                newF1Y = curF2.Y + 2 * c * Math.Sin(newRad);
                f1Status = FormatFocusResult("Фокус 1", curF1, newF1X, newF1Y);
                f2Status = $"без изменений ({(int)Math.Round(curF2.X)}, {(int)Math.Round(curF2.Y)})";
            }
            else
            {
                newF1X = center.X + c * Math.Cos(newRad);
                newF1Y = center.Y + c * Math.Sin(newRad);
                newF2X = center.X - c * Math.Cos(newRad);
                newF2Y = center.Y - c * Math.Sin(newRad);
                f1Status = FormatFocusResult("Фокус 1", curF1, newF1X, newF1Y);
                f2Status = FormatFocusResult("Фокус 2", curF2, newF2X, newF2Y);
            }

            _ellipseHintLabel!.Text =
                $"Фокус 1: {f1Status}\n" +
                $"Фокус 2: {f2Status}\n" +
                $"Угол: {ellipse.RotationDegrees}° → {ellipse.RotationDegrees + deltaAngle:F1}°";
        }

        private static string FormatFocusResult(string name, PointF oldF, double newX, double newY)
        {
            double dX = newX - oldF.X;
            double dY = newY - oldF.Y;
            string sign(double v) => v >= 0 ? "+" : "";
            return $"({Math.Round(newX)}, {Math.Round(newY)})  ({sign(dX)}{Math.Round(dX)}, {sign(dY)}{Math.Round(dY)})";
        }

        #endregion
    }
}
