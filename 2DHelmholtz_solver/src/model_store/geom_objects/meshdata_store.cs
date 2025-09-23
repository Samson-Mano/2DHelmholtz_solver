using _2DHelmholtz_solver.global_variables;
using _2DHelmholtz_solver.src.opentk_control.opentk_bgdraw;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

// OpenTK library
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using _2DHelmholtz_solver.src.model_store.fe_objects;
using System.Windows.Forms;


namespace _2DHelmholtz_solver.src.model_store.geom_objects
{
    public class meshdata_store
    {

        private point_list_store mesh_points { get; }
        private point_list_store selected_mesh_points { get; }
        private line_list_store mesh_half_edges { get; }
        private line_list_store mesh_boundaries { get; }
        private tri_list_store mesh_tris { get; }
        private tri_list_store selected_mesh_tris { get; }
        private quad_list_store mesh_quads { get; }
        private quad_list_store selected_mesh_quads { get; }

        private int half_edge_count = 0;

        // To control the drawing events
        //private point_list_store drawing_boundary_points { get; }
        //private line_list_store drawing_boundary_lines {  get; }

        public drawing_events graphic_events_control { get; private set; }

        public selectrectangle_store selection_rectangle { get; }
        public selectcircle_store selection_circle { get; }


        // Drawing bound data
        public Vector3 min_bounds { get; } = new Vector3(-1);
        public Vector3 max_bounds { get; } = new Vector3(1);
        public Vector3 geom_bounds { get; } = new Vector3(2);

        public bool is_ModelSet = false;


        // Update of mesh properties
        public bool isConstraintUpdateInProgress = false;
        public bool isLoadUpdateInProgress = false;
        public bool isMaterialUpdateInProgress = false;


        public List<int> selected_tri_ids { get; } = new List<int>();
        public List<int> selected_quad_ids { get; } = new List<int>();
        public List<int> selected_point_ids { get; } = new List<int>();


        public meshdata_store(Vector3 min_bounds, Vector3 max_bounds, Vector3 geom_bounds)
        {

            // Initialize the mesh points, lines, triangles and quadrilateral
            mesh_points = new point_list_store();
            selected_mesh_points = new point_list_store();
            mesh_half_edges = new line_list_store(mesh_points);
            mesh_boundaries = new line_list_store(mesh_points);
            mesh_tris = new tri_list_store(mesh_points, mesh_half_edges);
            selected_mesh_tris = new tri_list_store(mesh_points, mesh_half_edges);
            mesh_quads = new quad_list_store(mesh_points, mesh_half_edges);
            selected_mesh_quads = new quad_list_store(mesh_points, mesh_half_edges);

            // To control the drawing graphics
            graphic_events_control = new drawing_events(this);

            // Set the selection rectangle  & selection circle
            selection_rectangle = new selectrectangle_store();
            selection_circle = new selectcircle_store();

            // Geometry bounds
            this.min_bounds = min_bounds;
            this.max_bounds = max_bounds;
            this.geom_bounds = geom_bounds;

            // Selected mesh is drawn as shrunk triangle
            selected_mesh_tris.is_ShrinkTriangle = true;
            selected_mesh_quads.is_ShrinkTriangle = true;

            //// Create the boundary lines
            //drawing_boundary_points = new point_list_store();
            //drawing_boundary_lines = new line_list_store(drawing_boundary_points);

            is_ModelSet = false;

        }

