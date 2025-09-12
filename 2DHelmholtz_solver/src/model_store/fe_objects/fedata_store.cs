using _2DHelmholtz_solver.src.events_handler;
using _2DHelmholtz_solver.src.model_store.geom_objects;
using _2DHelmholtz_solver.src.opentk_control.opentk_bgdraw;
using OpenTK;
using OpenTK.Graphics.ES11;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2DHelmholtz_solver.src.model_store.fe_objects
{

    public class material_data
    {
        public int material_id = 0;
        public string material_name = "";

        public double material_density = 0.0;
        public double material_youngsmodulus = 0.0; // E
        public double material_shearmodulus = 0.0; // G
        public double shell_thickness = 0.0; // t
        public double poissons_ratio = 0.0; // mu

    }




    public class fedata_store
    {
        public node_list_store fe_nodes;
        public elementtri_list_store fe_tris;
        public elementquad_list_store fe_quads;

        public nodecnst_list_store fe_constraints;
        public nodeload_list_store fe_loads;

        public Dictionary<int, material_data> fe_materials;

        public meshdata_store meshdata;



        public fedata_store()
        {
            // (Re)Initialize the data
            fe_nodes = new node_list_store();
            fe_tris = new elementtri_list_store();
            fe_quads = new elementquad_list_store();

            fe_constraints = new nodecnst_list_store();
            fe_loads = new nodeload_list_store();

            fe_materials = new Dictionary<int, material_data>();

            meshdata = new meshdata_store();

        }

        public void importMesh(string fileContent)
        {
            List<Vector3> nodePtsList = new List<Vector3>();
            bool isModelLoadSuccess = false;

            file_events.import_mesh(fileContent,ref fe_nodes, ref fe_tris, ref fe_quads,
                ref fe_constraints, ref fe_loads,ref fe_materials,ref nodePtsList,ref isModelLoadSuccess);


            if (isModelLoadSuccess == false)
                return;


            // Create the mesh for drawing
            meshdata = new meshdata_store();







        }


    }
}
