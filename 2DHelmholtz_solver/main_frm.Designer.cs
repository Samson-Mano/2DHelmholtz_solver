namespace _2DHelmholtz_solver
{
    partial class main_frm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(main_frm));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importTXTFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importModelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportModelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addLoadsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addNodalConstraintsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addEdgeConstraintsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.materialPropertiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.solverToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dHelmholtzSolveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.glControl_main_panel = new OpenTK.GLControl();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel_zoom_value = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel_IsRefresh = new System.Windows.Forms.ToolStripStatusLabel();
            this.showResultsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.loadsToolStripMenuItem,
            this.solverToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(4, 1, 0, 1);
            this.menuStrip1.Size = new System.Drawing.Size(714, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importTXTFileToolStripMenuItem,
            this.importModelToolStripMenuItem,
            this.exportModelToolStripMenuItem,
            this.optionToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 22);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // importTXTFileToolStripMenuItem
            // 
            this.importTXTFileToolStripMenuItem.Name = "importTXTFileToolStripMenuItem";
            this.importTXTFileToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.importTXTFileToolStripMenuItem.Text = "Import TXT File";
            this.importTXTFileToolStripMenuItem.Click += new System.EventHandler(this.importTXTFileToolStripMenuItem_Click);
            // 
            // importModelToolStripMenuItem
            // 
            this.importModelToolStripMenuItem.Name = "importModelToolStripMenuItem";
            this.importModelToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.importModelToolStripMenuItem.Text = "Import Model";
            this.importModelToolStripMenuItem.Click += new System.EventHandler(this.importModelToolStripMenuItem_Click);
            // 
            // exportModelToolStripMenuItem
            // 
            this.exportModelToolStripMenuItem.Name = "exportModelToolStripMenuItem";
            this.exportModelToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.exportModelToolStripMenuItem.Text = "Export Model";
            this.exportModelToolStripMenuItem.Click += new System.EventHandler(this.exportModelToolStripMenuItem_Click);
            // 
            // optionToolStripMenuItem
            // 
            this.optionToolStripMenuItem.Name = "optionToolStripMenuItem";
            this.optionToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.optionToolStripMenuItem.Text = "Option";
            this.optionToolStripMenuItem.Click += new System.EventHandler(this.optionToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // loadsToolStripMenuItem
            // 
            this.loadsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addLoadsToolStripMenuItem,
            this.addNodalConstraintsToolStripMenuItem,
            this.addEdgeConstraintsToolStripMenuItem,
            this.materialPropertiesToolStripMenuItem});
            this.loadsToolStripMenuItem.Name = "loadsToolStripMenuItem";
            this.loadsToolStripMenuItem.Size = new System.Drawing.Size(50, 22);
            this.loadsToolStripMenuItem.Text = "Loads";
            // 
            // addLoadsToolStripMenuItem
            // 
            this.addLoadsToolStripMenuItem.Name = "addLoadsToolStripMenuItem";
            this.addLoadsToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.addLoadsToolStripMenuItem.Text = "Add Loads";
            this.addLoadsToolStripMenuItem.Click += new System.EventHandler(this.addLoadsToolStripMenuItem_Click);
            // 
            // addNodalConstraintsToolStripMenuItem
            // 
            this.addNodalConstraintsToolStripMenuItem.Name = "addNodalConstraintsToolStripMenuItem";
            this.addNodalConstraintsToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.addNodalConstraintsToolStripMenuItem.Text = "Add Nodal Constraints";
            this.addNodalConstraintsToolStripMenuItem.Click += new System.EventHandler(this.addNodalConstraintsToolStripMenuItem_Click);
            // 
            // addEdgeConstraintsToolStripMenuItem
            // 
            this.addEdgeConstraintsToolStripMenuItem.Name = "addEdgeConstraintsToolStripMenuItem";
            this.addEdgeConstraintsToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.addEdgeConstraintsToolStripMenuItem.Text = "Add Edge Constraints";
            this.addEdgeConstraintsToolStripMenuItem.Click += new System.EventHandler(this.addEdgeConstraintsToolStripMenuItem_Click);
            // 
            // materialPropertiesToolStripMenuItem
            // 
            this.materialPropertiesToolStripMenuItem.Name = "materialPropertiesToolStripMenuItem";
            this.materialPropertiesToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.materialPropertiesToolStripMenuItem.Text = "Material Properties";
            this.materialPropertiesToolStripMenuItem.Click += new System.EventHandler(this.materialPropertiesToolStripMenuItem_Click);
            // 
            // solverToolStripMenuItem
            // 
            this.solverToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.dHelmholtzSolveToolStripMenuItem,
            this.showResultsToolStripMenuItem});
            this.solverToolStripMenuItem.Name = "solverToolStripMenuItem";
            this.solverToolStripMenuItem.Size = new System.Drawing.Size(51, 22);
            this.solverToolStripMenuItem.Text = "Solver";
            // 
            // dHelmholtzSolveToolStripMenuItem
            // 
            this.dHelmholtzSolveToolStripMenuItem.Name = "dHelmholtzSolveToolStripMenuItem";
            this.dHelmholtzSolveToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.dHelmholtzSolveToolStripMenuItem.Text = "2D Helmholtz solve";
            this.dHelmholtzSolveToolStripMenuItem.Click += new System.EventHandler(this.dHelmholtzSolveToolStripMenuItem_Click);
            // 
            // glControl_main_panel
            // 
            this.glControl_main_panel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.glControl_main_panel.BackColor = System.Drawing.Color.Black;
            this.glControl_main_panel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.glControl_main_panel.Location = new System.Drawing.Point(65, 86);
            this.glControl_main_panel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.glControl_main_panel.Name = "glControl_main_panel";
            this.glControl_main_panel.Size = new System.Drawing.Size(138, 113);
            this.glControl_main_panel.TabIndex = 1;
            this.glControl_main_panel.VSync = false;
            this.glControl_main_panel.Load += new System.EventHandler(this.glControl_main_panel_Load);
            this.glControl_main_panel.SizeChanged += new System.EventHandler(this.glControl_main_panel_SizeChanged);
            this.glControl_main_panel.Paint += new System.Windows.Forms.PaintEventHandler(this.glControl_main_panel_Paint);
            this.glControl_main_panel.KeyDown += new System.Windows.Forms.KeyEventHandler(this.glControl_main_panel_KeyDown);
            this.glControl_main_panel.KeyUp += new System.Windows.Forms.KeyEventHandler(this.glControl_main_panel_KeyUp);
            this.glControl_main_panel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.glControl_main_panel_MouseDown);
            this.glControl_main_panel.MouseEnter += new System.EventHandler(this.glControl_main_panel_MouseEnter);
            this.glControl_main_panel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.glControl_main_panel_MouseMove);
            this.glControl_main_panel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.glControl_main_panel_MouseUp);
            this.glControl_main_panel.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.glControl_main_panel_MouseWheel);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel_zoom_value,
            this.toolStripStatusLabel_IsRefresh});
            this.statusStrip1.Location = new System.Drawing.Point(0, 362);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(714, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel_zoom_value
            // 
            this.toolStripStatusLabel_zoom_value.Name = "toolStripStatusLabel_zoom_value";
            this.toolStripStatusLabel_zoom_value.Size = new System.Drawing.Size(73, 17);
            this.toolStripStatusLabel_zoom_value.Text = "Zoom: 100%";
            // 
            // toolStripStatusLabel_IsRefresh
            // 
            this.toolStripStatusLabel_IsRefresh.Name = "toolStripStatusLabel_IsRefresh";
            this.toolStripStatusLabel_IsRefresh.Size = new System.Drawing.Size(10, 17);
            this.toolStripStatusLabel_IsRefresh.Text = " ";
            // 
            // showResultsToolStripMenuItem
            // 
            this.showResultsToolStripMenuItem.Name = "showResultsToolStripMenuItem";
            this.showResultsToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.showResultsToolStripMenuItem.Text = "Show Results";
            this.showResultsToolStripMenuItem.Click += new System.EventHandler(this.showResultsToolStripMenuItem_Click);
            // 
            // main_frm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(714, 384);
            this.Controls.Add(this.glControl_main_panel);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.statusStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "main_frm";
            this.Text = "2D Helmholtz Solver";
            this.Load += new System.EventHandler(this.main_frm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importModelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addLoadsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem solverToolStripMenuItem;
        private OpenTK.GLControl glControl_main_panel;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_zoom_value;
        private System.Windows.Forms.ToolStripMenuItem exportModelToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_IsRefresh;
        private System.Windows.Forms.ToolStripMenuItem addNodalConstraintsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem materialPropertiesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addEdgeConstraintsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dHelmholtzSolveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importTXTFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showResultsToolStripMenuItem;
    }
}