        public void create_drawing_boundary()
        {
            //// Create drawing boundary
            //drawing_boundary_points.add_point(0, this.min_bounds.X,this.min_bounds.Y,0.0,-1);
            //drawing_boundary_points.add_point(1, this.min_bounds.X, this.max_bounds.Y, 0.0, -1);
            //drawing_boundary_points.add_point(2, this.max_bounds.X, this.max_bounds.Y, 0.0, -1);
            //drawing_boundary_points.add_point(3, this.max_bounds.X, this.min_bounds.Y, 0.0, -1);

            //drawing_boundary_lines.add_line(0, 0, 1, -1);
            //drawing_boundary_lines.add_line(1, 1, 2, -1);
            //drawing_boundary_lines.add_line(2, 2, 3, -1);
            //drawing_boundary_lines.add_line(3, 3, 0, -1);

            //drawing_boundary_lines.set_buffer();
            //drawing_boundary_lines.update_buffer();

            //Matrix4 iMatrix = Matrix4.Identity;
            //drawing_boundary_lines.line_shader.SetMatrix4("projectionMatrix", iMatrix);
            //drawing_boundary_lines.line_shader.SetMatrix4("viewMatrix", iMatrix);
            //drawing_boundary_lines.line_shader.SetFloat("vertexTransparency", 1.0f);

        }



        public void add_mesh_point(int point_id, double x_coord, double y_coord, double z_coord, int color_id)
        {
            // Add the mesh point
            mesh_points.add_point(point_id, x_coord, y_coord, z_coord, color_id);

        }

        public void add_selected_points(List<int> selected_point_ids, bool isRemove)
        {
            bool is_selection_changed = false;

            if (isRemove == false)
            {
                // Add to the selected point list
                foreach (int pt_id in selected_point_ids)
                {
                    // Check whether the element is already in the list
                    if (!this.selected_point_ids.Contains(pt_id))
                    {
                        // Add to selected elements
                        this.selected_point_ids.Add(pt_id);

                        // Selection changed flag
                        is_selection_changed = true;
                    }

                }
            }
            else
            {
                // Remove from the selected point list
                foreach (int pt_id in selected_point_ids)
                {
                    // Remove the elements which is found in the list
                    this.selected_point_ids.Remove(pt_id);

                    // Selection changed flag
                    is_selection_changed = true;
                }

            }


            if (is_selection_changed == true)
            {
                // Add the selected points
                selected_mesh_points.clear_points();

                // Selected points id
                foreach (int pt_id in this.selected_point_ids)
                {
                    // get the point
                    point_store pt = mesh_points.pointMap[pt_id];

                    selected_mesh_points.add_point(pt_id, pt.x_coord, pt.y_coord, pt.z_coord, -2);

                }

                selected_mesh_points.set_buffer();
            }

        }

        public void add_selected_tris(List<int> selected_tri_ids, bool isRemove)
        {
            bool is_selection_changed = false;

            if(isRemove == false)
            {
                // Add to the selected tri elements list
                foreach (int tri_id in selected_tri_ids)
                {
                    // Check whether the element is already in the list
                    if (!this.selected_tri_ids.Contains(tri_id))
                    {
                        // Add to selected elements
                        this.selected_tri_ids.Add(tri_id);

                        // Selection changed flag
                        is_selection_changed = true;
                    }

                }
            }
            else
            {
                // Remove from the selected tri elements list
                foreach (int tri_id in selected_tri_ids)
                {
                    // Remove the elements which is found in the list
                    this.selected_tri_ids.Remove(tri_id);

                    // Selection changed flag
                    is_selection_changed = true;
                }

            }


            if(is_selection_changed == true)
            {
                // Add the selected triangles
                selected_mesh_tris.clear_triangles();

                // Selected triangles id
                foreach (int tri_id in this.selected_tri_ids)
                {
                    // get the triangle
                    tri_store tri = mesh_tris.triMap[tri_id];

                    selected_mesh_tris.add_tri(tri_id, tri.edge1_id, tri.edge2_id, tri.edge3_id, -2);

                }

                selected_mesh_tris.set_buffer();
            }

        }

