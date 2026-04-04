#include "spectral_mesh2d.h"

spectral_mesh2d::spectral_mesh2d()
{
	// Empty constructor
}



void spectral_mesh2d::generate_spectral_mesh(const helmholtz_system_store& linear_mesh)
{
	// Copy to local variable
    this->linear_mesh = linear_mesh;

	// Clear any existing spectral mesh data
	spectral_node_list.clear();
	spectral_edge_list.clear();
	spectral_trielement_list.clear();
	spectral_quadelement_list.clear();

	
	// p-Refinement
	// Generate spectral nodes, edges, and elements based on the linear mesh
	this->spectral_order = linear_mesh.spectral_order; // Set the spectral order from the linear mesh


	// Get the gll locations and gll weights for the given spectral order 
	std::vector<double> gll_locations = gll_utility::get_gll_locations(this->spectral_order);
	std::vector<double> gll_weights = gll_utility::get_gll_weights(this->spectral_order, gll_locations);


	unique_id_control node_id_control(0); // Unique ID control for nodes

	// Get the existing node IDs from the linear mesh to avoid duplicates
	std::vector<int> existing_node_ids;
    for (const auto& node : linear_mesh.node_list)
    {
        existing_node_ids.push_back(node.first);
	}

	node_id_control.create_free_ids(existing_node_ids); // Initialize the unique ID control with existing node IDs


	// QUADRILATERAL ELEMENTS

	for(const auto& quad : linear_mesh.quadelement_list)
	{
		const quadelement_store& quad_elm = quad.second;
		// Get the corner node IDs of the quadrilateral element
		int nd1_id = quad_elm.nodeid1; // Node id 1
		int nd2_id = quad_elm.nodeid2; // Node id 2
		int nd3_id = quad_elm.nodeid3; // Node id 3
		int nd4_id = quad_elm.nodeid4; // Node id 4
		// Get the material ID of the quadrilateral element
		int material_id = quad_elm.materialid;
	

		// Generate spectral nodes, edges, and elements for the quadrilateral element
		// Nodes are stored in the counter-clockwise order (nd1, nd2, nd3, nd4)
        const node_store& n1 = linear_mesh.node_list.at(nd1_id);
        const node_store& n2 = linear_mesh.node_list.at(nd2_id);
        const node_store& n3 = linear_mesh.node_list.at(nd3_id);
        const node_store& n4 = linear_mesh.node_list.at(nd4_id);

        // get the Four edge ids of the quadrilateral element
        int edge1_id = get_edge_id(nd1_id, nd2_id); // Edge 1
        int edge2_id = get_edge_id(nd2_id, nd3_id); // Edge 2
        int edge3_id = get_edge_id(nd3_id, nd4_id); // Edge 3
        int edge4_id = get_edge_id(nd4_id, nd1_id); // Edge 4


		// Get the edges of the quadrilateral element
		const edge_store& e1 = linear_mesh.edge_list.at(edge1_id); // Edge 1
        const edge_store& e2 = linear_mesh.edge_list.at(edge2_id); // Edge 2
        const edge_store& e3 = linear_mesh.edge_list.at(edge3_id); // Edge 3
        const edge_store& e4 = linear_mesh.edge_list.at(edge4_id); // Edge 4


		std::vector<int> corner_node_ids;
		std::vector<int> edge_node_ids; // To be filled with edge node IDs
		std::vector<int> internal_node_ids; // To be filled with internal node IDs




        // Create the edge nodes
        for (int i = 0; i < 4; i++)
        {
            // edge_store edge;
            const edge_store& edge = (i == 0) ? e1 : (i == 1) ? e2 : (i == 2) ? e3 : e4;
          
            int startnodeid = edge.startnodeid;
            int endnodeid = edge.endnodeid;

            // Get the start and end nodes of the edge
            const node_store& start_node = linear_mesh.node_list.at(startnodeid);
            const node_store& end_node = linear_mesh.node_list.at(endnodeid);
            
			std::vector<int> edge_internal_node_ids; // To store internal node IDs for this edge

            // Create edge nodes based on the spectral order
            for (int j = 1; j < spectral_order; j++)
            {

                int node_id = node_id_control.get_unique_id(); // Get a unique node ID

                double x = 0.5 * ((1 - gll_locations[j]) * start_node.x_coord + 
                    gll_locations[j] * end_node.x_coord);

                double y = 0.5 * ((1 - gll_locations[j]) * start_node.y_coord + 
                    gll_locations[j] * end_node.y_coord);

                create_spectral_nodes(node_id,
                    x, y, false, false, 0.0, 0.0); // Create edge node and store it
                edge_node_ids.push_back(node_id); // Add to edge node IDs

				edge_internal_node_ids.push_back(node_id); // Add to this edge's internal node IDs
            }

            // Add spectral edge to the spectral edge list
            create_spectral_edges(edge.edge_id, 
                startnodeid, endnodeid, edge_internal_node_ids,
                edge.isboundaryedge, edge.isSommerfieldBC, edge.isFieldBC, edge.isDerivFieldBC,
				edge.fieldvalue, edge.normalderivfieldvalue);
            //
		}


		// Create the corner and internal nodes for the quadrilateral element using bilinear mapping
        for (int i= 0;  i <= spectral_order; i++)
        {
            for (int j = 0; j <= spectral_order; j++)
            {
                double xi = gll_locations[i];
                double eta = gll_locations[j];

                // Bilinear mapping
                double x = 0.25 * (
                    (1 - xi) * (1 - eta) * n1.x_coord +
                    (1 + xi) * (1 - eta) * n2.x_coord +
                    (1 + xi) * (1 + eta) * n3.x_coord +
                    (1 - xi) * (1 + eta) * n4.x_coord
                    );

                double y = 0.25 * (
                    (1 - xi) * (1 - eta) * n1.y_coord +
                    (1 + xi) * (1 - eta) * n2.y_coord +
                    (1 + xi) * (1 + eta) * n3.y_coord +
                    (1 - xi) * (1 + eta) * n4.y_coord
                    );

                           
				if (i != 0 && i != spectral_order && j != 0 && j != spectral_order)
                {
                    // Internal nodes
                    // Create spectral internal node and store it
                    int node_id = node_id_control.get_unique_id(); // Get a unique node ID
                    create_spectral_nodes(node_id,
                        x, y, false, false, 0.0, 0.0); // Create internal node and store it

					internal_node_ids.push_back(node_id); // Add to internal node ID
                }


                // Corner nodes
                if(i==0 && j ==0) // [-1,-1]
                {
                    // Node 1 (corner)
                    create_spectral_nodes(n1.node_id,
                        x, y, n1.isboundarynode, n1.isFieldBC,
						n1.fieldvalue, n1.sourcevalue); // Create corner node and store it

                    corner_node_ids.push_back(n1.node_id); // Add to corner node IDs
                }
				else if (i == 0 && j == spectral_order) // [-1,1]
                {
                    // Node 2 (corner)
                    create_spectral_nodes(n2.node_id,
                        x, y, n2.isboundarynode, n2.isFieldBC, 
						n2.fieldvalue, n2.sourcevalue); // Create corner node and store it

                    corner_node_ids.push_back(n2.node_id); // Add to corner node IDs
                }
				else if (i == spectral_order && j == spectral_order) // [1,1]
                {
                    // Node 3 (corner)
                    create_spectral_nodes(n3.node_id,
                        x, y, n3.isboundarynode, n3.isFieldBC, 
						n3.fieldvalue, n3.sourcevalue); // Create corner node and store it

                    corner_node_ids.push_back(n3.node_id); // Add to corner node IDs
                }
				else if (i == spectral_order && j == 0) // [1,-1]
                {
                    // Node 4 (corner)
                    create_spectral_nodes(n4.node_id,
                        x, y, n4.isboundarynode, n4.isFieldBC,
                        n4.fieldvalue, n4.sourcevalue); // Create corner node and store it

                    corner_node_ids.push_back(n4.node_id); // Add to corner node IDs
                }
                //
            }
            //
        }

        // Store spectral quad element
        spectral_quadelement_store spec_quad;

        spec_quad.quad_id = quad_elm.quad_id;
        spec_quad.materialid = quad_elm.materialid;

        spec_quad.corner_nodes = corner_node_ids;
		spec_quad.edge_nodes = edge_node_ids;
		spec_quad.internal_nodes = internal_node_ids;

        spectral_quadelement_list[spec_quad.quad_id] = spec_quad;
		//
	}


}






