namespace OOTPiSP_LR1
{
    partial class PropertiesPanel
    {
        private System.ComponentModel.IContainer components = null;

        // Точка привязки
        private System.Windows.Forms.Label labelAnchor;
        private System.Windows.Forms.TextBox textAnchorX;
        private System.Windows.Forms.TextBox textAnchorY;
        private System.Windows.Forms.Label labelAnchorX;
        private System.Windows.Forms.Label labelAnchorY;

        // Виртуальные границы
        private System.Windows.Forms.Label labelVirtualBounds;
        private System.Windows.Forms.Label labelBoundsLeft;
        private System.Windows.Forms.Label labelBoundsTop;
        private System.Windows.Forms.Label labelBoundsRight;
        private System.Windows.Forms.Label labelBoundsBottom;
        private System.Windows.Forms.TextBox textBoundsLeft;
        private System.Windows.Forms.TextBox textBoundsTop;
        private System.Windows.Forms.TextBox textBoundsRight;
        private System.Windows.Forms.TextBox textBoundsBottom;

        // Положение точки привязки
        private System.Windows.Forms.Label labelAnchorPosition;
        private System.Windows.Forms.ComboBox comboAnchorPosition;

        // Цвет заливки
        private System.Windows.Forms.Label labelFillColor;
        private System.Windows.Forms.Panel panelFillColor;

        // Грани
        private System.Windows.Forms.Label labelBorders;
        private System.Windows.Forms.Panel panelBorders;

        // Кнопки
        private System.Windows.Forms.Button buttonApply;

        // Размер фигуры
        private System.Windows.Forms.Label labelSize;
        private System.Windows.Forms.Button buttonSizeUp;
        private System.Windows.Forms.Button buttonSizeDown;

        // Длина стороны
        private System.Windows.Forms.Label labelSideLength;
        private System.Windows.Forms.ComboBox comboSideSelect;
        private System.Windows.Forms.TextBox textSideLength;
        private System.Windows.Forms.Button buttonSetSideLength;

        // Углы вершин
        private System.Windows.Forms.Label labelAngles;
        private System.Windows.Forms.ComboBox comboAngleSelect;
        private System.Windows.Forms.TextBox textAngleValue;
        private System.Windows.Forms.Button buttonSetAngle;

        // Разделители секций
        private System.Windows.Forms.Label separator1;
        private System.Windows.Forms.Label separator2;
        private System.Windows.Forms.Label separator3;
        private System.Windows.Forms.Label separator4;

