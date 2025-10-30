using _2DHelmholtz_solver.global_variables;
using _2DHelmholtz_solver.src.model_store.geom_objects;
using _2DHelmholtz_solver.src.opentk_control.opentk_bgdraw;
using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2DHelmholtz_solver.src.model_store.fe_objects
{

    public class nodecnst_data
    {
        public int ndcnst_id { get; set; } // constraint id

        public List<Vector3> constraint_node_pts { get; set; }

        public List<int> constraint_node_ids { get; set; }

        public double field_value { get; set; } // Dirichlet boundary condition

        public double source_value { get; set; } // Source/ External excitation 

        public bool isField { get; set; } // is Field value

    }


    public class nodecnst_list_store
    {
        public Dictionary<int, nodecnst_data> ndcnstMap = new Dictionary<int, nodecnst_data>();
        public int ndcnst_count = 0;

        private List<int> all_constraintset_ids = new List<int>();

        // Constraint visualization
        private meshdata_store ndcnst_meshdata;
        // Add labels for the constraint
        private label_list_store ndcnst_label;


        public nodecnst_list_store()
        {
            // (Re)Initialize the data
            ndcnstMap = new Dictionary<int, nodecnst_data>();
            ndcnst_count = 0;

            ndcnst_meshdata = new meshdata_store(false);
            ndcnst_label = new label_list_store();

        }


        public void add_nodeconstraint(List<int> constraint_node_ids, List<Vector3> constraint_node_pts,
            double field_value, double source_value, bool isField)
        {
            // Get an unique constraint set id
            int unique_constraintset_id = gvariables_static.get_unique_id(all_constraintset_ids);

            // Make a copy of the list
            List<int> idsCopy = new List<int>(constraint_node_ids);
            List<Vector3> nodePtsCopy = new List<Vector3>(constraint_node_pts);

            // Add the constraint to the particular node
            nodecnst_data temp_cnst = new nodecnst_data
            {
                ndcnst_id = unique_constraintset_id,
                constraint_node_pts = nodePtsCopy,
                constraint_node_ids = idsCopy,
                field_value = isField == true ? field_value : 0.0,
                source_value = isField == true ? 0.0 : source_value,
                isField = isField
            };

            // Insert the constraint to nodes
            ndcnstMap[unique_constraintset_id] = temp_cnst;
            ndcnst_count++;

            // Set the constraint data visualization
            set_constraint_visualization(unique_constraintset_id, true);

            // Add the constraint set id to list to track the unique constraint set id
            all_constraintset_ids.Add(unique_constraintset_id);

        }

        public void delete_nodeconstraint(int cnst_id)
        {
            // Remove the constraint set ID from all_constraintset_ids
            all_constraintset_ids.Remove(cnst_id);

            // Set the constraint data visualization
            set_constraint_visualization(cnst_id, false);

            // Remove the constraint data based on the key (constraint id)
            ndcnstMap.Remove(cnst_id);

            // adjust the constraint data count
            ndcnst_count--;
        }


        private void set_constraint_visualization(int ndcnst_id, bool isAdd)
        {
            // Get the constraint
            nodecnst_data cnstraint = ndcnstMap[ndcnst_id];

            if (isAdd == true)
            {
                // Add visualization for this constraint id
                int i = 0;
                int color_id = cnstraint.isField == true ? -3: -4;

                foreach (Vector3 node_pts in cnstraint.constraint_node_pts)
                {
                    int cnst_node_id = cnstraint.constraint_node_ids[i];

                    int ndid1 = (cnst_node_id * 4) + 0;
                    int ndid2 = (cnst_node_id * 4) + 1;
                    int ndid3 = (cnst_node_id * 4) + 2;
                    int ndid4 = (cnst_node_id * 4) + 3;

                    int lnid1 = (cnst_node_id * 2) + 0;
                    int lnid2 = (cnst_node_id * 2) + 1;

                    float constraint_size = gvariables_static.geom_size * 0.0025f;

                    ndcnst_meshdata.add_mesh_point(ndid1,
                        node_pts.X + constraint_size, node_pts.Y + constraint_size, node_pts.Z, -1);
                    ndcnst_meshdata.add_mesh_point(ndid2,
                        node_pts.X - constraint_size, node_pts.Y + constraint_size, node_pts.Z, -1);
                    ndcnst_meshdata.add_mesh_point(ndid3,
                        node_pts.X - constraint_size, node_pts.Y - constraint_size, node_pts.Z, -1);
                    ndcnst_meshdata.add_mesh_point(ndid4,
                        node_pts.X + constraint_size, node_pts.Y - constraint_size, node_pts.Z, -1);

                    ndcnst_meshdata.add_mesh_lines(lnid1, ndid1, ndid3, color_id);
                    ndcnst_meshdata.add_mesh_lines(lnid2, ndid2, ndid4, color_id);

                    i++;
                }

                // Add labels
                int mid_index = cnstraint.constraint_node_pts.Count / 2;
                string label_string1 = $"Nodal Constraint {ndcnst_id}";
                string label_string2 = "";
               

                if(cnstraint.isField == true)
                {
                    label_string2 = $"Field value = {cnstraint.field_value}";
                }
                else
                {
                    label_string2 = $"Source value = {cnstraint.source_value}";
                }

                float label_ht =  gvariables_static.get_text_height(12.0f) * 1.25f;

                Vector2 label_loc1 = new Vector2(cnstraint.constraint_node_pts[mid_index].X,
                        cnstraint.constraint_node_pts[mid_index].Y);
                Vector2 label_loc2 = new Vector2(cnstraint.constraint_node_pts[mid_index].X,
                        cnstraint.constraint_node_pts[mid_index].Y - label_ht);

                ndcnst_label.add_label((ndcnst_id * 2) + 0, label_string1, label_loc1, color_id);
                ndcnst_label.add_label((ndcnst_id * 2) + 1, label_string2, label_loc2, color_id);

            }
            else
            {
                // Delete visualization for this constraint if
                foreach (int cnst_node_id in cnstraint.constraint_node_ids)
                {

                    int ndid1 = (cnst_node_id * 4) + 0;
                    int ndid2 = (cnst_node_id * 4) + 1;
                    int ndid3 = (cnst_node_id * 4) + 2;
                    int ndid4 = (cnst_node_id * 4) + 3;

                    int lnid1 = (cnst_node_id * 2) + 0;
                    int lnid2 = (cnst_node_id * 2) + 1;


                    // Delete mesh line
                    ndcnst_meshdata.delete_mesh_line(lnid1);
                    ndcnst_meshdata.delete_mesh_line(lnid2);

                    // Delete mesh point
                    ndcnst_meshdata.delete_mesh_point(ndid1);
                    ndcnst_meshdata.delete_mesh_point(ndid2);
                    ndcnst_meshdata.delete_mesh_point(ndid3);
                    ndcnst_meshdata.delete_mesh_point(ndid4);

                }

                // Delete labels
                ndcnst_label.delete_label((ndcnst_id * 2) + 0);
                ndcnst_label.delete_label((ndcnst_id * 2) + 1);

            }

            //cnst_meshdata.set_shader();
            ndcnst_meshdata.set_buffer();
            ndcnst_label.set_buffer();

        }

        public void set_shader()
        {
            // Set the shader 
            ndcnst_meshdata.set_shader();
            ndcnst_label.set_shader();

        }


        public void paint_node_constraint()
        {
            // node constraint count check
            if (ndcnst_count == 0)
                return;

            // Paint the constraint label
            gvariables_static.LineWidth = 3.0f;
            ndcnst_meshdata.paint_static_mesh_lines();
            gvariables_static.LineWidth = 1.0f;

        }


        public void paint_node_constraint_label()
        {
            // node constraint count check
            if (ndcnst_count == 0)
                return;

            ndcnst_label.paint_static_labels();

        }


        public void update_openTK_uniforms(bool set_modelmatrix, bool set_viewmatrix, bool set_transparency,
            drawing_events graphic_events_control)
        {
            if (ndcnst_count == 0)
                return;


            ndcnst_meshdata.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                graphic_events_control.projectionMatrix,
                graphic_events_control.modelMatrix,
                graphic_events_control.viewMatrix,
                gvariables_static.geom_transparency);


            ndcnst_label.update_openTK_uniforms(set_modelmatrix, set_viewmatrix, set_transparency,
                graphic_events_control);

       
        }


        //public void delete_nodeconstraint(int node_id)
        //{

        //    if (ndcnst_count == 0)
        //        return;

        //    // Delete constraints for all the nodes
        //    List<int> delete_cnst_keys = new List<int>();

        //    foreach (var cnst_m in ndcnstMap)
        //    {
        //        var cnst = cnst_m.Value;

        //        // Check whether the constraint's nodeID has the delete nodeID
        //        if (cnst.constraint_node_ids.Contains(node_id))
        //        {
        //            delete_cnst_keys.Add(cnst_m.Key);

        //            // Remove the constraint set ID from all_constraintset_ids
        //            all_constraintset_ids.Remove(cnst_m.Key);
        //        }
        //    }

        //    // Iterate over the delete indices vector and erase constraints from the original vector
        //    foreach (int key in delete_cnst_keys)
        //    {
        //        // Remove the constraint data based on the key
        //        ndcnstMap.Remove(key);

        //        // adjust the constraint data count
        //        ndcnst_count--;

        //    }

        //}


    }
}