        public void add_selected_quads(List<int> selected_quad_ids, bool isRemove)
        {
            bool is_selection_changed = false;

            if (isRemove == false)
            {
                // Add to the selected quad elements list
                foreach (int quad_id in selected_quad_ids)
                {
                    // Check whether the element is already in the list
                    if (!this.selected_quad_ids.Contains(quad_id))
                    {
                        // Add to selected elements
                        this.selected_quad_ids.Add(quad_id);

                        // Selection changed flag
                        is_selection_changed = true;
                    }

                }
            }
            else
            {
                // Remove from the selected quad elements list
                foreach (int quad_id in selected_quad_ids)
                {
                    // Remove the elements which is found in the list
                    this.selected_quad_ids.Remove(quad_id);

                    // Selection changed flag
                    is_selection_changed = true;
                }

            }


            if (is_selection_changed == true)
            {
                // Add the selected quadrilaterals
                selected_mesh_quads.clear_quadrilaterals();

                // Selected quadrilaterals id
                foreach (int quad_id in this.selected_quad_ids)
                {
                    // get the quadrilateral
                    tri_store tri123 = mesh_quads.quadMap[quad_id].tri123;
                    tri_store tri341 = mesh_quads.quadMap[quad_id].tri341;

                    selected_mesh_quads.add_quad(quad_id, tri123.edge1_id, tri123.edge2_id, tri123.edge3_id,
                        tri341.edge1_id, tri341.edge2_id, tri341.edge3_id, -2);

                }
                selected_mesh_quads.set_buffer();

            }

        }

        public void clear_selected_mesh()
        {
            // Clear the selected triangles
            selected_tri_ids.Clear();   
            selected_mesh_tris.clear_triangles();
            selected_mesh_tris.set_buffer();

            // Clear the selected quadrilaterals
            selected_quad_ids.Clear();
            selected_mesh_quads.clear_quadrilaterals();
            selected_mesh_quads.set_buffer();

        }


        public void clear_selected_nodes()
        {
            // Clear the selected mesh points
            selected_point_ids.Clear();
            selected_mesh_points.clear_points();
            selected_mesh_points.set_buffer();

        }


        public void add_mesh_tris(int tri_id, int point_id1, int point_id2, int point_id3, int color_id)
        {
            //    2____3 
            //    |   /  
            //    | /    
            //    1      

            // Add the half triangle of the quadrilaterals
            // Add three half edges
            int line_id1, line_id2, line_id3;

            // Add edge 1
            line_id1 = add_half_edge(point_id1, point_id2);

            // Point 1 or point 2 not found
            if (line_id1 == -1)
                return;

            // Add edge 2
            line_id2 = add_half_edge(point_id2, point_id3);

            // Point 3 not found
            if (line_id2 == -1)
            {
                mesh_half_edges.lineMap.Remove(half_edge_count - 1); // remove the last item which is edge 1
                half_edge_count--;
                return;
            }


            // Add edge 3
            line_id3 = add_half_edge(point_id3, point_id1);


            //________________________________________
            // Add the mesh triangles
            mesh_tris.add_tri(tri_id, line_id1, line_id2, line_id3, color_id);


            // Set the half edges next line
            mesh_half_edges.lineMap[line_id1].next_line_id = line_id2;
            mesh_half_edges.lineMap[line_id2].next_line_id = line_id3;
            mesh_half_edges.lineMap[line_id3].next_line_id = line_id1;

            // Set the half edge face data
            mesh_half_edges.lineMap[line_id1].tri_face_id = tri_id;
            mesh_half_edges.lineMap[line_id2].tri_face_id = tri_id;
            mesh_half_edges.lineMap[line_id3].tri_face_id = tri_id;


            //_______________________________________________________________________________________________________
            // Add a text for material ID
            Vector3 nd_pt1 = mesh_points.pointMap[mesh_half_edges.lineMap[line_id1].start_pt_id].pt_coord;
            Vector3 nd_pt2 = mesh_points.pointMap[mesh_half_edges.lineMap[line_id2].start_pt_id].pt_coord;
            Vector3 nd_pt3 = mesh_points.pointMap[mesh_half_edges.lineMap[line_id3].start_pt_id].pt_coord;

            // Calculate the midpoint of the triangle
            Vector3 tri_mid_pt = new Vector3((nd_pt1.X + nd_pt2.X + nd_pt3.X) * 0.33333f,
                (nd_pt1.Y + nd_pt2.Y + nd_pt3.Y) * 0.33333f,
                (nd_pt1.Z + nd_pt2.Z + nd_pt3.Z) * 0.33333f);

            //// Add the material ID
            //glm::vec3 temp_str_color = geom_parameters::get_standard_color(0);
            //std::string temp_str = "1234";
            //mesh_tri_material_ids.add_text(tri_id, temp_str, tri_mid_pt, temp_str_color);

        }