        // Локальная точка привязки
        private System.Windows.Forms.Label labelLocalAnchor;
        private System.Windows.Forms.TextBox textLocalAnchorX;
        private System.Windows.Forms.TextBox textLocalAnchorY;
        private System.Windows.Forms.Label labelLocalAnchorX;
        private System.Windows.Forms.Label labelLocalAnchorY;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.labelAnchor = new System.Windows.Forms.Label();
            this.textAnchorX = new System.Windows.Forms.TextBox();
            this.textAnchorY = new System.Windows.Forms.TextBox();
            this.labelAnchorX = new System.Windows.Forms.Label();
            this.labelAnchorY = new System.Windows.Forms.Label();
            this.labelVirtualBounds = new System.Windows.Forms.Label();
            this.labelBoundsLeft = new System.Windows.Forms.Label();
            this.labelBoundsTop = new System.Windows.Forms.Label();
            this.labelBoundsRight = new System.Windows.Forms.Label();
            this.labelBoundsBottom = new System.Windows.Forms.Label();
            this.textBoundsLeft = new System.Windows.Forms.TextBox();
            this.textBoundsTop = new System.Windows.Forms.TextBox();
            this.textBoundsRight = new System.Windows.Forms.TextBox();
            this.textBoundsBottom = new System.Windows.Forms.TextBox();
            this.labelAnchorPosition = new System.Windows.Forms.Label();
            this.comboAnchorPosition = new System.Windows.Forms.ComboBox();
            this.labelFillColor = new System.Windows.Forms.Label();
            this.panelFillColor = new System.Windows.Forms.Panel();
            this.labelBorders = new System.Windows.Forms.Label();
            this.panelBorders = new System.Windows.Forms.Panel();
            this.buttonApply = new System.Windows.Forms.Button();
            this.labelLocalAnchor = new System.Windows.Forms.Label();
            this.textLocalAnchorX = new System.Windows.Forms.TextBox();
            this.textLocalAnchorY = new System.Windows.Forms.TextBox();
            this.labelLocalAnchorX = new System.Windows.Forms.Label();
            this.labelLocalAnchorY = new System.Windows.Forms.Label();
            this.labelSize = new System.Windows.Forms.Label();
            this.buttonSizeUp = new System.Windows.Forms.Button();
            this.buttonSizeDown = new System.Windows.Forms.Button();
            this.separator1 = new System.Windows.Forms.Label();
            this.separator2 = new System.Windows.Forms.Label();
            this.separator3 = new System.Windows.Forms.Label();
            this.labelSideLength = new System.Windows.Forms.Label();
            this.comboSideSelect = new System.Windows.Forms.ComboBox();
            this.textSideLength = new System.Windows.Forms.TextBox();
            this.buttonSetSideLength = new System.Windows.Forms.Button();
            this.labelAngles = new System.Windows.Forms.Label();
            this.comboAngleSelect = new System.Windows.Forms.ComboBox();
            this.textAngleValue = new System.Windows.Forms.TextBox();
            this.buttonSetAngle = new System.Windows.Forms.Button();
            this.separator4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            
            // Шрифт для всех элементов
            System.Drawing.Font mainFont = new System.Drawing.Font("Segoe UI", 12F);
            System.Drawing.Font boldFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            
            // 
            // labelAnchor
            // 
            this.labelAnchor.AutoSize = true;
            this.labelAnchor.Font = boldFont;
            this.labelAnchor.Location = new System.Drawing.Point(10, 10);
            this.labelAnchor.Name = "labelAnchor";
            this.labelAnchor.Size = new System.Drawing.Size(200, 28);
            this.labelAnchor.TabIndex = 0;
            this.labelAnchor.Text = "Абсолютная точка:";
            
            // 
            // labelAnchorX
            // 
            this.labelAnchorX.AutoSize = true;
            this.labelAnchorX.Font = mainFont;
            this.labelAnchorX.Location = new System.Drawing.Point(10, 45);
            this.labelAnchorX.Name = "labelAnchorX";
            this.labelAnchorX.Size = new System.Drawing.Size(30, 28);
            this.labelAnchorX.TabIndex = 1;
            this.labelAnchorX.Text = "X:";
            
            // 
            // textAnchorX
            // 
            this.textAnchorX.Font = mainFont;
            this.textAnchorX.Location = new System.Drawing.Point(50, 42);
            this.textAnchorX.Name = "textAnchorX";
            this.textAnchorX.Size = new System.Drawing.Size(70, 34);
            this.textAnchorX.TabIndex = 2;
            
            // 
            // labelAnchorY
            // 
            this.labelAnchorY.AutoSize = true;
            this.labelAnchorY.Font = mainFont;
            this.labelAnchorY.Location = new System.Drawing.Point(130, 45);
            this.labelAnchorY.Name = "labelAnchorY";
            this.labelAnchorY.Size = new System.Drawing.Size(30, 28);
            this.labelAnchorY.TabIndex = 3;
            this.labelAnchorY.Text = "Y:";
            
            // 
            // textAnchorY
            // 
            this.textAnchorY.Font = mainFont;
            this.textAnchorY.Location = new System.Drawing.Point(170, 42);
            this.textAnchorY.Name = "textAnchorY";
            this.textAnchorY.Size = new System.Drawing.Size(70, 34);
            this.textAnchorY.TabIndex = 4;
            
            // 
            // labelLocalAnchor
            // 
            this.labelLocalAnchor.AutoSize = true;
            this.labelLocalAnchor.Font = boldFont;
            this.labelLocalAnchor.Location = new System.Drawing.Point(10, 85);
            this.labelLocalAnchor.Name = "labelLocalAnchor";
            this.labelLocalAnchor.Size = new System.Drawing.Size(200, 28);
            this.labelLocalAnchor.TabIndex = 5;
            this.labelLocalAnchor.Text = "Локальная точка:";
            
