using _2DHelmholtz_solver.global_variables;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _2DHelmholtz_solver.other_windows
{
    public partial class contourrange_frm : Form
    {
        public contourrange_frm()
        {
            InitializeComponent();
        }

        public void UpdateContourRangeTextBoxes()
        {
            textBox_contourmax.Text = gvariables_static.contourLevel_rangeMax.ToString(CultureInfo.InvariantCulture);
            textBox_contourmin.Text = gvariables_static.contourLevel_rangeMin.ToString(CultureInfo.InvariantCulture);
        }


        private void button_updaterange_Click(object sender, EventArgs e)
        {
            // Update the Contour range limits
            // Validate the input values
            if (textBox_contourmax.Text == "" || textBox_contourmin.Text == "")
            {
                return;
            }

            if (float.TryParse(textBox_contourmax.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out float max) &&
               float.TryParse(textBox_contourmin.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out float min))
            {
                if (max > min && min >= 0.0f && max <= 1.0f)
                {
                    gvariables_static.contourLevel_rangeMax = max;
                    gvariables_static.contourLevel_rangeMin = min;

                    // modeldata.switch_result_option(true);

                    // Optional: Trigger immediate redraw
                    if (this.Owner is main_frm mainForm)
                    {
                        mainForm.CallFrom_contourrange_frm();
                    }
                }
                else
                {
                    // MessageBox.Show("Contour max must be greater than contour min.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBox_contourmin.Text = gvariables_static.contourLevel_rangeMin.ToString(CultureInfo.InvariantCulture);
                    textBox_contourmax.Text = gvariables_static.contourLevel_rangeMax.ToString(CultureInfo.InvariantCulture);

                    return;
                }
            }
            else
            {
                // MessageBox.Show("Please enter valid numeric values for contour range.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox_contourmin.Text = gvariables_static.contourLevel_rangeMin.ToString(CultureInfo.InvariantCulture);
                textBox_contourmax.Text = gvariables_static.contourLevel_rangeMax.ToString(CultureInfo.InvariantCulture);

                return;
            }

        }

        private void button_resetrange_Click(object sender, EventArgs e)
        {
            textBox_contourmax.Text = "1.0";
            textBox_contourmin.Text = "0.0";

            gvariables_static.contourLevel_rangeMax = 1.0f;
            gvariables_static.contourLevel_rangeMin = 0.0f;

            if (this.Owner is main_frm mainForm)
            {
                mainForm.CallFrom_contourrange_frm();
            }

        }
    }
}
