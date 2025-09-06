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
            this.importModelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addLoadsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.solverToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.modalAnalysisToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.responseAnalysisToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.loadsToolStripMenuItem,
            this.solverToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(737, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importModelToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // importModelToolStripMenuItem
            // 
            this.importModelToolStripMenuItem.Name = "importModelToolStripMenuItem";
            this.importModelToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.importModelToolStripMenuItem.Text = "Import Model";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            // 
            // loadsToolStripMenuItem
            // 
            this.loadsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addLoadsToolStripMenuItem});
            this.loadsToolStripMenuItem.Name = "loadsToolStripMenuItem";
            this.loadsToolStripMenuItem.Size = new System.Drawing.Size(50, 20);
            this.loadsToolStripMenuItem.Text = "Loads";
            // 
            // addLoadsToolStripMenuItem
            // 
            this.addLoadsToolStripMenuItem.Name = "addLoadsToolStripMenuItem";
            this.addLoadsToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.addLoadsToolStripMenuItem.Text = "Add Loads";
            // 
            // solverToolStripMenuItem
            // 
            this.solverToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.modalAnalysisToolStripMenuItem,
            this.responseAnalysisToolStripMenuItem});
            this.solverToolStripMenuItem.Name = "solverToolStripMenuItem";
            this.solverToolStripMenuItem.Size = new System.Drawing.Size(51, 20);
            this.solverToolStripMenuItem.Text = "Solver";
            // 
            // modalAnalysisToolStripMenuItem
            // 
            this.modalAnalysisToolStripMenuItem.Name = "modalAnalysisToolStripMenuItem";
            this.modalAnalysisToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.modalAnalysisToolStripMenuItem.Text = "Modal Analysis";
            // 
            // responseAnalysisToolStripMenuItem
            // 
            this.responseAnalysisToolStripMenuItem.Name = "responseAnalysisToolStripMenuItem";
            this.responseAnalysisToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.responseAnalysisToolStripMenuItem.Text = "Response Analysis";
            // 
            // main_frm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(737, 534);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "main_frm";
            this.Text = "2D Helmholtz Solver";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
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
        private System.Windows.Forms.ToolStripMenuItem modalAnalysisToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem responseAnalysisToolStripMenuItem;
    }
}

