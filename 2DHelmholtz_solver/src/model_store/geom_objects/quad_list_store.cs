using _2DHelmholtz_solver.opentk_control.opentk_buffer;
using _2DHelmholtz_solver.opentk_control.shader_compiler;
using _2DHelmholtz_solver.src.opentk_control.opentk_buffer;
// OpenTK library
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2DHelmholtz_solver.src.model_store.geom_objects
{
    public class quad_store
    {
        public int quad_id {  get; set; }
        public tri_store tri123 { get; set; }
        public tri_store tri341 { get; set; }

    }


    public class quad_list_store
    {
        public Dictionary<int, quad_store> quadMap { get; } = new Dictionary<int, quad_store>();
        public int quad_count = 0;
        private bool is_DynamicDraw = false;

        private graphicBuffers quad_buffer;
        private Shader quad_shader;

        private readonly point_list_store _allPts;
        private readonly line_list_store _allLines;


        public quad_list_store(point_list_store allPts, line_list_store allLines)
        {
            // (Re)Initialize the data
            quadMap = new Dictionary<int, quad_store>();
            quad_count = 0;

            // Shader
            quad_shader = new Shader(ShaderLibrary.get_vertex_shader(ShaderLibrary.ShaderType.MeshShader),
                ShaderLibrary.get_fragment_shader(ShaderLibrary.ShaderType.MeshShader));

            // store the all points data
            _allPts = allPts;
            _allLines = allLines;

        }


        public void add_quad(int quad_id, int edge1_id, int edge2_id, int edge3_id, 
            int edge4_id, int edge5_id, int edge6_id)
        {
            // Create the Half triangle Tri123 
            tri_store temp_tri123 = new tri_store
            {
                tri_id = quad_id,
                edge1_id = edge1_id,
                edge2_id = edge2_id,
                edge3_id = edge3_id,
            };

            // Create the Half triangle Tri341 
            tri_store temp_tri341 = new tri_store
            {
                tri_id = quad_id,
                edge1_id = edge4_id,
                edge2_id = edge5_id,
                edge3_id = edge6_id,
            };

            // Add the Quadrilateral to the list
            quad_store temp_quad = new quad_store
            {
                quad_id = quad_id,
                tri123 = temp_tri123,
                tri341 = temp_tri341
            };


            quadMap[quad_id] = temp_quad;
            quad_count++;

        }


        public void set_buffer()
        {

            // Set the buffer for index
            int quad_indices_count = 6 * quad_count; // 6 indices to form a quadrilateral ( 3 + 3 triangles)
            int[] quad_vertex_indices = new int[quad_indices_count];

            int quad_i_index = 0;

            // Set the quad index buffers
            foreach (var quad in quadMap)
            {
                get_quad_index_buffer(quad_vertex_indices, quad_i_index);
            }

            // Define the vertex layout
            var quadLayout = new VertexBufferLayout();
            quadLayout.AddFloat(2);  // Node center
            quadLayout.AddFloat(1);  // Is Dynamic data
            quadLayout.AddFloat(1);  // Normalized deflection scale

            // Define the vertex buffer size for a point 6 * ( 2 position, 2 dynamic data)
            int quad_vertex_count = 6 * 4 * quad_count;
            int quad_vertex_size = quad_vertex_count * sizeof(float);

            // Create the quadrilateral dynamic buffers
            is_DynamicDraw = true;
            quad_buffer = new graphicBuffers(null, quad_vertex_size, quad_vertex_indices,
                quad_indices_count, quadLayout, is_DynamicDraw);

            // Update the buffer
            update_buffer();

        }

        public void update_buffer()
        {
            // Define the vertex buffer size for a point 6 * ( 2 position, 2 dynamic data)
            int quad_vertex_count = 6 * 4 * quad_count;
            float[] quad_vertices = new float[quad_vertex_count];

            int quad_v_index = 0;

            // Set the quad vertex buffers
            foreach (var quad in quadMap)
            {
                // Add vertex buffers
                get_quad_vertex_buffer(quad.Value, quad_vertices, quad_v_index);
            }

            int quad_vertex_size = quad_vertex_count * sizeof(float); // Size of the quadrilateral vertex buffer

            // Update the buffer
            quad_buffer.UpdateDynamicVertexBuffer(quad_vertices, quad_vertex_size);

        }

        public void clear_quadrilaterals()
        {
            // Clear the data
            quadMap.Clear();
            quad_count = 0;

        }


        public void set_quad_color()
        {

        }

        public void paint_static_quadrilaterals()
        {
            // Paint all the static quadrilaterals
            quad_shader.Bind();
            quad_buffer.Bind();
            is_DynamicDraw = false;

            GL.DrawElements(PrimitiveType.Triangles, 6 * quad_count, DrawElementsType.UnsignedInt, 0);
            quad_buffer.UnBind();
            quad_shader.UnBind();

        }


        public void paint_dynamic_quadrilaterals()
        {
            // Paint all the dynamic quadrilaterals
            quad_shader.Bind();
            quad_buffer.Bind();

            // Update the point buffer data for dynamic drawing
            is_DynamicDraw = true;
            update_buffer();

            GL.DrawElements(PrimitiveType.Triangles, 6 * quad_count, DrawElementsType.UnsignedInt, 0);
            quad_buffer.UnBind();
            quad_shader.UnBind();

        }


        private void get_quad_vertex_buffer(quad_store quad, float[] quad_vertices, int quad_v_index)
        {
            // Get the node buffer for the shader
            // Point 1
            // Point location
            quad_vertices[quad_v_index + 0] = _allPts.pointMap[_allLines.lineMap[quad.tri123.edge1_id].start_pt_id].pt_coord.X;
            quad_vertices[quad_v_index + 1] = _allPts.pointMap[_allLines.lineMap[quad.tri123.edge1_id].start_pt_id].pt_coord.Y;

            quad_vertices[quad_v_index + 2] = is_DynamicDraw ? 1.0f : 0.0f;

            quad_vertices[quad_v_index + 3] = (float)_allPts.pointMap[_allLines.lineMap[quad.tri123.edge1_id].start_pt_id].normalized_defl_scale;

            // Iterate
            quad_v_index = quad_v_index + 4;


            // Point 2
            // Point location
            quad_vertices[quad_v_index + 0] = _allPts.pointMap[_allLines.lineMap[quad.tri123.edge2_id].start_pt_id].pt_coord.X;
            quad_vertices[quad_v_index + 1] = _allPts.pointMap[_allLines.lineMap[quad.tri123.edge2_id].start_pt_id].pt_coord.Y;

            quad_vertices[quad_v_index + 2] = is_DynamicDraw ? 1.0f : 0.0f;

            quad_vertices[quad_v_index + 3] = (float)_allPts.pointMap[_allLines.lineMap[quad.tri123.edge2_id].start_pt_id].normalized_defl_scale;

            // Iterate
            quad_v_index = quad_v_index + 4;


            // Point 3
            // Point location
            quad_vertices[quad_v_index + 0] = _allPts.pointMap[_allLines.lineMap[quad.tri341.edge1_id].start_pt_id].pt_coord.X;
            quad_vertices[quad_v_index + 1] = _allPts.pointMap[_allLines.lineMap[quad.tri341.edge1_id].start_pt_id].pt_coord.Y;

            quad_vertices[quad_v_index + 2] = is_DynamicDraw ? 1.0f : 0.0f;

            quad_vertices[quad_v_index + 3] = (float)_allPts.pointMap[_allLines.lineMap[quad.tri341.edge1_id].start_pt_id].normalized_defl_scale;

            // Iterate
            quad_v_index = quad_v_index + 4;


            // Point 4
            // Point location
            quad_vertices[quad_v_index + 0] = _allPts.pointMap[_allLines.lineMap[quad.tri341.edge2_id].start_pt_id].pt_coord.X;
            quad_vertices[quad_v_index + 1] = _allPts.pointMap[_allLines.lineMap[quad.tri341.edge2_id].start_pt_id].pt_coord.Y;

            quad_vertices[quad_v_index + 2] = is_DynamicDraw ? 1.0f : 0.0f;

            quad_vertices[quad_v_index + 3] = (float)_allPts.pointMap[_allLines.lineMap[quad.tri341.edge2_id].start_pt_id].normalized_defl_scale;

            // Iterate
            quad_v_index = quad_v_index + 4;


        }


        private void get_quad_index_buffer(int[] quad_vertex_indices, int quad_i_index)
        {
            // Add the indices
            // Index 0 1 2 
            quad_vertex_indices[quad_i_index + 0] = (int)((quad_i_index / 6.0) * 4.0) + 0;

            quad_vertex_indices[quad_i_index + 1] = (int)((quad_i_index / 6.0) * 4.0) + 1;

            quad_vertex_indices[quad_i_index + 2] = (int)((quad_i_index / 6.0) * 4.0) + 2;

            // Index 2 3 0 
            quad_vertex_indices[quad_i_index + 3] = (int)((quad_i_index / 6.0) * 4.0) + 2;

            quad_vertex_indices[quad_i_index + 4] = (int)((quad_i_index / 6.0) * 4.0) + 3;

            quad_vertex_indices[quad_i_index + 5] = (int)((quad_i_index / 6.0) * 4.0) + 0;

            // Iterate
            quad_i_index = quad_i_index + 6;

        }
















    }
}
