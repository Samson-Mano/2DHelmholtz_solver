using _2DHelmholtz_solver.global_variables;
using _2DHelmholtz_solver.src.events_handler;
using _2DHelmholtz_solver.src.model_store.geom_objects;
using _2DHelmholtz_solver.src.opentk_control.opentk_bgdraw;
using OpenTK.Graphics.ES11;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// OpenTK library
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;


namespace _2DHelmholtz_solver.src.model_store.fe_objects
{

    public class material_data
    {
        public int material_id = 0;
        public string material_name = "";

        public double material_permittivity = 0.0;
        public double material_permeability = 0.0; // E
        public double material_conductivity = 0.0; // G

        public int number_of_elements_appliedto = 0;
    }




    public class fedata_store
    {
        public node_list_store fe_nodes;
        public elementtri_list_store fe_tris;
        public elementquad_list_store fe_quads;

        public nodecnst_list_store fe_constraints;
        public nodeload_list_store fe_loads;

        public Dictionary<int, material_data> fe_materials;
        public List<int> materialids;

        public meshdata_store meshdata;
        bool isModelLoadSuccess = false;



        public fedata_store()
        {
            // (Re)Initialize the data
            fe_nodes = new node_list_store();
            fe_tris = new elementtri_list_store();
            fe_quads = new elementquad_list_store();

            fe_constraints = new nodecnst_list_store();
            fe_loads = new nodeload_list_store();

            fe_materials = new Dictionary<int, material_data>();
            materialids = new List<int>(); 

            meshdata = new meshdata_store(new Vector3(-1), new Vector3(1), new Vector3(2));

        }

        public void importMesh(string fileContent)
        {
            List<Vector3> nodePtsList = new List<Vector3>();
            isModelLoadSuccess = false;

            file_events.import_mesh(fileContent, ref fe_nodes, ref fe_tris, ref fe_quads,
                ref fe_constraints, ref fe_loads, ref fe_materials, ref materialids, ref nodePtsList, ref isModelLoadSuccess);


            if (isModelLoadSuccess == false)
                return;

            // Set the mesh boundaries
            Vector3 geometry_center = gvariables_static.FindGeometricCenter(nodePtsList);
            Tuple<Vector3, Vector3> geom_extremes = gvariables_static.FindMinMaxXY(nodePtsList);

            Vector3 geom_min_b = geom_extremes.Item1; // Minimum bound
            Vector3 geom_max_b = geom_extremes.Item2; // Maximum bound

            Vector3 geom_bounds = geom_max_b - geom_min_b;


            // Create the mesh for drawing
            meshdata = new meshdata_store(geom_min_b,geom_max_b, geom_bounds);

            // Add the mesh points
            foreach (var nd_m in fe_nodes.nodeMap)
            {
                node_store nd = nd_m.Value;

                meshdata.add_mesh_point(nd.node_id, nd.node_pt_x_coord, nd.node_pt_y_coord, nd.node_pt_z_coord, -1);

            }

            // Add the mesh tris
            foreach (var tri_m in fe_tris.elementtriMap)
            {
                elementtri_store tri = tri_m.Value;

                meshdata.add_mesh_tris(tri.tri_id, tri.nodeid1, tri.nodeid2, tri.nodeid3, tri.material_id);

            }

            // Add the mesh quads
            foreach (var quad_m in fe_quads.elementquadMap)
            {
                elementquad_store quad = quad_m.Value;

                meshdata.add_mesh_quads(quad.quad_id, quad.nodeid1, quad.nodeid2 , quad.nodeid3, quad.nodeid4, quad.material_id);

            }

            // Create the mesh boundaries
            meshdata.set_mesh_wireframe();
            meshdata.create_drawing_boundary();

            // Model is set
            meshdata.is_ModelSet = true;

            // Set the openTK buffer
            meshdata.set_shader();
            meshdata.set_buffer();

            // Update the openGL uniform
            meshdata.update_openTK_uniforms(true, true, true);

        }

        public void paint_model()
        {
            if (isModelLoadSuccess == false)
                return;

            meshdata.paint_drawing_boundary();

            // Paint the mesh quad & mesh tris
            if (gvariables_static.is_paint_mesh == true)
            {
                meshdata.paint_static_mesh();

            }

            // Paint the mesh boundaries
            if (gvariables_static.is_paint_mesh_boundaries == true)
            {
                meshdata.paint_static_mesh_boundaries();


            }

            // Paint the loads
            if (gvariables_static.is_paint_loads == true)
            {


            }


            // Paint the constraints
            if (gvariables_static.is_paint_constraints == true)
            {


            }

            // Paint the selected meshes and point
            meshdata.paint_selected_mesh();
            meshdata.paint_selected_points();

            // Paint the selection rectangle or circle
            meshdata.paint_selection_boundaries();


        }


        public void update_material_id(int material_id, bool isMaterialDelete)
        {
            // Flag to make sure material update happened
            bool isMaterialUpdate = false;

            if (isMaterialDelete == false)
            {
                if (meshdata.selected_tri_ids.Count > 0)
                {
                    // Update the material id of the Triangle element
                    fe_tris.update_material(meshdata.selected_tri_ids, material_id);
                    isMaterialUpdate = true;   
                }

                if (meshdata.selected_quad_ids.Count > 0)
                {
                    // Update the material id of the Quadrilateral element
                    fe_quads.update_material(meshdata.selected_quad_ids, material_id);
                    isMaterialUpdate = true;
                }

            }
            else
            {
                if (fe_materials[material_id].number_of_elements_appliedto > 0)
                {
                    // Material is deleted so assign the default material
                    fe_tris.execute_delete_material(material_id);
                    fe_quads.execute_delete_material(material_id);
                    isMaterialUpdate = true;
                }

            }


            // If Material update happened
            if(isMaterialUpdate == true)
            {
                // Clear the number of elements applied to data on material
                foreach (int mat_id in fe_materials.Keys)
                {
                    fe_materials[mat_id].number_of_elements_appliedto = 0;
                }

                // Update the mesh tris color id
                foreach (var tri_m in fe_tris.elementtriMap)
                {
                    elementtri_store tri = tri_m.Value;

                    meshdata.update_mesh_tris_color_id(tri.tri_id, tri.material_id);

                    // Increment the number of elements applied to
                    fe_materials[tri.material_id].number_of_elements_appliedto++;

                }

                // Update the mesh quads color id
                foreach (var quad_m in fe_quads.elementquadMap)
                {
                    elementquad_store quad = quad_m.Value;

                    meshdata.update_mesh_quads_color_id(quad.quad_id, quad.material_id);

                    // Increment the number of elements applied to
                    fe_materials[quad.material_id].number_of_elements_appliedto++;

                }

                // Clear the selected elements
                meshdata.clear_selected_mesh();

                // Update the buffer
                meshdata.update_mesh_color_buffer();

            }

        }



    }
}
