using _2DHelmholtz_solver.src.opentk_control.opentk_bgdraw;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public drawing_events graphic_events_control { get; private set; }


        public meshdata_store()
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

        }


        public void add_mesh_point(int point_id, double x_coord, double y_coord, double z_coord)
        {
            // Add the mesh point
            mesh_points.add_point(point_id, x_coord, y_coord, z_coord); 

        }

	    public void add_selected_points(List<int> selected_point_id)
        {
            // Add the selected points
            selected_mesh_points.clear_points();

            // Selected points id
            foreach (int pt_id in selected_point_id)
            {
                // get the point
                point_store pt = mesh_points.pointMap[pt_id];

                selected_mesh_points.add_point(pt_id, pt.x_coord, pt.y_coord, pt.z_coord);
                
            }

            selected_mesh_points.set_buffer();

        }

        public void add_selected_tris(List<int> selected_tri_id)
        {
            // Add the selected triangles
            selected_mesh_tris.clear_triangles();

            // Selected triangles id
            foreach (int tri_id in selected_tri_id)
            {
                // get the triangle
                tri_store tri = mesh_tris.triMap[tri_id];

                selected_mesh_tris.add_tri(tri_id, tri.edge1_id, tri.edge2_id, tri.edge3_id);

            }

            selected_mesh_tris.set_buffer();

        }

        public void add_selected_quads(List<int> selected_quad_id)
        {
            // Add the selected quadrilaterals
            selected_mesh_quads.clear_quadrilaterals();

            // Selected quadrilaterals id
            foreach (int quad_id in selected_quad_id)
            {
                // get the quadrilateral
                tri_store tri123 = mesh_quads.quadMap[quad_id].tri123;
                tri_store tri341 = mesh_quads.quadMap[quad_id].tri341;

                selected_mesh_quads.add_quad(quad_id, tri123.edge1_id, tri123.edge2_id, tri123.edge3_id,
                    tri341.edge1_id, tri341.edge2_id, tri341.edge3_id);

            }

            selected_mesh_quads.set_buffer();

        }

        public void add_mesh_tris(int tri_id,int  point_id1, int point_id2, int point_id3)
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
            mesh_tris.add_tri(tri_id, line_id1, line_id2, line_id3);


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

        public void add_mesh_quads(int quad_id, int point_id1, int point_id2, int point_id3, int point_id4)
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
            mesh_quads.add_quad(quad_id, line_id1, line_id2, line_id3, line_id4, line_id5, line_id6);

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

            HashSet<(int, int)> uniqueEdges = new HashSet<(int, int)>();


            foreach (var kvp in mesh_half_edges.lineMap)
            {
                int start = kvp.Value.start_pt_id;
                int end = kvp.Value.end_pt_id;

                // Store edge as an unordered pair
                (int, int) edge = (Math.Min(start, end), Math.Max(start, end));

                if (!uniqueEdges.Contains(edge))
                {
                    uniqueEdges.Add(edge);

                    // Add to wireframe rendering or storage
                    mesh_boundaries.add_line(kvp.Key, edge.Item1, edge.Item2);

                }
            }

        }


        public void update_tri_material_ids(List<int> selected_tri_id, int material_id)
        {
            // Update the material id of the Triangle element


        }


        public void update_quad_material_ids(List<int> selected_quad_id, int material_id)
        {
            // Update the material id of the Quadrilateral element


        }


        public void set_buffer()
        {
            // Set the buffer
            // mesh points
            mesh_points.set_buffer();

            // mesh boundaries
            mesh_boundaries.set_buffer();

            // mesh tris and quads
            mesh_tris.set_buffer();
            mesh_quads.set_buffer();



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
            // Paint the selected points
            selected_mesh_points.paint_static_points();

        }

        public void paint_selected_mesh()
        {
            // Paint the selected tris and quds
            selected_mesh_tris.paint_static_triangles();
            selected_mesh_quads.paint_static_quadrilaterals();

        }


        public void paint_mesh_materialids()
        {
            // // Paint the mesh material IDs
            // mesh_tri_material_ids.paint_static_texts();
            // mesh_quad_material_ids.paint_static_texts();

        }


        public void update_openTK_uniforms(bool is_zoomtofit, bool is_intellizoom, bool is_pan, bool is_drawingareachange)
        {
            // Following graphics operation is performed
            // 1) Zoom to Fit (Ctrl + F)
            // 2) Intelli Zoom (Ctrl + Scroll up/ down)
            // 3) Pan operation (Ctrl + Right button drag)
            // 4) Drawing Area change (Resize of drawing area)



        }



        private int add_half_edge(int startpt_id, int endpt_id)
        {
            mesh_half_edges.add_line(half_edge_count, startpt_id, endpt_id);

            // Iterate the half edge count
            half_edge_count++;

            return (half_edge_count - 1); // return the index of last addition
        }



    }
}
