using System;
using System.Drawing;
using System.Windows.Forms;
using OOTPiSP_LR1.Core;
using OOTPiSP_LR1.Shapes;
using OOTPiSP_LR1.Dialogs;

namespace OOTPiSP_LR1
{
    public partial class MainForm : Form
    {
        private readonly ShapeManager _shapeManager;
        private PropertiesPanel _propertiesPanel = null!;
        private bool _propertiesPanelVisible;
        private ShapeListPanel _shapeListPanel = null!;
        private bool _shapeListNeedsRefresh = true;
        
        // Отступ от краёв окна для виртуальных границ
        private const int CanvasMargin = 50;
        
        // Drag & drop для фигур
        private bool _isDragging;
        
        // Смещение между точкой клика и позицией фигуры (для корректного перетаскивания)
        private Point _dragOffset;
        
        // Последняя позиция мыши для определения движения
        private Point _lastMousePosition;
        
        // Drag & drop для точки привязки (локальное смещение)
        private bool _isDraggingAnchor;
        private const int AnchorHitRadius = 15;
        
        // Контекстное меню для создания фигур
        private ContextMenuStrip? _createShapeMenu;
        private Point _menuClickLocation;
        
        // === Режим рисования многоугольника ===
        
        /// <summary>
        /// Флаг активного режима рисования
        /// </summary>
        private bool _isDrawingMode;
        
        /// <summary>
        /// Список точек, добавленных пользователем
        /// </summary>
        private List<PointF> _drawingPoints = new();
        
        /// <summary>
        /// Текущая позиция мыши для предпросмотра следующей линии
        /// </summary>
        private Point _currentMousePosition;
        
        /// <summary>
        /// Текущая рисуемая фигура
        /// </summary>
        private PolygonShape? _drawingShape;

        // === Режим пошагового построения многоугольника ===
        
        /// <summary>
        /// Флаг активного режима пошагового построения
        /// </summary>
        private bool _isStepByStepMode;
        
        /// <summary>
        /// Текущая фигура в режиме пошагового построения
        /// </summary>
        private PolygonShape? _stepByStepShape;
        
        /// <summary>
        /// Начальная точка пошагового построения
        /// </summary>
        private Point _stepByStepOrigin;
        
        /// <summary>
        /// Текущий угол наклона (в радианах) - направление последней стороны
        /// </summary>
        private float _currentAngleRadians;
        
        /// <summary>
        /// Текущая конечная точка последнего отрезка
        /// </summary>
        private PointF _currentEndPoint;

        public MainForm()
        {
            InitializeComponent();
            
            _shapeManager = new ShapeManager();
            
            // Настройка полноэкранного режима
            SetupFullScreen();
            
            // Создание панели свойств
            SetupPropertiesPanel();
            SetupShapeListPanel();
            
            // Создание контекстного меню
            SetupCreateShapeMenu();
            
            // Создание начальных фигур
            Load += MainForm_Load;
        }

        private void SetupFullScreen()
        {
            // Полноэкранный режим без границ
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            BackColor = Color.White;
            KeyPreview = true;
            DoubleBuffered = true;
            
            // Подписываемся на события
            Paint += MainForm_Paint;
            MouseDown += MainForm_MouseDown;
            MouseDoubleClick += MainForm_MouseDoubleClick;
            MouseMove += MainForm_MouseMove;
            MouseUp += MainForm_MouseUp;
            KeyDown += MainForm_KeyDown;
            Resize += MainForm_Resize;
            
            // Создаём кнопку выхода
            CreateExitButton();
        }
        
        private void CreateExitButton()
        {
            var exitButton = new Button
            {
                Text = "✕",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(232, 17, 35),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(40, 40),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            
            exitButton.FlatAppearance.BorderSize = 0;
            exitButton.Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - exitButton.Width - 10, 10);
            exitButton.Click += (s, e) => Close();
            
            // Эффект наведения
            exitButton.MouseEnter += (s, e) => exitButton.BackColor = Color.FromArgb(200, 10, 25);
            exitButton.MouseLeave += (s, e) => exitButton.BackColor = Color.FromArgb(232, 17, 35);
            
            Controls.Add(exitButton);
        }

        private void SetupPropertiesPanel()
        {
            _propertiesPanel = new PropertiesPanel
            {
                Visible = false,
                Location = new Point(10, 10)
            };
            
            _propertiesPanel.ShapeChanged += PropertiesPanel_ShapeChanged;
            _propertiesPanel.AnchorPositionChanged += OnAnchorPositionChanged;
            _propertiesPanel.UngroupRequested += (s, e) =>
            {
                var ungrouped = _shapeManager.UngroupSelectedShape();
                if (ungrouped != null)
                {
                    if (_propertiesPanelVisible)
                    {
                        _propertiesPanel.SetShape(_shapeManager.SelectedShape);
                    }
                    Invalidate();
                }
            };
            
            Controls.Add(_propertiesPanel);
        }

        private void SetupShapeListPanel()
        {
            _shapeListPanel = new ShapeListPanel
            {
                Visible = true
            };
            _shapeListPanel.SetShapeManager(_shapeManager);
            _shapeListPanel.ShapeSelected += ShapeListPanel_ShapeSelected;
            Controls.Add(_shapeListPanel);
        }

        private void ShapeListPanel_ShapeSelected(object? sender, ShapeBase shape)
        {
            _shapeManager.SelectSingle(shape);
            _shapeManager.BringToFront(shape);
            if (_propertiesPanelVisible)
            {
                _propertiesPanel.SetShape(shape);
            }
            Invalidate();
        }

        private void RefreshShapeList()
        {
            _shapeListNeedsRefresh = true;
        }

        private void UpdateShapeListIfNeeded()
        {
            if (_shapeListNeedsRefresh)
            {
                _shapeListNeedsRefresh = false;
                _shapeListPanel.RefreshList();
                _shapeListPanel.SelectShape(_shapeManager.SelectedShape);
            }
        }

