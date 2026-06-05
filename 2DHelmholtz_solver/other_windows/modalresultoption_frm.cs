using _2DHelmholtz_solver.global_variables;
using _2DHelmholtz_solver.src.model_store.fe_objects;
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
    public partial class modalresultoption_frm : Form
    {

        private fedata_store fe_data;

        private const int max_number_of_modes = 100;


        private Panel panelPopup;
        private TextBox textPopup;
        private Button buttonPopupOK;
        private Button buttonPopupCancel;


        public modalresultoption_frm(ref fedata_store fe_data)
        {
            InitializeComponent();

            this.fe_data = fe_data;


            // === Popup Panel ===
            panelPopup = new Panel();
            panelPopup.Size = new Size(220, 70);
            panelPopup.BackColor = Form.DefaultBackColor;
            panelPopup.BorderStyle = BorderStyle.FixedSingle;
            panelPopup.Visible = false;   // Hidden by default
            panelPopup.BringToFront();
            groupBox1.Controls.Add(panelPopup);

            // === TextBox ===
            textPopup = new TextBox();
            textPopup.Size = new Size(200, 25);
            textPopup.Location = new Point(10, 10);
            panelPopup.Controls.Add(textPopup);

            // === OK Button ===
            buttonPopupOK = new Button();
            buttonPopupOK.Text = "OK";
            buttonPopupOK.Size = new Size(80, 25);
            buttonPopupOK.Location = new Point(20, 40);
            buttonPopupOK.Click += ButtonPopupOK_Click;
            panelPopup.Controls.Add(buttonPopupOK);

            // === Cancel Button ===
            buttonPopupCancel = new Button();
            buttonPopupCancel.Text = "Cancel";
            buttonPopupCancel.Size = new Size(80, 25);
            buttonPopupCancel.Location = new Point(120, 40);
            buttonPopupCancel.Click += ButtonPopupCancel_Click;
            panelPopup.Controls.Add(buttonPopupCancel);

        }



        public void initialize_modal_form()
        {
            // Initialie the Modal Form data

            // Get the model data
            int number_of_nodes = this.fe_data.modalresultmeshdata.natural_Frequencies.Count;


            // Clear the combobox existing items
            comboBox_modedata.Items.Clear();

            // Mode number
            int mode_number = 1;

            foreach (double nat_freq in this.fe_data.modalresultmeshdata.natural_Frequencies)
            {
                // Add to the combobox
                string modal_data_str = string.Format($"Mode {mode_number}:  Natural Frequency = {nat_freq:F4} Hz");

                comboBox_modedata.Items.Add(modal_data_str);

                mode_number++;
            }


            // Set the selected index to the first mode
            int selected_index = Properties.Settings.Default.Sett_modal_selected_index;

            if (selected_index >= 0 && selected_index < comboBox_modedata.Items.Count)
            {
                comboBox_modedata.SelectedIndex = selected_index;
            }
            else
            {
                comboBox_modedata.SelectedIndex = 0;
            }


            if (gvariables_static.animate_play)
            {

                // Set the status label Playing
                label_status.Text = "Playing";

            }
            else if (gvariables_static.animate_pause)
            {
                // Set the status label Paused
                label_status.Text = "Paused";
            }
            else
            {
                // Set the status label Stopped
                label_status.Text = "Stopped";

            }

            gvariables_static.modal_animation_speed = Properties.Settings.Default.Sett_modal_animation_speed;

            // Set the global variable
            double value = gvariables_static.modal_animation_speed;

            // Set label
            label_animation_speed.Text = value.ToString(CultureInfo.InvariantCulture);
            label_realtimeanim_speed.Text = $"1 second in real time = {value.ToString(CultureInfo.InvariantCulture)} second in Animation";


            //// Set the modal form is open
            //this.fe_data.isModalAnalysisPaint = true;
            //this.fe_data.update_model_transparency();

            ////___________________________________________________________________________________________________
            //// Animation control
            //this.fe_data.modalresultmeshdata.stop_animation();

            //gvariables_static.animate_play = true;
            //gvariables_static.animate_pause = false;
            //gvariables_static.animate_stop = false;

            //this.fe_data.modalresultmeshdata.start_animation();

            //this.fe_data.selected_mode_shape = comboBox_modedata.SelectedIndex;



            // Call to main form
            if (this.Owner is main_frm mainForm)
            {
                mainForm.CallFrom_modaloption_frm();
            }

        }

        private void button_close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void modalresultoption_frm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Update the settings
            Properties.Settings.Default.Sett_modal_selected_index = comboBox_modedata.SelectedIndex;
            Properties.Settings.Default.Sett_modal_animation_speed = gvariables_static.modal_animation_speed;

            Properties.Settings.Default.Save();

        }


        //__________________________________________________________________________________________________________

        private void button_animation_speed_Click(object sender, EventArgs e)
        {
            // Position near the button
            panelPopup.Location = new Point(button_animation_speed.Left, button_animation_speed.Bottom);

            textPopup.Text = "";         // Clear previous input
            panelPopup.Visible = true;   // Show popup
            label_realtimeanim_speed.Visible = false;
            textPopup.Focus();           // Focus for typing

        }




        private void ButtonPopupOK_Click(object sender, EventArgs e)
        {
            string input = textPopup.Text;

            // Test whether the input is a valid number (positive integer or float)
            // Try to parse the input as a floating-point number
            bool isNumeric = double.TryParse(
                input,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out double value
            );

            // Validate: numeric AND positive
            if (!isNumeric || value <= 0)
            {
                panelPopup.Visible = false;
                label_realtimeanim_speed.Visible = true;

                return; // Do not continue
            }

            // Set the modal animation global variable
            gvariables_static.modal_animation_speed = value;

            // Set label
            label_animation_speed.Text = value.ToString(CultureInfo.InvariantCulture);
            label_realtimeanim_speed.Text = $"1 second in real time = {value.ToString(CultureInfo.InvariantCulture)} second in Animation";

            panelPopup.Visible = false;
            label_realtimeanim_speed.Visible = true;
        }

        private void ButtonPopupCancel_Click(object sender, EventArgs e)
        {
            panelPopup.Visible = false;
            label_realtimeanim_speed.Visible = true;
        }


        private void button_play_pause_Click(object sender, EventArgs e)
        {
            if (gvariables_static.animate_play)
            {
                // Currently playing, so pause
                gvariables_static.animate_play = false;
                gvariables_static.animate_pause = true;

                fe_data.modalresultmeshdata.pause_animation();

                // Set the status label
                label_status.Text = "Paused";

            }
            else
            {
                // Currently paused/stopped, so play
                gvariables_static.animate_play = true;
                gvariables_static.animate_pause = false;

                // Set the status label
                label_status.Text = "Playing";

            }

            if (gvariables_static.animate_stop == true)
            {
                // Retart the animation from the beginning
                gvariables_static.animate_stop = false;

                // Restart the animation
                fe_data.modalresultmeshdata.start_animation();

            }


        }

        private void button_stop_Click(object sender, EventArgs e)
        {
            // Stop the animation
            gvariables_static.animate_play = false;
            gvariables_static.animate_pause = false;
            gvariables_static.animate_stop = true;

            // Reset the animation to the beginning
            fe_data.modalresultmeshdata.stop_animation();

            label_status.Text = "Stopped";

        }

        private void comboBox_modedata_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Call to main form
            if (this.Owner is main_frm mainForm)
            {
                mainForm.CallFrom_modaloption_frm();
            }

            // Update the mode shape
            this.fe_data.modalresultmeshdata.updateSelectedMode(comboBox_modedata.SelectedIndex);

        }


        //__________________________________________________________________________________________________________


    }
}
