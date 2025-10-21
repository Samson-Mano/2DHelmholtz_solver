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







        }

    }
}
