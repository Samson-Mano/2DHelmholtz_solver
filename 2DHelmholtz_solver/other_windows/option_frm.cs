using _2DHelmholtz_solver.global_variables;
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
    public partial class option_frm : Form
    {
        public option_frm()
        {
            InitializeComponent();
        }

        private void checkBox_paintmesh_CheckedChanged(object sender, EventArgs e)
        {
            gvariables_static.is_paint_mesh = checkBox_paintmesh.Checked;
            refreshGLControl();

        }

        private void checkBox_paintmeshboundaries_CheckedChanged(object sender, EventArgs e)
        {
            gvariables_static.is_paint_mesh_boundaries = checkBox_paintmeshboundaries.Checked;
            refreshGLControl();

        }

        private void checkBox_paintshrinkmesh_CheckedChanged(object sender, EventArgs e)
        {
            gvariables_static.is_paint_shrunk_triangle = checkBox_paintshrinkmesh.Checked;

            // Call the main form
            if (this.Owner is main_frm mainForm)
            {
                mainForm.CallFrom_option_frm(true);
            }

        }

        private void checkBox_paintloads_CheckedChanged(object sender, EventArgs e)
        {
            gvariables_static.is_paint_loads = checkBox_paintloads.Checked;
            refreshGLControl();

        }

        private void checkBox_paintconstraints_CheckedChanged(object sender, EventArgs e)
        {
            gvariables_static.is_paint_constraints = checkBox_paintconstraints.Checked; 
            refreshGLControl();

        }

        private void refreshGLControl()
        {
            // Call the main form
            if (this.Owner is main_frm mainForm)
            {
                mainForm.CallFrom_option_frm(false);
            }
        }


        private void button_ok_Click(object sender, EventArgs e)
        {
            refreshGLControl();
            this.Close();

        }

        private void option_frm_Load(object sender, EventArgs e)
        {
            // Set the default options
            checkBox_paintmesh.Checked =  gvariables_static.is_paint_mesh;
            checkBox_paintmeshboundaries.Checked = gvariables_static.is_paint_mesh_boundaries;
            checkBox_paintshrinkmesh.Checked =     gvariables_static.is_paint_shrunk_triangle;
            checkBox_paintloads.Checked =  gvariables_static.is_paint_loads;
            checkBox_paintconstraints.Checked = gvariables_static.is_paint_constraints;

        }

    }
}
