using _2DHelmholtz_solver.global_variables;
using _2DHelmholtz_solver.opentk_control.opentk_buffer;
using _2DHelmholtz_solver.opentk_control.shader_compiler;
using _2DHelmholtz_solver.src.opentk_control.opentk_bgdraw;
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

        public int color_id { get; set; }   

        public Vector3 quad_color { get; set; } 
    }


    public class quad_list_store
    {
        public Dictionary<int, quad_store> quadMap { get; } = new Dictionary<int, quad_store>();
        public int quad_count = 0;
        private bool is_DynamicDraw = false;
        public bool is_ShrinkTriangle = false;

        private graphicBuffers quad_buffer;
        public Shader quad_shader;

        private readonly point_list_store _allPts;
        private readonly line_list_store _allLines;


        public quad_list_store(point_list_store allPts, line_list_store allLines)
        {
            // (Re)Initialize the data
            quadMap = new Dictionary<int, quad_store>();
            quad_count = 0;

            // store the all points data
            _allPts = allPts;
            _allLines = allLines;

        }


        public void add_quad(int quad_id, int edge1_id, int edge2_id, int edge3_id, 
            int edge4_id, int edge5_id, int edge6_id, int color_id)
        {
            // Create the Half triangle Tri123 
            tri_store temp_tri123 = new tri_store
            {
                tri_id = quad_id,
                edge1_id = edge1_id,
                edge2_id = edge2_id,
                edge3_id = edge3_id,
                color_id = color_id
            };

            // Create the Half triangle Tri341 
            tri_store temp_tri341 = new tri_store
            {
                tri_id = quad_id,
                edge1_id = edge4_id,
                edge2_id = edge5_id,
                edge3_id = edge6_id,
                color_id = color_id
            };

            // Add the Quadrilateral to the list
            quad_store temp_quad = new quad_store
            {
                quad_id = quad_id,
                tri123 = temp_tri123,
                tri341 = temp_tri341,
                color_id = color_id,
                quad_color = gvariables_static.ColorUtils.MeshGetRandomColor(color_id)
            };


            quadMap[quad_id] = temp_quad;
            quad_count++;

        }

        public void set_shader()
        {

            // Create Shader
            quad_shader = new Shader(ShaderLibrary.get_vertex_shader(ShaderLibrary.ShaderType.MeshShader),
                ShaderLibrary.get_fragment_shader(ShaderLibrary.ShaderType.MeshShader));

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
                get_quad_index_buffer(ref quad_vertex_indices, ref quad_i_index);
            }

            // Define the vertex layout
            VertexBufferLayout quadLayout = new VertexBufferLayout();
            quadLayout.AddFloat(2);  // point center
            quadLayout.AddFloat(3);  // point color
            quadLayout.AddFloat(1);  // Is Dynamic data
            quadLayout.AddFloat(1);  // Normalized deflection scale

            // Define the vertex buffer size for a point 6 * ( 2 position, 3 color, 2 dynamic data)
            int quad_vertex_count = 6 * 7 * quad_count;
            int quad_vertex_size = quad_vertex_count * sizeof(float);

            // Create the quadrilateral dynamic buffers
            quad_buffer = new graphicBuffers(null, quad_vertex_size, quad_vertex_indices,
                quad_indices_count, quadLayout, true);

            // Update the buffer
            update_buffer();

        }

        public void update_buffer()
        {
            // Define the vertex buffer size for a point 6 * ( 2 position, 3 color, 2 dynamic data)
            int quad_vertex_count = 6 * 7 * quad_count;
            float[] quad_vertices = new float[quad_vertex_count];

            int quad_v_index = 0;

            // Set the quad vertex buffers
            foreach (var quad in quadMap)
            {
                // Add vertex buffers
                get_quad_vertex_buffer(quad.Value, ref quad_vertices, ref quad_v_index);
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


        public List<int> is_quad_selected(Vector2 corner_pt1, Vector2 corner_pt2, drawing_events graphic_events_control)
        {
            // Selected quadrilateral list index;
            List<int> selected_quad_index = new List<int>();

            // Loop through all quadrilateral in map
            foreach (var quad_m in quadMap)
            {
                quad_store quad = quad_m.Value;

                // End points
                Vector3 node_pt1 = _allPts.pointMap[_allLines.lineMap[quad.tri123.edge1_id].start_pt_id].pt_coord;
                Vector3 node_pt2 = _allPts.pointMap[_allLines.lineMap[quad.tri123.edge2_id].start_pt_id].pt_coord;
                Vector3 node_pt3 = _allPts.pointMap[_allLines.lineMap[quad.tri341.edge1_id].start_pt_id].pt_coord;
                Vector3 node_pt4 = _allPts.pointMap[_allLines.lineMap[quad.tri341.edge2_id].start_pt_id].pt_coord;

                // Mid points
                Vector3 md_pt_12 = gvariables_static.linear_interpolation3d(node_pt1, node_pt2, 0.50);
                Vector3 md_pt_23 = gvariables_static.linear_interpolation3d(node_pt2, node_pt3, 0.50);
                Vector3 md_pt_34 = gvariables_static.linear_interpolation3d(node_pt3, node_pt4, 0.50);
                Vector3 md_pt_41 = gvariables_static.linear_interpolation3d(node_pt4, node_pt1, 0.50);
                Vector3 quad_midpt = new Vector3((node_pt1.X + node_pt2.X + node_pt3.X + node_pt4.X) * 0.25f,
                    (node_pt1.Y + node_pt2.Y + node_pt3.Y + node_pt4.Y) * 0.25f,
                    (node_pt1.Z + node_pt2.Z + node_pt3.Z + node_pt4.Z) * 0.25f);


                //______________________________
                Vector4 node_pt1_fp = graphic_events_control.projectionMatrix * graphic_events_control.viewMatrix
                    * graphic_events_control.modelMatrix * new Vector4(node_pt1.X, node_pt1.Y, node_pt1.Z, 1.0f);
                Vector4 node_pt2_fp = graphic_events_control.projectionMatrix * graphic_events_control.viewMatrix
                    * graphic_events_control.modelMatrix * new Vector4(node_pt2.X, node_pt2.Y, node_pt2.Z, 1.0f);
                Vector4 node_pt3_fp = graphic_events_control.projectionMatrix * graphic_events_control.viewMatrix
                    * graphic_events_control.modelMatrix * new Vector4(node_pt3.X, node_pt3.Y, node_pt3.Z, 1.0f);
                Vector4 node_pt4_fp = graphic_events_control.projectionMatrix * graphic_events_control.viewMatrix
                    * graphic_events_control.modelMatrix * new Vector4(node_pt4.X, node_pt4.Y, node_pt4.Z, 1.0f);
                Vector4 md_pt_12_fp = graphic_events_control.projectionMatrix * graphic_events_control.viewMatrix
                    * graphic_events_control.modelMatrix * new Vector4(md_pt_12.X, md_pt_12.Y, md_pt_12.Z, 1.0f);
                Vector4 md_pt_23_fp = graphic_events_control.projectionMatrix * graphic_events_control.viewMatrix
                    * graphic_events_control.modelMatrix * new Vector4(md_pt_23.X, md_pt_23.Y, md_pt_23.Z, 1.0f);
                Vector4 md_pt_34_fp = graphic_events_control.projectionMatrix * graphic_events_control.viewMatrix
                    * graphic_events_control.modelMatrix * new Vector4(md_pt_34.X, md_pt_34.Y, md_pt_34.Z, 1.0f);
                Vector4 md_pt_41_fp = graphic_events_control.projectionMatrix * graphic_events_control.viewMatrix
                    * graphic_events_control.modelMatrix * new Vector4(md_pt_41.X, md_pt_41.Y, md_pt_41.Z, 1.0f);
                Vector4 quad_midpt_fp = graphic_events_control.projectionMatrix * graphic_events_control.viewMatrix
                    * graphic_events_control.modelMatrix * new Vector4(quad_midpt.X, quad_midpt.Y, quad_midpt.Z, 1.0f);


                // Check whether the point inside a rectangle
                if (gvariables_static.isPointSelected(corner_pt1, corner_pt2, new Vector2(node_pt1_fp.X, node_pt1_fp.Y)) == true ||
                    gvariables_static.isPointSelected(corner_pt1, corner_pt2, new Vector2(node_pt2_fp.X, node_pt2.Y)) == true ||
                    gvariables_static.isPointSelected(corner_pt1, corner_pt2, new Vector2(node_pt3_fp.X, node_pt3_fp.Y)) == true ||
                    gvariables_static.isPointSelected(corner_pt1, corner_pt2, new Vector2(node_pt4_fp.X, node_pt4_fp.Y)) == true ||
                    gvariables_static.isPointSelected(corner_pt1, corner_pt2, new Vector2(md_pt_12_fp.X, md_pt_12_fp.Y)) == true ||
                    gvariables_static.isPointSelected(corner_pt1, corner_pt2, new Vector2(md_pt_23_fp.X, md_pt_23_fp.Y)) == true ||
                    gvariables_static.isPointSelected(corner_pt1, corner_pt2, new Vector2(md_pt_34_fp.X, md_pt_34_fp.Y)) == true ||
                    gvariables_static.isPointSelected(corner_pt1, corner_pt2, new Vector2(md_pt_41_fp.X, md_pt_41_fp.Y)) == true ||
                    gvariables_static.isPointSelected(corner_pt1, corner_pt2, new Vector2(quad_midpt_fp.X, quad_midpt_fp.Y)) == true)
                {
                    selected_quad_index.Add(quad_m.Key);

                }

            }

            return selected_quad_index;

        }


        private void get_quad_vertex_buffer(quad_store quad, ref float[] quad_vertices, ref int quad_v_index)
        {
            // Get the quad points
            Vector3 pt1 = _allPts.pointMap[_allLines.lineMap[quad.tri123.edge1_id].start_pt_id].pt_coord;
            Vector3 pt2 = _allPts.pointMap[_allLines.lineMap[quad.tri123.edge2_id].start_pt_id].pt_coord;
            Vector3 pt3 = _allPts.pointMap[_allLines.lineMap[quad.tri341.edge1_id].start_pt_id].pt_coord;
            Vector3 pt4 = _allPts.pointMap[_allLines.lineMap[quad.tri341.edge2_id].start_pt_id].pt_coord;

            if(is_ShrinkTriangle == true)
            {
                // Shrink the triangle
                Vector3 midPt = new Vector3((pt1.X + pt2.X + pt3.X + pt4.X) * 0.25f,
                    (pt1.Y + pt2.Y + pt3.Y + pt4.Y) * 0.25f,
                    (pt1.Z + pt2.Z + pt3.Z + pt4.Z) * 0.25f);

                float shrink_factor = (float)gvariables_static.mesh_shrink_factor;

                pt1 = gvariables_static.linear_interpolation3d(midPt, pt1, shrink_factor);
                pt2 = gvariables_static.linear_interpolation3d(midPt, pt2, shrink_factor);
                pt3 = gvariables_static.linear_interpolation3d(midPt, pt3, shrink_factor);
                pt4 = gvariables_static.linear_interpolation3d(midPt, pt4, shrink_factor);

            }


            // Get the node buffer for the shader
            // Point 1
            // Point location
            quad_vertices[quad_v_index + 0] = pt1.X;
            quad_vertices[quad_v_index + 1] = pt1.Y;

            // Point color
            quad_vertices[quad_v_index + 2] = quad.quad_color.X;
            quad_vertices[quad_v_index + 3] = quad.quad_color.Y;
            quad_vertices[quad_v_index + 4] = quad.quad_color.Z;

            quad_vertices[quad_v_index + 5] = is_DynamicDraw ? 1.0f : 0.0f;

            quad_vertices[quad_v_index + 6] = (float)_allPts.pointMap[_allLines.lineMap[quad.tri123.edge1_id].start_pt_id].normalized_defl_scale;

            // Iterate
            quad_v_index = quad_v_index + 7;


            // Point 2
            // Point location
            quad_vertices[quad_v_index + 0] = pt2.X;
            quad_vertices[quad_v_index + 1] = pt2.Y;

            // Point color
            quad_vertices[quad_v_index + 2] = quad.quad_color.X;
            quad_vertices[quad_v_index + 3] = quad.quad_color.Y;
            quad_vertices[quad_v_index + 4] = quad.quad_color.Z;

            quad_vertices[quad_v_index + 5] = is_DynamicDraw ? 1.0f : 0.0f;

            quad_vertices[quad_v_index + 6] = (float)_allPts.pointMap[_allLines.lineMap[quad.tri123.edge2_id].start_pt_id].normalized_defl_scale;

            // Iterate
            quad_v_index = quad_v_index + 7;


            // Point 3
            // Point location
            quad_vertices[quad_v_index + 0] = pt3.X;
            quad_vertices[quad_v_index + 1] = pt3.Y;

            // Point color
            quad_vertices[quad_v_index + 2] = quad.quad_color.X;
            quad_vertices[quad_v_index + 3] = quad.quad_color.Y;
            quad_vertices[quad_v_index + 4] = quad.quad_color.Z;

            quad_vertices[quad_v_index + 5] = is_DynamicDraw ? 1.0f : 0.0f;

            quad_vertices[quad_v_index + 6] = (float)_allPts.pointMap[_allLines.lineMap[quad.tri341.edge1_id].start_pt_id].normalized_defl_scale;

            // Iterate
            quad_v_index = quad_v_index + 7;


            // Point 4
            // Point location
            quad_vertices[quad_v_index + 0] = pt4.X;
            quad_vertices[quad_v_index + 1] = pt4.Y;

            // Point color
            quad_vertices[quad_v_index + 2] = quad.quad_color.X;
            quad_vertices[quad_v_index + 3] = quad.quad_color.Y;
            quad_vertices[quad_v_index + 4] = quad.quad_color.Z;

            quad_vertices[quad_v_index + 5] = is_DynamicDraw ? 1.0f : 0.0f;

            quad_vertices[quad_v_index + 6] = (float)_allPts.pointMap[_allLines.lineMap[quad.tri341.edge2_id].start_pt_id].normalized_defl_scale;

            // Iterate
            quad_v_index = quad_v_index + 7;


        }


        private void get_quad_index_buffer(ref int[] quad_vertex_indices, ref int quad_i_index)
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
