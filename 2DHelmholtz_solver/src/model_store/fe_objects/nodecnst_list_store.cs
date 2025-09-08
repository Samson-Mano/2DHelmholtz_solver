using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2DHelmholtz_solver.src.model_store.fe_objects
{

    public class nodecnst_data
    {
        public int cnst_node_id { get; set; }
        public int constraint_type { get; set; }

    }


    public class nodecnst_list_store
    {
        public Dictionary<int, nodecnst_data> ndcnstMap = new Dictionary<int, nodecnst_data>();
        public int ndcnst_count = 0;

        public nodecnst_list_store()
        {
            // (Re)Initialize the data
            ndcnstMap = new Dictionary<int, nodecnst_data>();
            ndcnst_count = 0;

        }


        public void add_nodeconstraint(int node_id, int constraint_type)
        {
            // Add the constraint to the particular node
            nodecnst_data temp_cnst = new nodecnst_data
            {
                cnst_node_id = node_id,
                constraint_type = constraint_type
            };

            // Insert the constraint to nodes
            ndcnstMap[node_id] = temp_cnst;
            ndcnst_count++;

        }


        public void delete_nodeconstraint(int node_id)
        {
            // Delete the constraint in this node
            if(ndcnst_count != 0)
            {
                if (ndcnstMap.ContainsKey(node_id))
                {
                    // Node is already have constraint data
                    // so remove the constraint data
                    ndcnstMap.Remove(node_id);

                    // adjust the constraint data count
                    ndcnst_count--;
                }

            }
        }


    }
}