        /// <summary>
        /// Создать контекстное меню для добавления новых фигур
        /// </summary>
        private void SetupCreateShapeMenu()
        {
            _createShapeMenu = new ContextMenuStrip();
            
            // Заголовок
            _createShapeMenu.Items.Add("Добавить фигуру:", null, null!).Enabled = false;
            _createShapeMenu.Items.Add(new ToolStripSeparator());
            
            // Стандартные фигуры
            _createShapeMenu.Items.Add("Окружность", null, CreateCircle_Click!);
            _createShapeMenu.Items.Add("Прямоугольник", null, CreateRectangle_Click!);
            _createShapeMenu.Items.Add("Треугольник", null, CreateTriangle_Click!);
            _createShapeMenu.Items.Add("Шестиугольник", null, CreateHexagon_Click!);
            _createShapeMenu.Items.Add("Трапеция", null, CreateTrapezoid_Click!);
            
            _createShapeMenu.Items.Add(new ToolStripSeparator());
            
            // Новые фигуры
            _createShapeMenu.Items.Add("Многоугольник (PolygonShape)", null, CreatePolygonShape_Click!);
            _createShapeMenu.Items.Add("Составная фигура (CompositeShape)", null, CreateCompositeShape_Click!);
            
            _createShapeMenu.Items.Add(new ToolStripSeparator());
            
            // Режим рисования
            _createShapeMenu.Items.Add("✏️ Рисовать многоугольник", null, StartDrawingPolygon_Click!);
            _createShapeMenu.Items.Add("📐 Пошаговое построение n-угольника", null, StartStepByStepPolygon_Click!);
            _createShapeMenu.Items.Add("⬡ Построить правильный n-угольник", null, CreateRegularPolygon_Click!);
            
            _createShapeMenu.Items.Add(new ToolStripSeparator());
            
            // Удаление
            _createShapeMenu.Items.Add("Удалить выбранную фигуру", null, DeleteSelectedShape_Click!);
            
            _createShapeMenu.Items.Add(new ToolStripSeparator());
            
            // Объединение выбранных фигур
            _createShapeMenu.Items.Add("Объединить выбранные фигуры", null, CombineSelectedShapes_Click!);
            
            _createShapeMenu.Items.Add(new ToolStripSeparator());
            
            // Группировка
            _createShapeMenu.Items.Add("📦 Сгруппировать (Ctrl+G)", null, GroupShapes_Click!);
            _createShapeMenu.Items.Add("📂 Разгруппировать (Ctrl+Shift+G)", null, UngroupShape_Click!);

            _createShapeMenu.Items.Add(new ToolStripSeparator());

            _createShapeMenu.Items.Add("💾 Сохранить всё (Ctrl+S)", null, SaveShapes_Click!);
            _createShapeMenu.Items.Add("💾 Сохранить выделенное", null, SaveSelectedShapes_Click!);
            _createShapeMenu.Items.Add("📂 Импортировать фигуры", null, ImportShapes_Click!);
        }

        private void MainForm_Load(object? sender, EventArgs e)
        {
            UpdatePropertiesPanelSize();
            _shapeManager.CreateInitialShapes(
                ClientSize.Width - 2 * CanvasMargin, 
                ClientSize.Height - 2 * CanvasMargin,
                CanvasMargin
            );
            RefreshShapeList();
            Invalidate();
        }

        private void MainForm_Resize(object? sender, EventArgs e)
        {
            UpdatePropertiesPanelSize();
            
            if (_shapeManager.ShapeCount == 0)
            {
                _shapeManager.CreateInitialShapes(
                    ClientSize.Width - 2 * CanvasMargin, 
                    ClientSize.Height - 2 * CanvasMargin,
                    CanvasMargin
                );
            }
            Invalidate();
        }

        /// <summary>
        /// Обновить размер панели свойств в зависимости от размера экрана
        /// </summary>
        private void UpdatePropertiesPanelSize()
        {
            int panelWidth = Math.Max(350, Math.Min(450, ClientSize.Width / 5));
            int panelHeight = Math.Max(700, ClientSize.Height * 85 / 100);

            _propertiesPanel.Size = new Size(panelWidth, panelHeight);
            _propertiesPanel.Location = new Point(10, 10);

            _propertiesPanel.CanvasBounds = new Rectangle(
                CanvasMargin,
                CanvasMargin,
                ClientSize.Width - 2 * CanvasMargin,
                ClientSize.Height - 2 * CanvasMargin
            );

            _propertiesPanel.UpdateLayout(panelWidth, panelHeight);

            int listWidth = Math.Max(220, Math.Min(300, ClientSize.Width / 7));
            _shapeListPanel.Size = new Size(listWidth, panelHeight);
            _shapeListPanel.Location = new Point(ClientSize.Width - listWidth - 10, 10);
        }

        private void MainForm_Paint(object? sender, PaintEventArgs e)
        {
            UpdateShapeListIfNeeded();
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            
            // Рисуем область для фигур (виртуальные границы с отступом)
            DrawCanvasArea(e.Graphics);
            
            // Рисуем все фигуры
            _shapeManager.DrawAll(e.Graphics);
            
            // === Отрисовка в режиме рисования ===
            if (_isDrawingMode)
            {
                DrawPolygonPreview(e.Graphics);
                DrawDrawingModeIndicator(e.Graphics);
            }
        }

        /// <summary>
        /// Нарисовать область для фигур с отступом от краёв
        /// </summary>
        private void DrawCanvasArea(Graphics g)
        {
            var canvasRect = new Rectangle(
                CanvasMargin, 
                CanvasMargin, 
                ClientSize.Width - 2 * CanvasMargin, 
                ClientSize.Height - 2 * CanvasMargin
            );
            
            // Рисуем границу области
            using (var pen = new Pen(Color.LightGray, 1))
            {
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                g.DrawRectangle(pen, canvasRect);
            }
        }

        private void MainForm_MouseDown(object? sender, MouseEventArgs e)
        {
            this.Focus(); // Возвращаем фокус форме от панели свойств
            
            // В режиме рисования обрабатываем отдельно
            if (_isDrawingMode)
            {
                HandleDrawingClick(e);
                return;
            }
            
            // Правый клик - показываем контекстное меню для создания фигур
            if (e.Button == MouseButtons.Right)
            {
                _menuClickLocation = e.Location;
                _createShapeMenu?.Show(this, e.Location);
                return;
            }
            
            if (e.Button == MouseButtons.Left)
            {
                // СНАЧАЛА проверяем попадание в фигуру
                var hitShape = _shapeManager.HitTest(e.Location);
                
                // Если попали в фигуру - выбираем её и начинаем перетаскивание
                if (hitShape != null)
                {
                    // Проверяем, зажата ли клавиша Ctrl для множественного выделения
                    bool isCtrlPressed = ModifierKeys.HasFlag(Keys.Control);
                    
                    if (isCtrlPressed)
                    {
                        // Множественное выделение с Ctrl
                        _shapeManager.ToggleSelection(hitShape);
                        _shapeManager.BringToFront(hitShape);
                    }
                    else
                    {
                        // Одиночное выделение без Ctrl
                        _shapeManager.SelectSingle(hitShape);
                        _shapeManager.BringToFront(hitShape);
                    }
                    
                    // Вычисляем смещение между точкой клика и позицией фигуры
                    // Это нужно для корректного перетаскивания - фигура не должна "прыгать" к курсору
                    _dragOffset = new Point(
                        e.Location.X - hitShape.WorldPosition.X,
                        e.Location.Y - hitShape.WorldPosition.Y
                    );
                    
                    // Запоминаем начальную позицию мыши
                    _lastMousePosition = e.Location;
                    
                    // Начинаем перетаскивание фигуры
                    _isDragging = true;
                    _isDraggingAnchor = false;
                    
                    // Обновляем панель свойств
                    if (_propertiesPanelVisible)
                    {
                        _propertiesPanel.SetShape(hitShape);
                    }
                    RefreshShapeList();
                }
                else
                {
                    // ЕСЛИ НЕ попали в фигуру - проверяем попадание в точку привязки выбранной фигуры
                    if (_shapeManager.SelectedShape != null && IsHitAnchor(_shapeManager.SelectedShape, e.Location))
                    {
                        // Начинаем перетаскивание точки привязки (локальное смещение)
                        _isDraggingAnchor = true;
                        _isDragging = false;
                    }
                    else
                    {
                        // Клик по пустому месту - снимаем выделение
                        _shapeManager.ClearSelection();
                        if (_propertiesPanelVisible)
                        {
                            _propertiesPanel.SetShape(null);
                        }
                        RefreshShapeList();
                    }
                }
                
                Invalidate();
            }
        }

