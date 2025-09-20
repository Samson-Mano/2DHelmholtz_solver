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
        private fedata_store fe_data;

        public matprop_frm(ref fedata_store fe_data)
        {
            InitializeComponent();

           this.fe_data = fe_data;

        }


        public void update_material_data()
        {
            // Get the fe materials
            List<material_data> fe_materials = this.fe_data.fe_materials.Values.ToList();

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

            // Generate a unique material ID
            int material_id = global_variables.gvariables_static.get_unique_id(fe_data.materialids);

            // Read and validate input from text boxes
            string material_name = textBox_materialname.Text.Trim();
            if (string.IsNullOrWhiteSpace(material_name))
            {
                MessageBox.Show("Material name cannot be empty.");
                return;
            }

            // Test the data
            if (!double.TryParse(textBox_permittivity.Text, out double permittivity) ||
                !double.TryParse(textBox_permeability.Text, out double permeability) ||
                !double.TryParse(textBox_conductivity.Text, out double conductivity))
            {
                MessageBox.Show("Please enter valid numeric values for permittivity, permeability, and conductivity.");
                return;
            }

            // Add a new row to the DataGridView
            dataGridView_MaterialList.Rows.Add(
                material_id,
                material_name,
                permittivity.ToString("G"),
                permeability.ToString("G"),
                conductivity.ToString("G")
            );

            // Create and store the material object
            var newMaterial = new material_data
            {
                material_id = material_id,
                material_name = material_name,
                material_permittivity = permittivity,
                material_permeability = permeability,
                material_conductivity = conductivity
            };

            fe_data.fe_materials[material_id] = newMaterial;
            fe_data.materialids.Add(material_id);

        }

        private void button_update_Click(object sender, EventArgs e)
        {

            // Update the material data
            if (dataGridView_MaterialList.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridView_MaterialList.SelectedRows[0];

                // Safely Retrieve the material ID
                string idString = selectedRow.Cells["Column1_materialid"].Value?.ToString();

                if (!int.TryParse(idString, out int material_id))
                {
                    // MessageBox.Show("Invalid material ID.");
                    return;
                }


                // Read and validate input from text boxes
                string material_name = textBox_materialname.Text.Trim();
                if (string.IsNullOrWhiteSpace(material_name))
                {
                    MessageBox.Show("Material name cannot be empty.");
                    return;
                }

                // Test the data
                if (!double.TryParse(textBox_permittivity.Text, out double permittivity) ||
                    !double.TryParse(textBox_permeability.Text, out double permeability) ||
                    !double.TryParse(textBox_conductivity.Text, out double conductivity))
                {
                    MessageBox.Show("Please enter valid numeric values for permittivity, permeability, and conductivity.");
                    return;
                }

                // update the material data in the dictionary
                fe_data.fe_materials[material_id].material_name = material_name;
                fe_data.fe_materials[material_id].material_permittivity = permittivity;
                fe_data.fe_materials[material_id].material_permeability = permeability;
                fe_data.fe_materials[material_id].material_conductivity = conductivity;

                // Update the DataGridView row
                selectedRow.Cells["Column2_materialname"].Value = material_name;
                selectedRow.Cells["Column3_permittivity"].Value = permittivity.ToString("G");
                selectedRow.Cells["Column4_Permeability"].Value = permeability.ToString("G");
                selectedRow.Cells["Column5_Conductivity"].Value = conductivity.ToString("G");

            }

        }

        private void button_delete_Click(object sender, EventArgs e)
        {

            // Delete the material
            if (dataGridView_MaterialList.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridView_MaterialList.SelectedRows[0];

                // Safely Retrieve the material ID
                string idString = selectedRow.Cells["Column1_materialid"].Value?.ToString();

                if (!int.TryParse(idString, out int material_id))
                {
                    // MessageBox.Show("Invalid material ID.");
                    return;
                }

                // Remove from the dictionary
                fe_data.fe_materials.Remove(material_id);
                fe_data.materialids.Remove(material_id);


                // remove the row from the data grid view
                dataGridView_MaterialList.Rows.Remove(selectedRow);

            }

        }

        public void update_selected_element_list()
        {
            // Clear the text box
            textBox_selectedelements.Clear();

            List<int> all_selected_ids = new List<int>();

            all_selected_ids.AddRange(fe_data.meshdata.selected_tri_ids);
            all_selected_ids.AddRange(fe_data.meshdata.selected_quad_ids);

            textBox_selectedelements.Text = string.Join(", ", all_selected_ids);

            //foreach (int id in all_selected_ids)
            //{
            //    textBox_selectedelements.Text += $"{id} ,";

            //}

            textBox_selectedelements.Invalidate();

        }


        private void button_assignmaterial_Click(object sender, EventArgs e)
        {

            // Call the main form
            if (this.Owner is main_frm mainForm)
            {
                mainForm.CallFrom_matprop_frm();
            }
        }

        private void matprop_frm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Control the flag
            fe_data.meshdata.isMaterialUpdateInProgress = false;

            // Call the main form
            if (this.Owner is main_frm mainForm)
            {
                mainForm.CallFrom_matprop_frm(); 
            }

        }

    }
}
