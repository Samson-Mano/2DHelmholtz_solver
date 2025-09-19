using _2DHelmholtz_solver.opentk_control.opentk_buffer;
using _2DHelmholtz_solver.opentk_control.shader_compiler;
using _2DHelmholtz_solver.src.opentk_control.opentk_buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

// OpenTK library
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using _2DHelmholtz_solver.global_variables;

namespace _2DHelmholtz_solver.src.model_store.geom_objects
{
    public class selectrectangle_store
    {

        private Vector2 o_pt = new Vector2(0);
        private Vector2 c_pt = new Vector2(0);
        private bool isPaint = false;

        private graphicBuffers selrect_bndry_buffer;
        private graphicBuffers selrect_interior_buffer;
        public Shader selrect_shader;

        

        public selectrectangle_store()
        {
            // Empty shader

        }

        public void set_shader()
        {
            // Create Shader
            selrect_shader = new Shader(ShaderLibrary.get_vertex_shader(ShaderLibrary.ShaderType.SelectionShader),
                ShaderLibrary.get_fragment_shader(ShaderLibrary.ShaderType.SelectionShader));

        }


        public void set_buffer()
        {
            // Set the buffer
            set_boundaryline_buffer();
            set_shadedtriangle_buffer();

        }


        private void set_boundaryline_buffer()
        {
            // Set the buffer for index
            int line_indices_count = 2 * 4; // 2 Points, 4 lines
            int[] line_vertex_indices = new int[line_indices_count];


            // Line 1
            line_vertex_indices[0] = 0;
            line_vertex_indices[1] = 1;

            // Line 2
            line_vertex_indices[2] = 1;
            line_vertex_indices[3] = 2;

            // Line 3
            line_vertex_indices[4] = 2;
            line_vertex_indices[5] = 3;

            // Line 4
            line_vertex_indices[6] = 3;
            line_vertex_indices[7] = 0;

            VertexBufferLayout line_pt_layout = new VertexBufferLayout();
            line_pt_layout.AddFloat(2);  // Node point

            // Define the node vertices of the model for a node (2 position) 
            int line_vertex_count = 2 * 4;
            int line_vertex_size = line_vertex_count * sizeof(float); // Size of the node_vertex

            // Create the Selection Rectangle Boundary buffers
            selrect_bndry_buffer = new graphicBuffers(null, line_vertex_size, line_vertex_indices,
                 line_indices_count, line_pt_layout, true);

        }

        private void set_shadedtriangle_buffer()
        {
            // Set the buffer for index
            int tri_indices_count = 3 * 2; // 3 Points, 2 Triangles
            int[] tri_vertex_indices = new int[tri_indices_count];

            // Triangle 1
            tri_vertex_indices[0] = 0;
            tri_vertex_indices[1] = 1;
            tri_vertex_indices[2] = 3;

            // Triangle 2
            tri_vertex_indices[3] = 3;
            tri_vertex_indices[4] = 1;
            tri_vertex_indices[5] = 2;


            VertexBufferLayout tri_pt_layout = new VertexBufferLayout();
            tri_pt_layout.AddFloat(2);  // Node point

            // Define the node vertices of the model for a node (2 position) 
            int tri_vertex_count = 2 * 4;
            int tri_vertex_size = tri_vertex_count * sizeof(float); // Size of the node_vertex

            // Create the Select Rectangle Interior buffers
            selrect_interior_buffer = new graphicBuffers(null, tri_vertex_size, tri_vertex_indices,
                 tri_indices_count, tri_pt_layout, true);


        }


        public void update_selection_rectangle(Vector2 o_pt, Vector2 c_pt, bool isPaint)
        {
            //// Set the parameters
            //int max_dim = geom_param_ptr->window_width > geom_param_ptr->window_height ? geom_param_ptr->window_width : geom_param_ptr->window_height;

            //// Transform the mouse location to openGL screen coordinates
            //double screen_opt_x = 2.0f * ((o_pt.x - (geom_param_ptr->window_width * 0.5f)) / max_dim);
            //double screen_opt_y = 2.0f * (((geom_param_ptr->window_height * 0.5f) - o_pt.y) / max_dim);

            //double screen_cpt_x = 2.0f * ((c_pt.x - (geom_param_ptr->window_width * 0.5f)) / max_dim);
            //double screen_cpt_y = 2.0f * (((geom_param_ptr->window_height * 0.5f) - c_pt.y) / max_dim);

            // Assign to private rectangle points
            this.o_pt = o_pt;
            this.c_pt = c_pt;
            this.isPaint = isPaint;

        }


        public void paint_selection_rectangle()
        {
            //_____________________________________________________________
            if (isPaint == false)
                return;

            selrect_shader.Bind();

            // Update the point buffer data for dynamic drawing
            update_buffer();

            // Paint the boundary lines
            selrect_bndry_buffer.Bind();

            GL.DrawElements(PrimitiveType.Lines, (2 * 4), DrawElementsType.UnsignedInt, 0);
            selrect_bndry_buffer.UnBind();

            // Paint the interior shaded triangle
            selrect_interior_buffer.Bind();

            GL.DrawElements(PrimitiveType.Triangles, (3 * 2), DrawElementsType.UnsignedInt, 0);
            selrect_interior_buffer.UnBind();

            selrect_shader.UnBind();
        }


        private void update_buffer()
        {
            int line_vertex_count = 2 * 4;
            float[] line_vertices = new float[line_vertex_count];

            int line_v_index = 0;

            // Set the line point vertices
            // Point 1 (Index 0)
            line_vertices[line_v_index + 0] = o_pt.X;
            line_vertices[line_v_index + 1] = o_pt.Y;

            line_v_index = line_v_index + 2;

            // Point 2
            line_vertices[line_v_index + 0] = o_pt.X;
            line_vertices[line_v_index + 1] = c_pt.Y;

            line_v_index = line_v_index + 2;

            // Point 3
            line_vertices[line_v_index + 0] = c_pt.X;
            line_vertices[line_v_index + 1] = c_pt.Y;

            line_v_index = line_v_index + 2;

            // Point 4
            line_vertices[line_v_index + 0] = c_pt.X;
            line_vertices[line_v_index + 1] = o_pt.Y;

            line_v_index = line_v_index + 2;

            int line_vertex_size = line_vertex_count * sizeof(float); // Size of the line point vertex

            // Update the buffer
            selrect_bndry_buffer.UpdateDynamicVertexBuffer(line_vertices, line_vertex_size);
            selrect_interior_buffer.UpdateDynamicVertexBuffer(line_vertices, line_vertex_size);

        }



    }
}
