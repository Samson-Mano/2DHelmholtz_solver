using _2DHelmholtz_solver.global_variables;
using _2DHelmholtz_solver.src.model_store.fe_objects;
// OpenTK library
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;




namespace _2DHelmholtz_solver
{
    public partial class main_frm : Form
    {
        // main finite element data store
        public fedata_store fedata { get; }


        public main_frm()
        {
            // Initialize the finite element model data
            fedata = new fedata_store();

            InitializeComponent();

            // Fill the gcontrol panel
            glControl_main_panel.Dock = DockStyle.Fill;

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Exit application
            this.Close();

        }

        private void main_frm_Load(object sender, EventArgs e)
        {
            
            
        }





        #region "glControl Main Panel Events"
        private void glControl_main_panel_Load(object sender, EventArgs e)
        {
            // Update the size of the drawing area
            fedata.meshdata.graphic_events_control.update_drawing_area_size(glControl_main_panel.Width,
                glControl_main_panel.Height, 2.0, 2.0);

            // Refresh the controller (doesnt do much.. nothing to draw)
            glControl_main_panel.Invalidate();

        }

        private void glControl_main_panel_Paint(object sender, PaintEventArgs e)
        {



            // OpenTK windows are what's known as "double-buffered". In essence, the window manages two buffers.
            // One is rendered to while the other is currently displayed by the window.
            // This avoids screen tearing, a visual artifact that can happen if the buffer is modified while being displayed.
            // After drawing, call this function to swap the buffers. If you don't, it won't display what you've rendered.
            glControl_main_panel.SwapBuffers();

        }

        private void glControl_main_panel_SizeChanged(object sender, EventArgs e)
        {
            // Update the size of the drawing area
            fedata.meshdata.graphic_events_control.update_drawing_area_size(glControl_main_panel.Width,
                glControl_main_panel.Height, geom_obj.geom_bound_width, geom_obj.geom_bound_height);

            toolStripStatusLabel_zoom_value.Text = "Zoom: " + (gvariables_static.RoundOff((int)(1.0f * 100))).ToString() + "%";

            // Refresh the painting area
            glControl_main_panel.Invalidate();
        }

        private void glControl_main_panel_MouseEnter(object sender, EventArgs e)
        {
            // set the focus to enable zoom/ pan & zoom to fit
            glControl_main_panel.Focus();

        }

        private void glControl_main_panel_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {

        }

        private void glControl_main_panel_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {

        }

        private void glControl_main_panel_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {

        }

        private void glControl_main_panel_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {

        }

        private void glControl_main_panel_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void glControl_main_panel_KeyUp(object sender, KeyEventArgs e)
        {

        }

        #endregion

    }
}
