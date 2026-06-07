using _2DHelmholtz_solver.global_variables;
using _2DHelmholtz_solver.src.model_store.fe_objects;
using _2DHelmholtz_solver.src.model_store.geom_objects;
using OpenTK;
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

        public double node_u_real { get; set; }

        public double node_u_imag { get; set; }

        public double node_u_magnitude { get; set; }

        public double node_u_phase { get; set; }

    }


    public class rsltedge_store
    {
        public int startnode { get; set; }

        public int endnode { get; set; }
    }


    public class rslttri_store
    {
        public int tri_node1 { get; set; }

        public int tri_node2 { get; set; }

        public int tri_node3 { get; set; }

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


    public class rsltdata_store
    {

        public Dictionary<int, rsltnode_store> rslt_nodes;
        public List<rsltedge_store> rslt_edges;
        public List<rslttri_store> rslt_tris;

        public meshdata_store rsltmeshdata;

        public result_data_extremes result_extremes;


        public bool isResultSet = false;


        public rsltdata_store()
        {
            rslt_nodes = new Dictionary<int, rsltnode_store>();
            rslt_edges = new List<rsltedge_store>();
            rslt_tris = new List<rslttri_store>();

            isResultSet = false;

        }


        public void setResultMesh()
        {
            // Create the Result mesh for drawing the results
            rsltmeshdata = new meshdata_store(true);

            // Add the mesh points
            foreach (var r_nd_m in rslt_nodes)
            {
                rsltnode_store r_nd = r_nd_m.Value;

                rsltmeshdata.add_mesh_point(r_nd.node_id,
                    r_nd.node_pt_x_coord,
                    r_nd.node_pt_y_coord, 0.0, -1);


            }

            // Add the mesh tris
            int tri_id = 0;
            foreach (rslttri_store r_tri in rslt_tris)
            {

                rsltmeshdata.add_mesh_tris(tri_id,
                    r_tri.tri_node1, r_tri.tri_node2, r_tri.tri_node3, 0);
                tri_id++;

            }

            // Create the mesh boundaries
            rsltmeshdata.set_mesh_wireframe();


            // Set the openTK buffer
            rsltmeshdata.set_shader();
            rsltmeshdata.set_buffer();

        }


        public void setResultExtremes()
        {
            // Set the result extremes
            // Initialize
            result_extremes.u_real_min = result_extremes.u_imag_min =
                result_extremes.u_magnitude_min = result_extremes.u_phase_min = double.MaxValue;

            result_extremes.u_real_max = result_extremes.u_imag_max =
                result_extremes.u_magnitude_max = result_extremes.u_phase_max = double.MinValue;


            foreach (var nd in rslt_nodes.Values)
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
                double maxAbs = Math.Max(Math.Abs(max), Math.Abs(min));

                if (Math.Abs(maxAbs) < 1e-12)  // Prevent division by zero
                    return 0.0; // Or 0.0 depending on what makes sense visually
                return value / maxAbs;
            }

            void UpdateMeshValues(Func<rsltnode_store, double> valueSelector, double min, double max)
            {
                foreach (var nd in rslt_nodes.Values)
                {
                    double normalized = Normalize(valueSelector(nd), min, max);
                    rsltmeshdata.update_mesh_point(
                        nd.node_id,
                        nd.node_pt_x_coord,
                        nd.node_pt_y_coord,
                        0.0,
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
            rsltmeshdata.update_buffer();

        }


        public void paint_result_mesh()
        {
            if (isResultSet == true)
            {
         
                rsltmeshdata.paint_static_mesh();

                rsltmeshdata.paint_static_mesh_boundaries();

            }
        }


        public void update_openTK_uniforms(Matrix4 projectionMatrix, Matrix4 modelMatrix, Matrix4 viewMatrix, float geom_transparency)
        {
            if (isResultSet == true)
            {
                rsltmeshdata.update_openTK_uniforms(projectionMatrix,
                    modelMatrix,
                    viewMatrix,
                    gvariables_static.rslt_transparency);


            }

        }

        //
    }
}