        private void MainForm_MouseMove(object? sender, MouseEventArgs e)
        {
            // В режиме рисования - обновляем позицию для preview
            if (_isDrawingMode)
            {
                _currentMousePosition = e.Location;
                Invalidate();
                return;
            }
            
            // Перетаскивание точки привязки (локальное смещение внутри фигуры)
            // Фигура остаётся на месте, меняется только положение точки привязки относительно фигуры
            if (_isDraggingAnchor && _shapeManager.SelectedShape != null)
            {
                var shape = _shapeManager.SelectedShape;
                var center = shape.GetCenter();
                
                // Новое смещение точки привязки относительно центра фигуры
                int newOffsetX = e.Location.X - center.X;
                int newOffsetY = e.Location.Y - center.Y;
                
                // Сохраняем текущий центр фигуры
                var savedCenter = center;
                
                // Устанавливаем новое смещение
                shape.AnchorOffset = new Point(newOffsetX, newOffsetY);
                shape.AnchorPos = AnchorPosition.Custom;
                
                // Корректируем LocalAnchor так, чтобы геометрический центр остался на месте
                // Формула: LocalAnchor = Center - GlobalOrigin + AnchorOffset
                shape.LocalAnchor = new Point(
                    savedCenter.X - shape.GlobalOrigin.X + newOffsetX,
                    savedCenter.Y - shape.GlobalOrigin.Y + newOffsetY
                );
                
                // Обновляем виртуальные границы
                shape.RefreshBounds();
                
                // Обновляем панель свойств
                if (_propertiesPanelVisible)
                {
                    _propertiesPanel.UpdateProperties();
                }
                
                Invalidate();
                return;
            }
            
            // Перетаскивание фигуры - фигура следует за курсором
            if (_isDragging && _shapeManager.SelectedShape != null)
            {
                var shape = _shapeManager.SelectedShape;
                
                // Проверяем, действительно ли мышь переместилась
                int deltaX = Math.Abs(e.Location.X - (_lastMousePosition.X + _dragOffset.X));
                int deltaY = Math.Abs(e.Location.Y - (_lastMousePosition.Y + _dragOffset.Y));
                
                // Диагностика
                System.Diagnostics.Debug.WriteLine($"[DRAG] Mouse at ({e.Location.X},{e.Location.Y}), Last=({_lastMousePosition.X},{_lastMousePosition.Y}), Delta=({deltaX},{deltaY}), Offset=({_dragOffset.X},{_dragOffset.Y})");
                
                // Новая позиция = позиция курсора - смещение (чтобы сохранить относительную позицию клика)
                int newX = e.Location.X - _dragOffset.X;
                int newY = e.Location.Y - _dragOffset.Y;
                
                // Корректируем позицию, чтобы фигура не выходила за границы
                newX = Math.Max(CanvasMargin, Math.Min(ClientSize.Width - CanvasMargin, newX));
                newY = Math.Max(CanvasMargin, Math.Min(ClientSize.Height - CanvasMargin, newY));
                
                _shapeManager.SelectedShape.MoveTo(new Point(newX, newY));
                
                // Обновляем последнюю позицию мыши
                _lastMousePosition = e.Location;
                
                // Обновляем панель свойств
                if (_propertiesPanelVisible)
                {
                    _propertiesPanel.UpdateProperties();
                }
                
                Invalidate();
                return;
            }
            
            // Меняем курсор при наведении на точку привязки
            if (_shapeManager.SelectedShape != null)
            {
                if (IsHitAnchor(_shapeManager.SelectedShape, e.Location))
                {
                    Cursor = Cursors.Hand;
                }
                else
                {
                    Cursor = Cursors.Default;
                }
            }
        }

        private void MainForm_MouseUp(object? sender, MouseEventArgs e)
        {
            // Диагностика окончания перетаскивания
            if (_isDragging && _shapeManager.SelectedShape != null)
            {
                System.Diagnostics.Debug.WriteLine($"[DRAG] End - Shape final position: ({_shapeManager.SelectedShape.WorldPosition.X},{_shapeManager.SelectedShape.WorldPosition.Y})");
            }
            
            _isDragging = false;
            _isDraggingAnchor = false;
        }

        /// <summary>
        /// Проверить попадание в точку привязки фигуры
        /// </summary>
        private bool IsHitAnchor(ShapeBase shape, Point p)
        {
            int dx = p.X - shape.WorldPosition.X;
            int dy = p.Y - shape.WorldPosition.Y;
            return dx * dx + dy * dy <= AnchorHitRadius * AnchorHitRadius;
        }

        private void MainForm_KeyDown(object? sender, KeyEventArgs e)
        {
            // Приоритет для режима рисования
            if (_isDrawingMode)
            {
                switch (e.KeyCode)
                {
                    case Keys.Enter:
                        CompleteDrawing();
                        e.Handled = true;
                        return;
                }
            }
            
            // Ctrl+G — сгруппировать выбранные фигуры
            if (e.Control && !e.Shift && e.KeyCode == Keys.G)
            {
                var group = _shapeManager.GroupSelectedShapes();
                if (group != null)
                {
                    _propertiesPanel.SetShape(group);
                    Invalidate();
                }
                e.Handled = true;
                return;
            }
            
            // Ctrl+Shift+G — разгруппировать выбранную группу
            if (e.Control && e.Shift && e.KeyCode == Keys.G)
            {
                var ungrouped = _shapeManager.UngroupSelectedShape();
                if (ungrouped != null)
                {
                    _propertiesPanel.SetShape(_shapeManager.SelectedShape);
                    Invalidate();
                }
                e.Handled = true;
                return;
            }

            // Ctrl+S — сохранить
            if (e.Control && !e.Shift && e.KeyCode == Keys.S)
            {
                SaveShapesToFile();
                e.Handled = true;
                return;
            }

            // Ctrl+O — импортировать фигуры
            if (e.Control && !e.Shift && e.KeyCode == Keys.O)
            {
                ImportShapesFromFile(new Point(ClientSize.Width / 2, ClientSize.Height / 2));
                e.Handled = true;
                return;
            }
            
            switch (e.KeyCode)
            {
                case Keys.F4:
                    TogglePropertiesPanel();
                    break;
            }
        }

