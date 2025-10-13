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
        public bool isModelSet = false;


        // Drawing bound data
        public Vector3 min_bounds = new Vector3(-1);
        public Vector3 max_bounds = new Vector3(1);
        public Vector3 geom_bounds = new Vector3(2);


        public selectrectangle_store selection_rectangle { get; }
        public selectcircle_store selection_circle { get; }

        // To control the drawing events
        public drawing_events graphic_events_control { get; private set; }

        // Update of mesh properties
        public bool isConstraintUpdateInProgress = false;
        public bool isLoadUpdateInProgress = false;
        public bool isMaterialUpdateInProgress = false;
        public bool isBoundaryUpdateInProgress = false;


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

            meshdata = new meshdata_store();

            // To control the drawing graphics
            graphic_events_control = new drawing_events(this);

            // Set the selection rectangle  & selection circle
            selection_rectangle = new selectrectangle_store();
            selection_circle = new selectcircle_store();

            // Set a default geometry bounds
            min_bounds = new Vector3(-1);
            max_bounds = new Vector3(1);
            geom_bounds = new Vector3(2);

        }

        public void importMesh(string fileContent)
        {
            List<Vector3> nodePtsList = new List<Vector3>();
            isModelSet = false;

            file_events.import_mesh(fileContent, ref fe_nodes, ref fe_tris, ref fe_quads,
                ref fe_constraints, ref fe_loads, ref fe_materials, ref materialids, ref nodePtsList, ref isModelSet);


            if (isModelSet == false)
                return;

            // Set the mesh boundaries
            Vector3 geometry_center = gvariables_static.FindGeometricCenter(nodePtsList);
            Tuple<Vector3, Vector3> geom_extremes = gvariables_static.FindMinMaxXY(nodePtsList);


            // Set the geometry bounds
            this.min_bounds = geom_extremes.Item1; // Minimum bound
            this.max_bounds = geom_extremes.Item2; // Maximum bound

            this.geom_bounds = max_bounds - min_bounds;

            // Create the mesh for drawing
            meshdata = new meshdata_store();

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

                meshdata.add_mesh_quads(quad.quad_id, quad.nodeid1, quad.nodeid2, quad.nodeid3, quad.nodeid4, quad.material_id);

            }

            // Create the mesh boundaries
            meshdata.set_mesh_wireframe();
            meshdata.create_drawing_boundary();

            //// Model is set
            //meshdata.is_ModelSet = true;

            // Set the openTK buffer
            meshdata.set_shader();
            meshdata.set_buffer();

            // Set the shader of selection rectangle and circle
            selection_rectangle.set_shader();
            selection_circle.set_shader();

            // Set the buffer of selection rectangle and circle
            selection_rectangle.set_buffer();
            selection_circle.set_buffer();


            // Update the openGL uniform
            meshdata.update_openTK_uniforms(true, true, true, graphic_events_control.projectionMatrix,
                graphic_events_control.modelMatrix, graphic_events_control.viewMatrix,
                graphic_events_control.geom_transparency);

        }

        public void paint_model()
        {
            if (isModelSet == false)
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



            if (isMaterialUpdateInProgress == true || isLoadUpdateInProgress == true || isConstraintUpdateInProgress == true
                || isBoundaryUpdateInProgress == true)
            {

                // Paint the selected meshes and point
                if (isLoadUpdateInProgress == true || isConstraintUpdateInProgress == true)
                {
                    meshdata.paint_selected_points();

                }


                if (isMaterialUpdateInProgress == true)
                {
                    meshdata.paint_selected_mesh();
                }

                if(isBoundaryUpdateInProgress == true)
                {
                    meshdata.paint_selected_edges();
                }


                if (gvariables_static.is_RectangleSelection == true)
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




        public void update_openTK_uniforms(bool set_modelmatrix, bool set_viewmatrix, bool set_transparency)
        {
            if (isModelSet == false)
                return;


            meshdata.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                graphic_events_control.projectionMatrix,
                graphic_events_control.modelMatrix,
                graphic_events_control.viewMatrix,
                graphic_events_control.geom_transparency);


        }



        public void select_mesh_objects(Vector2 o_pt, Vector2 c_pt, bool isRightButton)
        {
            // Perform the select option
            if (isMaterialUpdateInProgress == true)
            {
                meshdata.select_mesh_elements(o_pt, c_pt, isRightButton, graphic_events_control);

            }

            if (isConstraintUpdateInProgress == true || isLoadUpdateInProgress == true)
            {
                // Select the points for load or constraint update
                meshdata.select_mesh_points(o_pt, c_pt, isRightButton, graphic_events_control);

            }

            if (isBoundaryUpdateInProgress == true)
            {
                meshdata.select_mesh_edges(o_pt, c_pt, isRightButton, graphic_events_control);
            }

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
            if (isMaterialUpdate == true)
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