            // 
            // labelLocalAnchorX
            // 
            this.labelLocalAnchorX.AutoSize = true;
            this.labelLocalAnchorX.Font = mainFont;
            this.labelLocalAnchorX.Location = new System.Drawing.Point(10, 120);
            this.labelLocalAnchorX.Name = "labelLocalAnchorX";
            this.labelLocalAnchorX.Size = new System.Drawing.Size(30, 28);
            this.labelLocalAnchorX.TabIndex = 6;
            this.labelLocalAnchorX.Text = "X:";
            
            // 
            // textLocalAnchorX
            // 
            this.textLocalAnchorX.Font = mainFont;
            this.textLocalAnchorX.Location = new System.Drawing.Point(50, 117);
            this.textLocalAnchorX.Name = "textLocalAnchorX";
            this.textLocalAnchorX.Size = new System.Drawing.Size(70, 34);
            this.textLocalAnchorX.TabIndex = 7;
            
            // 
            // labelLocalAnchorY
            // 
            this.labelLocalAnchorY.AutoSize = true;
            this.labelLocalAnchorY.Font = mainFont;
            this.labelLocalAnchorY.Location = new System.Drawing.Point(130, 120);
            this.labelLocalAnchorY.Name = "labelLocalAnchorY";
            this.labelLocalAnchorY.Size = new System.Drawing.Size(30, 28);
            this.labelLocalAnchorY.TabIndex = 8;
            this.labelLocalAnchorY.Text = "Y:";
            
            // 
            // textLocalAnchorY
            // 
            this.textLocalAnchorY.Font = mainFont;
            this.textLocalAnchorY.Location = new System.Drawing.Point(170, 117);
            this.textLocalAnchorY.Name = "textLocalAnchorY";
            this.textLocalAnchorY.Size = new System.Drawing.Size(70, 34);
            this.textLocalAnchorY.TabIndex = 9;
            
            // 
            // labelVirtualBounds
            // 
            this.labelVirtualBounds.AutoSize = true;
            this.labelVirtualBounds.Font = boldFont;
            this.labelVirtualBounds.Location = new System.Drawing.Point(10, 165);
            this.labelVirtualBounds.Name = "labelVirtualBounds";
            this.labelVirtualBounds.Size = new System.Drawing.Size(230, 28);
            this.labelVirtualBounds.TabIndex = 10;
            this.labelVirtualBounds.Text = "Виртуальные границы:";
            
            // 
            // labelBoundsLeft
            // 
            this.labelBoundsLeft.AutoSize = true;
            this.labelBoundsLeft.Font = mainFont;
            this.labelBoundsLeft.Location = new System.Drawing.Point(10, 200);
            this.labelBoundsLeft.Name = "labelBoundsLeft";
            this.labelBoundsLeft.Size = new System.Drawing.Size(80, 28);
            this.labelBoundsLeft.TabIndex = 11;
            this.labelBoundsLeft.Text = "Лево X:";
            
            // 
            // textBoundsLeft
            // 
            this.textBoundsLeft.Font = mainFont;
            this.textBoundsLeft.Location = new System.Drawing.Point(100, 197);
            this.textBoundsLeft.Name = "textBoundsLeft";
            this.textBoundsLeft.ReadOnly = true;
            this.textBoundsLeft.Size = new System.Drawing.Size(60, 34);
            this.textBoundsLeft.TabIndex = 12;
            
            // 
            // labelBoundsTop
            // 
            this.labelBoundsTop.AutoSize = true;
            this.labelBoundsTop.Font = mainFont;
            this.labelBoundsTop.Location = new System.Drawing.Point(170, 200);
            this.labelBoundsTop.Name = "labelBoundsTop";
            this.labelBoundsTop.Size = new System.Drawing.Size(75, 28);
            this.labelBoundsTop.TabIndex = 13;
            this.labelBoundsTop.Text = "Верх Y:";
            