        public void add_mesh_quads(int quad_id, int point_id1, int point_id2, int point_id3, int point_id4, int color_id)
        {
            //    2____3     2____3      3
            //    |   /|     |   /     / |  
            //    | /  |     | /     /   |   
            //    1____4     1      1____4   

            // Add the quadrilaterals
            // Add the 1st half triangle of the quadrilaterals
            // Add three half edges
            int line_id1, line_id2, line_id3;

            // Add edge 1
            line_id1 = add_half_edge(point_id1, point_id2);

            // Add edge 2
            line_id2 = add_half_edge(point_id2, point_id3);

            // Add edge 3
            line_id3 = add_half_edge(point_id3, point_id1);

            // Set the half edges next line
            mesh_half_edges.lineMap[line_id1].next_line_id = line_id2;
            mesh_half_edges.lineMap[line_id2].next_line_id = line_id3;
            mesh_half_edges.lineMap[line_id3].next_line_id = line_id1;


            // Add the 2nd half triangle of the quadrilaterals
            // Add three half edges
            int line_id4, line_id5, line_id6;

            // Add edge 4
            line_id4 = add_half_edge(point_id3, point_id4);

            // Add edge 5
            line_id5 = add_half_edge(point_id4, point_id1);

            // Add edge 6
            line_id6 = add_half_edge(point_id1, point_id3);


            // Set the half edges next line
            mesh_half_edges.lineMap[line_id4].next_line_id = line_id5;
            mesh_half_edges.lineMap[line_id5].next_line_id = line_id6;
            mesh_half_edges.lineMap[line_id6].next_line_id = line_id4;


            //________________________________________
            // Add the mesh quadrilaterals
            mesh_quads.add_quad(quad_id, line_id1, line_id2, line_id3,
                line_id4, line_id5, line_id6, color_id);

            // Set the half edge face data 1st Half triangle of the quadrilateral
            mesh_half_edges.lineMap[line_id1].quad_face_id = quad_id;
            mesh_half_edges.lineMap[line_id2].quad_face_id = quad_id;
            mesh_half_edges.lineMap[line_id3].quad_face_id = quad_id;

            // Set the half edge face data 2nd Half triangle of the quadrilateral
            mesh_half_edges.lineMap[line_id4].quad_face_id = quad_id;
            mesh_half_edges.lineMap[line_id5].quad_face_id = quad_id;
            mesh_half_edges.lineMap[line_id6].quad_face_id = quad_id;


            //_______________________________________________________________________________________________________
            // Add a text for material ID
            Vector3 nd_pt1 = mesh_points.pointMap[mesh_half_edges.lineMap[line_id1].start_pt_id].pt_coord;
            Vector3 nd_pt2 = mesh_points.pointMap[mesh_half_edges.lineMap[line_id2].start_pt_id].pt_coord;
            Vector3 nd_pt3 = mesh_points.pointMap[mesh_half_edges.lineMap[line_id4].start_pt_id].pt_coord;
            Vector3 nd_pt4 = mesh_points.pointMap[mesh_half_edges.lineMap[line_id5].start_pt_id].pt_coord;

            // Calculate the midpoint of the triangle
            Vector3 quad_mid_pt = new Vector3((nd_pt1.X + nd_pt2.X + nd_pt3.X + nd_pt4.X) * 0.25f,
                (nd_pt1.Y + nd_pt2.Y + nd_pt3.Y + nd_pt4.Y) * 0.25f,
                (nd_pt1.Z + nd_pt2.Z + nd_pt3.Z + nd_pt4.Z) * 0.25f);

            //// Add the material ID
            //glm::vec3 temp_str_color = geom_parameters::get_standard_color(0);
            //std::string temp_str = "1234";
            //mesh_quad_material_ids.add_text(quad_id, temp_str, quad_mid_pt, temp_str_color);



        }

