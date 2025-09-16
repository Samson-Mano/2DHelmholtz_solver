namespace _2DHelmholtz_solver.other_windows
{
    partial class matprop_frm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(matprop_frm));
            this.dataGridView_MaterialList = new System.Windows.Forms.DataGridView();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_materialname = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_permittivity = new System.Windows.Forms.TextBox();
            this.textBox_permeability = new System.Windows.Forms.TextBox();
            this.textBox_conductivity = new System.Windows.Forms.TextBox();
            this.Column1_materialid = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2_materialname = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column3_permittivity = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column4_Permeability = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column5_Conductivity = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.button_create = new System.Windows.Forms.Button();
            this.button_update = new System.Windows.Forms.Button();
            this.button_delete = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox_selectedelements = new System.Windows.Forms.TextBox();
            this.button_assignmaterial = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_MaterialList)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridView_MaterialList
            // 
            this.dataGridView_MaterialList.AllowUserToAddRows = false;
            this.dataGridView_MaterialList.AllowUserToDeleteRows = false;
            this.dataGridView_MaterialList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView_MaterialList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView_MaterialList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1_materialid,
            this.Column2_materialname,
            this.Column3_permittivity,
            this.Column4_Permeability,
            this.Column5_Conductivity});
            this.dataGridView_MaterialList.Location = new System.Drawing.Point(14, 14);
            this.dataGridView_MaterialList.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.dataGridView_MaterialList.Name = "dataGridView_MaterialList";
            this.dataGridView_MaterialList.ReadOnly = true;
            this.dataGridView_MaterialList.Size = new System.Drawing.Size(717, 140);
            this.dataGridView_MaterialList.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button_delete);
            this.groupBox1.Controls.Add(this.button_update);
            this.groupBox1.Controls.Add(this.button_create);
            this.groupBox1.Controls.Add(this.textBox_conductivity);
            this.groupBox1.Controls.Add(this.textBox_permeability);
            this.groupBox1.Controls.Add(this.textBox_permittivity);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.textBox_materialname);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(14, 160);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(314, 206);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Material Data: ";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(52, 34);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(94, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Material Name: ";
            // 
            // textBox_materialname
            // 
            this.textBox_materialname.Location = new System.Drawing.Point(152, 31);
            this.textBox_materialname.Name = "textBox_materialname";
            this.textBox_materialname.Size = new System.Drawing.Size(100, 23);
            this.textBox_materialname.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(50, 63);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(96, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "Permittivity (ϵ):";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(42, 92);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(104, 15);
            this.label3.TabIndex = 3;
            this.label3.Text = "Permeability (μ): ";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(42, 121);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(104, 15);
            this.label4.TabIndex = 4;
            this.label4.Text = "Conductivity (σ): ";
            // 
            // textBox_permittivity
            // 
            this.textBox_permittivity.Location = new System.Drawing.Point(152, 60);
            this.textBox_permittivity.Name = "textBox_permittivity";
            this.textBox_permittivity.Size = new System.Drawing.Size(100, 23);
            this.textBox_permittivity.TabIndex = 5;
            // 
            // textBox_permeability
            // 
            this.textBox_permeability.Location = new System.Drawing.Point(152, 89);
            this.textBox_permeability.Name = "textBox_permeability";
            this.textBox_permeability.Size = new System.Drawing.Size(100, 23);
            this.textBox_permeability.TabIndex = 6;
            // 
            // textBox_conductivity
            // 
            this.textBox_conductivity.Location = new System.Drawing.Point(152, 118);
            this.textBox_conductivity.Name = "textBox_conductivity";
            this.textBox_conductivity.Size = new System.Drawing.Size(100, 23);
            this.textBox_conductivity.TabIndex = 7;
            // 
            // Column1_materialid
            // 
            this.Column1_materialid.HeaderText = "Material ID";
            this.Column1_materialid.Name = "Column1_materialid";
            this.Column1_materialid.ReadOnly = true;
            // 
            // Column2_materialname
            // 
            this.Column2_materialname.FillWeight = 180F;
            this.Column2_materialname.HeaderText = "Material Name";
            this.Column2_materialname.Name = "Column2_materialname";
            this.Column2_materialname.ReadOnly = true;
            this.Column2_materialname.Width = 180;
            // 
            // Column3_permittivity
            // 
            this.Column3_permittivity.FillWeight = 130F;
            this.Column3_permittivity.HeaderText = "Permittivity (ϵ)";
            this.Column3_permittivity.Name = "Column3_permittivity";
            this.Column3_permittivity.ReadOnly = true;
            this.Column3_permittivity.Width = 130;
            // 
            // Column4_Permeability
            // 
            this.Column4_Permeability.FillWeight = 130F;
            this.Column4_Permeability.HeaderText = "Permeability (μ)";
            this.Column4_Permeability.Name = "Column4_Permeability";
            this.Column4_Permeability.ReadOnly = true;
            this.Column4_Permeability.Width = 130;
            // 
            // Column5_Conductivity
            // 
            this.Column5_Conductivity.FillWeight = 130F;
            this.Column5_Conductivity.HeaderText = "Conductivity (σ)";
            this.Column5_Conductivity.Name = "Column5_Conductivity";
            this.Column5_Conductivity.ReadOnly = true;
            this.Column5_Conductivity.Width = 130;
            // 
            // button_create
            // 
            this.button_create.Location = new System.Drawing.Point(13, 163);
            this.button_create.Name = "button_create";
            this.button_create.Size = new System.Drawing.Size(93, 28);
            this.button_create.TabIndex = 8;
            this.button_create.Text = "Create";
            this.button_create.UseVisualStyleBackColor = true;
            // 
            // button_update
            // 
            this.button_update.Location = new System.Drawing.Point(112, 163);
            this.button_update.Name = "button_update";
            this.button_update.Size = new System.Drawing.Size(93, 28);
            this.button_update.TabIndex = 9;
            this.button_update.Text = "Update";
            this.button_update.UseVisualStyleBackColor = true;
            // 
            // button_delete
            // 
            this.button_delete.Location = new System.Drawing.Point(211, 163);
            this.button_delete.Name = "button_delete";
            this.button_delete.Size = new System.Drawing.Size(93, 28);
            this.button_delete.TabIndex = 10;
            this.button_delete.Text = "Delete";
            this.button_delete.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.button_assignmaterial);
            this.groupBox2.Controls.Add(this.textBox_selectedelements);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Location = new System.Drawing.Point(334, 160);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(397, 205);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Assign Material:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 31);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(112, 15);
            this.label5.TabIndex = 1;
            this.label5.Text = "Selected Elements: ";
            // 
            // textBox_selectedelements
            // 
            this.textBox_selectedelements.Location = new System.Drawing.Point(9, 49);
            this.textBox_selectedelements.Multiline = true;
            this.textBox_selectedelements.Name = "textBox_selectedelements";
            this.textBox_selectedelements.Size = new System.Drawing.Size(382, 108);
            this.textBox_selectedelements.TabIndex = 2;
            // 
            // button_assignmaterial
            // 
            this.button_assignmaterial.Location = new System.Drawing.Point(131, 163);
            this.button_assignmaterial.Name = "button_assignmaterial";
            this.button_assignmaterial.Size = new System.Drawing.Size(137, 28);
            this.button_assignmaterial.TabIndex = 3;
            this.button_assignmaterial.Text = "Assign Material";
            this.button_assignmaterial.UseVisualStyleBackColor = true;
            // 
            // matprop_frm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(744, 381);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.dataGridView_MaterialList);
            this.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximumSize = new System.Drawing.Size(760, 420);
            this.Name = "matprop_frm";
            this.Text = "Material Properties";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_MaterialList)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView_MaterialList;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_materialname;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1_materialid;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2_materialname;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column3_permittivity;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column4_Permeability;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column5_Conductivity;
        private System.Windows.Forms.TextBox textBox_conductivity;
        private System.Windows.Forms.TextBox textBox_permeability;
        private System.Windows.Forms.TextBox textBox_permittivity;
        private System.Windows.Forms.Button button_delete;
        private System.Windows.Forms.Button button_update;
        private System.Windows.Forms.Button button_create;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button button_assignmaterial;
        private System.Windows.Forms.TextBox textBox_selectedelements;
    }
}