using System;
using System.Drawing;
using System.Windows.Forms;

namespace OOTPiSP_LR1.Dialogs
{
    /// <summary>
    /// Диалог для ввода длины стороны многоугольника
    /// </summary>
    public class LengthInputDialog : Form
    {
        private Label _promptLabel;
        private NumericUpDown _lengthNumericUpDown;
        private Button _okButton;
        private Button _cancelButton;
        private TableLayoutPanel _tableLayoutPanel;

        /// <summary>
        /// Введённая длина стороны
        /// </summary>
        public float Length
        {
            get => (float)_lengthNumericUpDown.Value;
            set => _lengthNumericUpDown.Value = (decimal)value;
        }

        /// <summary>
        /// Минимальная допустимая длина
        /// </summary>
        public float MinLength
        {
            get => (float)_lengthNumericUpDown.Minimum;
            set => _lengthNumericUpDown.Minimum = (decimal)value;
        }

        /// <summary>
        /// Максимальная допустимая длина
        /// </summary>
        public float MaxLength
        {
            get => (float)_lengthNumericUpDown.Maximum;
            set => _lengthNumericUpDown.Maximum = (decimal)value;
        }

        /// <summary>
        /// Создать диалог ввода длины
        /// </summary>
        /// <param name="defaultLength">Значение по умолчанию</param>
        /// <param name="title">Заголовок окна</param>
        public LengthInputDialog(float defaultLength = 100f, string title = "Ввод длины стороны")
        {
            InitializeComponents(defaultLength, title);
        }

        private void InitializeComponents(float defaultLength, string title)
        {
            // Настройка формы
            Text = title;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(300, 130);
            AcceptButton = _okButton;
            CancelButton = _cancelButton;

            // TableLayoutPanel для размещения элементов
            _tableLayoutPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(10),
                ColumnStyles =
                {
                    new ColumnStyle(SizeType.Percent, 40F),
                    new ColumnStyle(SizeType.Percent, 60F)
                },
                RowStyles =
                {
                    new RowStyle(SizeType.Absolute, 30F),
                    new RowStyle(SizeType.Absolute, 40F),
                    new RowStyle(SizeType.Absolute, 40F)
                }
            };

            // Метка
            _promptLabel = new Label
            {
                Text = "Длина стороны (px):",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10F)
            };
            _tableLayoutPanel.Controls.Add(_promptLabel, 0, 0);
            _tableLayoutPanel.SetColumnSpan(_promptLabel, 2);

            // Поле ввода
            _lengthNumericUpDown = new NumericUpDown
            {
                Dock = DockStyle.Fill,
                Minimum = 1m,
                Maximum = 2000m,
                Value = (decimal)defaultLength,
                DecimalPlaces = 1,
                Increment = 10m,
                Font = new Font("Segoe UI", 12F)
            };
            _tableLayoutPanel.Controls.Add(_lengthNumericUpDown, 0, 1);
            _tableLayoutPanel.SetColumnSpan(_lengthNumericUpDown, 2);

            // Панель кнопок
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Fill
            };

            // Кнопка OK
            _okButton = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Size = new Size(85, 30),
                Location = new Point(100, 5),
                Font = new Font("Segoe UI", 9F)
            };
            buttonPanel.Controls.Add(_okButton);

            // Кнопка Отмена
            _cancelButton = new Button
            {
                Text = "Отмена",
                DialogResult = DialogResult.Cancel,
                Size = new Size(85, 30),
                Location = new Point(195, 5),
                Font = new Font("Segoe UI", 9F)
            };
            buttonPanel.Controls.Add(_cancelButton);

            _tableLayoutPanel.Controls.Add(buttonPanel, 0, 2);
            _tableLayoutPanel.SetColumnSpan(buttonPanel, 2);

            Controls.Add(_tableLayoutPanel);

            // Фокус на поле ввода
            ActiveControl = _lengthNumericUpDown;
        }

        /// <summary>
        /// Показать диалог и получить длину
        /// </summary>
        /// <param name="owner">Родительская форма</param>
        /// <param name="length">Введённая длина (если OK)</param>
        /// <param name="defaultLength">Значение по умолчанию</param>
        /// <returns>true если пользователь нажал OK</returns>
        public static bool ShowDialog(IWin32Window owner, out float length, float defaultLength = 100f)
        {
            using var dialog = new LengthInputDialog(defaultLength);
            if (dialog.ShowDialog(owner) == DialogResult.OK)
            {
                length = dialog.Length;
                return true;
            }
            length = 0;
            return false;
        }
    }
}
