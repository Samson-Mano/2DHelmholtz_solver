namespace _2DHelmholtz_solver.other_windows
{
    partial class solver_frm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(solver_frm));
            this.richTextBox_AnalysisUpdate = new System.Windows.Forms.RichTextBox();
            this.button_performsolve = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_frequency = new System.Windows.Forms.TextBox();
            this.textBox_angularfrequency = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_wavelength = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_xyextent = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.comboBox_solvertype = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.comboBox_spectralorderN = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // richTextBox_AnalysisUpdate
            // 
            this.richTextBox_AnalysisUpdate.Font = new System.Drawing.Font("Cambria", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBox_AnalysisUpdate.Location = new System.Drawing.Point(12, 12);
            this.richTextBox_AnalysisUpdate.Name = "richTextBox_AnalysisUpdate";
            this.richTextBox_AnalysisUpdate.Size = new System.Drawing.Size(575, 267);
            this.richTextBox_AnalysisUpdate.TabIndex = 0;
            this.richTextBox_AnalysisUpdate.Text = "";
            // 
            // button_performsolve
            // 
            this.button_performsolve.Location = new System.Drawing.Point(464, 327);
            this.button_performsolve.Name = "button_performsolve";
            this.button_performsolve.Size = new System.Drawing.Size(123, 68);
            this.button_performsolve.TabIndex = 1;
            this.button_performsolve.Text = "Solve";
            this.button_performsolve.UseVisualStyleBackColor = true;
            this.button_performsolve.Click += new System.EventHandler(this.button_performsolve_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(69, 375);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(256, 20);
            this.label1.TabIndex = 2;
            this.label1.Text = "Wave field angular frequency (ω): ";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(118, 346);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(191, 20);
            this.label2.TabIndex = 3;
            this.label2.Text = "Wave field frequency (f): ";
            // 
            // textBox_frequency
            // 
            this.textBox_frequency.Location = new System.Drawing.Point(271, 343);
            this.textBox_frequency.Name = "textBox_frequency";
            this.textBox_frequency.Size = new System.Drawing.Size(100, 27);
            this.textBox_frequency.TabIndex = 4;
            this.textBox_frequency.TextChanged += new System.EventHandler(this.textBox_frequency_TextChanged);
            // 
            // textBox_angularfrequency
            // 
            this.textBox_angularfrequency.Enabled = false;
            this.textBox_angularfrequency.Location = new System.Drawing.Point(271, 372);
            this.textBox_angularfrequency.Name = "textBox_angularfrequency";
            this.textBox_angularfrequency.Size = new System.Drawing.Size(100, 27);
            this.textBox_angularfrequency.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(34, 404);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(302, 20);
            this.label3.TabIndex = 6;
            this.label3.Text = "If v = 3 × 10⁸ unit/s, wave length λ= v/f : \r\n";
            // 
            // textBox_wavelength
            // 
            this.textBox_wavelength.Enabled = false;
            this.textBox_wavelength.Location = new System.Drawing.Point(271, 401);
            this.textBox_wavelength.Name = "textBox_wavelength";
            this.textBox_wavelength.Size = new System.Drawing.Size(100, 27);
            this.textBox_wavelength.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(158, 433);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(142, 20);
            this.label4.TabIndex = 8;
            this.label4.Text = "Model X, Y extent: ";
            // 
            // textBox_xyextent
            // 
            this.textBox_xyextent.Enabled = false;
            this.textBox_xyextent.Location = new System.Drawing.Point(271, 430);
            this.textBox_xyextent.Name = "textBox_xyextent";
            this.textBox_xyextent.Size = new System.Drawing.Size(100, 27);
            this.textBox_xyextent.TabIndex = 9;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(377, 346);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(61, 20);
            this.label5.TabIndex = 10;
            this.label5.Text = "E+6 Hz";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(377, 375);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(96, 20);
            this.label6.TabIndex = 11;
            this.label6.Text = "E+6 rad/sec";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(377, 404);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(45, 20);
            this.label7.TabIndex = 12;
            this.label7.Text = "units";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(377, 433);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(45, 20);
            this.label8.TabIndex = 13;
            this.label8.Text = "units";
            // 
            // comboBox_solvertype
            // 
            this.comboBox_solvertype.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_solvertype.FormattingEnabled = true;
            this.comboBox_solvertype.Items.AddRange(new object[] {
            "Elimination method",
            "Lagrange Augmentation method"});
            this.comboBox_solvertype.Location = new System.Drawing.Point(271, 285);
            this.comboBox_solvertype.Name = "comboBox_solvertype";
            this.comboBox_solvertype.Size = new System.Drawing.Size(217, 27);
            this.comboBox_solvertype.TabIndex = 14;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(186, 288);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(103, 20);
            this.label9.TabIndex = 15;
            this.label9.Text = "Solver Type: ";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(153, 317);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(148, 20);
            this.label10.TabIndex = 17;
            this.label10.Text = "Spectral order (N): ";
            // 
            // comboBox_spectralorderN
            // 
            this.comboBox_spectralorderN.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_spectralorderN.FormattingEnabled = true;
            this.comboBox_spectralorderN.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10"});
            this.comboBox_spectralorderN.Location = new System.Drawing.Point(271, 314);
            this.comboBox_spectralorderN.Name = "comboBox_spectralorderN";
            this.comboBox_spectralorderN.Size = new System.Drawing.Size(69, 27);
            this.comboBox_spectralorderN.TabIndex = 18;
            this.comboBox_spectralorderN.SelectedIndexChanged += new System.EventHandler(this.comboBox_spectralorderN_SelectedIndexChanged);
            // 
            // solver_frm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(597, 543);
            this.Controls.Add(this.comboBox_spectralorderN);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.comboBox_solvertype);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBox_xyextent);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBox_wavelength);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox_angularfrequency);
            this.Controls.Add(this.textBox_frequency);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button_performsolve);
            this.Controls.Add(this.richTextBox_AnalysisUpdate);
            this.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximumSize = new System.Drawing.Size(615, 590);
            this.MinimumSize = new System.Drawing.Size(610, 585);
            this.Name = "solver_frm";
            this.Opacity = 0.85D;
            this.Text = "2D Helmholtz Solver";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.solver_frm_FormClosing);
            this.Load += new System.EventHandler(this.solver_frm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBox_AnalysisUpdate;
        private System.Windows.Forms.Button button_performsolve;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_frequency;
        private System.Windows.Forms.TextBox textBox_angularfrequency;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_wavelength;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_xyextent;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox comboBox_solvertype;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ComboBox comboBox_spectralorderN;
    }
}