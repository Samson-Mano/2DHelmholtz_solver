#include "helmholtz2d_solver.h"

helmholtz2d_solver::helmholtz2d_solver()
{
	// Empty constructor
}

void helmholtz2d_solver::init(helmholtz_system_store* helmholtz_2dsystem_ptr)
{
	// Set the initialized system ptr
	this->helmholtz_2dsystem_ptr = helmholtz_2dsystem_ptr;

}

void helmholtz2d_solver::create_global_matrices()
{
	// Set the k, m matrix
	helmholtz_system_store helmholtz_2dsystem = (*this->helmholtz_2dsystem_ptr);

	// Create a node ID map (to create a nodes as ordered and numbered from 0,1,2...n)
	int i = 0;
	for (auto& nd : helmholtz_2dsystem.node_list)
	{
		nodeid_map[nd.second.node_id] = i;
		i++;
	}


	// Set the number of DOF
	this->numDOF = static_cast<int>(helmholtz_2dsystem.node_list.size());

	// Triangle element
	for (auto& tri_elm_m : helmholtz_2dsystem.trielement_list)
	{
		// get the element
		trielement_store tri_elm = tri_elm_m.second;

		//_______________________________________________________________________________________________
		// Step: 1 Get the element data
		// set the element ID
		int elm_id = tri_elm.tri_id;

		// get the node ids of the element
		int nd1_id = tri_elm.nodeid1; // Node id 1
		int nd2_id = tri_elm.nodeid2; // Node id 2
		int nd3_id = tri_elm.nodeid3; // Node id 3

		// get the three edge ids of the elemnt
		int edg1_id = get_edge_id(nd1_id, nd2_id); // Edge 1
		int edg2_id = get_edge_id(nd2_id, nd3_id); // Edge 2
		int edg3_id = get_edge_id(nd3_id, nd1_id); // Edge 3

		// get the edge lengths
		double edg1_length = get_line_length(helmholtz_2dsystem.node_list[nd1_id], helmholtz_2dsystem.node_list[nd2_id]);
		double edg2_length = get_line_length(helmholtz_2dsystem.node_list[nd2_id], helmholtz_2dsystem.node_list[nd3_id]);
		double edg3_length = get_line_length(helmholtz_2dsystem.node_list[nd3_id], helmholtz_2dsystem.node_list[nd1_id]);

		// get the material parameters of this element
		double permeability_mu = helmholtz_2dsystem.material_list[tri_elm.materialid].permeability; // Permeability
		double permittivity_epsilon = helmholtz_2dsystem.material_list[tri_elm.materialid].permittivity; // Permitivity
		double elm_area = get_triangle_area(helmholtz_2dsystem.node_list[nd1_id], 
			helmholtz_2dsystem.node_list[nd2_id], 
			helmholtz_2dsystem.node_list[nd3_id]);

		//________________________________________________________________________________________________
		// Step 2: Create element k matrix
		Eigen::MatrixXd element_k_matrix; // Element k matrix
		element_k_matrix.setZero(3);

		//get_trielement_k_matrix(elm.nd1->node_pt,
		//	elm.nd2->node_pt,
		//	elm.nd3->node_pt,
		//	elm_kx,
		//	elm_ky,
		//	elm_thickness,
		//	elm_area,
		//	Element_conduction_matrix);







	}






}



int helmholtz2d_solver::get_edge_id(const int& startnodeid, const int& endnodeid)
{

	helmholtz_system_store helmholtz_2dsystem = (*this->helmholtz_2dsystem_ptr);

	// Return the edge id
	for (const auto& line_m : helmholtz_2dsystem.edge_list)
	{
		const edge_store& line = line_m.second;

		if ((line.startnodeid == startnodeid && line.endnodeid == endnodeid) ||
			(line.startnodeid == endnodeid && line.endnodeid == startnodeid))
		{
			// Line with the same start and end nodes already exists (do not add)
			return line.edge_id;
		}
	}

	// Non found
	return -1;
}



double helmholtz2d_solver::get_line_length(const node_store& pt1, const node_store& pt2)
{
	// Length of line
	double length = std::sqrt(std::pow(pt1.x_coord - pt2.x_coord, 2) + std::pow(pt1.y_coord - pt2.y_coord, 2));

	return length;
}




double helmholtz2d_solver::get_triangle_area(const node_store& pt1, const node_store& pt2, const node_store& pt3)
{
	double x1 = pt1.x_coord;
	double y1 = pt1.y_coord;
	double x2 = pt2.x_coord;
	double y2 = pt2.y_coord;
	double x3 = pt3.x_coord;
	double y3 = pt3.y_coord;

	// Shoelace formula
	double area = 0.5 * (x1 * (y2 - y3) + x2 * (y3 - y1) + x3 * (y1 - y2));

	return area;
}

