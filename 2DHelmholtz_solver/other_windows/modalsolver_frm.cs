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
        public uint NumEdges;   
        public uint NumTriangles;
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

        private void modalsolver_frm_Load(object sender, EventArgs e)
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

            comboBox_spectralorderN.SelectedIndex = (fe_data.spectral_order_N - 3);

        }


        private async void button_performsolve_Click(object sender, EventArgs e)
        {
            try
            {
                // Disable the button to prevent multiple clicks
                button_performsolve.Enabled = false;
                richTextBox_AnalysisUpdate.Clear();

                // Check if boundary conditions are applied
                if (fe_data.fe_edgeconstraints.edgecnst_count == 0 &&
                    fe_data.fe_nodeconstraints.ndcnst_count == 0)
                {
                    AppendStatus("Information: No boundary conditions applied!\n");
                    //AppendStatus("Please apply boundary conditions before running the solver.\n");
                    //MessageBox.Show("No Boundary Conditions applied!\n\nPlease apply boundary conditions before running the solver.",
                    //              "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //return;
                }

                AppendStatus("\nChecking dependencies...\n");

                if (!CheckAllDependencies())
                {
                    AppendStatus("\n❌ Missing dependencies detected. Please copy all required DLLs to the application directory.\n");
                    MessageBox.Show("Missing required DLL dependencies.\n\nPlease ensure all solver DLLs are copied to the application directory.",
                                  "DLL Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }


                AppendStatus("Starting solver initialization...\n");

                // Step 1: Get diagnostic info (optional, good for debugging)
                string diagnosticInfo = modalSolverInterop.GetDiagnosticInfo();
                AppendStatus(diagnosticInfo);

                // Step 2: Initialize the DLL
                AppendStatus("\nInitializing solver DLL...\n");
                if (!modalSolverInterop.Initialize())
                {
                    AppendStatus($"✗ DLL initialization failed: {modalSolverInterop.LastError}\n");
                    MessageBox.Show($"Failed to initialize solver DLL:\n\n{modalSolverInterop.LastError}",
                                  "DLL Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                AppendStatus("✓ DLL loaded successfully!\n");

                // Step 3: Run the solver
                AppendStatus("\nStarting modal analysis...\n");
                await RunSolverAsync();
            }
            catch (Exception ex)
            {
                AppendStatus($"\n❌ Unexpected error: {ex.Message}\n");
                AppendStatus($"Stack trace: {ex.StackTrace}\n");
                MessageBox.Show($"An unexpected error occurred:\n\n{ex.Message}",
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Re-enable the button
                button_performsolve.Enabled = true;
            }
        }


        private bool CheckAllDependencies()
        {
            string targetDir = AppDomain.CurrentDomain.BaseDirectory;
            string[] requiredDlls = new string[]
            {
        "modalspectral_solverCPP.dll",
        "libarpack.dll",
        "libgcc_s_seh-1.dll",
        "libgfortran-5.dll",
        "liblapack.dll",
        "libquadmath-0.dll",
        "libwinpthread-1.dll",
        "openblas.dll"
            };

            bool allExist = true;
            foreach (string dll in requiredDlls)
            {
                string fullPath = Path.Combine(targetDir, dll);
                bool exists = File.Exists(fullPath);
                AppendStatus($"  {(exists ? "✓" : "✗")} {dll}: {(exists ? "Found" : "MISSING")}\n");
                if (!exists) allExist = false;
            }

            return allExist;
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

        private void modalsolver_frm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Clear the status update rich text box 
            richTextBox_AnalysisUpdate.Clear();

        }

        private void comboBox_spectralorderN_SelectedIndexChanged(object sender, EventArgs e)
        {
            fe_data.spectral_order_N = comboBox_spectralorderN.SelectedIndex + 3;
        }



        private async Task RunSolverAsync()
        {
            try
            {
                // Step 1: Check if DLL is available
                if (!modalSolverInterop.CheckDllExists())
                {
                    richTextBox_AnalysisUpdate.AppendText($"ERROR: {modalSolverInterop.LastError}\n");
                    richTextBox_AnalysisUpdate.AppendText(modalSolverInterop.GetDiagnosticInfo());
                    return;
                }

                // Step 2: Initialize the DLL
                if (!modalSolverInterop.Initialize())
                {
                    richTextBox_AnalysisUpdate.AppendText($"Failed to initialize DLL: {modalSolverInterop.LastError}\n");
                    richTextBox_AnalysisUpdate.AppendText(modalSolverInterop.GetDiagnosticInfo());
                    return;
                }

                richTextBox_AnalysisUpdate.AppendText("DLL loaded successfully!\n");

                // Step 3: Validate input files
                string inputPath = Path.Combine(Application.StartupPath, "modal_analysis_input.bin");
                string outputPath = Path.Combine(Application.StartupPath, "modal_analysis_output.bin");

                // Delete existing input file if it exists
                if (File.Exists(inputPath))
                {
                    try
                    {
                        File.Delete(inputPath);
                        richTextBox_AnalysisUpdate.AppendText("Deleted existing input file.\n");
                    }
                    catch (IOException ex)
                    {
                        richTextBox_AnalysisUpdate.AppendText($"Warning: Could not delete existing input file: {ex.Message}\n");
                        // Try to force garbage collection to release any locks
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        File.Delete(inputPath);
                    }
                }

                // Delete existing output file if it exists
                if (File.Exists(outputPath))
                {
                    try
                    {
                        File.Delete(outputPath);
                        richTextBox_AnalysisUpdate.AppendText("Deleted existing output file.\n");
                    }
                    catch (IOException ex)
                    {
                        richTextBox_AnalysisUpdate.AppendText($"Warning: Could not delete existing output file: {ex.Message}\n");
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        File.Delete(outputPath);
                    }
                }



                // Write input file
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

                // Step 4: Prepare solver settings
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

                double[] solver_settings = new double[2];
                solver_settings[0] = number_of_modes;
                solver_settings[1] = solver_type;


                // Step 5: Safely call the solver
                var result = await Task.Run(() => modalSolverInterop.SolveSafely(
                    inputPath,
                    outputPath,
                    solver_settings,
                    OnStatusUpdate
                ));

                if (result.Success)
                {
                    richTextBox_AnalysisUpdate.AppendText("Solver completed successfully!\n");
                    
                    // Process results...
                    process_results(outputPath);

                }
                else
                {
                    richTextBox_AnalysisUpdate.AppendText($"Solver failed: {result.ErrorMessage}\n");
                }
            }
            catch (Exception ex)
            {
                richTextBox_AnalysisUpdate.AppendText($"Unexpected error: {ex.Message}\n");
                richTextBox_AnalysisUpdate.AppendText(modalSolverInterop.GetDiagnosticInfo());
            }
            finally
            {
                // Clean up
                modalSolverInterop.Unload();
            }
        }




        // Add a button to test DLL connectivity
        private void btnTestDLL_Click(object sender, EventArgs e)
        {
            string diagnosticInfo = modalSolverInterop.GetDiagnosticInfo();
            richTextBox_AnalysisUpdate.AppendText(diagnosticInfo);

            if (modalSolverInterop.Initialize())
            {
                richTextBox_AnalysisUpdate.AppendText("\n✓ DLL loaded successfully!\n");
                modalSolverInterop.Unload();
            }
            else
            {
                richTextBox_AnalysisUpdate.AppendText($"\n✗ DLL load failed: {modalSolverInterop.LastError}\n");
            }
        }



        private void process_results(string outputPath)
        {
            // Read the binary result file
            if (!File.Exists(outputPath))
            {
                AppendStatus("Result file not found: " + outputPath + "\n");
                return;
            }

            try
            {
                using (var fs = new FileStream(outputPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (var reader = new BinaryReader(fs))
                    {
                        // Read header
                        var header = ReadStruct<BinaryFileHeader>(reader);

                        if (System.Text.Encoding.ASCII.GetString(header.Magic) != "SEMF")
                            throw new InvalidDataException("Invalid file format");

                        AppendStatus($"File version: {header.Version}");
                        AppendStatus($"Number of modes: {header.NumModes}");
                        AppendStatus($"Number of nodes: {header.NumNodes}");
                        AppendStatus($"Number of edges: {header.NumEdges}");
                        AppendStatus($"Number of triangles: {header.NumTriangles}");
                        AppendStatus($"Reading results started...\n");

                        fe_data.modalresultmeshdata = new modal_rsltdata_store();

                        AppendStatus($"File position : {fs.Position}\n");

                        // Read nodes

                        for (int i = 0; i < header.NumNodes; i++)
                        {
                            int node_id = reader.ReadInt32();
                            double node_xcoord = reader.ReadDouble();
                            double node_ycoord = reader.ReadDouble();

                            fe_data.modalresultmeshdata.modal_rslt_nodes.Add(node_id,
                                new modal_rsltnode_store
                                {
                                    node_id = node_id,
                                    node_pt_x_coord = node_xcoord,
                                    node_pt_y_coord = node_ycoord
                                });

                        }

                        AppendStatus($"Reading results for {header.NumNodes} nodes complete \n");


                        // Read edges
                        for (int i = 0; i < header.NumEdges; i++)
                        {
                            int start_nodeid = reader.ReadInt32();
                            int end_nodeid = reader.ReadInt32();

                            fe_data.modalresultmeshdata.rslt_edges.Add(new rsltedge_store
                            {
                                startnode = start_nodeid,
                                endnode = end_nodeid
                            });
                        }

                        AppendStatus($"Reading results for {header.NumEdges} edges complete \n");


                        // Read triangles
                        for (int i = 0; i < header.NumTriangles; i++)
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

                        AppendStatus($"Reading results for {header.NumTriangles} triangles complete \n");

                        AppendStatus($"File position : {fs.Position}\n");

                        // Read mode index table
                        AppendStatus($"\nReading mode index table...\n");
                        fe_data.modalresultmeshdata.modes.Clear();
                        fe_data.modalresultmeshdata.natural_Frequencies.Clear();

                        // Set the Result mesh
                        // Read mode index table
                        for (int i = 0; i < header.NumModes; i++)
                        {
                            long posBeforeRead = fs.Position;

                            // Read manually to avoid struct alignment issues
                            uint modeId = reader.ReadUInt32();
                            double frequency = reader.ReadDouble();
                            ulong fileOffset = reader.ReadUInt64();
                            ulong dataSize = reader.ReadUInt64();

                            fe_data.modalresultmeshdata.natural_Frequencies.Add(frequency);

                            AppendStatus($"  Mode {modeId}: f={frequency:F3} Hz, offset={fileOffset}, size={dataSize}\n");

                            fe_data.modalresultmeshdata.modes.Add(new modeInfo
                            {
                                Id = (int)modeId,
                                Frequency = frequency,
                                FileOffset = (long)fileOffset,
                                DataSize = (long)dataSize
                            });
                        }

                        AppendStatus($"  File position at the end of mode index table: {fs.Position}\n");



                        fe_data.modalresultmeshdata.setResultMesh();
                        fe_data.modalresultmeshdata.isModalResultSet = true;
                        fe_data.modalresultmeshdata.updateSelectedMode(0);
                        fe_data.modalresultmeshdata.start_animation();

                        fe_data.update_openTK_uniforms(true, true, true);
                    }

                    // Call the main form
                    if (this.Owner is main_frm mainForm)
                    {
                        mainForm.set_ResultOption(5); // Set the result option = 5, Paint modal results
                    }

                    AppendStatus("Results read complete!\n");
                    MessageBox.Show("Solve completed successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                AppendStatus("Error reading binary results: " + ex.Message + "\n");
                MessageBox.Show("Error reading results file:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        //________________________
    }


}
