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
using System.Diagnostics;
using System.Drawing;
using System.IO;
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

        // Zoom To Fit 
        private Timer zoomToFitTimer;

        // Refreh and FPS Tracking variables
        private Timer refreshStatusResetTimer;
        private Stopwatch fpsStopwatch = new Stopwatch();


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
            fedata.meshdata.graphic_events_control.update_drawing_area_size(glControl_main_panel.Width,
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
            double zm_val = fedata.meshdata.graphic_events_control.zoom_val;
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
            //if (glControl_main_panel.Context.IsDisposed)
            //    return;

            // Update the size of the drawing area
            fedata.meshdata.graphic_events_control.update_drawing_area_size(glControl_main_panel.Width,
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
                isRefresh = fedata.meshdata.graphic_events_control.handleMouseLeftButtonClick(true, e.X, e.Y);

            }
            else if (e.Button == MouseButtons.Right)
            {
                // Right button down
                isRefresh = fedata.meshdata.graphic_events_control.handleMouseRightButtonClick(true, e.X, e.Y);

            }

            if (isRefresh == true)
            {
                glControl_main_panel.Invalidate();

            }

        }

        private void glControl_main_panel_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            // Mouse wheel
            bool isRefresh = fedata.meshdata.graphic_events_control.handleMouseScroll(e.Delta, e.X, e.Y);

            if (isRefresh == true)
            {
                glControl_main_panel.Invalidate();

            }

        }

        private void glControl_main_panel_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            // Mouse move 
            bool isRefresh = fedata.meshdata.graphic_events_control.handleMouseMove(e.X, e.Y);

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
                isRefresh = fedata.meshdata.graphic_events_control.handleMouseLeftButtonClick(false, e.X, e.Y);

            }
            else if (e.Button == MouseButtons.Right)
            {
                // Right button up
                isRefresh = fedata.meshdata.graphic_events_control.handleMouseRightButtonClick(false, e.X, e.Y);

            }

            if (isRefresh == true)
            {
                glControl_main_panel.Invalidate();

            }

        }

        private void glControl_main_panel_KeyDown(object sender, KeyEventArgs e)
        {
            // Keyboard Key Down
            bool isRefresh = fedata.meshdata.graphic_events_control.handleKeyboardAction(true, e.KeyValue);

            if (isRefresh == true)
            {
                glControl_main_panel.Invalidate();

            }

        }

        private void glControl_main_panel_KeyUp(object sender, KeyEventArgs e)
        {
            // Keyboard Key Up
            bool isRefresh = fedata.meshdata.graphic_events_control.handleKeyboardAction(false, e.KeyValue);

            if (isRefresh == true)
            {
                glControl_main_panel.Invalidate();

            }

            // If zoom-to-fit started, start the timer
            if (fedata.meshdata.graphic_events_control.isZoomToFitInProgress == true)
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

            if(fedata.meshdata.graphic_events_control.isZoomToFitInProgress == false)
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

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Exit application
            this.Close();

        }





        #endregion


        #region "Load Events"
        private void addLoadsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void addConstraintsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void materialPropertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        #endregion




    }
}
