using _2DHelmholtz_solver.global_variables;
using _2DHelmholtz_solver.src.model_store.geom_objects;
using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2DHelmholtz_solver.src.model_store.rslt_objects
{

    public class modal_rsltnode_store
    {
        public int node_id { get; set; }
        public double node_pt_x_coord { get; set; }
        public double node_pt_y_coord { get; set; }

       // public List<double> node_modal_displ_magnitude { get; set; }
    }

    public class modeInfo
    {
        public int Id { get; set; }
        public double Frequency { get; set; }
        public long FileOffset { get; set; }
        public long DataSize { get; set; }
    }


    public class modal_rsltdata_store
    {

        public Dictionary<int, modal_rsltnode_store> modal_rslt_nodes;
        public List<rsltedge_store> rslt_edges;
        public List<rslttri_store> rslt_tris;

        private FileStream _fileStream;
        private BinaryReader _reader;
        public List<modeInfo> modes;


        public List<double> natural_Frequencies;

        public meshdata_store modal_rsltmeshdata;

        public bool isModalResultSet = false;


        public modal_rsltdata_store()
        {
            modal_rslt_nodes = new Dictionary<int, modal_rsltnode_store>();
            rslt_edges = new List<rsltedge_store>();
            rslt_tris = new List<rslttri_store>();

            isModalResultSet = false;

        }

        public void setResultMesh()
        {
            // Create the Result mesh for drawing the results
            modal_rsltmeshdata = new meshdata_store(true);

            // Add the mesh points
            foreach (var r_nd_m in modal_rslt_nodes)
            {
                modal_rsltnode_store r_nd = r_nd_m.Value;

                modal_rsltmeshdata.add_mesh_point(r_nd.node_id,
                    r_nd.node_pt_x_coord,
                    r_nd.node_pt_y_coord, 0.0, -1);


            }

            // Add the mesh tris
            int tri_id = 0;
            foreach (rslttri_store r_tri in rslt_tris)
            {

                modal_rsltmeshdata.add_mesh_tris(tri_id,
                    r_tri.tri_node1, r_tri.tri_node2, r_tri.tri_node3, 0);
                tri_id++;

            }

            // Create the mesh boundaries
            modal_rsltmeshdata.set_mesh_wireframe();


            // Set the openTK buffer
            modal_rsltmeshdata.set_shader();
            modal_rsltmeshdata.set_buffer();


        }


        public void updateSelectedMode(int selected_mode)
        {

            if (selected_mode < 0 || selected_mode >= this.modes.Count())
                return;

            // Get the current mode
            modeInfo currentmode = this.modes[selected_mode];

            string outputPath = Path.Combine(System.Windows.Forms.Application.StartupPath, "modal_analysis_output.bin");

            // Open file if not already open
            if (_fileStream == null)
            {
                _fileStream = new FileStream(outputPath, FileMode.Open, FileAccess.Read);
                _reader = new BinaryReader(_fileStream);
            }

            // Seek to mode data
            _fileStream.Seek(currentmode.FileOffset, SeekOrigin.Begin);

            // Read mode ID (redundant check)
            int readModeId = _reader.ReadInt32();

            if (readModeId != selected_mode)
                Console.WriteLine($"Warning: Expected mode {selected_mode}, found {readModeId}");

            // Read mode shape
            for (int i = 0; i < modal_rslt_nodes.Count; i++)
            {
                int nodeId = _reader.ReadInt32();
                double modal_rslt_value = _reader.ReadDouble();

                modal_rsltnode_store rslt_nd = modal_rslt_nodes[nodeId];

                modal_rsltmeshdata.update_mesh_point(nodeId,
                         rslt_nd.node_pt_x_coord,
                         rslt_nd.node_pt_y_coord, 0.0, modal_rslt_value);

               //  modeShape[i] = value;
            }

            // _currentModeId = modeId;
            // _currentModeShape = modeShape;

            // return modeShape;



            //// Add the mesh points
            //foreach (var r_nd_m in modal_rslt_nodes)
            //{
            //    modal_rsltnode_store r_nd = r_nd_m.Value;

            //    double modal_rslt_value = r_nd.node_modal_displ_magnitude[selected_mode];

            //    modal_rsltmeshdata.update_mesh_point(r_nd.node_id,
            //        r_nd.node_pt_x_coord,
            //        r_nd.node_pt_y_coord, 0.0, modal_rslt_value);


            //}


            // Update the buffers once at the end
            modal_rsltmeshdata.update_buffer();

        }




        public void paint_modalresult_mesh()
        {
            if (isModalResultSet == true)
            {


                modal_rsltmeshdata.paint_static_mesh();

                modal_rsltmeshdata.paint_static_mesh_boundaries();

            }
        }


        public void update_modal_animation(double animation_time)
        {


        }


        public void update_openTK_uniforms(bool set_modelmatrix, bool set_viewmatrix, bool set_transparency,
           Matrix4 projectionMatrix, Matrix4 modelMatrix, Matrix4 viewMatrix, float geom_transparency)
        {
            if (isModalResultSet == true)
            {
                modal_rsltmeshdata.update_openTK_uniforms(
                    set_modelmatrix,
                    set_viewmatrix,
                    set_transparency,
                    projectionMatrix,
                    modelMatrix,
                    viewMatrix,
                    gvariables_static.rslt_transparency);


            }

        }

        //


    }
}