        public void update_mesh_point(int point_id, double x_coord, double y_coord, double z_coord, double normalized_defl_scale)
        {
            // Update the point with new - coordinates
            mesh_points.update_point(point_id, x_coord, y_coord, z_coord, normalized_defl_scale);

        }

        public void set_mesh_wireframe()
        {
            HashSet<int> unique_edge_ids = new HashSet<int>();

            // Get the unique edge Ids of Triangle Mesh
            foreach (var tri_m in mesh_tris.triMap)
            {
                // get the value of tri mesh
                tri_store tri = tri_m.Value;

                unique_edge_ids.Add(tri.edge1_id);
                unique_edge_ids.Add(tri.edge2_id);
                unique_edge_ids.Add(tri.edge3_id);

            }

            // Get the unique edge Ids of Quadrilateral Mesh
            foreach (var quad_m in mesh_quads.quadMap)
            {
                // get the value of quad mesh
                quad_store quad = quad_m.Value;

                unique_edge_ids.Add(quad.tri123.edge1_id);
                unique_edge_ids.Add(quad.tri123.edge2_id);
                unique_edge_ids.Add(quad.tri341.edge1_id);
                unique_edge_ids.Add(quad.tri341.edge2_id);

            }

            // Create the mesh wire frame
            foreach (int edge_id in unique_edge_ids)
            {
                // Add to wireframe rendering or storage
                mesh_boundaries.add_line(edge_id, mesh_half_edges.lineMap[edge_id].start_pt_id,
                     mesh_half_edges.lineMap[edge_id].end_pt_id, -1);

            }

        }


        public void update_mesh_tris_color_id(int tri_id, int color_id)
        {
            // Update the triangle element color id
            mesh_tris.triMap[tri_id].color_id = color_id;
            mesh_tris.triMap[tri_id].tri_color = gvariables_static.ColorUtils.MeshGetRandomColor(color_id);

        }


        public void update_mesh_quads_color_id(int quad_id, int color_id)
        {
            // Update the quadrilateral element color id
            mesh_quads.quadMap[quad_id].color_id = color_id;
            mesh_quads.quadMap[quad_id].quad_color = gvariables_static.ColorUtils.MeshGetRandomColor(color_id);

        }

        public void update_mesh_color_buffer()
        {
            // Update the buffer
            mesh_tris.update_buffer();
            mesh_quads.update_buffer();

        }


        //public void update_material_id(int material_id, bool isMaterialDelete)
        //{



        //    //if(isMaterialDelete == false)
        //    //{
        //    //    if (this.selected_tri_ids.Count > 0)
        //    //    {
        //    //        // Update the material id of the Triangle element
        //    //        foreach (int tri_id in this.selected_tri_ids)
        //    //        {
        //    //            mesh_tris.triMap[tri_id].color_id = material_id;
        //    //            mesh_tris.triMap[tri_id].tri_color = gvariables_static.ColorUtils.MeshGetRandomColor(material_id);
        //    //        }

        //    //        // Update the buffer to change the color based on new material id
        //    //        mesh_tris.update_buffer();
        //    //    }

