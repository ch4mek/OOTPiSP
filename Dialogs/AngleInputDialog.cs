using System;
using System.Drawing;
using System.Windows.Forms;

namespace OOTPiSP_LR1.Dialogs
{
    /// <summary>
    /// Диалог для ввода внутреннего угла многоугольника
    /// </summary>
    public class AngleInputDialog : Form
    {
        private Label _promptLabel;
        private NumericUpDown _angleNumericUpDown;
        private Button _okButton;
        private Button _cancelButton;
        private Button _completeButton;
        private TableLayoutPanel _tableLayoutPanel;

        /// <summary>
        /// Введённый угол в градусах
        /// </summary>
        public float Angle
        {
            get => (float)_angleNumericUpDown.Value;
            set => _angleNumericUpDown.Value = (decimal)value;
        }

        /// <summary>
        /// Показывать ли кнопку "Завершить построение"
        /// </summary>
        public bool ShowCompleteButton
        {
            get => _completeButton.Visible;
            set => _completeButton.Visible = value;
        }

        /// <summary>
        /// Была ли нажата кнопка "Завершить построение"
        /// </summary>
        public bool IsCompleteRequested { get; private set; }

        /// <summary>
        /// Создать диалог ввода угла
        /// </summary>
        /// <param name="defaultAngle">Значение по умолчанию (в градусах)</param>
        /// <param name="showComplete">Показывать кнопку "Завершить"</param>
        /// <param name="title">Заголовок окна</param>
        public AngleInputDialog(float defaultAngle = 90f, bool showComplete = false, string title = "Ввод внутреннего угла")
        {
            IsCompleteRequested = false;
            InitializeComponents(defaultAngle, showComplete, title);
        }

        private void InitializeComponents(float defaultAngle, bool showComplete, string title)
        {
            // Настройка формы
            Text = title;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            AcceptButton = _okButton;
            CancelButton = _cancelButton;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Padding = new Padding(10);

            // Заголовок
            _promptLabel = new Label
            {
                Text = "Введите внутренний угол (в градусах):",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10F),
                AutoSize = true
            };

            // Поле ввода с градусами
            _angleNumericUpDown = new NumericUpDown
            {
                Dock = DockStyle.Fill,
                Minimum = 0m,
                Maximum = 360m,
                Value = (decimal)defaultAngle,
                DecimalPlaces = 1,
                Increment = 5m,
                Font = new Font("Segoe UI", 12F)
            };

            // Кнопка завершения построения
            _completeButton = new Button
            {
                Text = "Завершить",
                Size = new Size(85, 30),
                Font = new Font("Segoe UI", 9F),
                Visible = showComplete,
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White
            };
            _completeButton.Click += CompleteButton_Click;

            // Кнопка OK
            _okButton = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Size = new Size(75, 30),
                Font = new Font("Segoe UI", 9F)
            };

            // Кнопка Отмена
            _cancelButton = new Button
            {
                Text = "Отмена",
                DialogResult = DialogResult.Cancel,
                Size = new Size(75, 30),
                Font = new Font("Segoe UI", 9F)
            };

            // Панель с кнопками
            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                AutoSize = true,
                Height = 40
            };

            // Добавляем кнопки в обратном порядке (справа налево)
            buttonPanel.Controls.Add(_cancelButton);
            buttonPanel.Controls.Add(_okButton);
            if (showComplete)
            {
                buttonPanel.Controls.Add(_completeButton);
            }

            // Основной контейнер
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                AutoSize = true
            };
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainPanel.Padding = new Padding(5);

            mainPanel.Controls.Add(_promptLabel, 0, 0);
            mainPanel.Controls.Add(_angleNumericUpDown, 0, 1);
            mainPanel.Controls.Add(buttonPanel, 0, 2);

            Controls.Add(mainPanel);

            // Фокус на поле ввода
            ActiveControl = _angleNumericUpDown;

            // Установка размера формы
            ClientSize = new Size(300, mainPanel.PreferredSize.Height + 20);
        }

        private void CompleteButton_Click(object? sender, EventArgs e)
        {
            IsCompleteRequested = true;
            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>
        /// Показать диалог и получить угол
        /// </summary>
        /// <param name="owner">Родительская форма</param>
        /// <param name="angle">Введённый угол (если OK)</param>
        /// <param name="defaultAngle">Значение по умолчанию</param>
        /// <param name="showComplete">Показывать кнопку завершения</param>
        /// <returns>Результат диалога</returns>
        public static DialogResult Show(IWin32Window owner, out float angle, 
                                         float defaultAngle = 90f, bool showComplete = false)
        {
            using var dialog = new AngleInputDialog(defaultAngle, showComplete);
            var result = dialog.ShowDialog(owner);
            angle = dialog.Angle;
            return result;
        }
    }
}
