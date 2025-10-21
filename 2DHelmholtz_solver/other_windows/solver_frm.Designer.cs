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
            this.SuspendLayout();
            // 
            // richTextBox_AnalysisUpdate
            // 
            this.richTextBox_AnalysisUpdate.Location = new System.Drawing.Point(12, 12);
            this.richTextBox_AnalysisUpdate.Name = "richTextBox_AnalysisUpdate";
            this.richTextBox_AnalysisUpdate.Size = new System.Drawing.Size(650, 316);
            this.richTextBox_AnalysisUpdate.TabIndex = 0;
            this.richTextBox_AnalysisUpdate.Text = "";
            // 
            // button_performsolve
            // 
            this.button_performsolve.Location = new System.Drawing.Point(540, 343);
            this.button_performsolve.Name = "button_performsolve";
            this.button_performsolve.Size = new System.Drawing.Size(122, 37);
            this.button_performsolve.TabIndex = 1;
            this.button_performsolve.Text = "Solve";
            this.button_performsolve.UseVisualStyleBackColor = true;
            this.button_performsolve.Click += new System.EventHandler(this.button_performsolve_Click);
            // 
            // solver_frm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(674, 401);
            this.Controls.Add(this.button_performsolve);
            this.Controls.Add(this.richTextBox_AnalysisUpdate);
            this.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "solver_frm";
            this.Opacity = 0.85D;
            this.Text = "2D Helmholtz Solver";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBox_AnalysisUpdate;
        private System.Windows.Forms.Button button_performsolve;
    }
}