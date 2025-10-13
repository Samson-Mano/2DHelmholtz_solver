namespace _2DHelmholtz_solver.other_windows
{
    partial class bndrycondition_frm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(bndrycondition_frm));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.radioButton_sommerfield = new System.Windows.Forms.RadioButton();
            this.radioButton_newmann = new System.Windows.Forms.RadioButton();
            this.radioButton_dirichlet = new System.Windows.Forms.RadioButton();
            this.textBox_neumann = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_dirichlet = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_selectededges = new System.Windows.Forms.TextBox();
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
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.radioButton_sommerfield);
            this.groupBox1.Controls.Add(this.radioButton_newmann);
            this.groupBox1.Controls.Add(this.radioButton_dirichlet);
            this.groupBox1.Controls.Add(this.textBox_neumann);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.textBox_dirichlet);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 40);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(310, 262);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Constraint Data: ";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(37, 222);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(93, 15);
            this.label4.TabIndex = 9;
            this.label4.Text = "∂ϕ/∂n - ikϕ = 0";
            // 
            // radioButton_sommerfield
            // 
            this.radioButton_sommerfield.AutoSize = true;
            this.radioButton_sommerfield.Location = new System.Drawing.Point(11, 191);
            this.radioButton_sommerfield.Name = "radioButton_sommerfield";
            this.radioButton_sommerfield.Size = new System.Drawing.Size(256, 19);
            this.radioButton_sommerfield.TabIndex = 8;
            this.radioButton_sommerfield.Text = "ABC or Sommerfield Radiation Condition: ";
            this.radioButton_sommerfield.UseVisualStyleBackColor = true;
            // 
            // radioButton_newmann
            // 
            this.radioButton_newmann.AutoSize = true;
            this.radioButton_newmann.Location = new System.Drawing.Point(11, 108);
            this.radioButton_newmann.Name = "radioButton_newmann";
            this.radioButton_newmann.Size = new System.Drawing.Size(260, 19);
            this.radioButton_newmann.TabIndex = 7;
            this.radioButton_newmann.Text = "Natural or Newmann Boundary Condition: ";
            this.radioButton_newmann.UseVisualStyleBackColor = true;
            // 
            // radioButton_dirichlet
            // 
            this.radioButton_dirichlet.AutoSize = true;
            this.radioButton_dirichlet.Checked = true;
            this.radioButton_dirichlet.Location = new System.Drawing.Point(11, 31);
            this.radioButton_dirichlet.Name = "radioButton_dirichlet";
            this.radioButton_dirichlet.Size = new System.Drawing.Size(262, 19);
            this.radioButton_dirichlet.TabIndex = 6;
            this.radioButton_dirichlet.TabStop = true;
            this.radioButton_dirichlet.Text = "Essential or Dirichlet Boundary Condition: ";
            this.radioButton_dirichlet.UseVisualStyleBackColor = true;
            // 
            // textBox_neumann
            // 
            this.textBox_neumann.Location = new System.Drawing.Point(93, 133);
            this.textBox_neumann.Name = "textBox_neumann";
            this.textBox_neumann.Size = new System.Drawing.Size(100, 23);
            this.textBox_neumann.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(37, 136);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 15);
            this.label2.TabIndex = 4;
            this.label2.Text = "∂ϕ/∂n=";
            // 
            // textBox_dirichlet
            // 
            this.textBox_dirichlet.Location = new System.Drawing.Point(93, 56);
            this.textBox_dirichlet.Name = "textBox_dirichlet";
            this.textBox_dirichlet.Size = new System.Drawing.Size(100, 23);
            this.textBox_dirichlet.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(61, 59);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(26, 15);
            this.label1.TabIndex = 1;
            this.label1.Text = "ϕ =";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 314);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(93, 15);
            this.label3.TabIndex = 1;
            this.label3.Text = "Selected Edges: ";
            // 
            // textBox_selectededges
            // 
            this.textBox_selectededges.Location = new System.Drawing.Point(12, 332);
            this.textBox_selectededges.Multiline = true;
            this.textBox_selectededges.Name = "textBox_selectededges";
            this.textBox_selectededges.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_selectededges.Size = new System.Drawing.Size(310, 101);
            this.textBox_selectededges.TabIndex = 3;
            // 
            // button_applyconstraint
            // 
            this.button_applyconstraint.Location = new System.Drawing.Point(95, 454);
            this.button_applyconstraint.Name = "button_applyconstraint";
            this.button_applyconstraint.Size = new System.Drawing.Size(130, 28);
            this.button_applyconstraint.TabIndex = 4;
            this.button_applyconstraint.Text = "Apply Constraint";
            this.button_applyconstraint.UseVisualStyleBackColor = true;
            this.button_applyconstraint.Click += new System.EventHandler(this.button_applyconstraint_Click);
            // 
            // button_deleteconstraint
            // 
            this.button_deleteconstraint.Location = new System.Drawing.Point(95, 500);
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
            // bndrycondition_frm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(334, 551);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.button_deleteconstraint);
            this.Controls.Add(this.button_applyconstraint);
            this.Controls.Add(this.textBox_selectededges);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximumSize = new System.Drawing.Size(360, 600);
            this.MinimumSize = new System.Drawing.Size(350, 590);
            this.Name = "bndrycondition_frm";
            this.Opacity = 0.85D;
            this.Text = "Boundary Conditions";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.bndrycondition_frm_FormClosing);
            this.Load += new System.EventHandler(this.constraint_frm_Load);
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
        private System.Windows.Forms.TextBox textBox_neumann;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_dirichlet;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_selectededges;
        private System.Windows.Forms.Button button_applyconstraint;
        private System.Windows.Forms.Button button_deleteconstraint;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem rectangleSelectionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem circleSelectionToolStripMenuItem;
        private System.Windows.Forms.RadioButton radioButton_sommerfield;
        private System.Windows.Forms.RadioButton radioButton_newmann;
        private System.Windows.Forms.RadioButton radioButton_dirichlet;
        private System.Windows.Forms.Label label4;
    }
}