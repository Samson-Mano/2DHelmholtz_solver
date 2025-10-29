using _2DHelmholtz_solver.src.model_store.fe_objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2DHelmholtz_solver.src.model_store.rslt_objects
{
    public class rsltnode_store
    {
        public int node_id { get; set; }
        public double node_pt_x_coord { get; set; }
        public double node_pt_y_coord { get; set; }
        public double node_pt_z_coord { get; set; }

        public double node_u_real { get; set; }

        public double node_u_imag { get; set; }


    }


    public class rsltnode_list_store
    {
        public Dictionary<int, rsltnode_store> rsltnodeMap = new Dictionary<int, rsltnode_store>();
        public int rsltnode_count = 0;

        public rsltnode_list_store()
        {
            // (Re)Initialize the data
            rsltnodeMap = new Dictionary<int, rsltnode_store>();
            rsltnode_count = 0;
        }

        public void add_node(int node_id, double node_pt_x_coord, double node_pt_y_coord, double node_pt_z_coord)
        {

        }


    }
}
