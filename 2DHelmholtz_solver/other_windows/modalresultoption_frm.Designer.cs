namespace _2DHelmholtz_solver.other_windows
{
    partial class modalresultoption_frm
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
            this.label_status = new System.Windows.Forms.Label();
            this.button_stop = new System.Windows.Forms.Button();
            this.button_play_pause = new System.Windows.Forms.Button();
            this.button_close = new System.Windows.Forms.Button();
            this.comboBox_modedata = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label_animation_speed = new System.Windows.Forms.Label();
            this.label_realtimeanim_speed = new System.Windows.Forms.Label();
            this.button_animation_speed = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label_status
            // 
            this.label_status.AutoSize = true;
            this.label_status.Location = new System.Drawing.Point(10, 183);
            this.label_status.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_status.Name = "label_status";
            this.label_status.Size = new System.Drawing.Size(48, 15);
            this.label_status.TabIndex = 34;
            this.label_status.Text = "Playing";
            // 
            // button_stop
            // 
            this.button_stop.Location = new System.Drawing.Point(13, 125);
            this.button_stop.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button_stop.Name = "button_stop";
            this.button_stop.Size = new System.Drawing.Size(155, 38);
            this.button_stop.TabIndex = 33;
            this.button_stop.Text = "Stop Animation";
            this.button_stop.UseVisualStyleBackColor = true;
            this.button_stop.Click += new System.EventHandler(this.button_stop_Click);
            // 
            // button_play_pause
            // 
            this.button_play_pause.Location = new System.Drawing.Point(13, 80);
            this.button_play_pause.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button_play_pause.Name = "button_play_pause";
            this.button_play_pause.Size = new System.Drawing.Size(155, 38);
            this.button_play_pause.TabIndex = 32;
            this.button_play_pause.Text = "Play Animation";
            this.button_play_pause.UseVisualStyleBackColor = true;
            this.button_play_pause.Click += new System.EventHandler(this.button_play_pause_Click);
            // 
            // button_close
            // 
            this.button_close.Location = new System.Drawing.Point(192, 366);
            this.button_close.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button_close.Name = "button_close";
            this.button_close.Size = new System.Drawing.Size(146, 53);
            this.button_close.TabIndex = 30;
            this.button_close.Text = "Close";
            this.button_close.UseVisualStyleBackColor = true;
            this.button_close.Click += new System.EventHandler(this.button_close_Click);
            // 
            // comboBox_modedata
            // 
            this.comboBox_modedata.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_modedata.FormattingEnabled = true;
            this.comboBox_modedata.Items.AddRange(new object[] {
            "Linear Interpolation",
            "Cubic bezier Interpolation",
            "Sine Interpolation",
            "Rectangular Interpolation",
            "Single Node"});
            this.comboBox_modedata.Location = new System.Drawing.Point(135, 27);
            this.comboBox_modedata.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.comboBox_modedata.Name = "comboBox_modedata";
            this.comboBox_modedata.Size = new System.Drawing.Size(401, 23);
            this.comboBox_modedata.TabIndex = 28;
            this.comboBox_modedata.SelectedIndexChanged += new System.EventHandler(this.comboBox_modedata_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 30);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(115, 15);
            this.label1.TabIndex = 27;
            this.label1.Text = "Natural Frequency: ";
            // 
            // label_animation_speed
            // 
            this.label_animation_speed.AutoSize = true;
            this.label_animation_speed.Location = new System.Drawing.Point(252, 35);
            this.label_animation_speed.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_animation_speed.Name = "label_animation_speed";
            this.label_animation_speed.Size = new System.Drawing.Size(24, 15);
            this.label_animation_speed.TabIndex = 2;
            this.label_animation_speed.Text = "1.0";
            // 
            // label_realtimeanim_speed
            // 
            this.label_realtimeanim_speed.AutoSize = true;
            this.label_realtimeanim_speed.Location = new System.Drawing.Point(7, 84);
            this.label_realtimeanim_speed.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_realtimeanim_speed.Name = "label_realtimeanim_speed";
            this.label_realtimeanim_speed.Size = new System.Drawing.Size(259, 15);
            this.label_realtimeanim_speed.TabIndex = 1;
            this.label_realtimeanim_speed.Text = "1 second in real time = 1 second in Animation";
            // 
            // button_animation_speed
            // 
            this.button_animation_speed.Location = new System.Drawing.Point(10, 25);
            this.button_animation_speed.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button_animation_speed.Name = "button_animation_speed";
            this.button_animation_speed.Size = new System.Drawing.Size(178, 37);
            this.button_animation_speed.TabIndex = 0;
            this.button_animation_speed.Text = "Animation Speed";
            this.button_animation_speed.UseVisualStyleBackColor = true;
            this.button_animation_speed.Click += new System.EventHandler(this.button_animation_speed_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label_animation_speed);
            this.groupBox1.Controls.Add(this.label_realtimeanim_speed);
            this.groupBox1.Controls.Add(this.button_animation_speed);
            this.groupBox1.Location = new System.Drawing.Point(14, 225);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox1.Size = new System.Drawing.Size(520, 135);
            this.groupBox1.TabIndex = 31;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Animation Speed: ";
            // 
            // modalresultoption_frm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(554, 441);
            this.Controls.Add(this.label_status);
            this.Controls.Add(this.button_stop);
            this.Controls.Add(this.button_play_pause);
            this.Controls.Add(this.button_close);
            this.Controls.Add(this.comboBox_modedata);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "modalresultoption_frm";
            this.Text = "Modal Result Options";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.modalresultoption_frm_FormClosing);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label_status;
        private System.Windows.Forms.Button button_stop;
        private System.Windows.Forms.Button button_play_pause;
        private System.Windows.Forms.Button button_close;
        private System.Windows.Forms.ComboBox comboBox_modedata;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label_animation_speed;
        private System.Windows.Forms.Label label_realtimeanim_speed;
        private System.Windows.Forms.Button button_animation_speed;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}