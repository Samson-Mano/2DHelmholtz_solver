namespace _2DHelmholtz_solver.other_windows
{
    partial class load_frm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(load_frm));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_loadamplitude = new System.Windows.Forms.TextBox();
            this.textBox_loadfrequency = new System.Windows.Forms.TextBox();
            this.textBox_loadphase = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.button_applyload = new System.Windows.Forms.Button();
            this.button_deleteload = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.textBox_loadphase);
            this.groupBox1.Controls.Add(this.textBox_loadfrequency);
            this.groupBox1.Controls.Add(this.textBox_loadamplitude);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(310, 170);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Load Data: ";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 39);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Load Amplitude: ";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(22, 81);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(101, 15);
            this.label2.TabIndex = 1;
            this.label2.Text = "Load Frequency: ";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(45, 124);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(75, 15);
            this.label3.TabIndex = 2;
            this.label3.Text = "Load Phase: ";
            // 
            // textBox_loadamplitude
            // 
            this.textBox_loadamplitude.Location = new System.Drawing.Point(126, 36);
            this.textBox_loadamplitude.Name = "textBox_loadamplitude";
            this.textBox_loadamplitude.Size = new System.Drawing.Size(100, 23);
            this.textBox_loadamplitude.TabIndex = 3;
            // 
            // textBox_loadfrequency
            // 
            this.textBox_loadfrequency.Location = new System.Drawing.Point(126, 78);
            this.textBox_loadfrequency.Name = "textBox_loadfrequency";
            this.textBox_loadfrequency.Size = new System.Drawing.Size(100, 23);
            this.textBox_loadfrequency.TabIndex = 4;
            // 
            // textBox_loadphase
            // 
            this.textBox_loadphase.Location = new System.Drawing.Point(126, 121);
            this.textBox_loadphase.Name = "textBox_loadphase";
            this.textBox_loadphase.Size = new System.Drawing.Size(100, 23);
            this.textBox_loadphase.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 192);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(96, 15);
            this.label4.TabIndex = 1;
            this.label4.Text = "Selected Nodes: ";
            // 
            // textBox4
            // 
            this.textBox4.Location = new System.Drawing.Point(12, 210);
            this.textBox4.Multiline = true;
            this.textBox4.Name = "textBox4";
            this.textBox4.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox4.Size = new System.Drawing.Size(310, 101);
            this.textBox4.TabIndex = 2;
            // 
            // button_applyload
            // 
            this.button_applyload.Location = new System.Drawing.Point(89, 332);
            this.button_applyload.Name = "button_applyload";
            this.button_applyload.Size = new System.Drawing.Size(130, 28);
            this.button_applyload.TabIndex = 3;
            this.button_applyload.Text = "Apply Load";
            this.button_applyload.UseVisualStyleBackColor = true;
            // 
            // button_deleteload
            // 
            this.button_deleteload.Location = new System.Drawing.Point(89, 378);
            this.button_deleteload.Name = "button_deleteload";
            this.button_deleteload.Size = new System.Drawing.Size(130, 28);
            this.button_deleteload.TabIndex = 4;
            this.button_deleteload.Text = "Delete Load";
            this.button_deleteload.UseVisualStyleBackColor = true;
            // 
            // load_frm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(334, 441);
            this.Controls.Add(this.button_deleteload);
            this.Controls.Add(this.button_applyload);
            this.Controls.Add(this.textBox4);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.MaximumSize = new System.Drawing.Size(360, 490);
            this.MinimumSize = new System.Drawing.Size(350, 480);
            this.Name = "load_frm";
            this.Opacity = 0.85D;
            this.Text = "Loads";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.load_frm_FormClosing);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_loadamplitude;
        private System.Windows.Forms.TextBox textBox_loadfrequency;
        private System.Windows.Forms.TextBox textBox_loadphase;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.Button button_applyload;
        private System.Windows.Forms.Button button_deleteload;
    }
}