            // 
            // textBoundsTop
            // 
            this.textBoundsTop.Font = mainFont;
            this.textBoundsTop.Location = new System.Drawing.Point(260, 197);
            this.textBoundsTop.Name = "textBoundsTop";
            this.textBoundsTop.ReadOnly = true;
            this.textBoundsTop.Size = new System.Drawing.Size(60, 34);
            this.textBoundsTop.TabIndex = 14;
            
            // 
            // labelBoundsRight
            // 
            this.labelBoundsRight.AutoSize = true;
            this.labelBoundsRight.Font = mainFont;
            this.labelBoundsRight.Location = new System.Drawing.Point(10, 240);
            this.labelBoundsRight.Name = "labelBoundsRight";
            this.labelBoundsRight.Size = new System.Drawing.Size(80, 28);
            this.labelBoundsRight.TabIndex = 15;
            this.labelBoundsRight.Text = "Право X:";
            
            // 
            // textBoundsRight
            // 
            this.textBoundsRight.Font = mainFont;
            this.textBoundsRight.Location = new System.Drawing.Point(100, 237);
            this.textBoundsRight.Name = "textBoundsRight";
            this.textBoundsRight.ReadOnly = true;
            this.textBoundsRight.Size = new System.Drawing.Size(60, 34);
            this.textBoundsRight.TabIndex = 16;
            
            // 
            // labelBoundsBottom
            // 
            this.labelBoundsBottom.AutoSize = true;
            this.labelBoundsBottom.Font = mainFont;
            this.labelBoundsBottom.Location = new System.Drawing.Point(170, 240);
            this.labelBoundsBottom.Name = "labelBoundsBottom";
            this.labelBoundsBottom.Size = new System.Drawing.Size(70, 28);
            this.labelBoundsBottom.TabIndex = 17;
            this.labelBoundsBottom.Text = "Низ Y:";
            
            // 
            // textBoundsBottom
            // 
            this.textBoundsBottom.Font = mainFont;
            this.textBoundsBottom.Location = new System.Drawing.Point(260, 237);
            this.textBoundsBottom.Name = "textBoundsBottom";
            this.textBoundsBottom.ReadOnly = true;
            this.textBoundsBottom.Size = new System.Drawing.Size(60, 34);
            this.textBoundsBottom.TabIndex = 18;
            
            // 
            // labelAnchorPosition
            // 
            this.labelAnchorPosition.AutoSize = true;
            this.labelAnchorPosition.Font = boldFont;
            this.labelAnchorPosition.Location = new System.Drawing.Point(10, 290);
            this.labelAnchorPosition.Name = "labelAnchorPosition";
            this.labelAnchorPosition.Size = new System.Drawing.Size(280, 28);
            this.labelAnchorPosition.TabIndex = 19;
            this.labelAnchorPosition.Text = "Положение точки привязки:";
            
            //
            // comboAnchorPosition
            //
            this.comboAnchorPosition.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboAnchorPosition.Font = mainFont;
            this.comboAnchorPosition.FormattingEnabled = true;
            this.comboAnchorPosition.Items.AddRange(new object[] {
                "Центр",
                "Верхний левый",
                "Верхний правый",
                "Нижний левый",
                "Нижний правый",
                "Верх",
                "Низ",
                "Лево",
                "Право",
                "Произвольно"
            });
            this.comboAnchorPosition.Location = new System.Drawing.Point(10, 323);
            this.comboAnchorPosition.Name = "comboAnchorPosition";
            this.comboAnchorPosition.Size = new System.Drawing.Size(180, 36);
            this.comboAnchorPosition.TabIndex = 20;
            this.comboAnchorPosition.SelectedIndexChanged += new System.EventHandler(this.comboAnchorPosition_SelectedIndexChanged);
            
            // 
            // labelFillColor
            // 
            this.labelFillColor.AutoSize = true;
            this.labelFillColor.Font = boldFont;
            this.labelFillColor.Location = new System.Drawing.Point(10, 370);
            this.labelFillColor.Name = "labelFillColor";
            this.labelFillColor.Size = new System.Drawing.Size(150, 28);
            this.labelFillColor.TabIndex = 21;
            this.labelFillColor.Text = "Цвет заливки:";
            
