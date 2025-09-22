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

        public int color_id { get; set; }

        public Vector3 tri_color { get; set; }
    }

    public class tri_list_store
    {
        public Dictionary<int, tri_store> triMap { get; } = new Dictionary<int, tri_store>();
        public int tri_count = 0;
        private bool is_DynamicDraw = false;
        public bool is_ShrinkTriangle = false;

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


        public void add_tri(int tri_id, int edge1_id, int edge2_id, int edge3_id, int color_id)
        {
            // Add the Tri to the list
            tri_store temp_tri = new tri_store
            {
                tri_id = tri_id,
                edge1_id = edge1_id,
                edge2_id = edge2_id,
                edge3_id = edge3_id,
                color_id = color_id,
                tri_color = gvariables_static.ColorUtils.MeshGetRandomColor(color_id)

            };

            triMap[tri_id] = temp_tri;
            tri_count++;

        }

        public void set_shader()
        {

            // Create Shader
            tri_shader = new Shader(ShaderLibrary.get_vertex_shader(ShaderLibrary.ShaderType.MeshShader),
                ShaderLibrary.get_fragment_shader(ShaderLibrary.ShaderType.MeshShader));

        }


        public void set_buffer()
        {

            // Set the buffer for index
            int tri_indices_count = 3 * tri_count; // 3 indices to form a triangle
            int[] tri_vertex_indices = new int[tri_indices_count];

            int tri_i_index = 0;

            // Set the tri index buffers
            foreach (var tri in triMap)
            {
                get_tri_index_buffer(ref tri_vertex_indices, ref tri_i_index);
            }

            // Define the vertex layout
            VertexBufferLayout triLayout = new VertexBufferLayout();
            triLayout.AddFloat(2);  // point center
            triLayout.AddFloat(3);  // point color
            triLayout.AddFloat(1);  // Is Dynamic data
            triLayout.AddFloat(1);  // Normalized deflection scale

            // Define the vertex buffer size for a point 3 * ( 2 position, 3 color, 2 dynamic data)
            int tri_vertex_count = 3 * 7 * tri_count;
            int tri_vertex_size = tri_vertex_count * sizeof(float);

            // Create the triangle dynamic buffers
            tri_buffer = new graphicBuffers(null, tri_vertex_size, tri_vertex_indices,
                tri_indices_count, triLayout, true);

            // Update the buffer
            update_buffer();

        }

        public void update_buffer()
        {
            // Define the vertex buffer size for a point 3 * ( 2 position, 3 color, 2 dynamic data)
            int tri_vertex_count = 3 * 7 * tri_count;
            float[] tri_vertices = new float[tri_vertex_count];

            int tri_v_index = 0;

            // Set the tri vertex buffers
            foreach (var tri in triMap)
            {
                // Add vertex buffers
                get_tri_vertex_buffer(tri.Value, ref tri_vertices, ref tri_v_index);
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


        public List<int> is_tri_selected(Vector2 corner_pt1, Vector2 corner_pt2, drawing_events graphic_events_control)
        {
            // Selected triangle list index;
            List<int> selected_tri_index = new List<int>();

            // Loop through all triangle in map
            foreach (var tri_m in triMap)
            {
                tri_store tri = tri_m.Value;

                // End points
                Vector3 node_pt1 = _allPts.pointMap[_allLines.lineMap[tri.edge1_id].start_pt_id].pt_coord;
                Vector3 node_pt2 = _allPts.pointMap[_allLines.lineMap[tri.edge2_id].start_pt_id].pt_coord;
                Vector3 node_pt3 = _allPts.pointMap[_allLines.lineMap[tri.edge3_id].start_pt_id].pt_coord;

                // Mid points
                Vector3 md_pt_12 = gvariables_static.linear_interpolation3d(node_pt1, node_pt2, 0.50);
                Vector3 md_pt_23 = gvariables_static.linear_interpolation3d(node_pt2, node_pt3, 0.50);
                Vector3 md_pt_31 = gvariables_static.linear_interpolation3d(node_pt3, node_pt1, 0.50);
                Vector3 tri_midpt = new Vector3((node_pt1.X + node_pt2.X + node_pt3.X) * 0.33f,
                    (node_pt1.Y + node_pt2.Y + node_pt3.Y) * 0.33f,
                    (node_pt1.Z + node_pt2.Z + node_pt3.Z) * 0.33f);


                //______________________________
                Vector4 node_pt1_fp = graphic_events_control.projectionMatrix * graphic_events_control.viewMatrix
                    * graphic_events_control.modelMatrix * new Vector4(node_pt1.X, node_pt1.Y, node_pt1.Z, 1.0f);
                Vector4 node_pt2_fp = graphic_events_control.projectionMatrix * graphic_events_control.viewMatrix
                    * graphic_events_control.modelMatrix * new Vector4(node_pt2.X, node_pt2.Y, node_pt2.Z, 1.0f);
                Vector4 node_pt3_fp = graphic_events_control.projectionMatrix * graphic_events_control.viewMatrix
                    * graphic_events_control.modelMatrix * new Vector4(node_pt3.X, node_pt3.Y, node_pt3.Z, 1.0f);
                Vector4 md_pt_12_fp = graphic_events_control.projectionMatrix * graphic_events_control.viewMatrix
                    * graphic_events_control.modelMatrix * new Vector4(md_pt_12.X, md_pt_12.Y, md_pt_12.Z, 1.0f);
                Vector4 md_pt_23_fp = graphic_events_control.projectionMatrix * graphic_events_control.viewMatrix
                    * graphic_events_control.modelMatrix * new Vector4(md_pt_23.X, md_pt_23.Y, md_pt_23.Z, 1.0f);
                Vector4 md_pt_31_fp = graphic_events_control.projectionMatrix * graphic_events_control.viewMatrix
                    * graphic_events_control.modelMatrix * new Vector4(md_pt_31.X, md_pt_31.Y, md_pt_31.Z, 1.0f);
                Vector4 tri_midpt_fp = graphic_events_control.projectionMatrix * graphic_events_control.viewMatrix
                    * graphic_events_control.modelMatrix * new Vector4(tri_midpt.X, tri_midpt.Y, tri_midpt.Z, 1.0f);


                // Check whether the point inside a rectangle
                if (gvariables_static.isPointSelected(corner_pt1, corner_pt2, new Vector2(node_pt1_fp.X, node_pt1_fp.Y)) == true ||
                    gvariables_static.isPointSelected(corner_pt1, corner_pt2, new Vector2(node_pt2_fp.X, node_pt2.Y)) == true ||
                    gvariables_static.isPointSelected(corner_pt1, corner_pt2, new Vector2(node_pt3_fp.X, node_pt3_fp.Y)) == true ||
                    gvariables_static.isPointSelected(corner_pt1, corner_pt2, new Vector2(md_pt_12_fp.X, md_pt_12_fp.Y)) == true ||
                    gvariables_static.isPointSelected(corner_pt1, corner_pt2, new Vector2(md_pt_23_fp.X, md_pt_23_fp.Y)) == true ||
                    gvariables_static.isPointSelected(corner_pt1, corner_pt2, new Vector2(md_pt_31_fp.X, md_pt_31_fp.Y)) == true ||
                    gvariables_static.isPointSelected(corner_pt1, corner_pt2, new Vector2(tri_midpt_fp.X, tri_midpt_fp.Y)) == true)
                {
                    selected_tri_index.Add(tri_m.Key);

                }

            }

            return selected_tri_index;

        }


        private void get_tri_vertex_buffer(tri_store tri, ref float[] tri_vertices, ref int tri_v_index)
        {
            // Get the quad points
            Vector3 pt1 = _allPts.pointMap[_allLines.lineMap[tri.edge1_id].start_pt_id].pt_coord;
            Vector3 pt2 = _allPts.pointMap[_allLines.lineMap[tri.edge2_id].start_pt_id].pt_coord;
            Vector3 pt3 = _allPts.pointMap[_allLines.lineMap[tri.edge3_id].start_pt_id].pt_coord;

            if (is_ShrinkTriangle == true)
            {
                // Shrink the triangle
                Vector3 midPt = new Vector3((pt1.X + pt2.X + pt3.X) * 0.33f,
                    (pt1.Y + pt2.Y + pt3.Y) * 0.33f,
                    (pt1.Z + pt2.Z + pt3.Z) * 0.33f);

                float shrink_factor = (float)gvariables_static.mesh_shrink_factor;

                pt1 = gvariables_static.linear_interpolation3d(midPt, pt1, shrink_factor);
                pt2 = gvariables_static.linear_interpolation3d(midPt, pt2, shrink_factor);
                pt3 = gvariables_static.linear_interpolation3d(midPt, pt3, shrink_factor);

            }


            // Get the node buffer for the shader
            // Point 1
            // Point location
            tri_vertices[tri_v_index + 0] = pt1.X;
            tri_vertices[tri_v_index + 1] = pt1.Y;

            // Point color
            tri_vertices[tri_v_index + 2] = tri.tri_color.X;
            tri_vertices[tri_v_index + 3] = tri.tri_color.Y;
            tri_vertices[tri_v_index + 4] = tri.tri_color.Z;

            tri_vertices[tri_v_index + 5] = is_DynamicDraw ? 1.0f : 0.0f;

            tri_vertices[tri_v_index + 6] = (float)_allPts.pointMap[_allLines.lineMap[tri.edge1_id].start_pt_id].normalized_defl_scale;

            // Iterate
            tri_v_index = tri_v_index + 7;


            // Point 2
            // Point location
            tri_vertices[tri_v_index + 0] = pt2.X;
            tri_vertices[tri_v_index + 1] = pt2.Y;

            // Point color
            tri_vertices[tri_v_index + 2] = tri.tri_color.X;
            tri_vertices[tri_v_index + 3] = tri.tri_color.Y;
            tri_vertices[tri_v_index + 4] = tri.tri_color.Z;

            tri_vertices[tri_v_index + 5] = is_DynamicDraw ? 1.0f : 0.0f;

            tri_vertices[tri_v_index + 6] = (float)_allPts.pointMap[_allLines.lineMap[tri.edge2_id].start_pt_id].normalized_defl_scale;

            // Iterate
            tri_v_index = tri_v_index + 7;


            // Point 3
            // Point location
            tri_vertices[tri_v_index + 0] = pt3.X;
            tri_vertices[tri_v_index + 1] = pt3.Y;

            // Point color
            tri_vertices[tri_v_index + 2] = tri.tri_color.X;
            tri_vertices[tri_v_index + 3] = tri.tri_color.Y;
            tri_vertices[tri_v_index + 4] = tri.tri_color.Z;

            tri_vertices[tri_v_index + 5] = is_DynamicDraw ? 1.0f : 0.0f;

            tri_vertices[tri_v_index + 6] = (float)_allPts.pointMap[_allLines.lineMap[tri.edge3_id].start_pt_id].normalized_defl_scale;

            // Iterate
            tri_v_index = tri_v_index + 7;

        }


        private void get_tri_index_buffer(ref int[] tri_vertex_indices, ref int tri_i_index)
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
