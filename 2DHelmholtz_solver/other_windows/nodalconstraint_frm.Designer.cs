namespace _2DHelmholtz_solver.other_windows
{
    partial class nodalconstraint_frm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(nodalconstraint_frm));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textBox_neumann = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.checkBox_newmann = new System.Windows.Forms.CheckBox();
            this.textBox_dirichlet = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBox_dirichlet = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_selectednodes = new System.Windows.Forms.TextBox();
            this.button_applyconstraint = new System.Windows.Forms.Button();
            this.button_deleteconstraint = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.rectangleSelectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.circleSelectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.textBox_neumann);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.checkBox_newmann);
            this.groupBox1.Controls.Add(this.textBox_dirichlet);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.checkBox_dirichlet);
            this.groupBox1.Location = new System.Drawing.Point(12, 40);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(310, 195);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Nodal Constraint Data: ";
            // 
            // textBox_neumann
            // 
            this.textBox_neumann.Location = new System.Drawing.Point(93, 151);
            this.textBox_neumann.Name = "textBox_neumann";
            this.textBox_neumann.Size = new System.Drawing.Size(100, 23);
            this.textBox_neumann.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(37, 154);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 15);
            this.label2.TabIndex = 4;
            this.label2.Text = "∂ϕ/∂n=";
            // 
            // checkBox_newmann
            // 
            this.checkBox_newmann.AutoSize = true;
            this.checkBox_newmann.Location = new System.Drawing.Point(11, 119);
            this.checkBox_newmann.Name = "checkBox_newmann";
            this.checkBox_newmann.Size = new System.Drawing.Size(261, 19);
            this.checkBox_newmann.TabIndex = 3;
            this.checkBox_newmann.Text = "Natrual or Newmann Boundary Condition: ";
            this.checkBox_newmann.UseVisualStyleBackColor = true;
            // 
            // textBox_dirichlet
            // 
            this.textBox_dirichlet.Location = new System.Drawing.Point(93, 64);
            this.textBox_dirichlet.Name = "textBox_dirichlet";
            this.textBox_dirichlet.Size = new System.Drawing.Size(100, 23);
            this.textBox_dirichlet.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(61, 67);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(26, 15);
            this.label1.TabIndex = 1;
            this.label1.Text = "ϕ =";
            // 
            // checkBox_dirichlet
            // 
            this.checkBox_dirichlet.AutoSize = true;
            this.checkBox_dirichlet.Location = new System.Drawing.Point(11, 32);
            this.checkBox_dirichlet.Name = "checkBox_dirichlet";
            this.checkBox_dirichlet.Size = new System.Drawing.Size(263, 19);
            this.checkBox_dirichlet.TabIndex = 0;
            this.checkBox_dirichlet.Text = "Essential or Dirichlet Boundary Condition: ";
            this.checkBox_dirichlet.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 238);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(96, 15);
            this.label3.TabIndex = 1;
            this.label3.Text = "Selected Nodes: ";
            // 
            // textBox_selectednodes
            // 
            this.textBox_selectednodes.Location = new System.Drawing.Point(12, 256);
            this.textBox_selectednodes.Multiline = true;
            this.textBox_selectednodes.Name = "textBox_selectednodes";
            this.textBox_selectednodes.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_selectednodes.Size = new System.Drawing.Size(310, 101);
            this.textBox_selectednodes.TabIndex = 3;
            // 
            // button_applyconstraint
            // 
            this.button_applyconstraint.Location = new System.Drawing.Point(95, 377);
            this.button_applyconstraint.Name = "button_applyconstraint";
            this.button_applyconstraint.Size = new System.Drawing.Size(130, 28);
            this.button_applyconstraint.TabIndex = 4;
            this.button_applyconstraint.Text = "Apply Constraint";
            this.button_applyconstraint.UseVisualStyleBackColor = true;
            this.button_applyconstraint.Click += new System.EventHandler(this.button_applyconstraint_Click);
            // 
            // button_deleteconstraint
            // 
            this.button_deleteconstraint.Location = new System.Drawing.Point(95, 423);
            this.button_deleteconstraint.Name = "button_deleteconstraint";
            this.button_deleteconstraint.Size = new System.Drawing.Size(130, 28);
            this.button_deleteconstraint.TabIndex = 5;
            this.button_deleteconstraint.Text = "Delete Constraint";
            this.button_deleteconstraint.UseVisualStyleBackColor = true;
            this.button_deleteconstraint.Click += new System.EventHandler(this.button_deleteconstraint_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.rectangleSelectionToolStripMenuItem,
            this.circleSelectionToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(334, 24);
            this.menuStrip1.TabIndex = 6;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // rectangleSelectionToolStripMenuItem
            // 
            this.rectangleSelectionToolStripMenuItem.Checked = true;
            this.rectangleSelectionToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.rectangleSelectionToolStripMenuItem.Name = "rectangleSelectionToolStripMenuItem";
            this.rectangleSelectionToolStripMenuItem.Size = new System.Drawing.Size(122, 20);
            this.rectangleSelectionToolStripMenuItem.Text = "Rectangle Selection";
            this.rectangleSelectionToolStripMenuItem.Click += new System.EventHandler(this.rectangleSelectionToolStripMenuItem_Click);
            // 
            // circleSelectionToolStripMenuItem
            // 
            this.circleSelectionToolStripMenuItem.Name = "circleSelectionToolStripMenuItem";
            this.circleSelectionToolStripMenuItem.Size = new System.Drawing.Size(100, 20);
            this.circleSelectionToolStripMenuItem.Text = "Circle Selection";
            this.circleSelectionToolStripMenuItem.Click += new System.EventHandler(this.circleSelectionToolStripMenuItem_Click);
            // 
            // nodalconstraint_frm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(334, 471);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.button_deleteconstraint);
            this.Controls.Add(this.button_applyconstraint);
            this.Controls.Add(this.textBox_selectednodes);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximumSize = new System.Drawing.Size(360, 520);
            this.MinimumSize = new System.Drawing.Size(350, 510);
            this.Name = "nodalconstraint_frm";
            this.Opacity = 0.85D;
            this.Text = "Nodal Constraints";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.nodalconstraint_frm_FormClosing);
            this.Load += new System.EventHandler(this.nodalconstraint_frm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBox_dirichlet;
        private System.Windows.Forms.TextBox textBox_neumann;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox checkBox_newmann;
        private System.Windows.Forms.TextBox textBox_dirichlet;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_selectednodes;
        private System.Windows.Forms.Button button_applyconstraint;
        private System.Windows.Forms.Button button_deleteconstraint;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem rectangleSelectionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem circleSelectionToolStripMenuItem;
    }
}