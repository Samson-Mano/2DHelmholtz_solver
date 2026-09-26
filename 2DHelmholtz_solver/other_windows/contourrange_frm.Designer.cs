namespace _2DHelmholtz_solver.other_windows
{
    partial class contourrange_frm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(contourrange_frm));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_contourmax = new System.Windows.Forms.TextBox();
            this.textBox_contourmin = new System.Windows.Forms.TextBox();
            this.button_updaterange = new System.Windows.Forms.Button();
            this.button_resetrange = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(45, 36);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Contour Max: ";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(49, 79);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(97, 16);
            this.label2.TabIndex = 1;
            this.label2.Text = "Contour Min: ";
            // 
            // textBox_contourmax
            // 
            this.textBox_contourmax.Location = new System.Drawing.Point(152, 33);
            this.textBox_contourmax.Name = "textBox_contourmax";
            this.textBox_contourmax.Size = new System.Drawing.Size(68, 23);
            this.textBox_contourmax.TabIndex = 2;
            this.textBox_contourmax.Text = "1";
            // 
            // textBox_contourmin
            // 
            this.textBox_contourmin.Location = new System.Drawing.Point(152, 76);
            this.textBox_contourmin.Name = "textBox_contourmin";
            this.textBox_contourmin.Size = new System.Drawing.Size(68, 23);
            this.textBox_contourmin.TabIndex = 3;
            this.textBox_contourmin.Text = "0";
            // 
            // button_updaterange
            // 
            this.button_updaterange.Location = new System.Drawing.Point(48, 138);
            this.button_updaterange.Name = "button_updaterange";
            this.button_updaterange.Size = new System.Drawing.Size(87, 52);
            this.button_updaterange.TabIndex = 4;
            this.button_updaterange.Text = "Update Range";
            this.button_updaterange.UseVisualStyleBackColor = true;
            this.button_updaterange.Click += new System.EventHandler(this.button_updaterange_Click);
            // 
            // button_resetrange
            // 
            this.button_resetrange.Location = new System.Drawing.Point(152, 138);
            this.button_resetrange.Name = "button_resetrange";
            this.button_resetrange.Size = new System.Drawing.Size(87, 52);
            this.button_resetrange.TabIndex = 5;
            this.button_resetrange.Text = "Reset Range";
            this.button_resetrange.UseVisualStyleBackColor = true;
            this.button_resetrange.Click += new System.EventHandler(this.button_resetrange_Click);
            // 
            // contourrange_frm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(289, 224);
            this.Controls.Add(this.button_resetrange);
            this.Controls.Add(this.button_updaterange);
            this.Controls.Add(this.textBox_contourmin);
            this.Controls.Add(this.textBox_contourmax);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "contourrange_frm";
            this.Text = "Update Contour Range";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_contourmax;
        private System.Windows.Forms.TextBox textBox_contourmin;
        private System.Windows.Forms.Button button_updaterange;
        private System.Windows.Forms.Button button_resetrange;
    }
}