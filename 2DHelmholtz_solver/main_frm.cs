using _2DHelmholtz_solver.global_variables;
using _2DHelmholtz_solver.other_windows;
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
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;



namespace _2DHelmholtz_solver
{
    public partial class main_frm : Form
    {
        // main finite element data store
        public fedata_store fedata;

        // Zoom To Fit 
        private Timer zoomToFitTimer;

        // Refreh and FPS Tracking variables
        private Timer refreshStatusResetTimer;
        private Stopwatch fpsStopwatch = new Stopwatch();


        // Forms
        private option_frm option_Form;
        private matprop_frm matprop_Form;
        private load_frm load_Form;
        private nodalconstraint_frm nodalconstraint_Form;
        private edgeconstraint_frm edgeconstraint_Form;


        public main_frm()
        {

            InitializeComponent();

            // Initialize the finite element model data
            fedata = new fedata_store();

            // Initialize the timer
            zoomToFitTimer = new Timer();
            zoomToFitTimer.Interval = 10; // ~60 FPS refresh (16 ms)
            zoomToFitTimer.Tick += ZoomToFitTimer_Tick;


            refreshStatusResetTimer = new Timer();
            refreshStatusResetTimer.Interval = 500; // milliseconds before resetting status
            refreshStatusResetTimer.Tick += RefreshStatusResetTimer_Tick;

        }


        private void main_frm_Load(object sender, EventArgs e)
        {
            // Initialize the GLControl in the Load event
            // Fill the gcontrol panel
            glControl_main_panel.Dock = DockStyle.Fill;

            // Create the main font atlas
            gvariables_static.main_font.CreateAtlas();

        }


        #region "glControl Main Panel Events"
        private void glControl_main_panel_Load(object sender, EventArgs e)
        {
            // Paint the background
            Color clr_bg = gvariables_static.glcontrol_background_color;
            GL.ClearColor(((float)clr_bg.R / 255.0f),
                ((float)clr_bg.G / 255.0f),
                ((float)clr_bg.B / 255.0f),
                ((float)clr_bg.A / 255.0f));

            // Update the size of the drawing area
            fedata.graphic_events_control.update_drawing_area_size(glControl_main_panel.Width,
                glControl_main_panel.Height);

            fpsStopwatch.Start();

            // Refresh the controller (doesnt do much.. nothing to draw)
            glControl_main_panel.Invalidate();

        }

        private void glControl_main_panel_Paint(object sender, PaintEventArgs e)
        {
            // Paint the drawing area (glControl_main)
            // Tell OpenGL to use MyGLControl
            glControl_main_panel.MakeCurrent();

            // GL.Enable(EnableCap.Multisample);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(0, BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            // Clear the background
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            fedata.paint_model();

            // OpenTK windows are what's known as "double-buffered". In essence, the window manages two buffers.
            // One is rendered to while the other is currently displayed by the window.
            // This avoids screen tearing, a visual artifact that can happen if the buffer is modified while being displayed.
            // After drawing, call this function to swap the buffers. If you don't, it won't display what you've rendered.
            glControl_main_panel.SwapBuffers();

            // Update the zoom value
            double zm_val = fedata.graphic_events_control.zoom_val;
            toolStripStatusLabel_zoom_value.Text = "Zoom: " + (gvariables_static.RoundOff((int)(zm_val * 100))).ToString() + "%";
            toolStripStatusLabel_IsRefresh.Invalidate();

            // Update FPS every second
            if (fpsStopwatch.ElapsedMilliseconds >= 1000)
            {
                fpsStopwatch.Restart();

                SetRefreshStatus(true); // Update status bar
            }

        }

        private void glControl_main_panel_SizeChanged(object sender, EventArgs e)
        {
           // Note: SizeChanged can fire before the OpenGL context exists (e.g., during form initialization, Load etc).
           if (glControl_main_panel == null || fedata == null)
                 return;

            // Update the size of the drawing area
            fedata.graphic_events_control.update_drawing_area_size(glControl_main_panel.Width,
                glControl_main_panel.Height);

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
            bool isRefresh = false;
            if (e.Button == MouseButtons.Left)
            {
                // Left button down
                isRefresh = fedata.graphic_events_control.handleMouseLeftButtonClick(true, e.X, e.Y);

            }
            else if (e.Button == MouseButtons.Right)
            {
                // Right button down
                isRefresh = fedata.graphic_events_control.handleMouseRightButtonClick(true, e.X, e.Y);

            }

            if (isRefresh == true)
            {
                glControl_main_panel.Invalidate();

            }

        }

