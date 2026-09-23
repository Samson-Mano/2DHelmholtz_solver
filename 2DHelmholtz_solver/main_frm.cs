using _2DHelmholtz_solver.global_variables;
using _2DHelmholtz_solver.other_windows;
using _2DHelmholtz_solver.src.model_store.fe_objects;
using _2DHelmholtz_solver.src.model_store.geom_objects;

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
        private solver_frm solver_Form;
        private modalsolver_frm modalsolver_Form;
        private modalresultoption_frm modalresultoption_Form;


        // Drawing area Axis data store
        public axisdata_store axisdata;



        public main_frm()
        {

            InitializeComponent();

            // Initialize the finite element model data
            fedata = new fedata_store();

            axisdata = new axisdata_store();

            // Initialize the timer
            zoomToFitTimer = new Timer();
            zoomToFitTimer.Interval = 10; // ~60 FPS refresh (16 ms)
            zoomToFitTimer.Tick += ZoomToFitTimer_Tick;


            refreshStatusResetTimer = new Timer();
            refreshStatusResetTimer.Interval = 500; // milliseconds before resetting status
            refreshStatusResetTimer.Tick += RefreshStatusResetTimer_Tick;

            // Add loads is covered in Nodal Boundary condition (so hidden)
            addLoadsToolStripMenuItem.Visible = false;


            // Render timer
            Application.Idle += OnApplicationIdle;

        }


        private void main_frm_Load(object sender, EventArgs e)
        {
            // Initialize the GLControl in the Load event
            // Fill the gcontrol panel
            glControl_main_panel.Dock = DockStyle.Fill;

            // Create the main font atlas
            gvariables_static.main_font.CreateAtlas();

            gvariables_static.rslt_font.CreateAtlas("Calibri");

            axisdata.InitializeAxisData(glControl_main_panel.Width, glControl_main_panel.Height);


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

            // Draw the axis arrows
            axisdata.draw_axis_arrows();


            // OpenTK windows are what's known as "double-buffered". In essence, the window manages two buffers.
            // One is rendered to while the other is currently displayed by the window.
            // This avoids screen tearing, a visual artifact that can happen if the buffer is modified while being displayed.
            // After drawing, call this function to swap the buffers. If you don't, it won't display what you've rendered.
            glControl_main_panel.SwapBuffers();

            // Update the zoom value
            double zm_val = fedata.graphic_events_control.zoom_val;
            toolStripStatusLabel_zoom_value.Text = "Zoom: " + (gvariables_static.RoundOff((int)(zm_val * 100))).ToString() + "%";
            toolStripStatusLabel_FPS.Invalidate();

            // Update FPS every second
            if (fpsStopwatch.ElapsedMilliseconds >= 1000)
            {
                fpsStopwatch.Restart();

                // SetRefreshStatus(true); // Update status bar
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

            axisdata.UpdateAxisArrowCenter(glControl_main_panel.Width, glControl_main_panel.Height);
            fedata.update_contour_bar_position(glControl_main_panel.Width, glControl_main_panel.Height);


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
            if (e.Button == MouseButtons.Left)
            {
                // Left button down
                fedata.graphic_events_control.handleMouseLeftButtonClick(true, e.X, e.Y);

            }
            else if (e.Button == MouseButtons.Right)
            {
                // Right button down
                fedata.graphic_events_control.handleMouseRightButtonClick(true, e.X, e.Y);

            }

            glControl_main_panel.Invalidate();

        }

        private void glControl_main_panel_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            // Mouse wheel
            fedata.graphic_events_control.handleMouseScroll(e.Delta, e.X, e.Y);

            glControl_main_panel.Invalidate();

        }

        private void glControl_main_panel_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            // Mouse move 
            fedata.graphic_events_control.handleMouseMove(e.X, e.Y);

            glControl_main_panel.Invalidate();

        }

        private void glControl_main_panel_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Left button up
                fedata.graphic_events_control.handleMouseLeftButtonClick(false, e.X, e.Y);

            }
            else if (e.Button == MouseButtons.Right)
            {
                // Right button up
                fedata.graphic_events_control.handleMouseRightButtonClick(false, e.X, e.Y);

            }

            glControl_main_panel.Invalidate();

            // Update the Material Property Form data
            if (fedata.isMaterialUpdateInProgress == true)
            {
                matprop_Form.update_selected_element_list();

            }

            // Update the Load Form data
            if (fedata.isLoadUpdateInProgress == true)
            {
                load_Form.update_selected_node_list();

            }

            // Update the Nodal Constraint Form data
            if (fedata.isNodalConstraintUpdateInProgress == true)
            {
                nodalconstraint_Form.update_selected_node_list();

            }

            // Update the Edge Constraint Form data
            if (fedata.isEdgeConstraintUpdateInProgress == true)
            {
                edgeconstraint_Form.update_selected_edge_list();

            }

        }

        private void glControl_main_panel_KeyDown(object sender, KeyEventArgs e)
        {
            // Keyboard Key Down
            fedata.graphic_events_control.handleKeyboardAction(true, e.KeyValue);

            glControl_main_panel.Invalidate();

        }

        private void glControl_main_panel_KeyUp(object sender, KeyEventArgs e)
        {
            // Keyboard Key Up
            fedata.graphic_events_control.handleKeyboardAction(false, e.KeyValue);

            glControl_main_panel.Invalidate();

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
            // SetRefreshStatus(false);

        }


        #endregion


        #region "File Events"


        private void importTXTFileToolStripMenuItem_Click(object sender, EventArgs e)
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

                    fedata.importTXTFile(fileContent);

                    set_ResultOption(0); // Reset result option to hide results

                    // Do something with the file content, e.g., parse the model
                    // MessageBox.Show("Model file loaded successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error reading text file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            glControl_main_panel_SizeChanged(sender, e);

            glControl_main_panel.Refresh();
            glControl_main_panel.Invalidate();


        }


        private void importModelToolStripMenuItem_Click(object sender, EventArgs e)
        {

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Import Model File",
                Filter = "Text Files (*.bin)|*.bin|All Files (*.*)|*.*",
                // InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;

                try
                {

                    fedata.importBINFile(filePath);

                    set_ResultOption(0); // Reset result option to hide results

                    // Do something with the file content, e.g., parse the model
                    // MessageBox.Show("Model file loaded successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error reading binary file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            glControl_main_panel_SizeChanged(sender, e);

            glControl_main_panel.Refresh();
            glControl_main_panel.Invalidate();

        }

        private void exportModelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = "Export Model File",
                Filter = "Bindary Files (*.bin)|*.bin|All Files (*.*)|*.*",
                // InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog.FileName;

                try
                {

                    fedata.exportBINFile(filePath);

                    // Do something with the file content, e.g., parse the model
                    // MessageBox.Show("Model file exported successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            glControl_main_panel_SizeChanged(sender, e);

            glControl_main_panel.Refresh();
            glControl_main_panel.Invalidate();
        }


        private void CenterFormOnParent(Form childForm)
        {
            // Helper method to center a form on its owner
            if (childForm.Owner == null)
                return;

            // Get the screen bounds of the parent form
            Screen parentScreen = Screen.FromControl(childForm.Owner);
            Rectangle parentBounds = childForm.Owner.Bounds;

            // Calculate center position relative to the parent form
            int x = parentBounds.X + (parentBounds.Width - childForm.Width) / 2;
            int y = parentBounds.Y + (parentBounds.Height - childForm.Height) / 2;

            // Ensure the form stays within the screen bounds
            Rectangle screenBounds = parentScreen.WorkingArea;

            // Adjust if the form would go off-screen
            if (x < screenBounds.Left)
                x = screenBounds.Left;
            if (y < screenBounds.Top)
                y = screenBounds.Top;
            if (x + childForm.Width > screenBounds.Right)
                x = screenBounds.Right - childForm.Width;
            if (y + childForm.Height > screenBounds.Bottom)
                y = screenBounds.Bottom - childForm.Height;

            childForm.Location = new Point(x, y);
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

                // Set the start position to manual so we can control placement
                option_Form.StartPosition = FormStartPosition.Manual;

                // Center the form on the parent
                CenterFormOnParent(option_Form);

            }


            // Only show if not already visible; otherwise just bring to front
            if(!option_Form.Visible)
            {
                option_Form.Show(this);
            }
            
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

                // Set the start position to manual so we can control placement
                load_Form.StartPosition = FormStartPosition.Manual;

                // Center the form on the parent
                CenterFormOnParent(load_Form);

            }

            // Turn on Flag Loads update form is open
            fedata.isLoadUpdateInProgress = true;
            fedata.meshdata.clear_selected_nodes();

            // Only show if not already visible; otherwise just bring to front
            if(!load_Form.Visible)
            {
                // Show the form
                load_Form.update_selected_node_list();
                load_Form.Show(this);
            }

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

                // Set the start position to manual so we can control placement
                nodalconstraint_Form.StartPosition = FormStartPosition.Manual;

                // Center the form on the parent
                CenterFormOnParent(nodalconstraint_Form);

            }

            // Turn on Flag Nodal Constraint update form is open
            fedata.isNodalConstraintUpdateInProgress = true;
            fedata.meshdata.clear_selected_nodes();

            // Only show if not already visible; otherwise just bring to front
            if (!nodalconstraint_Form.Visible)
            {
                // Show the form
                nodalconstraint_Form.update_dataGridView();
                nodalconstraint_Form.update_selected_node_list();
                nodalconstraint_Form.Show(this);
            }

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

                // Set the start position to manual so we can control placement
                edgeconstraint_Form.StartPosition = FormStartPosition.Manual;

                // Center the form on the parent
                CenterFormOnParent(edgeconstraint_Form);

            }

            // Turn on Flag Edge Constraint update form is open
            fedata.isEdgeConstraintUpdateInProgress = true;
            fedata.meshdata.clear_selected_edges();

            // Only show if not already visible; otherwise just bring to front
            if (!edgeconstraint_Form.Visible)
            {
                // Show the form
                edgeconstraint_Form.update_dataGridView();
                edgeconstraint_Form.update_selected_edge_list();
                edgeconstraint_Form.Show(this);
            }


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


                // Set the start position to manual so we can control placement
                matprop_Form.StartPosition = FormStartPosition.Manual;

                // Center the form on the parent
                CenterFormOnParent(matprop_Form);

            }

            // Turn on Flag Material update form is open
            fedata.isMaterialUpdateInProgress = true;
            fedata.meshdata.clear_selected_mesh();

            // Only show if not already visible; otherwise just bring to front
            if (!matprop_Form.Visible)
            {
                // Show the form
                matprop_Form.update_material_data();
                matprop_Form.update_selected_element_list();
                matprop_Form.Show(this);
            }

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
            fedata.update_openTK_uniforms(true, true, true);

            glControl_main_panel.Invalidate();

        }


        public void CallFrom_matprop_frm(int material_id, bool isAssignMaterial = false, bool isDeleteMaterial = false)
        {
            if (isAssignMaterial == true)
            {
                // Assign the material to the selected elements
                fedata.update_material_id(material_id, false);
            }


            if (isDeleteMaterial == true)
            {
                // Material is deleted, update with default material
                fedata.update_material_id(material_id, true);
            }

            // Refresh 
            glControl_main_panel.Invalidate();

        }

        public void CallFrom_option_frm(bool isShrinkMesh = false)
        {
            if (isShrinkMesh == true)
            {
                // Perform the shrinkage of the mesh
                fedata.meshdata.update_mesh_shrinkage();

                if (fedata.resultmeshdata.isResultSet == true)
                {
                    fedata.resultmeshdata.rsltmeshdata.update_mesh_shrinkage();
                }
            }

            // Refresh 
            glControl_main_panel.Invalidate();

        }


        public void CallFrom_modaloption_frm()
        {
            // Refresh 
            glControl_main_panel.Invalidate();
        }

        #endregion


        #region"Solver Events"

        private void dHelmholtzSolveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 2D Helmholtz Solver
            if (fedata.isModelSet == false)
                return;

            // Check if solver_Form is null or disposed
            if (solver_Form == null || solver_Form.IsDisposed)
            {
                solver_Form = new solver_frm(ref fedata);

                // Make it behave like a tool window
                solver_Form.FormBorderStyle = FormBorderStyle.SizableToolWindow;
                solver_Form.ShowInTaskbar = false;
                solver_Form.TopLevel = true;
                solver_Form.Owner = this;


                // Set the start position to manual so we can control placement
                solver_Form.StartPosition = FormStartPosition.Manual;

                // Center the form on the parent
                CenterFormOnParent(solver_Form);

            }

            // Only show if not already visible; otherwise just bring to front
            if (!solver_Form.Visible)
            {
                solver_Form.updateTextBox();
                solver_Form.Show(this);
            }

            solver_Form.BringToFront();

            glControl_main_panel.Invalidate();


        }


        private void dModalSolveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 2D Modal Solver
            if (fedata.isModelSet == false)
                return;

            // Check if modalsolver_Form is null or disposed
            if (modalsolver_Form == null || modalsolver_Form.IsDisposed)
            {
                modalsolver_Form = new modalsolver_frm(ref fedata);

                // Make it behave like a tool window
                modalsolver_Form.FormBorderStyle = FormBorderStyle.SizableToolWindow;
                modalsolver_Form.ShowInTaskbar = false;
                modalsolver_Form.TopLevel = true;
                modalsolver_Form.Owner = this;


                // Set the start position to manual so we can control placement
                modalsolver_Form.StartPosition = FormStartPosition.Manual;

                // Center the form on the parent
                CenterFormOnParent(modalsolver_Form);

            }

            // Only show if not already visible; otherwise just bring to front
            if (!modalsolver_Form.Visible)
            {
                modalsolver_Form.updateTextBox();
                modalsolver_Form.Show(this);
            }

            modalsolver_Form.BringToFront();

            glControl_main_panel.Invalidate();

        }

        private void showResultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // showResultsToolStripMenuItem.Checked = !showResultsToolStripMenuItem.Checked;

        }


        private void fieldRealPlotToolStripMenuItem_Click(object sender, EventArgs e) => TrySetResultOption(1);

        private void fieldImaginaryPlotToolStripMenuItem_Click(object sender, EventArgs e) => TrySetResultOption(2);

        private void fieldMagnitudePlotToolStripMenuItem_Click(object sender, EventArgs e) => TrySetResultOption(3);

        private void fieldPhasePlotToolStripMenuItem_Click(object sender, EventArgs e) => TrySetResultOption(4);

        private void modalResultsToolStripMenuItem_Click(object sender, EventArgs e) => TrySetResultOption(5);

        private void hideResultsToolStripMenuItem_Click(object sender, EventArgs e) => TrySetResultOption(0);


        private void modeResultsSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!fedata.modalresultmeshdata.isModalResultSet)
                return;

            // Check if modal_Form is null or disposed
            if (modalresultoption_Form == null || modalresultoption_Form.IsDisposed)
            {
                modalresultoption_Form = new modalresultoption_frm(ref this.fedata);

                // Make it behave like a tool window
                modalresultoption_Form.FormBorderStyle = FormBorderStyle.SizableToolWindow;
                modalresultoption_Form.ShowInTaskbar = false;
                modalresultoption_Form.TopLevel = true;
                modalresultoption_Form.Owner = this;


                // Set the start position to manual so we can control placement
                modalresultoption_Form.StartPosition = FormStartPosition.Manual;

                // Center the form on the parent
                CenterFormOnParent(modalresultoption_Form);

            }

            if(!modalresultoption_Form.Visible)
            {
                // Show the form
                modalresultoption_Form.initialize_modal_form();
                modalresultoption_Form.Show(this);
            }


            modalresultoption_Form.BringToFront();

            modalresultoption_Form.Invalidate();

        }



        private void TrySetResultOption(int option)
        {
            if (option == 5)
            {
                if (!fedata.modalresultmeshdata.isModalResultSet)
                    return;

                set_ResultOption(option);
                return;
            }

            if (!fedata.resultmeshdata.isResultSet && !fedata.modalresultmeshdata.isModalResultSet)
                return;

            set_ResultOption(option);
        }

        public void set_ResultOption(int option = 0)
        {
            // Reset menu checks
            fieldRealPlotToolStripMenuItem.Checked = option == 1 ? true : false;
            fieldImaginaryPlotToolStripMenuItem.Checked = option == 2 ? true : false;
            fieldMagnitudePlotToolStripMenuItem.Checked = option == 3 ? true : false;
            fieldPhasePlotToolStripMenuItem.Checked = option == 4 ? true : false;
            modalResultsToolStripMenuItem.Checked = option == 5 ? true : false;

            // Reset all flags
            gvariables_static.is_paint_ureal = false;
            gvariables_static.is_paint_uimag = false;
            gvariables_static.is_paint_umagnitude = false;
            gvariables_static.is_paint_uphase = false;
            gvariables_static.is_paint_modalresults = false;

            // Transparency defaults
            gvariables_static.geom_transparency = 1.0f;
            gvariables_static.rslt_transparency = 0.0f;

            float rslt_geom_transparency = 0.2f;
            float rslt_rslt_transparency = 0.98f;    

            // Apply selection
            switch (option)
            {
                case 1:
                    // Field Real values
                    gvariables_static.is_paint_ureal = true;
                    gvariables_static.geom_transparency = rslt_geom_transparency;
                    gvariables_static.rslt_transparency = rslt_rslt_transparency;
                    break;

                case 2:
                    // Field Imaginary values
                    gvariables_static.is_paint_uimag = true;
                    gvariables_static.geom_transparency = rslt_geom_transparency;
                    gvariables_static.rslt_transparency = rslt_rslt_transparency;
                    break;

                case 3:
                    // Field Magnitude values
                    gvariables_static.is_paint_umagnitude = true;
                    gvariables_static.geom_transparency = rslt_geom_transparency;
                    gvariables_static.rslt_transparency = rslt_rslt_transparency;
                    break;

                case 4:
                    // Field Phase values
                    gvariables_static.is_paint_uphase = true;
                    gvariables_static.geom_transparency = rslt_geom_transparency;
                    gvariables_static.rslt_transparency = rslt_rslt_transparency;
                    break;
                case 5:
                    // Paint modal results
                    gvariables_static.is_paint_modalresults = true;
                    gvariables_static.geom_transparency = rslt_geom_transparency;
                    gvariables_static.rslt_transparency = rslt_rslt_transparency;
                    break;
                case 0:
                default:
                    break;
            }


            if(option != 5 && option != 0)
            {
                fedata.resultmeshdata.updateResultType(fedata.graphic_events_control);
            }
            else
            {
                // fedata.modalresultmeshdata.updateSelectedMode(0)
            }
            
            fedata.update_openTK_uniforms(false, false, true);

            // Refresh 
            glControl_main_panel.Invalidate();

        }




        private bool IsApplicationIdle()
        {
            Message msg;
            return !gvariables_static.PeekMessage(out msg, IntPtr.Zero, 0, 0, 0);
        }

        private void OnApplicationIdle(object sender, EventArgs e)
        {
            while (IsApplicationIdle())
            {
                fedata.modalresultmeshdata.update_modal_animation();   // Update animation
                glControl_main_panel.Invalidate(); // Redraw
            }
        }


        #endregion

    }
}