            // 
            // panelFillColor
            // 
            this.panelFillColor.BackColor = System.Drawing.Color.LightGray;
            this.panelFillColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelFillColor.Cursor = System.Windows.Forms.Cursors.Hand;
            this.panelFillColor.Location = new System.Drawing.Point(170, 367);
            this.panelFillColor.Name = "panelFillColor";
            this.panelFillColor.Size = new System.Drawing.Size(80, 34);
            this.panelFillColor.TabIndex = 22;
            this.panelFillColor.Click += new System.EventHandler(this.panelFillColor_Click);
            
            // 
            // labelSize
            // 
            this.labelSize.AutoSize = true;
            this.labelSize.Font = boldFont;
            this.labelSize.Location = new System.Drawing.Point(10, 415);
            this.labelSize.Name = "labelSize";
            this.labelSize.Size = new System.Drawing.Size(80, 28);
            this.labelSize.TabIndex = 25;
            this.labelSize.Text = "Размер:";
            
            // 
            // buttonSizeUp
            // 
            this.buttonSizeUp.Font = mainFont;
            this.buttonSizeUp.Location = new System.Drawing.Point(100, 412);
            this.buttonSizeUp.Name = "buttonSizeUp";
            this.buttonSizeUp.Size = new System.Drawing.Size(40, 34);
            this.buttonSizeUp.TabIndex = 26;
            this.buttonSizeUp.Text = "+";
            this.buttonSizeUp.UseVisualStyleBackColor = true;
            this.buttonSizeUp.Click += new System.EventHandler(this.buttonSizeUp_Click);
            
            // 
            // buttonSizeDown
            // 
            this.buttonSizeDown.Font = mainFont;
            this.buttonSizeDown.Location = new System.Drawing.Point(150, 412);
            this.buttonSizeDown.Name = "buttonSizeDown";
            this.buttonSizeDown.Size = new System.Drawing.Size(40, 34);
            this.buttonSizeDown.TabIndex = 27;
            this.buttonSizeDown.Text = "−";
            this.buttonSizeDown.UseVisualStyleBackColor = true;
            this.buttonSizeDown.Click += new System.EventHandler(this.buttonSizeDown_Click);
            
            // 
            // labelSideLength
            // 
            this.labelSideLength.AutoSize = true;
            this.labelSideLength.Font = boldFont;
            this.labelSideLength.Location = new System.Drawing.Point(10, 455);
            this.labelSideLength.Name = "labelSideLength";
            this.labelSideLength.Size = new System.Drawing.Size(180, 28);
            this.labelSideLength.TabIndex = 33;
            this.labelSideLength.Text = "Длина стороны:";
            
            // 
            // comboSideSelect
            // 
            this.comboSideSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboSideSelect.Font = mainFont;
            this.comboSideSelect.FormattingEnabled = true;
            this.comboSideSelect.Location = new System.Drawing.Point(10, 488);
            this.comboSideSelect.Name = "comboSideSelect";
            this.comboSideSelect.Size = new System.Drawing.Size(120, 36);
            this.comboSideSelect.TabIndex = 34;
            this.comboSideSelect.SelectedIndexChanged += new System.EventHandler(this.comboSideSelect_SelectedIndexChanged);
            
            // 
            // textSideLength
            // 
            this.textSideLength.Font = mainFont;
            this.textSideLength.Location = new System.Drawing.Point(140, 491);
            this.textSideLength.Name = "textSideLength";
            this.textSideLength.Size = new System.Drawing.Size(80, 34);
            this.textSideLength.TabIndex = 35;
            
            // 
            // buttonSetSideLength
            // 
            this.buttonSetSideLength.Font = mainFont;
            this.buttonSetSideLength.Location = new System.Drawing.Point(230, 488);
            this.buttonSetSideLength.Name = "buttonSetSideLength";
            this.buttonSetSideLength.Size = new System.Drawing.Size(90, 34);
            this.buttonSetSideLength.TabIndex = 36;
            this.buttonSetSideLength.Text = "Задать";
            this.buttonSetSideLength.UseVisualStyleBackColor = true;
            this.buttonSetSideLength.Click += new System.EventHandler(this.buttonSetSideLength_Click);
            
