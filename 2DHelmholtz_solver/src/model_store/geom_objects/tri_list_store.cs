using _2DHelmholtz_solver.opentk_control.opentk_buffer;
using _2DHelmholtz_solver.opentk_control.shader_compiler;
using _2DHelmholtz_solver.src.opentk_control.opentk_buffer;
// OpenTK library
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2DHelmholtz_solver.src.model_store.geom_objects
{
    public class tri_store
    {
        public int tri_id { get; set; }
        public int edge1_id { get; set; }
        public int edge2_id { get; set; }
        public int edge3_id { get; set; }

    }

    public class tri_list_store
    {
        public Dictionary<int, tri_store> triMap { get; } = new Dictionary<int, tri_store>();
        public int tri_count = 0;
        private bool is_DynamicDraw = false;

        private graphicBuffers tri_buffer;
        public Shader tri_shader;

        private readonly point_list_store _allPts;
        private readonly line_list_store _allLines;


        public tri_list_store(point_list_store allPts, line_list_store allLines)
        {
            // (Re)Initialize the data
            triMap = new Dictionary<int, tri_store>();
            tri_count = 0;

            // store the all points data
            _allPts = allPts;
            _allLines = allLines;

        }


        public void add_tri(int tri_id, int edge1_id, int edge2_id, int edge3_id)
        {
            // Add the Tri to the list
            tri_store temp_tri = new tri_store
            {
                tri_id = tri_id,
                edge1_id = edge1_id,
                edge2_id = edge2_id,
                edge3_id = edge3_id,
            };

            triMap[tri_id] = temp_tri;
            tri_count++;

        }


        public void set_buffer()
        {

            // Create Shader
            tri_shader = new Shader(ShaderLibrary.get_vertex_shader(ShaderLibrary.ShaderType.MeshShader),
                ShaderLibrary.get_fragment_shader(ShaderLibrary.ShaderType.MeshShader));

            // Set the buffer for index
            int tri_indices_count = 3 * tri_count; // 3 indices to form a triangle
            int[] tri_vertex_indices = new int[tri_indices_count];

            int tri_i_index = 0;

            // Set the tri index buffers
            foreach (var tri in triMap)
            {
                get_tri_index_buffer(tri_vertex_indices, tri_i_index);
            }

            // Define the vertex layout
            var triLayout = new VertexBufferLayout();
            triLayout.AddFloat(2);  // Node center
            triLayout.AddFloat(1);  // Is Dynamic data
            triLayout.AddFloat(1);  // Normalized deflection scale

            // Define the vertex buffer size for a point 3 * ( 2 position, 2 dynamic data)
            int tri_vertex_count = 3 * 4 * tri_count;
            int tri_vertex_size = tri_vertex_count * sizeof(float);

            // Create the triangle dynamic buffers
            is_DynamicDraw = true;
            tri_buffer = new graphicBuffers(null, tri_vertex_size, tri_vertex_indices,
                tri_indices_count, triLayout, is_DynamicDraw);

            // Update the buffer
            update_buffer();

        }

        public void update_buffer()
        {
            // Define the vertex buffer size for a point 3 * ( 2 position, 2 dynamic data)
            int tri_vertex_count = 3 * 4 * tri_count;
            float[] tri_vertices = new float[tri_vertex_count];

            int tri_v_index = 0;

            // Set the tri vertex buffers
            foreach (var tri in triMap)
            {
                // Add vertex buffers
                get_tri_vertex_buffer(tri.Value, tri_vertices, tri_v_index);
            }

            int tri_vertex_size = tri_vertex_count * sizeof(float); // Size of the triangle vertex buffer

            // Update the buffer
           tri_buffer.UpdateDynamicVertexBuffer(tri_vertices, tri_vertex_size);

        }

        public void clear_triangles()
        {
            // Clear the data
            triMap.Clear();
            tri_count = 0;

        }

        public void set_tri_color()
        {

        }


        public void paint_static_triangles()
        {
            // Paint all the static triangles
           tri_shader.Bind();
           tri_buffer.Bind();
            is_DynamicDraw = false;

            GL.DrawElements(PrimitiveType.Triangles, 3 * tri_count, DrawElementsType.UnsignedInt, 0);
            tri_buffer.UnBind();
            tri_shader.UnBind();

        }


        public void paint_dynamic_triangles()
        {
            // Paint all the dynamic triangles
            tri_shader.Bind();
            tri_buffer.Bind();

            // Update the point buffer data for dynamic drawing
            is_DynamicDraw = true;
            update_buffer();

            GL.DrawElements(PrimitiveType.Triangles, 3 * tri_count, DrawElementsType.UnsignedInt, 0);
            tri_buffer.UnBind();
            tri_shader.UnBind();

        }


        private void get_tri_vertex_buffer(tri_store tri, float[] tri_vertices, int tri_v_index)
        {
            // Get the node buffer for the shader
            // Point 1
            // Point location
            tri_vertices[tri_v_index + 0] = _allPts.pointMap[_allLines.lineMap[tri.edge1_id].start_pt_id].pt_coord.X;
            tri_vertices[tri_v_index + 1] = _allPts.pointMap[_allLines.lineMap[tri.edge1_id].start_pt_id].pt_coord.Y;

            tri_vertices[tri_v_index + 2] = is_DynamicDraw ? 1.0f : 0.0f;

            tri_vertices[tri_v_index + 3] = (float)_allPts.pointMap[_allLines.lineMap[tri.edge1_id].start_pt_id].normalized_defl_scale;

            // Iterate
            tri_v_index = tri_v_index + 4;


            // Point 2
            // Point location
            tri_vertices[tri_v_index + 0] = _allPts.pointMap[_allLines.lineMap[tri.edge2_id].start_pt_id].pt_coord.X;
            tri_vertices[tri_v_index + 1] = _allPts.pointMap[_allLines.lineMap[tri.edge2_id].start_pt_id].pt_coord.Y;

            tri_vertices[tri_v_index + 2] = is_DynamicDraw ? 1.0f : 0.0f;

            tri_vertices[tri_v_index + 3] = (float)_allPts.pointMap[_allLines.lineMap[tri.edge2_id].start_pt_id].normalized_defl_scale;

            // Iterate
            tri_v_index = tri_v_index + 4;


            // Point 3
            // Point location
            tri_vertices[tri_v_index + 0] = _allPts.pointMap[_allLines.lineMap[tri.edge3_id].start_pt_id].pt_coord.X;
            tri_vertices[tri_v_index + 1] = _allPts.pointMap[_allLines.lineMap[tri.edge3_id].start_pt_id].pt_coord.Y;

            tri_vertices[tri_v_index + 2] = is_DynamicDraw ? 1.0f : 0.0f;

            tri_vertices[tri_v_index + 3] = (float)_allPts.pointMap[_allLines.lineMap[tri.edge3_id].start_pt_id].normalized_defl_scale;

            // Iterate
            tri_v_index = tri_v_index + 4;



        }


        private void get_tri_index_buffer(int[] tri_vertex_indices, int tri_i_index)
        {
            // Add the indices
            // Index 1
            tri_vertex_indices[tri_i_index] = tri_i_index;

            tri_i_index = tri_i_index + 1;

            // Index 2
            tri_vertex_indices[tri_i_index] = tri_i_index;

            tri_i_index = tri_i_index + 1;

            // Index 3
            tri_vertex_indices[tri_i_index] = tri_i_index;

            tri_i_index = tri_i_index + 1;

        }



    }
}
