using _2DHelmholtz_solver.src.model_store.geom_objects;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2DHelmholtz_solver.src.opentk_control.opentk_bgdraw
{
    public class drawing_events
    {
        private readonly meshdata_store meshdata;

        public Vector2 click_pt = new Vector2(0);
        public Vector2 curr_pt = new Vector2(0);
        public Vector2 prev_translation = new Vector2(0);
        public Vector2 total_translation = new Vector2(0);

        public bool is_pan = false;
        public bool is_rightbutton = false;
        public bool is_select = false;
        private double zoom_val = 1.0;

        private double window_width = 0.0;
        private double window_height = 0.0;

        public drawing_events(meshdata_store meshdata)
        {
            // Set the mesh data
            this.meshdata = meshdata;
        }

        public void mouse_location(float e_X, float e_Y)
        {
            Vector2 loc = new Vector2(e_X, e_Y);

            if (is_pan == true)
            {
                // Pan operation in progress
                Vector2 delta_d = click_pt - loc;
                // pan
                Vector2 current_translataion = delta_d / (float)(Math.Max(window_width, window_height) * 0.5f);

                pan_operation(current_translataion);
            }

            // Select operation in progress
            if (is_select == true)
            {
                select_operation(click_pt, loc);
            }

        }


        public void update_drawing_area_size(int width, int height, double bound_x, double bound_y)
        {
            // Update the drawing area size

        }


        public void zoom_operation(double e_Delta, int e_X, int e_Y)
        {
            // Perform intelli zoom operation

        }


        public void pan_operation_start(float et_X, float et_Y)
        {
            // Pan operation start
            is_pan = true;
            // Note the click point when the pan operation start
            click_pt = new Vector2(et_X, et_Y);

        }


        public void pan_operation_end()
        {
            // End the pan operation saving translate transformation
            // Pan operation complete
            prev_translation = total_translation;
            is_pan = false;
        }


        public void zoom_to_fit()
        {
            // Perform zoom to fit operation

        }



        // Private function
        private void pan_operation(Vector2 current_translation)
        {

        }


        private void select_operation(Vector2 click_loc, Vector2 current_loc)
        {


        }

    }
}



