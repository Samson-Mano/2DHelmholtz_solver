using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2DHelmholtz_solver.src.model_store.fe_objects
{

    public class node_store
    {
        public int node_id { get; set; }
        public double node_pt_x_coord { get; set; }
        public double node_pt_y_coord { get; set; }
        public double node_pt_z_coord { get; set; }

        //  public Vector3 node_color { get; set; }

    }

    public class node_list_store
    {
        public Dictionary<int, node_store> nodeMap = new Dictionary<int, node_store>();
        public int node_count = 0;

        public node_list_store() 
        {
            // (Re)Initialize the data
            nodeMap = new Dictionary<int, node_store>();
            node_count = 0;
        }

        public void add_node(int node_id, double node_pt_x_coord, double node_pt_y_coord, double node_pt_z_coord)
        {
            // Check whether the node_id is already there
            if (nodeMap.ContainsKey(node_id))
                return;

            // Add the Node to the list
            node_store temp_node = new node_store
            {
                node_id = node_id,
                node_pt_x_coord = node_pt_x_coord,
                node_pt_y_coord = node_pt_y_coord,
                node_pt_z_coord = node_pt_z_coord
            };

            nodeMap[node_id] = temp_node;
            node_count++;

        }


        //public List<int> IsNodeSelected(Vector2 cornerPt1, Vector2 cornerPt2)
        //{
        //    int maxDim = Math.Max(geomParam.WindowWidth, geomParam.WindowHeight);
        //    var selectedNodeIndices = new List<int>();

        //    Vector2 screenCpt1 = new(
        //        2.0f * ((cornerPt1.X - (geomParam.WindowWidth * 0.5f)) / maxDim),
        //        2.0f * (((geomParam.WindowHeight * 0.5f) - cornerPt1.Y) / maxDim)
        //    );

        //    Vector2 screenCpt2 = new(
        //        2.0f * ((cornerPt2.X - (geomParam.WindowWidth * 0.5f)) / maxDim),
        //        2.0f * (((geomParam.WindowHeight * 0.5f) - cornerPt2.Y) / maxDim)
        //    );

        //    Matrix4 scalingMatrix = Matrix4.CreateScale((float)geomParam.ZoomScale);
        //    Matrix4 viewMatrix = geomParam.PanTranslation.Transposed() * geomParam.RotateTranslation * scalingMatrix;

        //    foreach (var kvp in nodeMap)
        //    {
        //        var node = kvp.Value.NodePt;
        //        Vector4 finalPosition = geomParam.ProjectionMatrix * viewMatrix * geomParam.ModelMatrix * new Vector4(node, 1.0f);

        //        if (geomParam.IsPointInsideRectangle(screenCpt1, screenCpt2, finalPosition))
        //        {
        //            selectedNodeIndices.Add(kvp.Key);
        //        }
        //    }

        //    return selectedNodeIndices;
        //}


    }
}