int spectral_mesh2d::get_edge_id(const int& startnodeid, const int& endnodeid)
{

    // Get the connected edges to start node
    const std::vector<int>& connected_edges = this->linear_mesh.node_edge_map[startnodeid];

    for (const int& edge_id : connected_edges)
    {
        const auto& edge = this->linear_mesh.edge_list[edge_id];
        if ((edge.startnodeid == startnodeid && edge.endnodeid == endnodeid) ||
            (edge.startnodeid == endnodeid && edge.endnodeid == startnodeid))
        {
            // Line with the same start and end nodes
            return edge_id;
        }

    }

    return -1;

}


void spectral_mesh2d::create_spectral_nodes(int node_id,
    double x_coord,
    double y_coord,
    bool isboundarynode,
    bool isFieldBC,
    double fieldvalue,
    double sourcevalue)
{

    // Create spectral edge node and store it
    spectral_node_store spec_node;
    spec_node.node_id = node_id; // Get a unique node ID
    spec_node.x_coord = x_coord;
    spec_node.y_coord = y_coord;

    spec_node.isboundarynode = isboundarynode;
    spec_node.isFieldBC = isFieldBC;
    spec_node.fieldvalue = fieldvalue;
    spec_node.sourcevalue = sourcevalue;

    spectral_node_list[spec_node.node_id] = spec_node; // Store the edge node
    //
}


void spectral_mesh2d::create_spectral_edges(int edge_id,
    int startnodeid,
    int endnodeid,
    const std::vector<int>& internal_node_ids,
    bool isboundaryedge,
    bool isSommerfieldBC,
    bool isFieldBC,
    bool isDerivFieldBC,
    double fieldvalue,
    double normalderivfieldvalue)
{

	// Create spectral edge and store it
	spectral_edge_store spec_edge;

	spec_edge.edge_id = edge_id;
	spec_edge.startnodeid = startnodeid;
	spec_edge.endnodeid = endnodeid;
	spec_edge.internal_node_ids.copy(internal_node_ids);
	spec_edge.isboundaryedge = isboundaryedge;
	spec_edge.isSommerfieldBC = isSommerfieldBC;
	spec_edge.isFieldBC = isFieldBC;
	spec_edge.isDerivFieldBC = isDerivFieldBC;
	spec_edge.fieldvalue = fieldvalue;
	spec_edge.normalderivfieldvalue = normalderivfieldvalue;

	spectral_edge_list[spec_edge.edge_id] = spec_edge; // Store the edge
    //
}