        private void glControl_main_panel_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            // Mouse wheel
            bool isRefresh = fedata.graphic_events_control.handleMouseScroll(e.Delta, e.X, e.Y);

            if (isRefresh == true)
            {
                glControl_main_panel.Invalidate();

            }

        }

        private void glControl_main_panel_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            // Mouse move 
            bool isRefresh = fedata.graphic_events_control.handleMouseMove(e.X, e.Y);

            if (isRefresh == true)
            {
                glControl_main_panel.Invalidate();

            }

        }

        private void glControl_main_panel_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            bool isRefresh = false;
            if (e.Button == MouseButtons.Left)
            {
                // Left button up
                isRefresh = fedata.graphic_events_control.handleMouseLeftButtonClick(false, e.X, e.Y);

            }
            else if (e.Button == MouseButtons.Right)
            {
                // Right button up
                isRefresh = fedata.graphic_events_control.handleMouseRightButtonClick(false, e.X, e.Y);

            }

            if (isRefresh == true)
            {
                glControl_main_panel.Invalidate();

                // Update the Material Property Form data
                if (fedata.isMaterialUpdateInProgress == true)
                {
                    matprop_Form.update_selected_element_list();

                }

                // Update the Load Form data
                if(fedata.isLoadUpdateInProgress  == true)
                {
                    load_Form.update_selected_node_list();

                }

                // Update the Nodal Constraint Form data
                if(fedata.isNodalConstraintUpdateInProgress == true)
                {
                    nodalconstraint_Form.update_selected_node_list();

                }

                // Update the Edge Constraint Form data
                if (fedata.isEdgeConstraintUpdateInProgress == true)
                {
                    edgeconstraint_Form.update_selected_edge_list();

                }

            }

        }

        private void glControl_main_panel_KeyDown(object sender, KeyEventArgs e)
        {
            // Keyboard Key Down
            bool isRefresh = fedata.graphic_events_control.handleKeyboardAction(true, e.KeyValue);

            if (isRefresh == true)
            {
                glControl_main_panel.Invalidate();

            }

        }

        private void glControl_main_panel_KeyUp(object sender, KeyEventArgs e)
        {
            // Keyboard Key Up
            bool isRefresh = fedata.graphic_events_control.handleKeyboardAction(false, e.KeyValue);

            if (isRefresh == true)
            {
                glControl_main_panel.Invalidate();

            }

            // If zoom-to-fit started, start the timer
            if (fedata.graphic_events_control.isZoomToFitInProgress == true)
            {
                // Start the zoomToFit timer
                if (!zoomToFitTimer.Enabled)
                    zoomToFitTimer.Start();

            }


        }

        private void ZoomToFitTimer_Tick(object sender, EventArgs e)
        {
            glControl_ZoomToFitOperation();

        }


        private void glControl_ZoomToFitOperation()
        {
            // Refresh the glControl_main_panel as the zoom to fit operation in progress
            glControl_main_panel.Invalidate();

            if (fedata.graphic_events_control.isZoomToFitInProgress == false)
            {
                // End the zoom to fit operation
                // Stop zoom-to-fit operation once done
                zoomToFitTimer.Stop();

            }

        }


        private void RefreshStatusResetTimer_Tick(object sender, EventArgs e)
        {
            refreshStatusResetTimer.Stop();
            SetRefreshStatus(false);

        }


        // Utility function for status updates
        private void SetRefreshStatus(bool isRefreshing)
        {

            if (isRefreshing)
            {
                toolStripStatusLabel_IsRefresh.Text = "REFRESH";
                toolStripStatusLabel_IsRefresh.ForeColor = Color.Green;
                toolStripStatusLabel_IsRefresh.Invalidate();

                // Start timer to reset status
                refreshStatusResetTimer.Stop(); // restart if already running
                refreshStatusResetTimer.Start();

            }
            else
            {
                toolStripStatusLabel_IsRefresh.Text = "";
                toolStripStatusLabel_IsRefresh.ForeColor = SystemColors.Control;
                toolStripStatusLabel_IsRefresh.Invalidate();

            }

        }

        #endregion


        #region "File Events"

        private void importModelToolStripMenuItem_Click(object sender, EventArgs e)
        {

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Import Model File",
                Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                // InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;

                try
                {
                    string fileContent = File.ReadAllText(filePath);

                    fedata.importMesh(fileContent);

                    // Do something with the file content, e.g., parse the model
                    // MessageBox.Show("Model file loaded successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error reading file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            glControl_main_panel_SizeChanged(sender, e);

            glControl_main_panel.Refresh();
            glControl_main_panel.Invalidate();

        }

        private void exportModelToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }


        private void optionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fedata.isModelSet == false)
                return;

            // Check if option_Form is null or disposed
            if (option_Form == null || option_Form.IsDisposed)
            {
                option_Form = new option_frm();

                // Make it behave like a tool window
                option_Form.FormBorderStyle = FormBorderStyle.SizableToolWindow;
                option_Form.ShowInTaskbar = false;
                option_Form.TopLevel = true;
                option_Form.Owner = this;

                // Manually center the form on the parent
                int x = this.Location.X + (this.Width - option_Form.Width) / 2;
                int y = this.Location.Y + (this.Height - option_Form.Height) / 2;
                option_Form.StartPosition = FormStartPosition.Manual;
                option_Form.Location = new Point(Math.Max(x, 0), Math.Max(y, 0)); // avoid negative positions

            }

            //// Turn on Flag Material update form is open
            //fedata.meshdata.isMaterialUpdateInProgress = true;
            //fedata.meshdata.clear_selected_mesh();

            // Show the form
            option_Form.Show(this);
            option_Form.BringToFront();

            glControl_main_panel.Invalidate();

        }


        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Exit application
            this.Close();

        }

        #endregion



        #region "Load Events"
        private void addLoadsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fedata.isModelSet == false)
                return;

            // Check if load_Form is null or disposed
            if (load_Form == null || load_Form.IsDisposed)
            {
                load_Form = new load_frm(ref fedata);

                // Make it behave like a tool window
                load_Form.FormBorderStyle = FormBorderStyle.SizableToolWindow;
                load_Form.ShowInTaskbar = false;
                load_Form.TopLevel = true;
                load_Form.Owner = this;

                // Manually center the form on the parent
                int x = this.Location.X + (this.Width - load_Form.Width) / 2;
                int y = this.Location.Y + (this.Height - load_Form.Height) / 2;
                load_Form.StartPosition = FormStartPosition.Manual;
                load_Form.Location = new Point(Math.Max(x, 0), Math.Max(y, 0)); // avoid negative positions

            }

            // Turn on Flag Loads update form is open
            fedata.isLoadUpdateInProgress = true;
            fedata.meshdata.clear_selected_nodes();

            //// Show the form
            // matprop_Form.update_material_data();
            load_Form.update_selected_node_list();
            load_Form.Show(this);
            load_Form.BringToFront();

            glControl_main_panel.Invalidate();

        }

        private void addNodalConstraintsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fedata.isModelSet == false)
                return;

            // Check if nodalconstraint_Form is null or disposed
            if (nodalconstraint_Form == null || nodalconstraint_Form.IsDisposed)
            {
                nodalconstraint_Form = new nodalconstraint_frm(ref fedata);

                // Make it behave like a tool window
                nodalconstraint_Form.FormBorderStyle = FormBorderStyle.SizableToolWindow;
                nodalconstraint_Form.ShowInTaskbar = false;
                nodalconstraint_Form.TopLevel = true;
                nodalconstraint_Form.Owner = this;

                // Manually center the form on the parent
                int x = this.Location.X + (this.Width - nodalconstraint_Form.Width) / 2;
                int y = this.Location.Y + (this.Height - nodalconstraint_Form.Height) / 2;
                nodalconstraint_Form.StartPosition = FormStartPosition.Manual;
                nodalconstraint_Form.Location = new Point(Math.Max(x, 0), Math.Max(y, 0)); // avoid negative positions

            }

            // Turn on Flag Nodal Constraint update form is open
            fedata.isNodalConstraintUpdateInProgress = true;
            fedata.meshdata.clear_selected_nodes();

            // Show the form
            // matprop_Form.update_material_data();
            nodalconstraint_Form.update_dataGridView();
            nodalconstraint_Form.update_selected_node_list();
            nodalconstraint_Form.Show(this);
            nodalconstraint_Form.BringToFront();

            glControl_main_panel.Invalidate();

        }


        private void addEdgeConstraintsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fedata.isModelSet == false)
                return;

            // Check if edgeconstraint_Form is null or disposed
            if (edgeconstraint_Form == null || edgeconstraint_Form.IsDisposed)
            {
                edgeconstraint_Form = new edgeconstraint_frm(ref fedata);

                // Make it behave like a tool window
                edgeconstraint_Form.FormBorderStyle = FormBorderStyle.SizableToolWindow;
                edgeconstraint_Form.ShowInTaskbar = false;
                edgeconstraint_Form.TopLevel = true;
                // edgeconstraint_Form.MdiParent = this;
                edgeconstraint_Form.Owner = this;

                // Manually center the form on the parent
                int x = this.Location.X + (this.Width - edgeconstraint_Form.Width) / 2;
                int y = this.Location.Y + (this.Height - edgeconstraint_Form.Height) / 2;
                edgeconstraint_Form.StartPosition = FormStartPosition.Manual;
                edgeconstraint_Form.Location = new Point(Math.Max(x, 0), Math.Max(y, 0)); // avoid negative positions

                // matprop_Form.StartPosition = FormStartPosition.CenterParent;

            }

            // Turn on Flag Edge Constraint update form is open
            fedata.isEdgeConstraintUpdateInProgress = true;
            fedata.meshdata.clear_selected_edges();

            // Show the form
            // matprop_Form.update_material_data();
            edgeconstraint_Form.update_selected_edge_list();
            edgeconstraint_Form.Show(this);
            edgeconstraint_Form.BringToFront();

            glControl_main_panel.Invalidate();

        }

        private void materialPropertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fedata.isModelSet == false)
                return;

            // Check if matprop_Form is null or disposed
            if (matprop_Form == null || matprop_Form.IsDisposed)
            {
                matprop_Form = new matprop_frm(ref fedata);

                // Make it behave like a tool window
                matprop_Form.FormBorderStyle = FormBorderStyle.SizableToolWindow;
                matprop_Form.ShowInTaskbar = false;
                matprop_Form.TopLevel = true;
                // matprop_Form.MdiParent = this;
                matprop_Form.Owner = this;

                // Manually center the form on the parent
                int x = this.Location.X + (this.Width - matprop_Form.Width) / 2;
                int y = this.Location.Y + (this.Height - matprop_Form.Height) / 2;
                matprop_Form.StartPosition = FormStartPosition.Manual;
                matprop_Form.Location = new Point(Math.Max(x, 0), Math.Max(y, 0)); // avoid negative positions

                // matprop_Form.StartPosition = FormStartPosition.CenterParent;

            }

            // Turn on Flag Material update form is open
            fedata.isMaterialUpdateInProgress = true;
            fedata.meshdata.clear_selected_mesh();

            // Show the form
            matprop_Form.update_material_data();
            matprop_Form.update_selected_element_list();
            matprop_Form.Show(this);
            matprop_Form.BringToFront();

            glControl_main_panel.Invalidate();

        }


        public void CallFrom_load_frm()
        {
            // Refresh 
            glControl_main_panel.Invalidate();

        }


        public void CallFrom_nodalconstraint_frm()
        {
            // Refresh 
            fedata.update_openTK_uniforms(true, true, true);

            glControl_main_panel.Invalidate();

        }

        public void CallFrom_edgeconstraint_frm()
        {
            // Refresh 
            glControl_main_panel.Invalidate();

        }


        public void CallFrom_matprop_frm(int material_id, bool isAssignMaterial = false, bool isDeleteMaterial = false)
        {
            if(isAssignMaterial == true)
            {
                // Assign the material to the selected elements
                fedata.update_material_id(material_id, false);
            }


            if(isDeleteMaterial == true)
            {
                // Material is deleted, update with default material
                fedata.update_material_id(material_id, true);
            }

            // Refresh 
            glControl_main_panel.Invalidate();

        }

        public void CallFrom_option_frm(bool isShrinkMesh = false)
        {
            if(isShrinkMesh == true)
            {
                // Perform the shrinkage of the mesh
                fedata.meshdata.update_mesh_shrinkage();
            }

            // Refresh 
            glControl_main_panel.Invalidate();

        }


        #endregion

    }
}
