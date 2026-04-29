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
            
            // Вызываем событие разгруппировки
            UngroupRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}
