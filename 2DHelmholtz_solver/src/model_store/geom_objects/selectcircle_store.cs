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
    public class selectcircle_store
    {

        private Vector2 o_pt = new Vector2(0);
        private Vector2 c_pt = new Vector2(0);
        private bool isPaint = false;

        private graphicBuffers selcircle_bndry_buffer;
        private graphicBuffers selcircle_interior_buffer;
        public Shader selcircle_shader;

        private Vector2 center_pt = new Vector2(0);
        private List<Vector2> circleBoundaryPoints = new List<Vector2>();

        private int numSegments = 36;


        public selectcircle_store()
        {
            // Empty shader

        }

        public void set_shader()
        {
            // Create Shader
            selcircle_shader = new Shader(ShaderLibrary.get_vertex_shader(ShaderLibrary.ShaderType.SelectionShader),
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
            int line_indices_count = 2 * numSegments; // 2 Points, numSegments lines
            int[] line_vertex_indices = new int[line_indices_count];


            for (int i = 0; i<numSegments-1; i++)
            {
                // Boundary line indices
                line_vertex_indices[(2 * i) + 0] = i + 1;
                line_vertex_indices[(2 * i) + 1] = i + 2;

            }

            // Final segment
            line_vertex_indices[(2 * numSegments) - 2] = numSegments;
            line_vertex_indices[(2 * numSegments) - 1] = 1;


            VertexBufferLayout line_pt_layout = new VertexBufferLayout();
            line_pt_layout.AddFloat(2);  // Node point

            // Define the node vertices of the model for a node (2 position) 
            int line_vertex_count = 2 * numSegments;
            int line_vertex_size = line_vertex_count * sizeof(float); // Size of the node_vertex

            // Create the Selection Circle Boundary buffers
            selcircle_bndry_buffer = new graphicBuffers(null, line_vertex_size, line_vertex_indices,
                 line_indices_count, line_pt_layout, true);

        }


        private void set_shadedtriangle_buffer()
        {
            // Set the buffer for index
            int tri_indices_count = 3 * numSegments; // 3 Points, numSegments Triangles
            int[] tri_vertex_indices = new int[tri_indices_count];


            for (int i = 0; i < numSegments - 1; i++)
            {
                // Triangles
                tri_vertex_indices[(3 * i) + 0] = 0;
                tri_vertex_indices[(3 * i) + 1] = i + 1;
                tri_vertex_indices[(3 * i) + 2] = i + 2;

            }

            // Final triangle
            tri_vertex_indices[(3 * numSegments) - 3] = 0;
            tri_vertex_indices[(3 * numSegments) - 2] = numSegments;
            tri_vertex_indices[(3 * numSegments) - 1] = 1;


            VertexBufferLayout tri_pt_layout = new VertexBufferLayout();
            tri_pt_layout.AddFloat(2);  // Node point

            // Define the node vertices of the model for a node (2 position) 
            int tri_vertex_count = 2 * (numSegments + 1);
            int tri_vertex_size = tri_vertex_count * sizeof(float); // Size of the node_vertex

            // Create the Select Circle Interior buffers
            selcircle_interior_buffer = new graphicBuffers(null, tri_vertex_size, tri_vertex_indices,
                 tri_indices_count, tri_pt_layout, true);

        }


        public void update_selection_circle(Vector2 o_pt, Vector2 c_pt, bool isPaint)
        {

            // Assign to private circle points
            this.o_pt = o_pt;
            this.c_pt = c_pt;
            this.isPaint = isPaint;

            // Center point
            this.center_pt = new Vector2((o_pt.X + c_pt.X) * 0.5f, (o_pt.Y + c_pt.Y) * 0.5f);
            double circle_diameter = Math.Sqrt(Math.Pow((c_pt.X - o_pt.X), 2) + Math.Pow((c_pt.Y - o_pt.Y), 2));
            double circle_radius = circle_diameter / 2.0;

            // Find the boundary points of the circle
           this.circleBoundaryPoints = new List<Vector2>();

            for (int i = 0; i < numSegments; i++)
            {

                double angle = 2 * Math.PI * i / numSegments;
                double x = center_pt.X + (circle_radius * Math.Cos(angle));
                double y = center_pt.Y + (circle_radius * Math.Sin(angle));
                this.circleBoundaryPoints.Add(new Vector2((float)x, (float)y));

            }

        }


        public void paint_selection_circle()
        {
            //_____________________________________________________________
            if (isPaint == false)
                return;

            selcircle_shader.Bind();

            // Update the point buffer data for dynamic drawing
            update_buffer();

            // Paint the boundary lines
            selcircle_bndry_buffer.Bind();

            GL.DrawElements(PrimitiveType.Lines, (2 * numSegments), DrawElementsType.UnsignedInt, 0);
            selcircle_bndry_buffer.UnBind();

            // Paint the interior shaded triangle
            selcircle_interior_buffer.Bind();

            GL.DrawElements(PrimitiveType.Triangles, (3 * numSegments), DrawElementsType.UnsignedInt, 0);
            selcircle_interior_buffer.UnBind();

            selcircle_shader.UnBind();
        }


        private void update_buffer()
        {
            int line_vertex_count = 2 * (numSegments + 1);
            float[] line_vertices = new float[line_vertex_count];

            int line_v_index = 0;

            // Set the line point vertices
            // Center Point (Index 0)
            line_vertices[line_v_index + 0] = center_pt.X;
            line_vertices[line_v_index + 1] = center_pt.Y;

            line_v_index = line_v_index + 2;


            // Circumference Boundary Points 
            foreach (Vector2 pt in circleBoundaryPoints)
            {
                line_vertices[line_v_index + 0] = pt.X;
                line_vertices[line_v_index + 1] = pt.Y;

                line_v_index = line_v_index + 2;

            }

       
            int line_vertex_size = line_vertex_count * sizeof(float); // Size of the line point vertex

            // Update the buffer
            selcircle_bndry_buffer.UpdateDynamicVertexBuffer(line_vertices, line_vertex_size);
            selcircle_interior_buffer.UpdateDynamicVertexBuffer(line_vertices, line_vertex_size);

        }




    }
}
