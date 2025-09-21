using _2DHelmholtz_solver.src.model_store.fe_objects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _2DHelmholtz_solver.other_windows
{
    public partial class constraint_frm : Form
    {
        private fedata_store fe_data;

        public constraint_frm(ref fedata_store fe_data)
        {
            InitializeComponent();

            this.fe_data = fe_data;

        }

        private void constraint_frm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Control the flag
            fe_data.meshdata.isConstraintUpdateInProgress = false;

            // Call the main form
            if (this.Owner is main_frm mainForm)
            {
                mainForm.CallFrom_constraint_frm();
            }

        }
    }
}
