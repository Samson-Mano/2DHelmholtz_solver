#include "helmholtz2d_spectral_solver.h"

helmholtz2d_spectral_solver::helmholtz2d_spectral_solver()
{
	// Empty constructor

}


void helmholtz2d_spectral_solver::init(helmholtz_system_store* helmholtz_2dsystem_ptr,
	const char* output_file,
	stopwatch_events* stopwatch,
	void(*callback)(const char*))
{
	// Set the initialized system ptr
	this->helmholtz_2dsystem_ptr = helmholtz_2dsystem_ptr;

	// Set the stopwatch
	this->m_stopwatch = stopwatch;


	// Store callback locally
	this->m_callback = callback;

	// Store the output file name
	this->output_file = output_file;

}


void helmholtz2d_spectral_solver::create_global_matrices()
{
	// Create the spectral mesh
	helmholtz_system_store helmholtz_2dsystem = (*this->helmholtz_2dsystem_ptr);

	// Create the spectral mesh
	this->spec_mesh2d.generate_spectral_mesh(helmholtz_2dsystem);

	report("Spectral mesh created");


}


void helmholtz2d_spectral_solver::solve_helmholtz_matrices(const int& solver_type)
{
	// open the bin file
	bin_file.open(output_file, std::ios::binary);

	
	// Create a node ID map (to create a nodes as ordered and numbered from 0,1,2...n)
	int i = 0;
	for (auto& nd : this->spec_mesh2d.spectral_node_list)
	{
		nodeid_map[nd.second.node_id] = i;
		i++;
	}


	// Set the number of DOF
	this->numDOF = static_cast<int>(spec_mesh2d.spectral_node_list.size());

	global_k_matrix.setZero(numDOF, numDOF);; // Global k Matrix (Ke - k^2 * Me)
	global_kI_matrix.setZero(numDOF, numDOF);; // Global kI Matrix Boundary impedance matrix (Absorbing Boundary condition - Sommerfield)

	global_field_vector.setZero(numDOF); // Global field Vector
	global_normalderivfield_vector.setZero(numDOF); // Global derivative normal field Vector
	global_source_vector.setZero(numDOF); // Global source Vector

	global_dirichlet_BC_flags_vector.setZero(numDOF); // Global boundary condition Vector (To track the nodes where prescribed field is applied)


	// get the quadrature points
	int spectral_order = spec_mesh2d.spectral_order;
	this->triangle_quadrature_points = gll_utility::get_triangle_quadrature(spectral_order);


	// Triangle elements
	for (auto& tri_elm_m : spec_mesh2d.spectral_trielement_list)
	{
		// get the element
		spectral_trielement_store tri_elm = tri_elm_m.second;

		//_______ Build local node list _______________________________________________
		std::vector<int> elem_nodes;

		for (int i = 0; i < 3; i++)
		{
			// Add the corner i (0, 1, 2)
			elem_nodes.push_back(tri_elm.corner_nodes[i]);

			// Then the edge i (0, 1, 2)
			for (const auto& edge1_ndid : tri_elm.edge_node_ids[i])
			{
				elem_nodes.push_back(edge1_ndid);
			}
			//
		}
		
		// internal nodes
		for (int internal_ndid : tri_elm.internal_nodes)
		{
			elem_nodes.push_back(internal_ndid);
		}


		//________ Get node coordinates ________________________________________________
		std::vector<Eigen::Vector2d> elem_coords;

		for (int nid : elem_nodes)
		{
			const auto& node = spec_mesh2d.spectral_node_list.at(nid);
			elem_coords.emplace_back(node.x_coord, node.y_coord);
		}

		//________ Allocate local matices ________________________________________________
		int nen = static_cast<int>(elem_nodes.size());

		Eigen::MatrixXd element_k_grad_matrix = Eigen::MatrixXd::Zero(nen, nen); // Element k grad matrix
		Eigen::MatrixXd element_k_mass_matrix = Eigen::MatrixXd::Zero(nen, nen); // Element k mass matrix


		get_trielement_k_grad_k_mass_matrix(elem_nodes, elem_coords, tri_elm.tri_area,
			element_k_grad_matrix, element_k_mass_matrix);


		//________ Integration loop  _____________________________________________________
		
		for (int gp = 0; gp < num_gauss_points; gp++)
		{
			// 1. Get local coords (xi, eta)
			double xi = 0.0;
			double eta = 0.0;
			double w = 0.0;

				// 2. Evaluate shape functions
			Eigen::VectorXd N(nen);
			Eigen::MatrixXd dN_dxi(nen, 2);

			evaluate_shape_functions(xi, eta, N, dN_dxi);

			// 3. Compute Jacobian
			Eigen::Matrix2d J = Eigen::Matrix2d::Zero();

			for (int i = 0; i < nen; i++)
			{
				J(0, 0) += dN_dxi(i, 0) * elem_coords[i].x();
				J(0, 1) += dN_dxi(i, 0) * elem_coords[i].y();
				J(1, 0) += dN_dxi(i, 1) * elem_coords[i].x();
				J(1, 1) += dN_dxi(i, 1) * elem_coords[i].y();
			}

			double detJ = J.determinant();
			Eigen::Matrix2d invJ = J.inverse();

			// 4. Convert derivatives to physical space
			Eigen::MatrixXd dN_dx = dN_dxi * invJ;

			// 5. Assemble element matrices
			for (int i = 0; i < nen; i++)
			{
				for (int j = 0; j < nen; j++)
				{
					// Mass
					Me(i, j) += N(i) * N(j) * detJ * w;

					// Stiffness
					Ke(i, j) += (dN_dx.row(i).dot(dN_dx.row(j))) * detJ * w;
				}
			}
		}


		//________ Helmholtz operator _____________________________________________________




	}







	// Write the results
	store_results();


	// close the bin file
	bin_file.close();
//
}


