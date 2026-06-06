using _2DHelmholtz_solver.global_variables;
using _2DHelmholtz_solver.src.model_store.geom_objects;
using OpenTK;
using SharpFont.Cache;
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



        // Animation control data
        public System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        //private double accumulatedTime = 0.0;
        //private int time_step = 0;




        public modal_rsltdata_store()
        {
            modal_rslt_nodes = new Dictionary<int, modal_rsltnode_store>();
            rslt_edges = new List<rsltedge_store>();
            rslt_tris = new List<rslttri_store>();

            modes = new List<modeInfo>();

            natural_Frequencies = new List<double>();

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

            if (isModalResultSet == false)
                return;

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

            Dictionary<int, double> mode_results = new Dictionary<int, double>();
            double mode_max = Double.MinValue;
            double mode_min = Double.MaxValue;

            // Read mode shape and add to dictionary
            for (int i = 0; i < modal_rslt_nodes.Count; i++)
            {
                int nodeId = _reader.ReadInt32();
                double modal_rslt_value = _reader.ReadDouble();

                mode_results[nodeId] = modal_rslt_value;

                mode_max = Math.Max(mode_max, modal_rslt_value);
                mode_min = Math.Min(mode_min, modal_rslt_value);

            }

            // Normalize to [-1, 1] range (symmetric around zero)
            double maxAbs = Math.Max(Math.Abs(mode_max), Math.Abs(mode_min));

            // Normalze the result and add to mesh
            foreach (var md_rslt in mode_results)
            {
                // Normalize to -1..1
                double normalized = maxAbs > 1e-12 ? md_rslt.Value / maxAbs : 0.0;
                double val = (normalized + 1.0) / 2.0;

                modal_rsltnode_store rslt_nd = modal_rslt_nodes[md_rslt.Key];

                modal_rsltmeshdata.update_mesh_point(md_rslt.Key,
                         rslt_nd.node_pt_x_coord,
                         rslt_nd.node_pt_y_coord, 0.0, val);

            }

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



        public void start_animation()
        {
            // Restart the animation stopwatch
            stopwatch.Start();

        }


        public void pause_animation()
        {
            // Pause the animation
            stopwatch.Stop();

        }

        public void stop_animation()
        {

            // Reset the animation stopwatch and time step
            stopwatch.Reset();
            stopwatch.Stop();

            // pendulum_data.reset_simulation();

        }



        public void update_modal_animation()
        {
            if (!isModalResultSet || !gvariables_static.is_paint_modalresults)
                return;


            // Results are stored, animate the modal results
            double elapsedRealTime = stopwatch.Elapsed.TotalSeconds;


            if (gvariables_static.animate_play == true)
            {

                // double animscale = Math.Cos(Math.PI * elapsedRealTime * gvariables_static.modal_animation_speed);

                // float sinevalue = (float)(1.0f + animscale) * 0.5f;

                // modal_rsltmeshdata.updateAnimation(sinevalue);


                // Oscillation: -1 to 1
                float oscillation = (float)Math.Sin(2.0 * Math.PI * elapsedRealTime * gvariables_static.modal_animation_speed);

                modal_rsltmeshdata.updateAnimation((oscillation + 1.0f) * 0.5f);

                //if (isModalAnalysisPaint == true)
                //{
                //    double displ_scale = displextent * gvariables_static.displacement_scale;

                //    // Update the modal analysis results
                //    stringlinemodalresults_data.update_modalresults_time_step(elapsedRealTime, selected_mode_shape, displ_scale);
                //}


                // pendulum_data.simulate(elapsedRealTime);

            }


            //
        }


        public void update_openTK_uniforms(Matrix4 projectionMatrix, Matrix4 modelMatrix, Matrix4 viewMatrix, float geom_transparency)
        {
            if (isModalResultSet == true)
            {
                modal_rsltmeshdata.update_openTK_uniforms(projectionMatrix,
                    modelMatrix,
                    viewMatrix,
                    gvariables_static.rslt_transparency);


            }

        }

        //


    }
}
