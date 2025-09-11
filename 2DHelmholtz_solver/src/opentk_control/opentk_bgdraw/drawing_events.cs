using _2DHelmholtz_solver.src.model_store.geom_objects;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _2DHelmholtz_solver.src.opentk_control.opentk_bgdraw
{
    public class drawing_events
    {
        private readonly meshdata_store meshdata;

        private Vector2 click_pt = new Vector2(0);
        // private Vector2 curr_pt = new Vector2(0);
        private Vector2 prev_translation = new Vector2(0);
        private Vector2 total_translation = new Vector2(0);


        private bool isCtrlDown = false;
        private bool isShiftDown = false;


        private bool is_pan = false;
        private bool is_rightbutton = false;
        private bool is_select = false;
        public double zoom_val = 1.0;

        public int window_width = 0;
        public int window_height = 0;


        public Matrix4 modelMatrix { get; private set; } = Matrix4.Zero; // model matrix
        public Matrix4 viewMatrix { get; private set; } = Matrix4.Identity; // view matrix
        public Matrix4 projectionMatrix { get; private set; } = Matrix4.Identity; // projection matrix

        public float geom_transparency = 1.0f;


        public drawing_events(meshdata_store meshdata)
        {
            // Set the mesh data
            this.meshdata = meshdata;


            // Set the default projection matrix
            // Define the orthographic projection parameters
            float left = -1.0f;
            float right = 1.0f;
            float bottom = -1.0f;
            float top = 1.0f;
            float nearPlane = 100.0f;
            float farPlane = -100.0f;

            // Create the orthographic projection matrix
            Matrix4 projectionMatrix = Matrix4.CreateOrthographicOffCenter(left, right, bottom, top, nearPlane, farPlane);

            // Assign it to your class or struct
            this.projectionMatrix = projectionMatrix;

            this.meshdata.update_openTK_uniforms(true, true, true);
        }


        #region "Handle Mouse Events"
        public void handleMouseLeftButtonClick(bool isDown, float e_X, float e_Y)
        {
            if(isDown == true)
            {
                // Left mouse button press
                if(isCtrlDown == true)
                {
                    // Rotate operation
                }
                
                if(isShiftDown == true)
                {
                    // Select operation starts (Left drag)
                    select_operation_start(new Vector2(e_X, e_Y), false);
                }
            }
            else
            {
                // Left mouse button release

                select_operation_end(new Vector2(e_X, e_Y));

            }

        }

        public void handleMouseRightButtonClick(bool isDown, float e_X, float e_Y)
        {
            if (isDown == true)
            {
             
                // Right mouse button press
                if (isCtrlDown == true)
                {
                    // Pan operation
                    pan_operation_start(new Vector2(e_X, e_Y));

                }

                if (isShiftDown == true)
                {
                    // Select operation starts (Right drag)
                    select_operation_start(new Vector2(e_X, e_Y), true);
                }

            }
            else
            {
                // Right mouse button release
                pan_operation_end();
                select_operation_end(new Vector2(e_X, e_Y));

            }

        }


        public void handleMouseMove(float e_X, float e_Y)
        {
            if(isCtrlDown == true || isShiftDown == true)
            {
                // Perform the mouse move operation
                Vector2 loc = new Vector2(e_X, e_Y);
                mouse_location(loc);
            }

        }

        public void handleMouseScroll(double e_Delta, float e_X, float e_Y)
        {
            if (isCtrlDown == true)
            {
                // Perform zoom operation
                zoom_operation(e_Delta, e_X, e_Y);  

            }
        }

        public void handleKeyboardAction(bool isDown, int key)
        {
            if (isDown == true)
            {
                // Key pressed
                Keys pressedKey = (Keys)key;


                // Modifier keys
                isShiftDown = (Control.ModifierKeys & Keys.Shift) == Keys.Shift;
                isCtrlDown = (Control.ModifierKeys & Keys.Control) == Keys.Control;

                if (isCtrlDown && pressedKey == Keys.F)
                {
                    // Perform zoom to fit
                    zoom_to_fit();

                }
            }
            else
            {
                // key released
                isCtrlDown = false;
                isShiftDown = false;

            }

        }

        #endregion


        private void mouse_location(Vector2 loc)
        {

            if (is_pan == true)
            {
                // Pan operation in progress
                Vector2 delta_d = click_pt - loc;
                // pan
                Vector2 current_translation = delta_d / (float)(Math.Max(window_width, window_height) * 0.5f);

                pan_operation(current_translation);
            }

            // Select operation in progress
            if (is_select == true)
            {
                select_operation(click_pt, loc);
            }

        }


        public void update_drawing_area_size(int window_width, int window_height)
        {
            // Update the drawing area size
            this.window_width = window_width;
            this.window_height = window_height;

            // 1. Determine max dimension
            int maxDim = Math.Max(window_width, window_height);

            // 2. Normalize screen dimensions
            double normalizedScreenWidth = 1.8 * ((double)window_width / maxDim);
            double normalizedScreenHeight = 1.8 * ((double)window_height / maxDim);


            // 3. Compute scale factor
            double geom_scale = Math.Min(normalizedScreenWidth / meshdata.geom_bounds.X,
                normalizedScreenHeight / meshdata.geom_bounds.Y);


            // 4. Compute translation to center geometry
            Vector3 geomTranslation = new Vector3(
                -1.0f * (float)((meshdata.max_bounds.X + meshdata.min_bounds.X) * 0.5 * geom_scale),
                -1.0f * (float)((meshdata.max_bounds.Y + meshdata.min_bounds.Y) * 0.5 * geom_scale),
                0.0f
            );


            // 5. Build model matrix
            Matrix4 translationMatrix = Matrix4.CreateTranslation(geomTranslation);
            Matrix4 scaleMatrix = Matrix4.CreateScale((float)geom_scale);

            // 6. Combine into model matrix
            this.modelMatrix = translationMatrix * scaleMatrix;

            this.meshdata.update_openTK_uniforms(true, false, false);
        }


        private void zoom_operation(double e_Delta, float e_X, float e_Y)
        {
            // Perform intelli zoom operation
            // Screen point before zoom
            Vector2 screen_pt_b4_scale = intellizoom_normalized_screen_pt(e_X, e_Y);

            // Zoom operation
            if ((e_Delta) > 0)
            {
                // Scroll Up
                if (zoom_val < 1000)
                {
                    zoom_val = zoom_val + 0.1f;
                }
            }
            else if ((e_Delta) < 0)
            {
                // Scroll Down
                if (zoom_val > 0.101)
                {
                    zoom_val = zoom_val - 0.1f;
                }
            }

            // Transformed Hypothetical Screen point after zoom
            Vector2 screen_pt_a4_scale = intellizoom_normalized_screen_pt(e_X, e_Y);
            Vector2 g_tranl = -0.5f * (float)zoom_val * (screen_pt_b4_scale - screen_pt_a4_scale);

            // Set the zoom




            // Perform Translation for Intelli Zoom
            pan_operation(g_tranl);
            pan_operation_end();

        }


        private void pan_operation_start(Vector2 loc)
        {
            // Pan operation start
            is_pan = true;
            // Note the click point when the pan operation start
            click_pt = loc;

        }


        private void pan_operation_end()
        {
            // End the pan operation saving translate transformation
            // Pan operation complete
            prev_translation = total_translation;
            is_pan = false;
        }


        private void select_operation_start(Vector2 loc, bool is_rightbutton)
        {
            // Select operation start
            is_select = true;
            this.is_rightbutton = is_rightbutton;

            // Note the click point when the pan operation start
            click_pt = loc;

        }


        private void select_operation_end(Vector2 current_loc)
        {
            // Location when the selection rectangle ends


            // End the pan operation saving translate transformation
            // Pan operation complete
            bool is_paint_selctionrectangle = false;



            is_select = false;
        }




        public void zoom_to_fit()
        {
            // Perform zoom to fit operation

        }


        public Vector2 intellizoom_normalized_screen_pt(float e_X, float e_Y)
        {
            // Function returns normalized screen point for zoom operation
            Vector2 loc = new Vector2(e_X, e_Y);

            Vector2 mid_pt = new Vector2((float)window_width, (float)window_height) * 0.5f;
            int min_size = (int)Math.Min(window_width, window_height);

            Vector2 mouse_pt = (-1.0f * (loc - mid_pt)) / (min_size * 0.5f);

            return (mouse_pt - (2.0f * prev_translation)) / (float)(zoom_val);

        }


        // Private function
        private void pan_operation(Vector2 current_translation)
        {
            // Pan operation in progress
            total_translation = (prev_translation + current_translation);

            //__________________________________________________________________________________________
            // Update the openGL Uniform matrix (View Matrix)
            Matrix4 panTranslation = Matrix4.Identity;

            // Apply translation
            panTranslation.M41 = -1.0f * total_translation.X; // X translation (negated)
            panTranslation.M42 = total_translation.Y;         // Y translation


            Matrix4 scalingMatrix = Matrix4.Identity * (float)zoom_val;

            this.viewMatrix = Matrix4.Transpose(panTranslation) * scalingMatrix;

            this.meshdata.update_openTK_uniforms(false, true, false);
        }


        private void select_operation(Vector2 click_loc, Vector2 current_loc)
        {


        }

    }
}



