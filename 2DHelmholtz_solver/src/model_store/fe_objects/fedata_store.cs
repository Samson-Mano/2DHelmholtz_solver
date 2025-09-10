using _2DHelmholtz_solver.src.model_store.geom_objects;
using _2DHelmholtz_solver.src.opentk_control.opentk_bgdraw;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2DHelmholtz_solver.src.model_store.fe_objects
{
    public class fedata_store
    {
        public node_list_store fe_nodes {get; }
        public elementtri_list_store fe_tris { get; }
        public elementquad_list_store fe_quads { get; }

        public nodecnst_list_store fe_constraints { get; }
        public nodeload_list_store fe_loads { get; }

        public meshdata_store meshdata { get; }



        public fedata_store()
        {
            // (Re)Initialize the data
            fe_nodes = new node_list_store();
            fe_tris = new elementtri_list_store();
            fe_quads = new elementquad_list_store();

            fe_constraints = new nodecnst_list_store();
            fe_loads = new nodeload_list_store();

            meshdata = new meshdata_store();

        }






    }
}