        //    //    if(this.selected_quad_ids.Count > 0)
        //    //    {
        //    //        // Update the material id of the Quadrilateral element
        //    //        foreach (int quad_id in this.selected_quad_ids)
        //    //        {
        //    //            mesh_quads.quadMap[quad_id].color_id = material_id;
        //    //            mesh_quads.quadMap[quad_id].quad_color = gvariables_static.ColorUtils.MeshGetRandomColor(material_id);
        //    //        }

        //    //        // Update the buffer to change the color based on new material id
        //    //        mesh_quads.update_buffer();

        //    //    }

        //    //}
        //    //else
        //    //{
        //    //    // Material is deleted so assign the default material
        //    //    bool isMaterialUpdate = false;

        //    //    // Update the material id of the Triangle element
        //    //    foreach (var tri in mesh_tris.triMap)
        //    //    {
        //    //        if(mesh_tris.triMap[tri.Key].color_id == material_id)
        //    //        {
        //    //            mesh_tris.triMap[tri.Key].color_id = 0;
        //    //            mesh_tris.triMap[tri.Key].tri_color = gvariables_static.ColorUtils.MeshGetRandomColor(0);

        //    //            isMaterialUpdate = true;
        //    //        }
        //    //    }

        //    //    if(isMaterialUpdate == true)
        //    //    {
        //    //        // Update the buffer to change the color based on new material id
        //    //        mesh_tris.set_buffer();
        //    //    }

        //    //    // Check the quad material id
        //    //    isMaterialUpdate = false;

        //    //    // Update the material id of the Quadrilateral element
        //    //    foreach (var quad in mesh_quads.quadMap)
        //    //    {
        //    //        if (mesh_quads.quadMap[quad.Key].color_id == material_id)
        //    //        {
        //    //            mesh_quads.quadMap[quad.Key].color_id = 0;
        //    //            mesh_quads.quadMap[quad.Key].quad_color = gvariables_static.ColorUtils.MeshGetRandomColor(0);

        //    //            isMaterialUpdate = true;
        //    //        }
        //    //    }
        //    //    if (isMaterialUpdate == true)
        //    //    {
        //    //        // Update the buffer to change the color based on new material id
        //    //        mesh_quads.set_buffer();
        //    //    }

        //    //}

        //}



        public void set_shader()
        {
            // Set the buffer of selection rectangle
            selection_rectangle.set_shader();
            selection_circle.set_shader();

            // Set the shader
            // mesh points
            mesh_points.set_shader();
            selected_mesh_points.set_shader();

            // mesh boundaries
            mesh_boundaries.set_shader();

            // mesh tris and quads
            mesh_tris.set_shader();
            mesh_quads.set_shader();
            selected_mesh_tris.set_shader();
            selected_mesh_quads.set_shader();

        }


        public void set_buffer()
        {
            // Set the buffer of selection rectangle
            selection_rectangle.set_buffer();
            selection_circle.set_buffer();

            // Set the buffer
            // mesh points
            mesh_points.set_buffer();
            selected_mesh_points.set_buffer();

            // mesh boundaries
            mesh_boundaries.set_buffer();

            // mesh tris and quads
            mesh_tris.set_buffer();
            mesh_quads.set_buffer();
            selected_mesh_tris.set_buffer();
            selected_mesh_quads.set_buffer();

        }


        public void paint_drawing_boundary()
        {
            // // Paint the boundary of the model
            // drawing_boundary_lines.paint_static_lines();

        }


        public void paint_static_mesh()
        {
            // Paint the static mesh (mesh which are fixed)
            // Paint the mesh triangles
            mesh_tris.paint_static_triangles();
            mesh_quads.paint_static_quadrilaterals();

        }


        public void paint_static_mesh_boundaries()
        {
            // Paint the mesh boundaries
            mesh_boundaries.paint_static_lines();

        }


        public void paint_static_mesh_points()
        {
            // Paint the mesh points
            mesh_points.paint_static_points();

        }