        private void TogglePropertiesPanel()
        {
            _propertiesPanelVisible = !_propertiesPanelVisible;
            _propertiesPanel.Visible = _propertiesPanelVisible;
            
            if (_propertiesPanelVisible && _shapeManager.SelectedShape != null)
            {
                _propertiesPanel.SetShape(_shapeManager.SelectedShape);
            }
        }

        private void PropertiesPanel_ShapeChanged(object? sender, EventArgs e)
        {
            RefreshShapeList();
            Invalidate();
        }

        private void OnAnchorPositionChanged(object? sender, AnchorPosition position)
        {
            if (_shapeManager.SelectedShape != null)
            {
                _shapeManager.SelectedShape.SetAnchorPosition(position);
                _propertiesPanel.UpdateProperties();
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            // Рисуем подсказку внизу экрана
            using (var font = new Font("Segoe UI", 12F))
            using (var brush = new SolidBrush(Color.Gray))
            {
                string hint = _isDrawingMode
                    ? "Клик - добавить точку | Двойной клик/Enter - завершить | Esc - отмена"
                    : "F4 - панель свойств | Esc - выход | Правый клик - меню | Ctrl+клик - множественное выделение | Ctrl+G - сгруппировать | Ctrl+Shift+G - разгруппировать";
                var size = e.Graphics.MeasureString(hint, font);
                e.Graphics.DrawString(hint, font, brush,
                    (ClientSize.Width - size.Width) / 2,
                    ClientSize.Height - size.Height - 15);
            }
        }

        #region Обработчики создания фигур

        private void CreateCircle_Click(object? sender, EventArgs e)
        {
            var circle = new CircleShape(_menuClickLocation, 80)
            {
                FillColor = Color.LightBlue
            };
            circle.SetBorder(0, 10f, Color.DarkBlue);
            _shapeManager.AddShape(circle);
            _shapeManager.Select(circle);
            Invalidate();
        }

        private void CreateRectangle_Click(object? sender, EventArgs e)
        {
            var rect = new RectangleShape(_menuClickLocation, 160, 100)
            {
                FillColor = Color.LightGreen
            };
            rect.SetBorder(0, 8f, Color.DarkGreen);
            rect.SetBorder(1, 8f, Color.Blue);
            rect.SetBorder(2, 8f, Color.Red);
            rect.SetBorder(3, 8f, Color.Purple);
            _shapeManager.AddShape(rect);
            _shapeManager.Select(rect);
            Invalidate();
        }

        private void CreateTriangle_Click(object? sender, EventArgs e)
        {
            var triangle = new TriangleShape(_menuClickLocation, 100)
            {
                FillColor = Color.LightYellow
            };
            triangle.SetBorder(0, 10f, Color.Orange);
            triangle.SetBorder(1, 10f, Color.DarkBlue);
            triangle.SetBorder(2, 10f, Color.Crimson);
            _shapeManager.AddShape(triangle);
            _shapeManager.Select(triangle);
            Invalidate();
        }

        private void CreateHexagon_Click(object? sender, EventArgs e)
        {
            var hexagon = new HexagonShape(_menuClickLocation, 80)
            {
                FillColor = Color.LightPink
            };
            for (int i = 0; i < 6; i++)
            {
                hexagon.SetBorder(i, 10f, Color.Purple);
            }
            _shapeManager.AddShape(hexagon);
            _shapeManager.Select(hexagon);
            Invalidate();
        }

        private void CreateTrapezoid_Click(object? sender, EventArgs e)
        {
            var trapezoid = new TrapezoidShape(_menuClickLocation, 180, 100, 120)
            {
                FillColor = Color.LightCoral
            };
            trapezoid.SetBorder(0, 10f, Color.DarkRed);
            trapezoid.SetBorder(1, 10f, Color.Blue);
            trapezoid.SetBorder(2, 10f, Color.DarkGreen);
            trapezoid.SetBorder(3, 10f, Color.Purple);
            _shapeManager.AddShape(trapezoid);
            _shapeManager.Select(trapezoid);
            Invalidate();
        }

        private void CreatePolygonShape_Click(object? sender, EventArgs e)
        {
            // Создаём пятиугольник
            var polygon = new PolygonShape(_menuClickLocation, new PointF(0, -70))
            {
                FillColor = Color.Lavender,
                IsClosed = true
            };
            
            // Добавляем 5 отрезков для пятиугольника
            polygon.AddSegmentByLengthAngle(82f, 54f);   // Первый отрезок
            polygon.AddSegmentByLengthAngle(82f, 72f);   // Второй отрезок
            polygon.AddSegmentByLengthAngle(82f, 72f);   // Третий отрезок
            polygon.AddSegmentByLengthAngle(82f, 72f);   // Четвёртый отрезок
            polygon.AddSegmentByLengthAngle(82f, 72f);   // Пятый отрезок
            
            for (int i = 0; i < 5; i++)
            {
                polygon.SetBorder(i, 10f, Color.Indigo);
            }
            
            _shapeManager.AddShape(polygon);
            _shapeManager.Select(polygon);
            Invalidate();
        }

        private void CreateCompositeShape_Click(object? sender, EventArgs e)
        {
            // Создаём составную фигуру из двух кругов
            var composite = new CompositeShape();
            
            // Добавляем два круга с небольшим смещением
            var circle1 = new CircleShape(new Point(_menuClickLocation.X - 30, _menuClickLocation.Y), 60)
            {
                FillColor = Color.LightBlue
            };
            circle1.SetBorder(0, 8f, Color.DarkBlue);
            
            var circle2 = new CircleShape(new Point(_menuClickLocation.X + 30, _menuClickLocation.Y), 60)
            {
                FillColor = Color.LightCoral
            };
            circle2.SetBorder(0, 8f, Color.DarkRed);
            
            composite.AddChild(circle1);
            composite.AddChild(circle2);
            composite.FillColor = Color.FromArgb(128, Color.Purple); // Полупрозрачная заливка
            
            _shapeManager.AddShape(composite);
            _shapeManager.Select(composite);
            Invalidate();
        }

        private void DeleteSelectedShape_Click(object? sender, EventArgs e)
        {
            if (_shapeManager.SelectedShape != null)
            {
                _shapeManager.RemoveShape(_shapeManager.SelectedShape);
                _propertiesPanel.SetShape(null);
                Invalidate();
            }
        }
        
        /// <summary>
        /// Объединить выбранные фигуры в новую фигуру с единым контуром (выпуклая оболочка)
        /// </summary>
        private void CombineSelectedShapes_Click(object? sender, EventArgs e)
        {
            // Минимум 2 фигуры для объединения
            if (_shapeManager.SelectedShapes.Count < 2)
            {
                MessageBox.Show(
                    "Выберите минимум 2 фигуры для объединения (Ctrl+клик для множественного выделения)",
                    "Объединение фигур",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }
            
            // Собираем полигоны из выбранных фигур
            var polygons = new List<PointF[]>();
            foreach (var shape in _shapeManager.SelectedShapes)
            {
                var vertices = shape.GetVertices();
                if (vertices != null && vertices.Length >= 3)
                {
                    polygons.Add(vertices);
                }
            }
            
            if (polygons.Count < 2)
            {
                MessageBox.Show(
                    "Недостаточно валидных фигур для объединения",
                    "Объединение фигур",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            
            // Выполняем объединение полигонов с помощью Clipper2
            var unionVertices = PolygonConverter.UnionPolygons(polygons);
            
            if (unionVertices == null || unionVertices.Length < 3)
            {
                MessageBox.Show(
                    "Не удалось выполнить объединение фигур",
                    "Объединение фигур",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            
            // Создаём новый многоугольник из вершин объединения
            var unifiedPolygon = PolygonShape.CreateFromVertices(unionVertices, true);
            
            if (unifiedPolygon == null)
            {
                MessageBox.Show(
                    "Не удалось создать объединённую фигуру",
                    "Объединение фигур",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            
            // Устанавливаем заливку и контур для новой фигуры
            unifiedPolygon.FillColor = Color.FromArgb(128, Color.Purple); // Полупрозрачная заливка
            
            // Устанавливаем единый контур для всех сторон
            for (int i = 0; i < unifiedPolygon.Segments.Count; i++)
            {
                unifiedPolygon.SetBorder(i, 3f, Color.DarkMagenta);
            }
            
            // Удаляем исходные фигуры из менеджера
            foreach (var shape in _shapeManager.SelectedShapes.ToList())
            {
                _shapeManager.RemoveShape(shape);
            }
            
            // Добавляем новую объединённую фигуру
            _shapeManager.AddShape(unifiedPolygon);
            _shapeManager.SelectSingle(unifiedPolygon);
            
            // Обновляем панель свойств
            if (_propertiesPanelVisible)
            {
                _propertiesPanel.SetShape(unifiedPolygon);
            }
            
            Invalidate();
        }
        
        /// <summary>
        /// Сгруппировать выбранные фигуры в GroupShape
        /// </summary>
        private void GroupShapes_Click(object? sender, EventArgs e)
        {
            var group = _shapeManager.GroupSelectedShapes();
            if (group != null)
            {
                if (_propertiesPanelVisible)
                {
                    _propertiesPanel.SetShape(group);
                }
                Invalidate();
            }
            else if (_shapeManager.SelectedShapes.Count < 2)
            {
                MessageBox.Show(
                    "Выберите минимум 2 фигуры для группировки (Ctrl+клик для множественного выделения)",
                    "Группировка фигур",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }
        
        /// <summary>
        /// Разгруппировать выбранную группу
        /// </summary>
        private void UngroupShape_Click(object? sender, EventArgs e)
        {
            var ungrouped = _shapeManager.UngroupSelectedShape();
            if (ungrouped != null)
            {
                if (_propertiesPanelVisible)
                {
                    _propertiesPanel.SetShape(_shapeManager.SelectedShape);
                }
                Invalidate();
            }
            else if (!_shapeManager.IsSelectedShapeGroup())
            {
                MessageBox.Show(
                    "Выберите группу фигур для разгруппировки",
                    "Разгруппировка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Создать правильный n-угольник с диалогом ввода количества сторон
        /// </summary>
        private void CreateRegularPolygon_Click(object? sender, EventArgs e)
        {
            // Запрашиваем количество сторон через InputBox
            string input = Microsoft.VisualBasic.Interaction.InputBox(
                "Введите количество сторон (3-100):",
                "Построение правильного многоугольника",
                "5");
            
            // Проверяем, что пользователь не отменил диалог
            if (string.IsNullOrEmpty(input))
                return;
            
            // Валидация ввода
            if (!int.TryParse(input, out int sides))
            {
                MessageBox.Show(
                    "Введите целое число.",
                    "Ошибка ввода",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            
            // Проверка диапазона
            if (sides < 3)
            {
                MessageBox.Show(
                    "Минимальное количество сторон: 3",
                    "Ошибка ввода",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            
            if (sides > 100)
            {
                MessageBox.Show(
                    "Максимальное количество сторон: 100",
                    "Ошибка ввода",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            
            // Создаём правильный многоугольник с радиусом 80
            const float radius = 80f;
            var polygon = PolygonShape.CreateRegularPolygon(_menuClickLocation, sides, radius);
            polygon.FillColor = Color.Lavender;
            
            // Устанавливаем стили границ
            for (int i = 0; i < polygon.Segments.Count; i++)
            {
                polygon.SetBorder(i, 10f, Color.Indigo);
            }
            
            _shapeManager.AddShape(polygon);
            _shapeManager.Select(polygon);
            
            // Открываем панель свойств
            if (!_propertiesPanelVisible)
            {
                TogglePropertiesPanel();
            }
            _propertiesPanel.SetShape(polygon);
            
            Invalidate();
        }

        #endregion

        #region Режим рисования многоугольника

        /// <summary>
        /// Начать режим рисования многоугольника
        /// </summary>
        private void StartDrawingPolygon_Click(object? sender, EventArgs e)
        {
            // Входим в режим рисования
            _isDrawingMode = true;
            _drawingPoints.Clear();
            _drawingShape = null;
            
            // Первая точка - место клика контекстного меню
            _drawingPoints.Add(new PointF(_menuClickLocation.X, _menuClickLocation.Y));
            
            // Создаём пустой многоугольник
            _drawingShape = new PolygonShape(_menuClickLocation, PointF.Empty)
            {
                FillColor = Color.LightYellow,
                IsClosed = false  // Пока рисуем - незамкнутый
            };
            
            // Меняем курсор
            Cursor = Cursors.Cross;
            
            Invalidate();
        }

        /// <summary>
        /// Обработка клика в режиме рисования
        /// </summary>
        private void HandleDrawingClick(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            
            // Проверяем клик на первую точку для замыкания (нужно минимум 3 точки)
            if (_drawingPoints.Count >= 3)
            {
                const float closeThreshold = 15f; // Радиус захвата первой точки
                var firstPoint = _drawingPoints[0];
                float dx = e.Location.X - firstPoint.X;
                float dy = e.Location.Y - firstPoint.Y;
                float distance = (float)Math.Sqrt(dx * dx + dy * dy);
                
                if (distance <= closeThreshold)
                {
                    CompleteDrawing();
                    return;
                }
            }
            
            // Добавляем новую точку
            _drawingPoints.Add(new PointF(e.Location.X, e.Location.Y));
            
            // Обновляем фигуру
            UpdateDrawingShape();
            
            Invalidate();
        }

        /// <summary>
        /// Двойной клик - завершить рисование
        /// </summary>
        private void MainForm_MouseDoubleClick(object? sender, MouseEventArgs e)
        {
            if (!_isDrawingMode) return;
            
            CompleteDrawing();
        }

        /// <summary>
        /// Обновить рисуемую фигуру на основе точек
        /// </summary>
        private void UpdateDrawingShape()
        {
            if (_drawingShape == null || _drawingPoints.Count < 2) return;
            
            // Очищаем существующие отрезки
            _drawingShape.Segments.Clear();
            
            // Первая точка относительно GlobalOrigin
            var origin = new PointF(
                _drawingPoints[0].X - _menuClickLocation.X,
                _drawingPoints[0].Y - _menuClickLocation.Y
            );
            _drawingShape.OriginPoint = origin;
            
            // Вычисляем отрезки между точками
            for (int i = 1; i < _drawingPoints.Count; i++)
            {
                var prev = _drawingPoints[i - 1];
                var curr = _drawingPoints[i];
                
                // Длина отрезка
                float dx = curr.X - prev.X;
                float dy = curr.Y - prev.Y;
                float length = (float)Math.Sqrt(dx * dx + dy * dy);
                
                // Угол относительно предыдущего отрезка
                float angle = CalculateSegmentAngle(i);
                
                _drawingShape.AddSegmentByLengthAngle(length, angle);
            }
        }

        /// <summary>
        /// Вычислить угол отрезка относительно предыдущего
        /// </summary>
        private float CalculateSegmentAngle(int segmentIndex)
        {
            if (segmentIndex <= 0 || segmentIndex >= _drawingPoints.Count)
                return 0;
            
            var prev = _drawingPoints[segmentIndex - 1];
            var curr = _drawingPoints[segmentIndex];
            
            // Угол относительно горизонтали
            float dx = curr.X - prev.X;
            float dy = curr.Y - prev.Y;
            float absoluteAngle = (float)(Math.Atan2(dy, dx) * 180 / Math.PI);
            
            // Для первого отрезка возвращаем абсолютный угол
            if (segmentIndex == 1)
                return absoluteAngle;
            
            // Для остальных - относительный угол
            var prevPrev = _drawingPoints[segmentIndex - 2];
            float prevDx = prev.X - prevPrev.X;
            float prevDy = prev.Y - prevPrev.Y;
            float prevAngle = (float)(Math.Atan2(prevDy, prevDx) * 180 / Math.PI);
            
            return absoluteAngle - prevAngle;
        }

        /// <summary>
        /// Завершить рисование многоугольника
        /// </summary>
        private void CompleteDrawing()
        {
            // Минимум 3 точки для многоугольника
            if (_drawingPoints.Count < 3)
            {
                CancelDrawing();
                return;
            }
            
            if (_drawingShape == null)
            {
                CancelDrawing();
                return;
            }
            
            // Замыкаем фигуру
            _drawingShape.IsClosed = true;
            
            // Добавляем замыкающий отрезок
            AddClosingSegment();
            
            // Устанавливаем стили границ
            for (int i = 0; i < _drawingShape.Segments.Count; i++)
            {
                _drawingShape.SetBorder(i, 2f, Color.DarkBlue);
            }
            
            // Добавляем в менеджер фигур
            _shapeManager.AddShape(_drawingShape);
            _shapeManager.Select(_drawingShape);
            
            // Открываем панель свойств для редактирования
            if (!_propertiesPanelVisible)
            {
                TogglePropertiesPanel();
            }
            _propertiesPanel.SetShape(_drawingShape);
            
            // Сбрасываем состояние
            ResetDrawingState();
            
            Invalidate();
        }

        /// <summary>
        /// Добавить замыкающий отрезок
        /// </summary>
        private void AddClosingSegment()
        {
            if (_drawingPoints.Count < 3 || _drawingShape == null) return;
            
            var last = _drawingPoints[_drawingPoints.Count - 1];
            var first = _drawingPoints[0];
            
            float dx = first.X - last.X;
            float dy = first.Y - last.Y;
            float length = (float)Math.Sqrt(dx * dx + dy * dy);
            
            // Вычисляем относительный угол
            var prev = _drawingPoints[_drawingPoints.Count - 2];
            float prevDx = last.X - prev.X;
            float prevDy = last.Y - prev.Y;
            float prevAngle = (float)(Math.Atan2(prevDy, prevDx) * 180 / Math.PI);
            float closingAngle = (float)(Math.Atan2(dy, dx) * 180 / Math.PI);
            
            _drawingShape.AddSegmentByLengthAngle(length, closingAngle - prevAngle);
        }

        /// <summary>
        /// Отменить рисование
        /// </summary>
        private void CancelDrawing()
        {
            ResetDrawingState();
            Invalidate();
        }

        /// <summary>
        /// Сбросить состояние рисования
        /// </summary>
        private void ResetDrawingState()
        {
            _isDrawingMode = false;
            _drawingPoints.Clear();
            _drawingShape = null;
            Cursor = Cursors.Default;
        }

        /// <summary>
        /// Нарисовать превью рисуемого многоугольника
        /// </summary>
        private void DrawPolygonPreview(Graphics g)
        {
            if (_drawingPoints.Count == 0) return;
            
            // 1. Рисуем линии между добавленными точками
            if (_drawingPoints.Count >= 2)
            {
                using (var pen = new Pen(Color.DodgerBlue, 2f))
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    for (int i = 0; i < _drawingPoints.Count - 1; i++)
                    {
                        g.DrawLine(pen, _drawingPoints[i], _drawingPoints[i + 1]);
                    }
                }
            }
            
            // 2. Рисуем preview линию к текущей позиции мыши
            if (_drawingPoints.Count >= 1)
            {
                var lastPoint = _drawingPoints[_drawingPoints.Count - 1];
                
                // Пунктирная линия к курсору
                using (var pen = new Pen(Color.Gray, 1f))
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                    g.DrawLine(pen, lastPoint, _currentMousePosition);
                }
                
                // Preview замыкающей линии (если 3+ точек)
                if (_drawingPoints.Count >= 3)
                {
                    using (var pen = new Pen(Color.LightGray, 1f))
                    {
                        pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                        g.DrawLine(pen, _currentMousePosition, _drawingPoints[0]);
                    }
                }
            }
            
            // 3. Рисуем маркеры точек
            DrawPointMarkers(g);
        }

        /// <summary>
        /// Нарисовать маркеры добавленных точек
        /// </summary>
        private void DrawPointMarkers(Graphics g)
        {
            const int markerRadius = 6;
            
            for (int i = 0; i < _drawingPoints.Count; i++)
            {
                var point = _drawingPoints[i];
                
                // Первая точка - зелёная, увеличенная если 3+ точек (можно замкнуть)
                // Остальные - синие
                Color markerColor;
                int radius = markerRadius;
                
                if (i == 0)
                {
                    if (_drawingPoints.Count >= 3)
                    {
                        // Можно замкнуть - подсвечиваем ярко-зелёным и увеличиваем
                        markerColor = Color.LimeGreen;
                        radius = markerRadius + 4;
                    }
                    else
                    {
                        markerColor = Color.Green;
                    }
                }
                else
                {
                    markerColor = Color.DodgerBlue;
                }
                
                using (var brush = new SolidBrush(markerColor))
                using (var outlinePen = new Pen(Color.White, 2f))
                {
                    var rect = new RectangleF(
                        point.X - radius,
                        point.Y - radius,
                        radius * 2,
                        radius * 2
                    );
                    
                    g.FillEllipse(brush, rect);
                    g.DrawEllipse(outlinePen, rect);
                }
                
                // Номер точки
                using (var font = new Font("Segoe UI", 8f))
                using (var brush = new SolidBrush(Color.Black))
                {
                    var label = (i + 1).ToString();
                    g.DrawString(label, font, brush, point.X + 10, point.Y - 10);
                }
            }
        }

        /// <summary>
        /// Нарисовать индикатор режима рисования
        /// </summary>
        private void DrawDrawingModeIndicator(Graphics g)
        {
            if (!_isDrawingMode) return;
            
            string modeText = "РЕЖИМ РИСОВАНИЯ | Клик - добавить точку | Двойной клик/Enter - завершить | Esc - отмена";
            
            using (var font = new Font("Segoe UI", 14F, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.White))
            using (var bgBrush = new SolidBrush(Color.FromArgb(52, 152, 219)))
            {
                var textSize = g.MeasureString(modeText, font);
                var rect = new RectangleF(0, 0, ClientSize.Width, 40);
                
                g.FillRectangle(bgBrush, rect);
                g.DrawString(modeText, font, brush, 
                    (ClientSize.Width - textSize.Width) / 2, 10);
            }
        }

        #endregion

        #region Сохранение/Загрузка

        private void SaveShapes_Click(object? sender, EventArgs e)
        {
            SaveShapesToFile();
        }

        private void LoadShapes_Click(object? sender, EventArgs e)
        {
            ImportShapesFromFile(_menuClickLocation);
        }

        private void SaveSelectedShapes_Click(object? sender, EventArgs e)
        {
            SaveSelectedShapesToFile();
        }

        private void ImportShapes_Click(object? sender, EventArgs e)
        {
            ImportShapesFromFile(_menuClickLocation);
        }

        private void SaveShapesToFile()
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                dialog.DefaultExt = "json";
                dialog.Title = "Сохранить все фигуры";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _shapeManager.SaveToFile(dialog.FileName);
                        MessageBox.Show("Фигуры успешно сохранены", "Сохранение",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void SaveSelectedShapesToFile()
        {
            var selected = _shapeManager.SelectedShapes;
            if (selected.Count == 0)
            {
                MessageBox.Show("Выберите фигуры для сохранения (клик с Ctrl)", "Сохранение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                dialog.DefaultExt = "json";
                dialog.Title = "Сохранить выделенные фигуры";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _shapeManager.SaveShapesToFile(dialog.FileName, selected);
                        MessageBox.Show($"Сохранено фигур: {selected.Count}", "Сохранение",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ImportShapesFromFile(Point location)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                dialog.DefaultExt = "json";
                dialog.Title = "Импортировать фигуры на полотно";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _shapeManager.ClearSelection();
                        var imported = _shapeManager.AddFromFile(dialog.FileName, location);
                        if (_propertiesPanelVisible)
                        {
                            _propertiesPanel.SetShape(null);
                        }
                        Invalidate();
                        MessageBox.Show($"Импортировано фигур: {imported.Count}", "Импорт",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при импорте: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        #endregion

        #region Гарантированная обработка клавиш

        /// <summary>
        /// Переопределение для гарантированной обработки ESC и Alt+F4
        /// </summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // ESC всегда закрывает форму
            if (keyData == Keys.Escape)
            {
                Close();
                return true;
            }
            
            // Alt+F4 закрывает форму
            if (keyData == (Keys.Alt | Keys.F4))
            {
                Close();
                return true;
            }
            
            return base.ProcessCmdKey(ref msg, keyData);
        }

        #endregion

        #region Пошаговое построение многоугольника

        /// <summary>
        /// Начать пошаговое построение многоугольника
        /// </summary>
        private void StartStepByStepPolygon_Click(object? sender, EventArgs e)
        {
            // Входим в режим пошагового построения
            _isStepByStepMode = true;
            _stepByStepOrigin = _menuClickLocation;
            
            // Создаём пустой многоугольник
            _stepByStepShape = new PolygonShape(_stepByStepOrigin, PointF.Empty)
            {
                FillColor = Color.LightCyan,
                IsClosed = false
            };
            
            // Инициализируем начальные значения
            _currentAngleRadians = 0; // Начинаем с горизонтального направления вправо
            _currentEndPoint = new PointF(_stepByStepOrigin.X, _stepByStepOrigin.Y);
            
            // Запускаем цикл ввода
            ContinueStepByStepBuilding();
        }

        /// <summary>
        /// Продолжить пошаговое построение - показывает диалог для ввода длины или угла
        /// </summary>
        private void ContinueStepByStepBuilding()
        {
                if (!_isStepByStepMode || _stepByStepShape == null)
                    return;
                
                // Если это первая сторона - запрашиваем только длину
                // Иначе - запрашиваем длину и затем угол
                bool isFirstSide = _stepByStepShape.Segments.Count == 0;
                
                // Запрашиваем длину стороны
                using (var lengthDialog = new Dialogs.LengthInputDialog(100f, "Ввод длины стороны"))
                {
                    lengthDialog.StartPosition = FormStartPosition.CenterParent;
                    
                    if (lengthDialog.ShowDialog(this) != DialogResult.OK)
                    {
                        // Пользователь отменил - завершаем построение
                        CancelStepByStepBuilding();
                        return;
                    }
                    
                    float length = lengthDialog.Length;
                    
                    // Если это не первая сторона - запрашиваем угол
                    float angleDegrees = 0;
                    bool wantToComplete = false;
                    
                    if (!isFirstSide)
                    {
                        // Запрашиваем угол между предыдущей и новой стороной
                        using (var angleDialog = new Dialogs.AngleInputDialog(90f, true, "Ввод угла между сторонами"))
                        {
                            angleDialog.StartPosition = FormStartPosition.CenterParent;
                            
                            var result = angleDialog.ShowDialog(this);
                            
                            if (result == DialogResult.Cancel)
                            {
                                // Пользователь отменил - завершаем построение
                                CancelStepByStepBuilding();
                                return;
                            }
                            
                            angleDegrees = angleDialog.Angle;
                            wantToComplete = angleDialog.IsCompleteRequested;
                        }
                    }
                    
                    // Добавляем новый отрезок
                    if (isFirstSide)
                    {
                        // Первая сторона - угол 0 (горизонтально вправо)
                        _stepByStepShape.AddSegmentByLengthAngle(length, 0);
                    }
                    else
                    {
                        // Преобразуем угол в относительный поворот
                        // angleDegrees - это внутренний угол между сторонами
                        // Для поворота нужно: 180 - angleDegrees
                        float turnAngle = 180 - angleDegrees;
                        _stepByStepShape.AddSegmentByLengthAngle(length, turnAngle);
                    }
                    
                    // Обновляем текущий угол и конечную точку
                    _currentAngleRadians += angleDegrees * (float)Math.PI / 180;
                    _currentEndPoint = new PointF(
                        _currentEndPoint.X + length * (float)Math.Cos(_currentAngleRadians),
                        _currentEndPoint.Y + length * (float)Math.Sin(_currentAngleRadians)
                    );
                    
                    // Устанавливаем стиль границы для новой стороны
                    int sideIndex = _stepByStepShape.Segments.Count - 1;
                    _stepByStepShape.SetBorder(sideIndex, 2f, Color.DarkCyan);
                    
                    // Обновляем отображение
                    Invalidate();
                    
                    // Проверяем, хочет ли пользователь завершить
                    if (wantToComplete)
                    {
                        CompleteStepByStepBuilding();
                    }
                    else
                    {
                        // Продолжаем ввод
                        ContinueStepByStepBuilding();
                    }
                }
            }

        /// <summary>
        /// Завершить пошаговое построение многоугольника
        /// </summary>
        private void CompleteStepByStepBuilding()
        {
                if (!_isStepByStepMode || _stepByStepShape == null)
                    return;
                
                // Минимум 3 стороны для замкнутого многоугольника
                if (_stepByStepShape.Segments.Count < 3)
                {
                    MessageBox.Show(
                        "Для создания многоугольника необходимо минимум 3 стороны.",
                        "Пошаговое построение",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    
                    // Продолжаем ввод
                    ContinueStepByStepBuilding();
                    return;
                }
                
                // Замыкаем фигуру
                _stepByStepShape.IsClosed = true;
                
                // Добавляем замыкающий отрезок (от последней точки к начальной)
                AddStepByStepClosingSegment();
                
                // Устанавливаем стили границ для всех сторон
                for (int i = 0; i < _stepByStepShape.Segments.Count; i++)
                {
                    _stepByStepShape.SetBorder(i, 2f, Color.DarkCyan);
                }
                
                // Добавляем в менеджер фигур
                _shapeManager.AddShape(_stepByStepShape);
                _shapeManager.Select(_stepByStepShape);
                
                // Открываем панель свойств
                if (!_propertiesPanelVisible)
                {
                    TogglePropertiesPanel();
                }
                _propertiesPanel.SetShape(_stepByStepShape);
                
                // Сбрасываем состояние
                ResetStepByStepState();
                
                Invalidate();
            }

        /// <summary>
        /// Добавить замыкающий отрезок для пошагового построения
        /// </summary>
        private void AddStepByStepClosingSegment()
        {
                if (_stepByStepShape == null || _stepByStepShape.Segments.Count < 3)
                    return;
                
                // Вычисляем длину замыкающего отрезка
                float dx = _stepByStepOrigin.X - _currentEndPoint.X;
                float dy = _stepByStepOrigin.Y - _currentEndPoint.Y;
                float length = (float)Math.Sqrt(dx * dx + dy * dy);
                
                // Вычисляем угол замыкающего отрезка относительно последнего
                float closingAngle = (float)(Math.Atan2(dy, dx) * 180 / Math.PI);
                
                // Получаем угол последнего отрезка
                var lastSegment = _stepByStepShape.Segments[_stepByStepShape.Segments.Count - 1];
                float lastAngleDegrees = 0;
                
                // Суммируем все углы отрезков
                float totalAngle = 0;
                foreach (var segment in _stepByStepShape.Segments)
                {
                    totalAngle += segment.AngleDegrees;
                }
                
                // Относительный угол для замыкающего отрезка
                float relativeAngle = closingAngle - (_currentAngleRadians * 180 / (float)Math.PI);
                
                _stepByStepShape.AddSegmentByLengthAngle(length, relativeAngle);
            }

        /// <summary>
        /// Отменить пошаговое построение
        /// </summary>
        private void CancelStepByStepBuilding()
            {
                ResetStepByStepState();
                Invalidate();
            }

        /// <summary>
        /// Сбросить состояние пошагового построения
        /// </summary>
        private void ResetStepByStepState()
            {
                _isStepByStepMode = false;
                _stepByStepShape = null;
            }

        /// <summary>
        /// Нарисовать preview пошагового построения
        /// </summary>
        private void DrawStepByStepPreview(Graphics g)
            {
                if (!_isStepByStepMode || _stepByStepShape == null)
                    return;
                
                // Рисуем уже добавленные отрезки
                var vertices = _stepByStepShape.GetWorldPoints();
                if (vertices.Length >= 2)
                {
                    using (var pen = new Pen(Color.DarkCyan, 2f))
                    {
                        for (int i = 0; i < vertices.Length - 1; i++)
                        {
                            g.DrawLine(pen, vertices[i], vertices[i + 1]);
                        }
                    }
                }
                
                // Рисуем маркеры точек
                foreach (var vertex in vertices)
                {
                    using (var brush = new SolidBrush(Color.Cyan))
                    using (var outlinePen = new Pen(Color.White, 2f))
                    {
                        var rect = new RectangleF(vertex.X - 5, vertex.Y - 5, 10, 10);
                        g.FillEllipse(brush, rect);
                        g.DrawEllipse(outlinePen, rect);
                    }
                }
                
                // Рисуем начальную точку (зелёная)
                using (var startBrush = new SolidBrush(Color.LimeGreen))
                using (var startPen = new Pen(Color.White, 2f))
                {
                    var startRect = new RectangleF(_stepByStepOrigin.X - 7, _stepByStepOrigin.Y - 7, 14, 14);
                    g.FillEllipse(startBrush, startRect);
                    g.DrawEllipse(startPen, startRect);
                }
                
                // Рисуем текущую конечную точку (красная)
                using (var endBrush = new SolidBrush(Color.OrangeRed))
                using (var endPen = new Pen(Color.White, 2f))
                {
                    var endRect = new RectangleF(_currentEndPoint.X - 7, _currentEndPoint.Y - 7, 14, 14);
                    g.FillEllipse(endBrush, endRect);
                    g.DrawEllipse(endPen, endRect);
                }
                
                // Показываем информацию о текущем состоянии
                string infoText = $"Сторон: {_stepByStepShape.Segments.Count} | Введите следующую сторону";
                using (var font = new Font("Segoe UI", 10F))
                using (var brush = new SolidBrush(Color.DarkCyan))
                {
                    g.DrawString(infoText, font, brush, _stepByStepOrigin.X + 20, _stepByStepOrigin.Y - 20);
                }
            }

        /// <summary>
        /// Нарисовать индикатор режима пошагового построения
        /// </summary>
        private void DrawStepByStepModeIndicator(Graphics g)
            {
                if (!_isStepByStepMode) return;
                
                string modeText = "ПОШАГОВОЕ ПОСТРОЕНИЕ | Вводите длину и угол сторон | Завершить - для завершения";
                
                using (var font = new Font("Segoe UI", 14F, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.White))
                using (var bgBrush = new SolidBrush(Color.FromArgb(46, 204, 113)))
                {
                    var textSize = g.MeasureString(modeText, font);
                    var rect = new RectangleF(0, 0, ClientSize.Width, 40);
                    
                    g.FillRectangle(bgBrush, rect);
                    g.DrawString(modeText, font, brush,
                        (ClientSize.Width - textSize.Width) / 2, 10);
                }
            }

        #endregion
    }
}
