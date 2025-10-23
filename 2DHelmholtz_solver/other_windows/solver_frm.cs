using _2DHelmholtz_solver.src.events_handler;
using _2DHelmholtz_solver.src.global_variables;
using _2DHelmholtz_solver.src.model_store.fe_objects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _2DHelmholtz_solver.other_windows
{
    public partial class solver_frm : Form
    {
        private fedata_store fe_data;

        public solver_frm(ref fedata_store fe_data)
        {
            InitializeComponent();

            this.fe_data = fe_data;

        }

        private async void button_performsolve_Click(object sender, EventArgs e)
        {
            // Check the inputs (Whether the boundary condition is applied or not)
            if (fe_data.fe_edgeconstraints.edgecnst_count == 0 &&
                fe_data.fe_nodeconstraints.ndcnst_count == 0)
            {
                richTextBox_AnalysisUpdate.Clear();
                AppendStatus("No boundary conditions applied...\n");

                MessageBox.Show("No Boundary Conditions applied!!!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }

            // C# GUI exports model to a .bin file.
            // C# calls your C++ DLL (using P/Invoke).
            // C++ DLL reads the.bin file, performs the simulation, and writes results to another .bin.
            // C# re-imports and displays results.

            string inputPath = Path.Combine(Application.StartupPath, "model_input.bin");
            string outputPath = Path.Combine(Application.StartupPath, "model_output.bin");

            // Write the binary file
            file_events.export_binary_mesh(inputPath,
                                        fe_data.fe_nodes,
                                        fe_data.fe_tris,
                                        fe_data.fe_quads,
                                        fe_data.fe_nodeconstraints,
                                        fe_data.fe_edgeconstraints,
                                        fe_data.fe_loads,
                                        fe_data.meshdata.mesh_boundaries,
                                        fe_data.fe_materials);


            bool isAnalysisSuccess = false;

            // Call the C++ dll solver
            try
            {
                richTextBox_AnalysisUpdate.Clear();
                AppendStatus("Finite Element Solve started...\n");

                // Run solver asynchronously
                await Task.Run(() =>
                {
                    // Call C++ solver
                    helmholtzSolverInterop.solve_helmholtzsolverCPP(inputPath, outputPath, ref isAnalysisSuccess, OnStatusUpdate);

                });

                if(isAnalysisSuccess == true)
                {

                    AppendStatus("Solve completed successfully!\n");
                    MessageBox.Show("Solve completed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                }
                else
                {
                    AppendStatus("Solve Failed!\n");
                    MessageBox.Show($"Solve failed !!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Solve failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


        }


        private void OnStatusUpdate(string message)
        {
            // Marshal back to UI thread safely
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => AppendStatus(message + "\n")));
            }
            else
            {
                AppendStatus(message + "\n");
            }
        }

        private void AppendStatus(string text)
        {
            richTextBox_AnalysisUpdate.AppendText(text);
            richTextBox_AnalysisUpdate.ScrollToCaret();
        }

        private void solver_frm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Clear the status update rich text box 
            richTextBox_AnalysisUpdate.Clear();

        }


    }
}
