using _2DHelmholtz_solver.global_variables;
using _2DHelmholtz_solver.opentk_control.opentk_buffer;
using _2DHelmholtz_solver.opentk_control.shader_compiler;
using _2DHelmholtz_solver.src.model_store.fe_objects;
using _2DHelmholtz_solver.src.opentk_control.opentk_buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// OpenTK library
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using _2DHelmholtz_solver.src.opentk_control.opentk_bgdraw;


namespace _2DHelmholtz_solver.src.model_store.geom_objects
{
    public class point_store
    {
        public int point_id { get; set; }
        public double x_coord { get; set; }
        public double y_coord { get; set; }
        public double z_coord { get; set; } 
        public int point_index { get; set; }
        public double normalized_defl_scale { get; set; }
        public int color_id { get; set; }
        public Vector3 point_color { get; set; }

        public Vector3 pt_coord
        {
            get { return new Vector3((float)x_coord, (float)y_coord, (float)z_coord); }
        }


    }


    public class point_list_store
    {
        public Dictionary<int, point_store> pointMap { get; }  = new Dictionary<int, point_store>();
        public int point_count = 0;
        private bool is_DynamicDraw = false;    

        private graphicBuffers point_buffer;
        public Shader point_shader;

        public point_list_store()
        {
            // (Re)Initialize the data
            pointMap = new Dictionary<int, point_store>();
            point_count = 0;

        }

        public void add_point(int point_id, double x_coord, double y_coord, double z_coord, int color_id)
        {
            // Add the Point to the list
            point_store temp_point = new point_store
            {
                point_id = point_id,
                x_coord = x_coord,
                y_coord = y_coord,
                z_coord = z_coord,
                point_index = point_count,
                normalized_defl_scale = 0.0,
                color_id = color_id,
                point_color = gvariables_static.ColorUtils.MeshGetRandomColor(color_id)

            };

            pointMap[point_id] = temp_point;
            point_count++;

        }

        public void update_point(int point_id, double x_coord, double y_coord, double z_coord, double normalized_defl_scale)
        {
            // Update the point co-ordinates
            pointMap[point_id].x_coord = x_coord;
            pointMap[point_id].y_coord = y_coord;
            pointMap[point_id].z_coord = z_coord;
            pointMap[point_id].normalized_defl_scale = normalized_defl_scale;

        }

        public void set_shader()
        {

            // Create Shader
            point_shader = new Shader(ShaderLibrary.get_vertex_shader(ShaderLibrary.ShaderType.MeshShader),
                ShaderLibrary.get_fragment_shader(ShaderLibrary.ShaderType.MeshShader));

        }


        public void set_buffer()
        {

            // Set the buffer for index
            int point_indices_count = 1 * point_count; // 1 index per point
            int[] point_vertex_indices = new int[point_indices_count];

            int point_i_index = 0;

            // Set the point index buffers
            foreach (var pt in pointMap)
            {
                get_point_index_buffer(ref point_vertex_indices, ref point_i_index);
            }

            // Define the vertex layout
            VertexBufferLayout pointLayout = new VertexBufferLayout();
            pointLayout.AddFloat(2);  // Point center
            pointLayout.AddFloat(3);  // Point color
            pointLayout.AddFloat(1);  // Is Dynamic data
            pointLayout.AddFloat(1);  // Normalized deflection scale

            // Define the vertex buffer size for a point ( 2 position, 3 color, 2 dynamic data)
            int point_vertex_count = 7 * point_count;
            int point_vertex_size = point_vertex_count * sizeof(float);

            // Create the point dynamic buffers
            point_buffer = new graphicBuffers(null, point_vertex_size, point_vertex_indices,
                point_indices_count, pointLayout, true);

            // Update the buffer
            update_buffer();

        }

        public void update_buffer()
        {
            // Define the vertex buffer size for a point ( 2 position, 3 color, 2 dynamic data)
            int point_vertex_count = 7 * point_count;
            float[] point_vertices = new float[point_vertex_count];

            int point_v_index = 0;

            // Set the point vertex buffers
            foreach (var pt in pointMap)
            {
                // Add vertex buffers
                get_point_vertex_buffer(pt.Value, ref point_vertices, ref point_v_index);
            }

            int point_vertex_size = point_vertex_count * sizeof(float); // Size of the point vertex buffer

            // Update the buffer
            point_buffer.UpdateDynamicVertexBuffer(point_vertices, point_vertex_size);

        }

        public void clear_points()
        {
            // Clear the data
            pointMap.Clear();
            point_count = 0;

        }


        public void paint_static_points()
        {
            // Paint all the static points
            point_shader.Bind();
            point_buffer.Bind();
            is_DynamicDraw = false;

            GL.DrawElements(PrimitiveType.Points, point_count, DrawElementsType.UnsignedInt, 0);
            point_buffer.UnBind();
            point_shader.UnBind();

        }


        public void paint_dynamic_points()
        {
            // Paint all the dynamic points
            point_shader.Bind();
            point_buffer.Bind();

            // Update the point buffer data for dynamic drawing
            is_DynamicDraw = true;
            update_buffer();

            GL.DrawElements(PrimitiveType.Points, point_count, DrawElementsType.UnsignedInt, 0);
            point_buffer.UnBind();
            point_shader.UnBind();

        }



        public List<int> is_point_selected(Vector2 corner_pt1, Vector2 corner_pt2, drawing_events graphic_events_control)
        {
            // Selected point list index;
            List<int> selected_point_index = new List<int>();

            // Loop through all point in map
            foreach (var pt_m in pointMap)
            {
                point_store pt = pt_m.Value;

                //______________________________
                Vector4 node_pt = graphic_events_control.projectionMatrix * graphic_events_control.viewMatrix
                    * graphic_events_control.modelMatrix * new Vector4(pt.pt_coord.X, pt.pt_coord.Y, pt.pt_coord.Z, 1.0f);


                // Check whether the point inside a rectangle
                if (gvariables_static.isPointSelected(corner_pt1, corner_pt2, new Vector2(node_pt.X, node_pt.Y)) == true)
                {
                    selected_point_index.Add(pt_m.Key);

                }

            }
            return selected_point_index;

        }



        private void get_point_vertex_buffer(point_store pt, ref float[] point_vertices, ref int point_v_index)
        {
            // Get the node buffer for the shader
            // Point location
            point_vertices[point_v_index + 0] = pt.pt_coord.X;
            point_vertices[point_v_index + 1] = pt.pt_coord.Y;

            // Point color
            point_vertices[point_v_index + 2] = pt.point_color.X;
            point_vertices[point_v_index + 3] = pt.point_color.Y;
            point_vertices[point_v_index + 4] = pt.point_color.Y;

            point_vertices[point_v_index + 5] = is_DynamicDraw ? 1.0f : 0.0f;

            point_vertices[point_v_index + 6] = (float)pt.normalized_defl_scale;

            // Iterate
            point_v_index = point_v_index + 7;

        }


        private void get_point_index_buffer(ref int[] point_vertex_indices, ref int point_i_index)
        {
            // Add the indices
            point_vertex_indices[point_i_index] = point_i_index;

            point_i_index = point_i_index + 1;

        }



    }
}
