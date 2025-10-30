using _2DHelmholtz_solver.src.events_handler;
using _2DHelmholtz_solver.src.global_variables;
using _2DHelmholtz_solver.src.model_store.fe_objects;
using OpenTK.Graphics.ES11;
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
        private double frequency_values = 10.0;
        private int solver_type = 0;

        public solver_frm(ref fedata_store fe_data)
        {
            InitializeComponent();

            this.fe_data = fe_data;

        }

        private void solver_frm_Load(object sender, EventArgs e)
        {
       
        }


        public void updateTextBox()
        {

            comboBox_solvertype.SelectedIndex = solver_type;

            textBox_frequency.Text = frequency_values.ToString();
            updateFrequencyTextBox();
            // Update the model extent

            double x_extent = fe_data.geom_bounds.X;
            double y_extent = fe_data.geom_bounds.X;

            // Use general format: no decimals for large values, scientific for small (<1)
            string formatValue(double v)
            {
                if (Math.Abs(v) >= 1.0)
                    return v.ToString("F0");  // 0 digits after decimal
                else
                    return v.ToString("0.####E+0");  // scientific notation, 4 significant digits
            }


            textBox_xyextent.Text = $"[{formatValue(x_extent)}, {formatValue(y_extent)}]";
        }


        private void textBox_frequency_TextChanged(object sender, EventArgs e)
        {

            updateFrequencyTextBox();

        }

        private void updateFrequencyTextBox()
        {
            // Use Trim() to avoid issues with whitespace
            string input = textBox_frequency.Text.Trim();

            if (double.TryParse(input, out double Freq) && Freq > 0)
            {
                const double c = 300; // speed of light (m/s) * 10^6
                double angularfreq = Freq * 2.0 * Math.PI;
                double wavelength = c / Freq;

                // Reset colors
                textBox_angularfrequency.ForeColor = SystemColors.WindowText;
                textBox_wavelength.ForeColor = SystemColors.WindowText;

                // Use general formatting: fixed-point for large, scientific for small values
                string formatValue(double v)
                {
                    if (Math.Abs(v) >= 1.0)
                        return v.ToString("F3");  // 3 decimals
                    else
                        return v.ToString("0.###E+0"); // scientific
                }

                textBox_angularfrequency.Text = formatValue(angularfreq);
                textBox_wavelength.Text = formatValue(wavelength);
            }
            else
            {
                // Show errors in red
                textBox_angularfrequency.ForeColor = Color.Red;
                textBox_wavelength.ForeColor = Color.Red;

                textBox_angularfrequency.Text = "Invalid input";
                textBox_wavelength.Text = "Invalid input";
            }

            // Refresh UI
            textBox_angularfrequency.Invalidate();
            textBox_wavelength.Invalidate();

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

            string input = textBox_frequency.Text.Trim();

            if (double.TryParse(input, out double Freq) && Freq > 0)
            {
                frequency_values = Freq;
                solver_type = comboBox_solvertype.SelectedIndex;

            }
            else
            {
                // Invalid frequency input
                MessageBox.Show("Invalid frequency input!!!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            double[] solver_settings = new double[2];
            solver_settings[0] = frequency_values;
            solver_settings[1] = solver_type;

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

            // Reset the results
            fe_data.isResultSet = false;


            // Call the C++ dll solver
            try
            {
                richTextBox_AnalysisUpdate.Clear();
                AppendStatus("Finite Element Solve started...\n");

                // Run solver asynchronously
                await Task.Run(() =>
                {
                    // Call C++ solver
                    helmholtzSolverInterop.solve_helmholtzsolverCPP(inputPath, outputPath,
                        solver_settings, solver_settings.Length,
                        ref isAnalysisSuccess, OnStatusUpdate);

                });

                if(isAnalysisSuccess == true)
                {

                    AppendStatus("Solve completed successfully!\n");

                    // Read the binary result file
                    if (!File.Exists(outputPath))
                    {
                        AppendStatus("Result file not found: " + outputPath + "\n");
                        return;
                    }

                    try
                    {
                        using (var reader = new BinaryReader(File.Open(outputPath, FileMode.Open, FileAccess.Read)))
                        {
                            // Read number of nodes
                            int nodeCount = reader.ReadInt32();
                            AppendStatus($"Reading results for {nodeCount} nodes...\n");

                            for (int i = 0; i < nodeCount; i++)
                            {
                                int node_id = reader.ReadInt32();
                                double field_real_value = reader.ReadDouble();
                                double field_imag_value = reader.ReadDouble();

                                fe_data.fe_nodes.update_results(node_id, field_real_value, field_imag_value);
                            }

                            fe_data.setResultMesh();
                            fe_data.setResultExtremes();
                            fe_data.isResultSet = true; 
                        }

                        // Call the main form
                        if (this.Owner is main_frm mainForm)
                        {
                            mainForm.set_ResultOption(1);
                        }

                        AppendStatus("Results read complete!\n");
                        MessageBox.Show("Solve completed successfully!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        AppendStatus("Error reading binary results: " + ex.Message + "\n");
                        MessageBox.Show("Error reading results file:\n" + ex.Message,
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

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
