using _2DHelmholtz_solver.src.events_handler;
using _2DHelmholtz_solver.src.global_variables;
using _2DHelmholtz_solver.src.model_store.fe_objects;
using _2DHelmholtz_solver.src.model_store.rslt_objects;
using _2DHelmholtz_solver.src.solver;
using OpenTK.Graphics.ES11;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace _2DHelmholtz_solver.other_windows
{

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BinaryFileHeader
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] Magic;  // 'SEMF'
        public uint Version;
        public uint NumModes;
        public uint NumNodes;
        public ulong ModeDataOffset;
        public ulong ModeIndexOffset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ModeIndexEntry
    {
        public uint ModeId;
        public double Frequency;
        public ulong FileOffset;
        public ulong DataSize;
    }


    public class ModeInfo
    {
        public int Id { get; set; }
        public double Frequency { get; set; }
        public long FileOffset { get; set; }
        public long DataSize { get; set; }
    }



    public partial class modalsolver_frm : Form
    {
        private fedata_store fe_data;
        private int number_of_modes = 20;
        private int solver_type = 0;

        public modalsolver_frm(ref fedata_store fe_data)
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

            textBox_numofmodes.Text = number_of_modes.ToString();

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

            comboBox_spectralorderN.SelectedIndex = (fe_data.spectral_order_N - 1);

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

            string input = textBox_numofmodes.Text.Trim();

            if (int.TryParse(input, out int numofmodes) && numofmodes > 0)
            {
                number_of_modes = numofmodes;
                solver_type = comboBox_solvertype.SelectedIndex + 1;

            }
            else
            {
                // Invalid frequency input
                MessageBox.Show("Invalid number of modes input!!!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            double[] solver_settings = new double[1];
            solver_settings[0] = number_of_modes;
            solver_settings[1] = solver_type;

            // C# GUI exports model to a .bin file.
            // C# calls your C++ DLL (using P/Invoke).
            // C++ DLL reads the.bin file, performs the simulation, and writes results to another .bin.
            // C# re-imports and displays results.

            string inputPath = Path.Combine(Application.StartupPath, "modal_analysis_input.bin");
            string outputPath = Path.Combine(Application.StartupPath, "modal_analysis_output.bin");


            // Write the binary file
            file_events.export_binary_mesh(inputPath,
                                        fe_data.spectral_order_N,
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
                    modalSolverInterop.solve_spectralmodalanalysisCPP(inputPath, outputPath,
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
                        using (var fs = new FileStream(outputPath, FileMode.Open, FileAccess.Read))

                        using (var reader = new BinaryReader(File.Open(outputPath, FileMode.Open, FileAccess.Read)))
                        {


                            // Read header
                            var header = ReadStruct<BinaryFileHeader>(reader);

                            if (System.Text.Encoding.ASCII.GetString(header.Magic) != "SEMF")
                                throw new InvalidDataException("Invalid file format");

                            AppendStatus($"File version: {header.Version}");
                            AppendStatus($"Number of modes: {header.NumModes}");
                            AppendStatus($"Number of nodes: {header.NumNodes}");

                            AppendStatus($"Reading results started...\n");

                            fe_data.modalresultmeshdata = new modal_rsltdata_store();

                            // Read nodes
              
                            for (int i = 0; i < header.NumNodes; i++)
                            {
                                int node_id = reader.ReadInt32();
                                double node_xcoord = reader.ReadDouble();
                                double node_ycoord = reader.ReadDouble();

                                fe_data.modalresultmeshdata.modal_rslt_nodes.Add(node_id, 
                                    new modal_rsltnode_store { node_id = node_id,
                                                        node_pt_x_coord = node_xcoord,
                                                        node_pt_y_coord = node_ycoord
                                    });

                            }

                            AppendStatus($"Reading results for {header.NumNodes} nodes complete \n");


                            // Read number of edges
                            long edgesCount = (long)(header.ModeIndexOffset - (ulong)fs.Position) / (2 * sizeof(int));

                            for (int i = 0; i < edgesCount; i++)
                            {
                                int start_nodeid = reader.ReadInt32();
                                int end_nodeid = reader.ReadInt32();

                                fe_data.modalresultmeshdata.rslt_edges.Add(new rsltedge_store
                                {
                                    startnode = start_nodeid,
                                    endnode = end_nodeid
                                });
                            }

                            AppendStatus($"Reading results for {edgesCount} edges complete \n");


                            // Read number of triangles
                            long trianglesCount = (long)(header.ModeDataOffset - (ulong)fs.Position) / (3 * sizeof(int));

                            for (int i = 0; i < trianglesCount; i++)
                            {
                                int n1 = reader.ReadInt32();
                                int n2 = reader.ReadInt32();
                                int n3 = reader.ReadInt32();

                                fe_data.modalresultmeshdata.rslt_tris.Add(new rslttri_store
                                {
                                    tri_node1 = n1,
                                    tri_node2 = n2,
                                    tri_node3 = n3,
                                });
                            }

                            AppendStatus($"Reading results for {trianglesCount} triangles complete \n");

                            // Set the Result mesh
                            // Read mode index table
                            for (int i = 0; i < header.NumModes; i++)
                            {
                                var modeIndex = ReadStruct<ModeIndexEntry>(reader);
                                fe_data.modalresultmeshdata.modes.Add(new modeInfo
                                {
                                    Id = (int)modeIndex.ModeId,
                                    Frequency = modeIndex.Frequency,
                                    FileOffset = (long)modeIndex.FileOffset,
                                    DataSize = (long)modeIndex.DataSize
                                });
                            }



                            fe_data.modalresultmeshdata.setResultMesh();
                            fe_data.modalresultmeshdata.updateSelectedMode(0);
                            fe_data.modalresultmeshdata.isModalResultSet = true;

                            fe_data.update_openTK_uniforms(true, true, true);
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


        private T ReadStruct<T>(BinaryReader reader) where T : struct
        {
            byte[] bytes = reader.ReadBytes(Marshal.SizeOf<T>());
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            T structure = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
            handle.Free();
            return structure;
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

        private void comboBox_spectralorderN_SelectedIndexChanged(object sender, EventArgs e)
        {
            fe_data.spectral_order_N = comboBox_spectralorderN.SelectedIndex + 1;
        }
    }
}
