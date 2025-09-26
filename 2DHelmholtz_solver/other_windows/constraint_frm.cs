using _2DHelmholtz_solver.global_variables;
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
    public partial class constraint_frm : Form
    {
        private fedata_store fe_data;

        public constraint_frm(ref fedata_store fe_data)
        {
            InitializeComponent();

            this.fe_data = fe_data;

        }



        private void button_applyconstraint_Click(object sender, EventArgs e)
        {

        }

        private void button_deleteconstraint_Click(object sender, EventArgs e)
        {

        }



        public void update_selected_node_list()
        {
            // Clear the text box
            textBox_selectednodes.Clear();

            List<int> all_selected_nodes = new List<int>();

            all_selected_nodes.AddRange(fe_data.meshdata.selected_point_ids);

            textBox_selectednodes.Text = string.Join(", ", all_selected_nodes);

            textBox_selectednodes.Invalidate();

        }


        private void constraint_frm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Control the flag
            fe_data.isConstraintUpdateInProgress = false;

            // Call the main form
            if (this.Owner is main_frm mainForm)
            {
                mainForm.CallFrom_constraint_frm();
            }

        }


        private void rectangleSelectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Toggle to Rectangle selection
            SetSelectionMode(true);

        }


        private void circleSelectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Toggle to Circle selection
            SetSelectionMode(false);

        }


        private void constraint_frm_Load(object sender, EventArgs e)
        {
            // Initialize selection state from global variable
            SetSelectionMode(gvariables_static.is_RectangleSelection);

        }


        private void SetSelectionMode(bool isRectangle)
        {

            gvariables_static.is_RectangleSelection = isRectangle;

            rectangleSelectionToolStripMenuItem.Checked = isRectangle;
            circleSelectionToolStripMenuItem.Checked = !isRectangle;


            rectangleSelectionToolStripMenuItem.BackColor = isRectangle ? Color.LightBlue : SystemColors.Control;
            circleSelectionToolStripMenuItem.BackColor = !isRectangle ? Color.LightBlue : SystemColors.Control;

        }

    }
}
