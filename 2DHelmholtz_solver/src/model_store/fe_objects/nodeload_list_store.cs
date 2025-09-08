using SharpFont.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _2DHelmholtz_solver.src.model_store.fe_objects
{
    public class nodeload_data
    {
        public int load_set_id { get; set; }
        public List<int> load_node_ids { get; set; }
        public double load_amplitude { get; set; }
        public double load_frequency { get; set; }
        public double load_phase { get; set; }

    }


    public class nodeload_list_store
    {
        public Dictionary<int, nodeload_data> loadMap = new Dictionary<int, nodeload_data>();
        public int load_count = 0;

        private List<int> all_loadset_ids = new List<int>();

        public nodeload_list_store()
        {
            // (Re)Initialize the data
            loadMap = new Dictionary<int, nodeload_data>();
            load_count = 0;

        }


        public void add_loads(List<int> load_node_ids, double load_amplitude, double load_frequency, double load_phase)
        {
            // Get an unique load set id
            int unique_loadset_id = global_variables.gvariables_static.get_unique_id(all_loadset_ids);

            // Add the Load to the list
            nodeload_data temp_load = new nodeload_data
            {
                load_set_id = unique_loadset_id,
                load_node_ids = load_node_ids,
                load_amplitude = load_amplitude,
                load_frequency = load_frequency,
                load_phase = load_phase
            };


            loadMap[unique_loadset_id] = temp_load;
            load_count++;

            // Add the load set id to list to track the unique load set id
            all_loadset_ids.Add(unique_loadset_id);

        }


        public void delete_loads(int node_id)
        {
            if (load_count == 0)
                return;

            // Delete all the loads in the node
            List<int> delete_load_keys = new List<int>();

            foreach (var load_m in loadMap)
            {
                var load = load_m.Value;

                // Check whether the load's nodeID has the delete nodeID
                if (load.load_node_ids.Contains(node_id))
                {
                    delete_load_keys.Add(load_m.Key);

                    // Remove the load set ID from all_load_ids
                    all_loadset_ids.Remove(load.load_set_id);
                }
            }

            // // Delete loads in reverse order to avoid index shifting (Not used because Dictionary is used)
            // delete_load_index.Sort();
            // delete_load_index.Reverse();

            // Iterate over the delete indices vector and erase loads from the original vector
            foreach (int key in delete_load_keys)
            {
                loadMap.Remove(key);

                // Reduce the load count
                load_count--;

            }

        }


    }
}
