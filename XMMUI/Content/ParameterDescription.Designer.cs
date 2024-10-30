namespace XMMUI.Content
{
    partial class ParameterDescription
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            paramTypeLbl = new Label();
            label2 = new Label();
            paramNameLbl = new Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold | FontStyle.Underline, GraphicsUnit.Point, 0);
            label1.Location = new Point(3, 4);
            label1.Name = "label1";
            label1.Size = new Size(35, 15);
            label1.TabIndex = 0;
            label1.Text = "Type:";
            // 
            // paramTypeLbl
            // 
            paramTypeLbl.AutoSize = true;
            paramTypeLbl.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            paramTypeLbl.ForeColor = Color.FromArgb(0, 192, 0);
            paramTypeLbl.Location = new Point(51, 4);
            paramTypeLbl.Name = "paramTypeLbl";
            paramTypeLbl.Size = new Size(12, 15);
            paramTypeLbl.TabIndex = 1;
            paramTypeLbl.Text = "-";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold | FontStyle.Underline, GraphicsUnit.Point, 0);
            label2.Location = new Point(3, 27);
            label2.Name = "label2";
            label2.Size = new Size(42, 15);
            label2.TabIndex = 2;
            label2.Text = "Name:";
            // 
            // paramNameLbl
            // 
            paramNameLbl.AutoSize = true;
            paramNameLbl.Font = new Font("Segoe UI", 9F, FontStyle.Italic, GraphicsUnit.Point, 0);
            paramNameLbl.Location = new Point(51, 27);
            paramNameLbl.Name = "paramNameLbl";
            paramNameLbl.Size = new Size(12, 15);
            paramNameLbl.TabIndex = 3;
            paramNameLbl.Text = "-";
            // 
            // ParameterDescription
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.GradientInactiveCaption;
            Controls.Add(paramNameLbl);
            Controls.Add(label2);
            Controls.Add(paramTypeLbl);
            Controls.Add(label1);
            MaximumSize = new Size(160, 50);
            MinimumSize = new Size(160, 50);
            Name = "ParameterDescription";
            Size = new Size(160, 50);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label paramTypeLbl;
        private Label label2;
        private Label paramNameLbl;
    }
}
