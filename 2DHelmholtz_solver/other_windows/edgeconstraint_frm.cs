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
    public partial class edgeconstraint_frm : Form
    {
        private fedata_store fe_data;

        public edgeconstraint_frm(ref fedata_store fe_data)
        {
            InitializeComponent();

            this.fe_data = fe_data;

            UpdateEnabledStateUI();
            UpdateEnabledStateUI2();

        }



        private void button_applyconstraint_Click(object sender, EventArgs e)
        {

        }

        private void button_deleteconstraint_Click(object sender, EventArgs e)
        {

        }



        public void update_selected_edge_list()
        {
            // Clear the text box
            textBox_selectededges.Clear();

            List<int> all_selected_edges = new List<int>();

            all_selected_edges.AddRange(fe_data.meshdata.selected_edge_ids);

            textBox_selectededges.Text = string.Join(", ", all_selected_edges);

            textBox_selectededges.Invalidate();

        }


        private void edgeconstraint_frm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Control the flag
            fe_data.isEdgeConstraintUpdateInProgress = false;

            // Call the main form
            if (this.Owner is main_frm mainForm)
            {
                mainForm.CallFrom_edgeconstraint_frm();
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


        private void edgeconstraint_frm_Load(object sender, EventArgs e)
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


        private void radioButton_boundaryconditions_CheckedChanged(object sender, EventArgs e)
        {
            UpdateEnabledStateUI();

        }


        private void radioButton_sommerfield_CheckedChanged(object sender, EventArgs e)
        {
            UpdateEnabledStateUI();

        }


        private void UpdateEnabledStateUI()
        {
            bool isBoundartConditionSelected = radioButton_boundaryconditions.Checked;

            // Boundary condition
            checkBox_dirichlet.Enabled = isBoundartConditionSelected;
            textBox_dirichlet.Enabled = isBoundartConditionSelected;
            label_dirichlet.Enabled = isBoundartConditionSelected;

            checkBox_neumann.Enabled = isBoundartConditionSelected;
            textBox_neumann.Enabled= isBoundartConditionSelected;
            label_neumann.Enabled = isBoundartConditionSelected;


            // ABC Sommerfield
            label_sommerfield.Enabled = !isBoundartConditionSelected;

            UpdateEnabledStateUI2();

        }

        private void checkBox_dirichlet_CheckedChanged(object sender, EventArgs e)
        {
            UpdateEnabledStateUI2();

        }

        private void checkBox_neumann_CheckedChanged(object sender, EventArgs e)
        {
            UpdateEnabledStateUI2();

        }


        private void UpdateEnabledStateUI2()
        {
            bool isDirichletSelected = checkBox_dirichlet.Checked;

            label_dirichlet.Enabled = isDirichletSelected;
            textBox_dirichlet.Enabled = isDirichletSelected;


            bool isNeumannSelected = checkBox_neumann.Checked;

            label_neumann.Enabled = isNeumannSelected;
            textBox_neumann.Enabled = isNeumannSelected;

        }



    }
}