void helmholtz2d_spectral_solver::get_trielement_k_grad_k_mass_matrix(const std::vector<int>& elem_nodes,
	const std::vector<Eigen::Vector2d>& elem_coords,
	const double& tri_area,
	Eigen::MatrixXd& element_k_grad_matrix,
	Eigen::MatrixXd& element_k_mass_matrix)
{
	int nen = static_cast<int>(elem_nodes.size());

	// --- 1. Loop over quadrature points ---
	for (int q = 0; q < static_cast<int>(triangle_quadrature_points.size()); q++)
	{
		double xi = triangle_quadrature_points[q].xi;
		double eta = triangle_quadrature_points[q].eta;
		double w = triangle_quadrature_points[q].weight; // weights are normalized to 1.0

		// --- 2. Evaluate shape functions ---
		Eigen::VectorXd N(nen);
		Eigen::MatrixXd dN_dxi(nen, 2); // [dN/dxi, dN/deta]

		evaluate_triangle_shape_functions(xi, eta, elem_coords, N, dN_dxi);

		// --- 3. Compute Jacobian ---
		Eigen::Matrix2d J = Eigen::Matrix2d::Zero();

		for (int i = 0; i < nen; i++)
		{
			J(0, 0) += dN_dxi(i, 0) * elem_coords;
			J(0, 1) += dN_dxi(i, 0) * elem_coords;
			J(1, 0) += dN_dxi(i, 1) * elem_coords;
			J(1, 1) += dN_dxi(i, 1) * elem_coords;
		}

		double detJ = J.determinant();
		Eigen::Matrix2d invJ = J.inverse();

		// --- 4. Transform gradients ---
		Eigen::MatrixXd dN_dx(nen, 2);

		for (int i = 0; i < nen; i++)
		{
			Eigen::Vector2d grad_ref(dN_dxi(i, 0), dN_dxi(i, 1));
			Eigen::Vector2d grad_phys = invJ.transpose() * grad_ref;

			dN_dx(i, 0) = grad_phys(0);
			dN_dx(i, 1) = grad_phys(1);
		}

		// --- 5. Assemble matrices ---
		for (int i = 0; i < nen; i++)
		{
			for (int j = 0; j < nen; j++)
			{
				// Gradient (stiffness)
				element_k_grad_matrix(i, j) += (dN_dx.row(i).dot(dN_dx.row(j))) * detJ * w;

				// Mass
				element_k_mass_matrix(i, j) += (N(i) * N(j)) * detJ * w;
			}
		}
	}
	//
}



void helmholtz2d_spectral_solver::store_results()
{

	int32_t node_points_count = static_cast<int32_t>(spec_mesh2d.renderer_node_points.size());
	bin_file.write(reinterpret_cast<const char*>(&node_points_count), sizeof(int32_t));

	// Write the nodes
	for (const auto& node : spec_mesh2d.renderer_node_points)
	{
		int32_t nodeid = static_cast<int32_t>(node.n_id);
		double rand_result = std::sin(node.x * 10.0) * std::cos(node.y * 10.0); // Random value between 0 and 1


		bin_file.write(reinterpret_cast<const char*>(&nodeid), sizeof(int32_t));
		bin_file.write(reinterpret_cast<const char*>(&node.x), sizeof(double));
		bin_file.write(reinterpret_cast<const char*>(&node.y), sizeof(double));
		bin_file.write(reinterpret_cast<const char*>(&rand_result), sizeof(double));
	}

	report("Results: Nodes written");

	int32_t edge_lines_count = static_cast<int32_t>(spec_mesh2d.renderer_edge_lines.size());
	bin_file.write(reinterpret_cast<const char*>(&edge_lines_count), sizeof(int32_t));

	// Write the edges
	for (const auto& edge : spec_mesh2d.renderer_edge_lines)
	{
		int32_t start_nodeid = static_cast<int32_t>(edge.nstart);
		int32_t end_nodeid = static_cast<int32_t>(edge.nend);

		bin_file.write(reinterpret_cast<const char*>(&start_nodeid), sizeof(int32_t));
		bin_file.write(reinterpret_cast<const char*>(&end_nodeid), sizeof(int32_t));
	}

	report("Results: Edges written");

	int32_t triangles_count = static_cast<int32_t>(spec_mesh2d.renderer_element_triangles.size());
	bin_file.write(reinterpret_cast<const char*>(&triangles_count), sizeof(int32_t));

	// Write the triangles
	for (const auto& tri : spec_mesh2d.renderer_element_triangles)
	{
		int32_t n1 = static_cast<int32_t>(tri.n1);
		int32_t n2 = static_cast<int32_t>(tri.n2);
		int32_t n3 = static_cast<int32_t>(tri.n3);

		bin_file.write(reinterpret_cast<const char*>(&n1), sizeof(int32_t));
		bin_file.write(reinterpret_cast<const char*>(&n2), sizeof(int32_t));
		bin_file.write(reinterpret_cast<const char*>(&n3), sizeof(int32_t));
	}

	report("Results: Triangles written");

	//
}


void helmholtz2d_spectral_solver::report(const char* msg)
{
	std::stringstream stopwatch_elapsed_str;

	stopwatch_elapsed_str << std::fixed << std::setprecision(6)
		<< this->m_stopwatch->elapsed();

	std::string final_msg = std::string(msg) +
		stopwatch_elapsed_str.str() +
		" secs";

	if (m_callback)
		m_callback(final_msg.c_str());
	//
}

