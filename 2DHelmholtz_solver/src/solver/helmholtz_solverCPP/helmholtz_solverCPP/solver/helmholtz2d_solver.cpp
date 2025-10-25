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
		int edge1_id = get_edge_id(nd1_id, nd2_id); // Edge 1
		int edge2_id = get_edge_id(nd2_id, nd3_id); // Edge 2
		int edge3_id = get_edge_id(nd3_id, nd1_id); // Edge 3

		// get the edge lengths
		double edge1_length = get_line_length(helmholtz_2dsystem.node_list[nd1_id], helmholtz_2dsystem.node_list[nd2_id]);
		double edge2_length = get_line_length(helmholtz_2dsystem.node_list[nd2_id], helmholtz_2dsystem.node_list[nd3_id]);
		double edge3_length = get_line_length(helmholtz_2dsystem.node_list[nd3_id], helmholtz_2dsystem.node_list[nd1_id]);

		// get the material parameters of this element
		double permeability_mu = helmholtz_2dsystem.material_list[tri_elm.materialid].permeability; // Permeability
		double permittivity_epsilon = helmholtz_2dsystem.material_list[tri_elm.materialid].permittivity; // Permitivity
		double elm_area = get_triangle_area(helmholtz_2dsystem.node_list[nd1_id], 
			helmholtz_2dsystem.node_list[nd2_id], 
			helmholtz_2dsystem.node_list[nd3_id]);

		//________________________________________________________________________________________________
		// Step 2: Create element k matrix
		Eigen::Matrix3d element_k_matrix; // Element k matrix
		element_k_matrix.setZero();

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

void helmholtz2d_solver::get_trielement_k_matrix(const node_store& nd1, const node_store& nd2, 
	const node_store& nd3, const double& triarea, Eigen::Matrix3d& element_k_matrix)
{
	// Element k matrix
	element_k_matrix = Eigen::Matrix3d::Zero();

	// b and c coefficients
	double b1 = nd2.y_coord - nd3.y_coord;
	double b2 = nd3.y_coord - nd1.y_coord;
	double b3 = nd1.y_coord - nd2.y_coord;

	double c1 = nd3.x_coord - nd2.x_coord;
	double c2 = nd1.x_coord - nd3.x_coord;
	double c3 = nd2.x_coord - nd1.x_coord;

	Eigen::MatrixXd B(2, 3);
	B << b1, b2, b3,
		c1, c2, c3;

	// K = (1 / (4 * Area)) * B^T * B
	element_k_matrix = (1.0 / (4.0 * triarea)) * (B.transpose() * B);

}


void helmholtz2d_solver::get_trielement_m_matrix(const node_store& nd1, const node_store& nd2, 
	const node_store& nd3, const double& triarea, Eigen::Matrix3d& element_m_matrix)
{
	// Element m matrix
	element_m_matrix = Eigen::Matrix3d::Zero();
	element_m_matrix << 2, 1, 1,
		1, 2, 1,
		1, 1, 2;

	element_m_matrix = (triarea / 12.0) * element_m_matrix;

}


void helmholtz2d_solver::get_trielement_kI_matrix(const int& edge1_id, const int& edge2_id, const int& edge3_id, 
	const double& edge1_length, const double& edge2_length, const double& edge3_length, 
	const double& k, Eigen::Matrix3d& element_kI_matrix)
{
	// Element ABC matrix (Sommerfield absorbtion boundary condition)
	element_kI_matrix = Eigen::Matrix3d::Zero();

	helmholtz_system_store& helmholtz_2dsystem = (*this->helmholtz_2dsystem_ptr);

	auto add_edge_contribution = [&](int edge_id, double edge_length, int n1, int n2)
		{
			if (helmholtz_2dsystem.edge_list[edge_id].isboundaryedge &&
				helmholtz_2dsystem.edge_list[edge_id].isSommerfieldBC)
			{
				Eigen::Matrix3d edge_matrix = Eigen::Matrix3d::Zero();

				edge_matrix(n1, n1) += 2.0;
				edge_matrix(n1, n2) += 1.0;
				edge_matrix(n2, n1) += 1.0;
				edge_matrix(n2, n2) += 2.0;

				edge_matrix *= (k * edge_length / 6.0);

				element_kI_matrix += edge_matrix;
			}
		};

	// Triangle edges (local node pairs)
	add_edge_contribution(edge1_id, edge1_length, 0, 1); // Edge between node 1 and 2
	add_edge_contribution(edge2_id, edge2_length, 1, 2); // Edge between node 2 and 3
	add_edge_contribution(edge3_id, edge3_length, 2, 0); // Edge between node 3 and 1

}


