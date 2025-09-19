using _2DHelmholtz_solver.opentk_control.opentk_buffer;
using _2DHelmholtz_solver.opentk_control.shader_compiler;
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
using _2DHelmholtz_solver.global_variables;

namespace _2DHelmholtz_solver.src.model_store.geom_objects
{
    public class line_store
    {
        public int line_id { get; set; }

        public int start_pt_id { get; set; } // Start point id 
        public int end_pt_id { get; set; } // End point id


        public int next_line_id { get; set; } // Next half-edge in the same face
        public int twin_line_id { get; set; } // Opposite half-edge

        public int tri_face_id { get; set; } // Triangle Face to the left of this half-edge
        public int quad_face_id { get; set; } // Triangle Face to the left of this half-edge


        public int color_id { get; set; }

        public Vector3 line_color { get; set; }

    }





    public class line_list_store
    {
        public Dictionary<int, line_store> lineMap { get; } = new Dictionary<int, line_store>();
        public int line_count = 0;
        private bool is_DynamicDraw = false;

        private graphicBuffers line_buffer;
        public Shader line_shader;

        private readonly point_list_store _allPts;

        public line_list_store(point_list_store allPts)
        {
            // (Re)Initialize the data
            lineMap = new Dictionary<int, line_store>();
            line_count = 0;

            // store the all points data
            _allPts = allPts;

        }


        public void add_line(int line_id, int start_pt_id, int end_pt_id, int color_id)
        {
            // Add the Line to the list
            line_store temp_line = new line_store
            {
                line_id = line_id,
                start_pt_id = start_pt_id,
                end_pt_id = end_pt_id,
                next_line_id = -1,
                twin_line_id = -1,
                tri_face_id = -1,
                quad_face_id = -1,
                color_id = color_id,
                line_color = gvariables_static.ColorUtils.MeshGetRandomColor(color_id)
            };

            lineMap[line_id] = temp_line;
            line_count++;

        }

        public void set_shader()
        {

            // Create Shader
            line_shader = new Shader(ShaderLibrary.get_vertex_shader(ShaderLibrary.ShaderType.MeshShader),
                ShaderLibrary.get_fragment_shader(ShaderLibrary.ShaderType.MeshShader));

        }

        public void set_buffer()
        {

            // Set the buffer for index
            int line_indices_count = 2 * line_count; // 2 indices to form a line
            int[] line_vertex_indices = new int[line_indices_count];

            int line_i_index = 0;

            // Set the line index buffers
            foreach (var ln in lineMap)
            {
                get_line_index_buffer(ref line_vertex_indices, ref line_i_index);
            }

            // Define the vertex layout
            VertexBufferLayout lineLayout = new VertexBufferLayout();
            lineLayout.AddFloat(2);  // point center
            lineLayout.AddFloat(3);  // point color
            lineLayout.AddFloat(1);  // Is Dynamic data
            lineLayout.AddFloat(1);  // Normalized deflection scale

            // Define the vertex buffer size for a point 2 * ( 2 position, 3 color, 2 dynamic data)
            int line_vertex_count = 2 * 7 * line_count;
            int line_vertex_size = line_vertex_count * sizeof(float);

            // Create the line dynamic buffers
            line_buffer = new graphicBuffers(null, line_vertex_size, line_vertex_indices,
                line_indices_count, lineLayout, true);

            // Update the buffer
            update_buffer();

        }

        public void update_buffer()
        {
            // Define the vertex buffer size for a point 2 * ( 2 position, 3 color, 2 dynamic data)
            int line_vertex_count = 2 * 7 * line_count;
            float[] line_vertices = new float[line_vertex_count];

            int line_v_index = 0;

            // Set the line vertex buffers
            foreach (var ln in lineMap)
            {
                // Add vertex buffers
                get_line_vertex_buffer(ln.Value, ref line_vertices, ref line_v_index);
            }

            int line_vertex_size = line_vertex_count * sizeof(float); // Size of the line vertex buffer

            // Update the buffer
            line_buffer.UpdateDynamicVertexBuffer(line_vertices, line_vertex_size);

        }


        public void paint_static_lines()
        {
            // Paint all the static lines
            line_shader.Bind();
            line_buffer.Bind();
            is_DynamicDraw = false;

            GL.DrawElements(PrimitiveType.Lines, 2 * line_count, DrawElementsType.UnsignedInt, 0);
            line_buffer.UnBind();
            line_shader.UnBind();

        }


        public void paint_dynamic_lines()
        {
            // Paint all the dynamic lines
            line_shader.Bind();
            line_buffer.Bind();

            // Update the point buffer data for dynamic drawing
            is_DynamicDraw = true;
            update_buffer();

            GL.DrawElements(PrimitiveType.Lines, 2 * line_count, DrawElementsType.UnsignedInt, 0);
            line_buffer.UnBind();
            line_shader.UnBind();

        }


        private void get_line_vertex_buffer(line_store ln, ref float[] line_vertices, ref int line_v_index)
        {
            // Get the node buffer for the shader
            // Start Point
            // Point location
            line_vertices[line_v_index + 0] = _allPts.pointMap[ln.start_pt_id].pt_coord.X;
            line_vertices[line_v_index + 1] = _allPts.pointMap[ln.start_pt_id].pt_coord.Y;

            // Point color
            line_vertices[line_v_index + 2] = ln.line_color.X;
            line_vertices[line_v_index + 3] = ln.line_color.Y;
            line_vertices[line_v_index + 4] = ln.line_color.Z;

            line_vertices[line_v_index + 5] = is_DynamicDraw ? 1.0f : 0.0f;

            line_vertices[line_v_index + 6] = (float)_allPts.pointMap[ln.start_pt_id].normalized_defl_scale;

            // Iterate
            line_v_index = line_v_index + 7;

            // End Point
            // Point location
            line_vertices[line_v_index + 0] = _allPts.pointMap[ln.end_pt_id].pt_coord.X;
            line_vertices[line_v_index + 1] = _allPts.pointMap[ln.end_pt_id].pt_coord.Y;

            // Point color
            line_vertices[line_v_index + 2] = ln.line_color.X;
            line_vertices[line_v_index + 3] = ln.line_color.Y;
            line_vertices[line_v_index + 4] = ln.line_color.Z;

            line_vertices[line_v_index + 5] = is_DynamicDraw ? 1.0f : 0.0f;

            line_vertices[line_v_index + 6] = (float)_allPts.pointMap[ln.end_pt_id].normalized_defl_scale;

            // Iterate
            line_v_index = line_v_index + 7;


        }


        private void get_line_index_buffer(ref int[] line_vertex_indices, ref int line_i_index)
        {
            // Add the indices
            // Index 1
            line_vertex_indices[line_i_index] = line_i_index;

            line_i_index = line_i_index + 1;

            // Index 2
            line_vertex_indices[line_i_index] = line_i_index;

            line_i_index = line_i_index + 1;

        }






    }
}
