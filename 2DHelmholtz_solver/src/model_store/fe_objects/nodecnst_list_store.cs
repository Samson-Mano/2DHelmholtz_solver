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
        public int cnst_id { get; set; } // constraint id

        public List<int> constraint_node_ids { get; set; }

        public double field_value { get; set; } // Dirichlet boundary condition

        public double source_value { get; set; } // Source/ External excitation 

    }


    public class nodecnst_list_store
    {
        public Dictionary<int, nodecnst_data> ndcnstMap = new Dictionary<int, nodecnst_data>();
        public int ndcnst_count = 0;

        private List<int> all_constraintset_ids = new List<int>();


        public nodecnst_list_store()
        {
            // (Re)Initialize the data
            ndcnstMap = new Dictionary<int, nodecnst_data>();
            ndcnst_count = 0;

        }


        public void add_nodeconstraint(List<int> constraint_node_ids, double field_value, double source_value)
        {
            // Get an unique constraint set id
            int unique_constraintset_id = global_variables.gvariables_static.get_unique_id(all_constraintset_ids);

            // Make a copy of the list
            List<int> idsCopy = new List<int>(constraint_node_ids);

            // Add the constraint to the particular node
            nodecnst_data temp_cnst = new nodecnst_data
            {
                cnst_id = unique_constraintset_id,
                constraint_node_ids = idsCopy,
                field_value = field_value,
                source_value = source_value
            };

            // Insert the constraint to nodes
            ndcnstMap[unique_constraintset_id] = temp_cnst;
            ndcnst_count++;

            // Add the constraint set id to list to track the unique constraint set id
            all_constraintset_ids.Add(unique_constraintset_id);

        }

        public void delete_nodeconstraint(int cnst_id)
        {
            // Remove the constraint set ID from all_constraintset_ids
            all_constraintset_ids.Remove(cnst_id);

            // Remove the constraint data based on the key (constraint id)
            ndcnstMap.Remove(cnst_id);

            // adjust the constraint data count
            ndcnst_count--;
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