            // 
            // separator1
            // 
            this.separator1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.separator1.Location = new System.Drawing.Point(10, 280);
            this.separator1.Name = "separator1";
            this.separator1.Size = new System.Drawing.Size(330, 2);
            this.separator1.TabIndex = 30;
            this.separator1.Text = "";
            
            // 
            // separator2
            // 
            this.separator2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.separator2.Location = new System.Drawing.Point(10, 400);
            this.separator2.Name = "separator2";
            this.separator2.Size = new System.Drawing.Size(330, 2);
            this.separator2.TabIndex = 31;
            this.separator2.Text = "";
            
            // 
            // separator3
            // 
            this.separator3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.separator3.Location = new System.Drawing.Point(10, 530);
            this.separator3.Name = "separator3";
            this.separator3.Size = new System.Drawing.Size(330, 2);
            this.separator3.TabIndex = 32;
            this.separator3.Text = "";
            
            // 
            // labelBorders
            // 
            this.labelBorders.AutoSize = true;
            this.labelBorders.Font = boldFont;
            this.labelBorders.Location = new System.Drawing.Point(10, 540);
            this.labelBorders.Name = "labelBorders";
            this.labelBorders.Size = new System.Drawing.Size(90, 28);
            this.labelBorders.TabIndex = 28;
            this.labelBorders.Text = "Грани:";
            
            // 
            // panelBorders
            // 
            this.panelBorders.AutoScroll = true;
            this.panelBorders.Location = new System.Drawing.Point(10, 568);
            this.panelBorders.Name = "panelBorders";
            this.panelBorders.Size = new System.Drawing.Size(330, 200);
            this.panelBorders.TabIndex = 29;
            
            // 
            // buttonApply
            // 
            this.buttonApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonApply.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.buttonApply.Location = new System.Drawing.Point(10, 780);
            this.buttonApply.Name = "buttonApply";
            this.buttonApply.Size = new System.Drawing.Size(330, 50);
            this.buttonApply.TabIndex = 25;
            this.buttonApply.Text = "Применить изменения";
            this.buttonApply.UseVisualStyleBackColor = true;
            this.buttonApply.Click += new System.EventHandler(this.buttonApply_Click);
            
            // 
            // PropertiesPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.buttonSetSideLength);
            this.Controls.Add(this.textSideLength);
            this.Controls.Add(this.comboSideSelect);
            this.Controls.Add(this.labelSideLength);
            this.Controls.Add(this.buttonApply);
            this.Controls.Add(this.panelBorders);
            this.Controls.Add(this.labelBorders);
            this.Controls.Add(this.separator3);
            this.Controls.Add(this.separator2);
            this.Controls.Add(this.separator1);
            this.Controls.Add(this.buttonSizeDown);
            this.Controls.Add(this.buttonSizeUp);
            this.Controls.Add(this.labelSize);
            this.Controls.Add(this.panelFillColor);
            this.Controls.Add(this.labelFillColor);
            this.Controls.Add(this.comboAnchorPosition);
            this.Controls.Add(this.labelAnchorPosition);
            this.Controls.Add(this.textBoundsBottom);
            this.Controls.Add(this.labelBoundsBottom);
            this.Controls.Add(this.textBoundsRight);
            this.Controls.Add(this.labelBoundsRight);
            this.Controls.Add(this.textBoundsTop);
            this.Controls.Add(this.labelBoundsTop);
            this.Controls.Add(this.textBoundsLeft);
            this.Controls.Add(this.labelBoundsLeft);
            this.Controls.Add(this.labelVirtualBounds);
            this.Controls.Add(this.textLocalAnchorY);
            this.Controls.Add(this.labelLocalAnchorY);
            this.Controls.Add(this.textLocalAnchorX);
            this.Controls.Add(this.labelLocalAnchorX);
            this.Controls.Add(this.labelLocalAnchor);
            this.Controls.Add(this.textAnchorY);
            this.Controls.Add(this.labelAnchorY);
            this.Controls.Add(this.textAnchorX);
            this.Controls.Add(this.labelAnchorX);
            this.Controls.Add(this.labelAnchor);
            this.Name = "PropertiesPanel";
            this.Size = new System.Drawing.Size(350, 840);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
