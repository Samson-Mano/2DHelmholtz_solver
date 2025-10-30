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

    public struct result_data_extremes
    {
        public double u_real_max;
        public double u_real_min;

        public double u_imag_max;
        public double u_imag_min;

        public double u_magnitude_max;
        public double u_magnitude_min;

        public double u_phase_max;
        public double u_phase_min;

    }


    public class fedata_store
    {
        public node_list_store fe_nodes;
        public elementtri_list_store fe_tris;
        public elementquad_list_store fe_quads;

        public nodecnst_list_store fe_nodeconstraints;
        public edgecnst_list_store fe_edgeconstraints;
        public nodeload_list_store fe_loads;

        public Dictionary<int, material_data> fe_materials;
        public List<int> materialids;
        public label_list_store materiallabels;

        public meshdata_store meshdata;
        public bool isModelSet = false;

        public meshdata_store resultmeshdata;
        public result_data_extremes result_extremes;
        public bool isResultSet = false;


        // Drawing bound data
        public Vector3 min_bounds = new Vector3(-1);
        public Vector3 max_bounds = new Vector3(1);
        public Vector3 geom_bounds = new Vector3(2);


        public selectrectangle_store selection_rectangle { get; }
        public selectcircle_store selection_circle { get; }

        // To control the drawing events
        public drawing_events graphic_events_control { get; private set; }

        // Update of mesh properties
        public bool isNodalConstraintUpdateInProgress = false;
        public bool isEdgeConstraintUpdateInProgress = false;
        public bool isLoadUpdateInProgress = false;
        public bool isMaterialUpdateInProgress = false;



        public fedata_store()
        {
            // (Re)Initialize the data
            fe_nodes = new node_list_store();
            fe_tris = new elementtri_list_store();
            fe_quads = new elementquad_list_store();

            fe_nodeconstraints = new nodecnst_list_store();
            fe_edgeconstraints = new edgecnst_list_store();
            fe_loads = new nodeload_list_store();

            fe_materials = new Dictionary<int, material_data>();
            materialids = new List<int>();

            meshdata = new meshdata_store(false);
            resultmeshdata = new meshdata_store(true);

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

        public void importTXTFile(string fileContent)
        {
            List<Vector3> nodePtsList = new List<Vector3>();
            isModelSet = false;
            isResultSet = false;

            file_events.import_txt_mesh(fileContent, ref fe_nodes, ref fe_tris, ref fe_quads,
                ref fe_nodeconstraints, ref fe_edgeconstraints, ref fe_loads, 
                ref fe_materials, ref materialids, ref nodePtsList, ref isModelSet);


            if (isModelSet == false)
                return;

            // Set the mesh boundaries
            Vector3 geometry_center = gvariables_static.FindGeometricCenter(nodePtsList);
            Tuple<Vector3, Vector3> geom_extremes = gvariables_static.FindMinMaxXY(nodePtsList);


            // Set the geometry bounds
            this.min_bounds = geom_extremes.Item1; // Minimum bound
            this.max_bounds = geom_extremes.Item2; // Maximum bound

            this.geom_bounds = max_bounds - min_bounds;

            // update the global static value
            gvariables_static.geom_size = this.geom_bounds.Length;


            // Create the mesh for drawing
            meshdata = new meshdata_store(false);

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

            // Material labels
            materiallabels = new label_list_store();
            materiallabels.set_shader();
            // Update the material labels
            updateMaterialIDLabels();
            materiallabels.update_openTK_uniforms(true, true, true, graphic_events_control);

            // Set the openTK buffer
            meshdata.set_shader();
            meshdata.set_buffer();

            fe_nodeconstraints.set_shader();
            fe_edgeconstraints.set_shader();

            // Set the shader of selection rectangle and circle
            selection_rectangle.set_shader();
            selection_circle.set_shader();

            // Set the buffer of selection rectangle and circle
            selection_rectangle.set_buffer();
            selection_circle.set_buffer();


            // Update the openGL uniform
            meshdata.update_openTK_uniforms(true, true, true, graphic_events_control.projectionMatrix,
                graphic_events_control.modelMatrix, graphic_events_control.viewMatrix,
                gvariables_static.geom_transparency);


            fe_nodeconstraints.update_openTK_uniforms(true, true, true, graphic_events_control);

            fe_edgeconstraints.update_openTK_uniforms(true, true, true, graphic_events_control);


        }


        public void importBINFile(string filePath)
        {
            List<Vector3> nodePtsList = new List<Vector3>();
            isModelSet = false;
            isResultSet = false;

            file_events.import_binary_mesh(filePath, ref fe_nodes, ref fe_tris, ref fe_quads,
                ref fe_nodeconstraints, ref fe_edgeconstraints, ref fe_loads,
                ref fe_materials, ref materialids, ref nodePtsList, ref isModelSet);


            if (isModelSet == false)
                return;

            // Set the mesh boundaries
            Vector3 geometry_center = gvariables_static.FindGeometricCenter(nodePtsList);
            Tuple<Vector3, Vector3> geom_extremes = gvariables_static.FindMinMaxXY(nodePtsList);


            // Set the geometry bounds
            this.min_bounds = geom_extremes.Item1; // Minimum bound
            this.max_bounds = geom_extremes.Item2; // Maximum bound

            this.geom_bounds = max_bounds - min_bounds;

            // update the global static value
            gvariables_static.geom_size = this.geom_bounds.Length;


            // Create the mesh for drawing
            meshdata = new meshdata_store(false);

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

            // Material labels
            materiallabels = new label_list_store();
            materiallabels.set_shader();
            // Update the material labels
            updateMaterialIDLabels();
            materiallabels.update_openTK_uniforms(true, true, true, graphic_events_control);

            // Set the openTK buffer
            meshdata.set_shader();
            meshdata.set_buffer();

            //fe_nodeconstraints.set_shader();
            //fe_edgeconstraints.set_shader();

            //fe_nodeconstraints.ndcnst_meshdata.set_buffer();
            //fe_nodeconstraints.ndcnst_label.set_buffer();

            // Set the shader of selection rectangle and circle
            selection_rectangle.set_shader();
            selection_circle.set_shader();

            // Set the buffer of selection rectangle and circle
            selection_rectangle.set_buffer();
            selection_circle.set_buffer();


            // Update the openGL uniform
            meshdata.update_openTK_uniforms(true, true, true, graphic_events_control.projectionMatrix,
                graphic_events_control.modelMatrix, graphic_events_control.viewMatrix,
                gvariables_static.geom_transparency);


            fe_nodeconstraints.update_openTK_uniforms(true, true, true, graphic_events_control);

            fe_edgeconstraints.update_openTK_uniforms(true, true, true, graphic_events_control);

        }


        public void exportBINFile(string filePath)
        {
            // Export the bindary mesh
            file_events.export_binary_mesh(filePath, fe_nodes, fe_tris, fe_quads,
              fe_nodeconstraints, fe_edgeconstraints, fe_loads, meshdata.mesh_boundaries,
              fe_materials);

        }


        public void setResultMesh()
        {

            // Create the Result mesh for drawing the results
            resultmeshdata = new meshdata_store(true);

            // Add the mesh points
            foreach (var nd_m in fe_nodes.nodeMap)
            {
                node_store nd = nd_m.Value;

                resultmeshdata.add_mesh_point(nd.node_id, nd.node_pt_x_coord, nd.node_pt_y_coord, nd.node_pt_z_coord, -1);
               

            }

            // Add the mesh tris
            foreach (var tri_m in fe_tris.elementtriMap)
            {
                elementtri_store tri = tri_m.Value;

                resultmeshdata.add_mesh_tris(tri.tri_id, tri.nodeid1, tri.nodeid2, tri.nodeid3, tri.material_id);

            }

            // Add the mesh quads
            foreach (var quad_m in fe_quads.elementquadMap)
            {
                elementquad_store quad = quad_m.Value;

                resultmeshdata.add_mesh_quads(quad.quad_id, quad.nodeid1, quad.nodeid2, quad.nodeid3, quad.nodeid4, quad.material_id);

            }

            // Create the mesh boundaries
            resultmeshdata.set_mesh_wireframe();


            // Set the openTK buffer
            resultmeshdata.set_shader();
            resultmeshdata.set_buffer();

            // Update the openGL uniform
            resultmeshdata.update_openTK_uniforms(true, true, true, graphic_events_control.projectionMatrix,
                graphic_events_control.modelMatrix, graphic_events_control.viewMatrix,
                gvariables_static.rslt_transparency);



        }

        public void setResultExtremes()
        {
            // Set the result extremes
            // Initialize
            result_extremes.u_real_min = result_extremes.u_imag_min =
                result_extremes.u_magnitude_min = result_extremes.u_phase_min = double.MaxValue;

            result_extremes.u_real_max = result_extremes.u_imag_max =
                result_extremes.u_magnitude_max = result_extremes.u_phase_max = double.MinValue;


            foreach (var nd in fe_nodes.nodeMap.Values)
            {
                result_extremes.u_real_min = Math.Min(result_extremes.u_real_min, nd.node_u_real);
                result_extremes.u_real_max = Math.Max(result_extremes.u_real_max, nd.node_u_real);

                result_extremes.u_imag_min = Math.Min(result_extremes.u_imag_min, nd.node_u_imag);
                result_extremes.u_imag_max = Math.Max(result_extremes.u_imag_max, nd.node_u_imag);

                result_extremes.u_magnitude_min = Math.Min(result_extremes.u_magnitude_min, nd.node_u_magnitude);
                result_extremes.u_magnitude_max = Math.Max(result_extremes.u_magnitude_max, nd.node_u_magnitude);

                result_extremes.u_phase_min = Math.Min(result_extremes.u_phase_min, nd.node_u_phase);
                result_extremes.u_phase_max = Math.Max(result_extremes.u_phase_max, nd.node_u_phase);
            }

        }

        public void updateResultType()
        {
            // Helper function for normalization
            double Normalize(double value, double min, double max)
            {
                double range = max - min;
                if (Math.Abs(range) < 1e-12)  // Prevent division by zero
                    return 0.5; // Or 0.0 depending on what makes sense visually
                return (value - min) / range;
            }

            void UpdateMeshValues(Func<node_store, double> valueSelector, double min, double max)
            {
                foreach (var nd in fe_nodes.nodeMap.Values)
                {
                    double normalized = Normalize(valueSelector(nd), min, max);
                    resultmeshdata.update_mesh_point(
                        nd.node_id,
                        nd.node_pt_x_coord,
                        nd.node_pt_y_coord,
                        nd.node_pt_z_coord,
                        normalized
                    );
                }
            }

            // U real
            if (gvariables_static.is_paint_ureal)
            {
                UpdateMeshValues(
                    nd => nd.node_u_real,
                    result_extremes.u_real_min,
                    result_extremes.u_real_max
                );
            }

            // U imaginary
            if (gvariables_static.is_paint_uimag)
            {
                UpdateMeshValues(
                    nd => nd.node_u_imag,
                    result_extremes.u_imag_min,
                    result_extremes.u_imag_max
                );
            }

            // U magnitude
            if (gvariables_static.is_paint_umagnitude)
            {
                UpdateMeshValues(
                    nd => nd.node_u_magnitude,
                    result_extremes.u_magnitude_min,
                    result_extremes.u_magnitude_max
                );
            }

            // U phase
            if (gvariables_static.is_paint_uphase)
            {
                UpdateMeshValues(
                    nd => nd.node_u_phase,
                    result_extremes.u_phase_min,
                    result_extremes.u_phase_max
                );
            }

            // Update the buffers once at the end
            resultmeshdata.update_buffer();

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
                fe_nodeconstraints.paint_node_constraint();
                fe_edgeconstraints.paint_edge_constraint();

            }

            // Paint the constraints labels
            if(gvariables_static.is_paint_constraints_label == true)
            {
                fe_nodeconstraints.paint_node_constraint_label();
                fe_edgeconstraints.paint_edge_constraint_label();

            }



            if (isMaterialUpdateInProgress == true || isLoadUpdateInProgress == true || isNodalConstraintUpdateInProgress == true
                || isEdgeConstraintUpdateInProgress == true)
            {

                // Paint the selected meshes and point
                if (isLoadUpdateInProgress == true || isNodalConstraintUpdateInProgress == true)
                {
                    meshdata.paint_selected_points();

                }


                if (isMaterialUpdateInProgress == true)
                {
                    meshdata.paint_selected_mesh();

                    // Paint the material labels
                    materiallabels.paint_static_labels();

                }

                if(isEdgeConstraintUpdateInProgress == true)
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



            if(isResultSet == true)
            {
                // Paint the results
                if(gvariables_static.is_paint_ureal == true || 
                    gvariables_static.is_paint_uimag == true ||
                    gvariables_static.is_paint_umagnitude == true ||
                    gvariables_static.is_paint_uphase == true)
                {
                    // Paint the static mesh
                    resultmeshdata.paint_static_mesh();

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
                gvariables_static.geom_transparency);

            materiallabels.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                graphic_events_control);

            fe_nodeconstraints.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                graphic_events_control);

            fe_edgeconstraints.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                graphic_events_control);


            if(isResultSet == true)
            {
                resultmeshdata.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                graphic_events_control.projectionMatrix,
                graphic_events_control.modelMatrix,
                graphic_events_control.viewMatrix,
                gvariables_static.rslt_transparency);

            }
            
        }



        public void select_mesh_objects(Vector2 o_pt, Vector2 c_pt, bool isRightButton)
        {
            // Perform the select option
            if (isMaterialUpdateInProgress == true)
            {
                meshdata.select_mesh_elements(o_pt, c_pt, isRightButton, graphic_events_control);

            }

            if (isNodalConstraintUpdateInProgress == true || isLoadUpdateInProgress == true)
            {
                // Select the points for load or constraint update
                meshdata.select_mesh_points(o_pt, c_pt, isRightButton, graphic_events_control);

            }

            if (isEdgeConstraintUpdateInProgress == true)
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

            // Update the material labels
            // updateMaterialIDLabels();


        }


        public void updateMaterialIDLabels()
        {

            Vector2 geom_center = new Vector2((max_bounds.X - min_bounds.X) * 0.5f,
                (max_bounds.Y - min_bounds.Y) * 0.5f);
            float label_height = gvariables_static.get_text_height(12.0f) * 1.25f;

            // Clear the labels
            materiallabels.clear_labels();
            int k = 0;
            foreach (int i in materialids)
            {
                // Material id
                material_data mat = fe_materials[i];
                double wave_velocity = 1.0 / Math.Sqrt(mat.material_permittivity * mat.material_permeability * Math.Pow(10, -3));
                string materiallabel = $"Medium name = {mat.material_name}, velocity = {wave_velocity.ToString("F4")} x 10^8";

                materiallabels.add_label(i, materiallabel, new Vector2(geom_center.X , geom_center.Y + (k * label_height)), 
                    mat.material_id);

                k++;
            }

            materiallabels.set_buffer();


        }




    }
}