        public void paint_dynamic_mesh()
        {
            // Paint the dynamic mesh (mesh which are not-fixed but variable)
            // Paint the mesh triangles
            mesh_tris.paint_dynamic_triangles();
            mesh_quads.paint_dynamic_quadrilaterals();

        }

        public void paint_dynamic_mesh_boundaries()
        {
            // Paint the mesh lines
            mesh_boundaries.paint_dynamic_lines();

        }

        public void paint_dynamic_mesh_points()
        {
            // Paint the mesh points
            mesh_points.paint_dynamic_points();

        }

        public void paint_selected_points()
        {
            if (isLoadUpdateInProgress == true || isConstraintUpdateInProgress == true)
            {
                // Paint the selected points
                GL.PointSize(4.0f);
                selected_mesh_points.paint_static_points();
                GL.PointSize(1.0f);
            }

        }

        public void paint_selected_mesh()
        {
            if (isMaterialUpdateInProgress == true)
            {
                // Paint the selected tris and quds
                selected_mesh_tris.paint_static_triangles();
                selected_mesh_quads.paint_static_quadrilaterals();
            }
            
        }


        public void paint_mesh_materialids()
        {
            // // Paint the mesh material IDs
            // mesh_tri_material_ids.paint_static_texts();
            // mesh_quad_material_ids.paint_static_texts();

        }


        public void paint_selection_boundaries()
        {

            if (isMaterialUpdateInProgress == true || isLoadUpdateInProgress == true || isConstraintUpdateInProgress == true)
            {
                if(gvariables_static.is_RectangleSelection == true)
                {
                    // Paint the selection rectangle
                    selection_rectangle.paint_selection_rectangle();
                }
                else
                {
                    // Paint the selection circle
                    selection_circle.paint_selection_circle();
                }
                
            }

        }

        public void select_mesh_objects(Vector2 o_pt, Vector2 c_pt, bool isRightButton)
        {
            // Perform the select option
            if (isMaterialUpdateInProgress == true)
            {
                // Select the elements for material property update
                List<int> selected_tri_ids = mesh_tris.is_tri_selected(o_pt, c_pt, graphic_events_control);
                List<int> selected_quad_ids = mesh_quads.is_quad_selected(o_pt, c_pt, graphic_events_control);

                add_selected_tris(selected_tri_ids, isRightButton);
                add_selected_quads(selected_quad_ids, isRightButton);

            }

            if(isConstraintUpdateInProgress == true || isLoadUpdateInProgress == true)
            {
                // Select the nodes for constraint update
                List<int> selected_point_index = mesh_points.is_point_selected(o_pt, c_pt, graphic_events_control);

                add_selected_points(selected_point_index, isRightButton);

            }

            //if(isLoadUpdateInProgress == true)
            //{
            //    // Select the nodes for load update


            //}

        }


        public void update_mesh_shrinkage()
        {
            // Perform the shrinkage of mesh
            mesh_tris.is_ShrinkTriangle = gvariables_static.is_paint_shrunk_triangle;
            mesh_tris.update_buffer();

            mesh_quads.is_ShrinkTriangle = gvariables_static.is_paint_shrunk_triangle;
            mesh_quads.update_buffer();

        }



