namespace _2DHelmholtz_solver.other_windows
{
    partial class option_frm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(option_frm));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBox_paintshrinkmesh = new System.Windows.Forms.CheckBox();
            this.checkBox_paintmesh = new System.Windows.Forms.CheckBox();
            this.checkBox_paintmeshboundaries = new System.Windows.Forms.CheckBox();
            this.checkBox_paintloads = new System.Windows.Forms.CheckBox();
            this.checkBox_paintconstraints = new System.Windows.Forms.CheckBox();
            this.button_ok = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkBox_paintconstraints);
            this.groupBox1.Controls.Add(this.checkBox_paintloads);
            this.groupBox1.Controls.Add(this.checkBox_paintmeshboundaries);
            this.groupBox1.Controls.Add(this.checkBox_paintmesh);
            this.groupBox1.Controls.Add(this.checkBox_paintshrinkmesh);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(260, 227);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Drawing Option";
            // 
            // checkBox_paintshrinkmesh
            // 
            this.checkBox_paintshrinkmesh.AutoSize = true;
            this.checkBox_paintshrinkmesh.Location = new System.Drawing.Point(29, 109);
            this.checkBox_paintshrinkmesh.Name = "checkBox_paintshrinkmesh";
            this.checkBox_paintshrinkmesh.Size = new System.Drawing.Size(126, 19);
            this.checkBox_paintshrinkmesh.TabIndex = 0;
            this.checkBox_paintshrinkmesh.Text = "Paint Shrink Mesh";
            this.checkBox_paintshrinkmesh.UseVisualStyleBackColor = true;
            this.checkBox_paintshrinkmesh.CheckedChanged += new System.EventHandler(this.checkBox_paintshrinkmesh_CheckedChanged);
            // 
            // checkBox_paintmesh
            // 
            this.checkBox_paintmesh.AutoSize = true;
            this.checkBox_paintmesh.Location = new System.Drawing.Point(29, 35);
            this.checkBox_paintmesh.Name = "checkBox_paintmesh";
            this.checkBox_paintmesh.Size = new System.Drawing.Size(87, 19);
            this.checkBox_paintmesh.TabIndex = 1;
            this.checkBox_paintmesh.Text = "Paint Mesh";
            this.checkBox_paintmesh.UseVisualStyleBackColor = true;
            this.checkBox_paintmesh.CheckedChanged += new System.EventHandler(this.checkBox_paintmesh_CheckedChanged);
            // 
            // checkBox_paintmeshboundaries
            // 
            this.checkBox_paintmeshboundaries.AutoSize = true;
            this.checkBox_paintmeshboundaries.Location = new System.Drawing.Point(29, 72);
            this.checkBox_paintmeshboundaries.Name = "checkBox_paintmeshboundaries";
            this.checkBox_paintmeshboundaries.Size = new System.Drawing.Size(153, 19);
            this.checkBox_paintmeshboundaries.TabIndex = 2;
            this.checkBox_paintmeshboundaries.Text = "Paint Mesh Boundaries";
            this.checkBox_paintmeshboundaries.UseVisualStyleBackColor = true;
            this.checkBox_paintmeshboundaries.CheckedChanged += new System.EventHandler(this.checkBox_paintmeshboundaries_CheckedChanged);
            // 
            // checkBox_paintloads
            // 
            this.checkBox_paintloads.AutoSize = true;
            this.checkBox_paintloads.Location = new System.Drawing.Point(29, 149);
            this.checkBox_paintloads.Name = "checkBox_paintloads";
            this.checkBox_paintloads.Size = new System.Drawing.Size(90, 19);
            this.checkBox_paintloads.TabIndex = 3;
            this.checkBox_paintloads.Text = "Paint Loads";
            this.checkBox_paintloads.UseVisualStyleBackColor = true;
            this.checkBox_paintloads.CheckedChanged += new System.EventHandler(this.checkBox_paintloads_CheckedChanged);
            // 
            // checkBox_paintconstraints
            // 
            this.checkBox_paintconstraints.AutoSize = true;
            this.checkBox_paintconstraints.Location = new System.Drawing.Point(29, 189);
            this.checkBox_paintconstraints.Name = "checkBox_paintconstraints";
            this.checkBox_paintconstraints.Size = new System.Drawing.Size(120, 19);
            this.checkBox_paintconstraints.TabIndex = 4;
            this.checkBox_paintconstraints.Text = "Paint Constraints";
            this.checkBox_paintconstraints.UseVisualStyleBackColor = true;
            this.checkBox_paintconstraints.CheckedChanged += new System.EventHandler(this.checkBox_paintconstraints_CheckedChanged);
            // 
            // button_ok
            // 
            this.button_ok.Location = new System.Drawing.Point(93, 394);
            this.button_ok.Name = "button_ok";
            this.button_ok.Size = new System.Drawing.Size(92, 35);
            this.button_ok.TabIndex = 1;
            this.button_ok.Text = "Ok";
            this.button_ok.UseVisualStyleBackColor = true;
            this.button_ok.Click += new System.EventHandler(this.button_ok_Click);
            // 
            // option_frm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 441);
            this.Controls.Add(this.button_ok);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximumSize = new System.Drawing.Size(310, 490);
            this.MinimumSize = new System.Drawing.Size(300, 480);
            this.Name = "option_frm";
            this.Opacity = 0.85D;
            this.Text = "Options";
            this.Load += new System.EventHandler(this.option_frm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkBox_paintmeshboundaries;
        private System.Windows.Forms.CheckBox checkBox_paintmesh;
        private System.Windows.Forms.CheckBox checkBox_paintshrinkmesh;
        private System.Windows.Forms.CheckBox checkBox_paintconstraints;
        private System.Windows.Forms.CheckBox checkBox_paintloads;
        private System.Windows.Forms.Button button_ok;
    }
}