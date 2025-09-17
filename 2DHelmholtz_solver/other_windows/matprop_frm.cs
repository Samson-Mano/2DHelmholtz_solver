using _2DHelmholtz_solver.src.model_store.fe_objects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _2DHelmholtz_solver.other_windows
{
    public partial class matprop_frm : Form
    {
       // private fedata_store fe_data;

        public matprop_frm()
        {
            InitializeComponent();

           // this.fe_data = fe_data;

        }


        public void update_material_data(List<material_data> fe_materials)
        {
            // Clear existing rows 
            dataGridView_MaterialList.Rows.Clear();

            // Add rows manually
            foreach (material_data mat in fe_materials)
            {
                dataGridView_MaterialList.Rows.Add(
                    mat.material_id.ToString(),
                    mat.material_name,
                    mat.material_permittivity.ToString("G"),
                    mat.material_permeability.ToString("G"),
                    mat.material_conductivity.ToString("G")
                );
            }

        }

        private void dataGridView_MaterialList_SelectionChanged(object sender, EventArgs e)
        {

            if (dataGridView_MaterialList.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridView_MaterialList.SelectedRows[0];


                // Check if the selected row is the first row (index 0)
                if (selectedRow.Index == 0)
                {
                    // First row update and delete not allowed
                    button_update.Enabled = false;
                    button_delete.Enabled = false;

                }
                else
                {
                    // Enable the update and delete for all other rows
                    button_update.Enabled = true;
                    button_delete.Enabled = true;

                }

                textBox_materialname.Text = selectedRow.Cells["Column2_materialname"].Value?.ToString();
                textBox_permittivity.Text = selectedRow.Cells["Column3_permittivity"].Value?.ToString();
                textBox_permeability.Text = selectedRow.Cells["Column4_Permeability"].Value?.ToString();
                textBox_conductivity.Text = selectedRow.Cells["Column5_Conductivity"].Value?.ToString();

            }

        }

        private void button_create_Click(object sender, EventArgs e)
        {
            // Create the Material 

            // Read values from text boxes
            string name = textBox_materialname.Text;
            string permittivity = textBox_permittivity.Text;
            string permeability = textBox_permeability.Text;
            string conductivity = textBox_conductivity.Text;

            // Add a new row to the DataGridView
            dataGridView_MaterialList.Rows.Add(
                dataGridView_MaterialList.Rows.Count + 1, // Material ID (auto-increment)
                name,
                permittivity,
                permeability,
                conductivity
            );

        }

        private void button_update_Click(object sender, EventArgs e)
        {

        }

        private void button_delete_Click(object sender, EventArgs e)
        {

        }
    }
}