        public void update_openTK_uniforms(bool set_modelmatrix, bool set_viewmatrix, bool set_transparency)
        {
            // Following graphics operation is performed
            // 1) Zoom to Fit (Ctrl + F)
            // 2) Intelli Zoom (Ctrl + Scroll up/ down)
            // 3) Pan operation (Ctrl + Right button drag)
            // 4) Drawing Area change (Resize of drawing area)

            if (is_ModelSet == false)
                return;

            // Update the openGl uniform matrices
            if (set_modelmatrix == true)
            {
                // Set the model matrix
                mesh_quads.quad_shader.SetMatrix4("modelMatrix", graphic_events_control.modelMatrix);
                mesh_tris.tri_shader.SetMatrix4("modelMatrix", graphic_events_control.modelMatrix);
                selected_mesh_quads.quad_shader.SetMatrix4("modelMatrix", graphic_events_control.modelMatrix);
                selected_mesh_tris.tri_shader.SetMatrix4("modelMatrix", graphic_events_control.modelMatrix);

                mesh_boundaries.line_shader.SetMatrix4("modelMatrix", graphic_events_control.modelMatrix);

                selected_mesh_points.point_shader.SetMatrix4("modelMatrix", graphic_events_control.modelMatrix);
                mesh_points.point_shader.SetMatrix4("modelMatrix", graphic_events_control.modelMatrix);

                // drawing_boundary_lines.line_shader.SetMatrix4("modelMatrix", graphic_events_control.modelMatrix);

                // Set the projection matrix
                mesh_quads.quad_shader.SetMatrix4("projectionMatrix", graphic_events_control.projectionMatrix);
                mesh_tris.tri_shader.SetMatrix4("projectionMatrix", graphic_events_control.projectionMatrix);
                selected_mesh_quads.quad_shader.SetMatrix4("projectionMatrix", graphic_events_control.projectionMatrix);
                selected_mesh_tris.tri_shader.SetMatrix4("projectionMatrix", graphic_events_control.projectionMatrix);

                mesh_boundaries.line_shader.SetMatrix4("projectionMatrix", graphic_events_control.projectionMatrix);

                selected_mesh_points.point_shader.SetMatrix4("projectionMatrix", graphic_events_control.projectionMatrix);
                mesh_points.point_shader.SetMatrix4("projectionMatrix", graphic_events_control.projectionMatrix);

            }

            if (set_viewmatrix == true)
            {
                // Set the view matrix
                mesh_quads.quad_shader.SetMatrix4("viewMatrix", graphic_events_control.viewMatrix);
                mesh_tris.tri_shader.SetMatrix4("viewMatrix", graphic_events_control.viewMatrix);
                selected_mesh_quads.quad_shader.SetMatrix4("viewMatrix", graphic_events_control.viewMatrix);
                selected_mesh_tris.tri_shader.SetMatrix4("viewMatrix", graphic_events_control.viewMatrix);

                mesh_boundaries.line_shader.SetMatrix4("viewMatrix", graphic_events_control.viewMatrix);

                selected_mesh_points.point_shader.SetMatrix4("viewMatrix", graphic_events_control.viewMatrix);
                mesh_points.point_shader.SetMatrix4("viewMatrix", graphic_events_control.viewMatrix);

            }

            if (set_transparency == true)
            {
                // Set the transparency float
                mesh_quads.quad_shader.SetFloat("vertexTransparency", graphic_events_control.geom_transparency);
                mesh_tris.tri_shader.SetFloat("vertexTransparency", graphic_events_control.geom_transparency);
                selected_mesh_quads.quad_shader.SetFloat("vertexTransparency", graphic_events_control.geom_transparency);
                selected_mesh_tris.tri_shader.SetFloat("vertexTransparency", graphic_events_control.geom_transparency);

                mesh_boundaries.line_shader.SetFloat("vertexTransparency", 0.1f);

                selected_mesh_points.point_shader.SetFloat("vertexTransparency", graphic_events_control.geom_transparency);
                mesh_points.point_shader.SetFloat("vertexTransparency", graphic_events_control.geom_transparency);

            }


            // mesh_tri_material_ids.update_opengl_uniforms(set_modelmatrix, set_viewmatrix, set_transparency);
            // mesh_quad_material_ids.update_opengl_uniforms(set_modelmatrix, set_viewmatrix, set_transparency);

        }



        private int add_half_edge(int startpt_id, int endpt_id)
        {
            mesh_half_edges.add_line(half_edge_count, startpt_id, endpt_id, -1);

            // Iterate the half edge count
            half_edge_count++;

            return (half_edge_count - 1); // return the index of last addition
        }



    }
}