void helmholtz2d_solver::get_trielement_field_vector(const node_store& nd1, const node_store& nd2, 
	const node_store& nd3, const int& edge1_id, const int& edge2_id, const int& edge3_id, 
	const double& edge1_length, const double& edge2_length, const double& edge3_length, 
	Eigen::Vector3d& dirichlet_vector)
{
	// Element field vector
	dirichlet_vector = Eigen::Vector3d::Zero();

	helmholtz_system_store& helmholtz_2dsystem = (*this->helmholtz_2dsystem_ptr);

	// Lambda for edge field contribution (Dirichlet-type BC)
	auto add_edge_contribution = [&](int edge_id, double edge_length, int n1, int n2)
		{
			const auto& edge = helmholtz_2dsystem.edge_list[edge_id];
			if (edge.isboundaryedge && !edge.isSommerfieldBC)
			{
				// Dirichlet (field) BC contribution
				double q_edge = edge.fieldvalue;  // field value

				Eigen::Vector3d edge_vec = Eigen::Vector3d::Zero();
				edge_vec(n1) += 1.0;
				edge_vec(n2) += 1.0;

				// Integral over edge:int( N^T * q d(gamma)) = q * L/2 * [1, 1]
				edge_vec *= (q_edge * edge_length / 2.0);

				dirichlet_vector += edge_vec;
			}
		};

	// Apply edge contributions
	add_edge_contribution(edge1_id, edge1_length, 0, 1); // Edge 1-2
	add_edge_contribution(edge2_id, edge2_length, 1, 2); // Edge 2-3
	add_edge_contribution(edge3_id, edge3_length, 2, 0); // Edge 3-1

	// --- Dirichlet-type (prescribed field) nodes ---
	// These are handled differently: typically, we *replace* the corresponding rows/columns
	// in the global system matrix, but here you can still accumulate their vector form
	// if you’re forming a right-hand side prior to modification.
	Eigen::Vector3d node_vec = Eigen::Vector3d::Zero();

	if (nd1.isboundarynode)
		node_vec(0) = nd1.fieldvalue;
	if (nd2.isboundarynode)
		node_vec(1) = nd2.fieldvalue;
	if (nd3.isboundarynode)
		node_vec(2) = nd3.fieldvalue;

	// Add nodal (Dirichlet) field components
	dirichlet_vector += node_vec;

}


void helmholtz2d_solver::get_trielement_normderivfield_vector(const int& edge1_id, const int& edge2_id, 
	const int& edge3_id, const double& edge1_length, const double& edge2_length, const double& edge3_length, 
	Eigen::Vector3d& neumann_vector)
{
	neumann_vector = Eigen::Vector3d::Zero();

	helmholtz_system_store& helmholtz_2dsystem = (*this->helmholtz_2dsystem_ptr);

	// Lambda for edge field contribution (Neumann-type BC)
	auto add_edge_contribution = [&](int edge_id, double edge_length, int n1, int n2)
		{
			const auto& edge = helmholtz_2dsystem.edge_list[edge_id];
			if (edge.isboundaryedge && !edge.isSommerfieldBC)
			{
				// Neumann (field flux) BC contribution
				double q_edge = edge.normalderivfieldvalue;  // prescribed flux value

				Eigen::Vector3d edge_vec = Eigen::Vector3d::Zero();
				edge_vec(n1) += 1.0;
				edge_vec(n2) += 1.0;

				// Integral over edge:int( N^T * q d(gamma)) = q * L/2 * [1, 1]
				edge_vec *= (q_edge * edge_length / 2.0);

				neumann_vector += edge_vec;
			}
		};

	// Apply edge contributions
	add_edge_contribution(edge1_id, edge1_length, 0, 1); // Edge 1-2
	add_edge_contribution(edge2_id, edge2_length, 1, 2); // Edge 2-3
	add_edge_contribution(edge3_id, edge3_length, 2, 0); // Edge 3-1

}

void helmholtz2d_solver::get_trielement_source_vector(const node_store& nd1, const node_store& nd2, 
	const node_store& nd3, Eigen::Vector3d& source_vector)
{

	// --- Source (prescribed source) nodes ---
	source_vector = Eigen::Vector3d::Zero();

	// Add nodal source components
	if (nd1.isboundarynode)
		source_vector(0) = nd1.sourcevalue;
	if (nd2.isboundarynode)
		source_vector(1) = nd2.sourcevalue;
	if (nd3.isboundarynode)
		source_vector(2) = nd3.sourcevalue;


}








