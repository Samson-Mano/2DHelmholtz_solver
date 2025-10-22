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

        private void button_performsolve_Click(object sender, EventArgs e)
        {
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
                                        fe_data.fe_materials);



            // Call the C++ dll solver
            try
            {
                helmholtzSolverInterop.solve_helmholtzsolverCPP(inputPath, outputPath);
                MessageBox.Show("Solver completed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Solver failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }



        }

    }
}
