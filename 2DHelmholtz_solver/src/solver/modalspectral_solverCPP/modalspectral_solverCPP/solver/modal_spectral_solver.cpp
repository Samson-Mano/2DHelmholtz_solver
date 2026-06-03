#include "modal_spectral_solver.h"

modal_spectral_solver::modal_spectral_solver()
{
	// Empty constructor
}

void modal_spectral_solver::init(helmholtz_system_store * helmholtz_2dsystem_ptr, 
	const char* output_file_char, stopwatch_events * stopwatch, void(*callback)(const char*))
{
	// Set the initialized system ptr
	this->helmholtz_2dsystem_ptr = helmholtz_2dsystem_ptr;

	// Set the stopwatch
	this->m_stopwatch = stopwatch;


	// Store callback locally
	this->m_callback = callback;

	// Store the output file name
	// CRITICAL: Copy the string to std::string for permanent storage
	this->output_file = std::string(output_file_char);

	std::string msg = "Output file set to: " + this->output_file;
	report(msg.c_str());


}

void modal_spectral_solver::create_global_matrices()
{
	// Create the spectral mesh
	helmholtz_system_store helmholtz_2dsystem = (*this->helmholtz_2dsystem_ptr);

	// Create the spectral mesh
	this->spec_mesh2d.generate_spectral_mesh(helmholtz_2dsystem);

	report("Spectral mesh created");


	// Create a node ID map (to create a nodes as ordered and numbered from 0,1,2...n)
	int i = 0;
	for (auto& nd : this->spec_mesh2d.spectral_node_list)
	{
		nodeid_map[nd.second.node_id] = i;
		i++;
	}


	// Set the number of DOF
	this->numDOF = static_cast<int>(spec_mesh2d.spectral_node_list.size());

	// Global k Matrix [ke]
	global_k_matrix.resize(numDOF, numDOF);
	global_k_matrix.setZero();

	// Global m Matrix [Me]
	global_m_matrix.resize(numDOF, numDOF);
	global_m_matrix.setZero();

	// Global boundary condition Vector (To track the nodes where prescribed field is applied)
	global_dirichlet_BC_flags_vector.setZero(numDOF); 

	// get the quadrature points and the basis term
	int spectral_order = spec_mesh2d.spectral_order;

	std::vector<Eigen::Triplet<double>> k_triplets;
	std::vector<Eigen::Triplet<double>> m_triplets;


	if (static_cast<int>(spec_mesh2d.spectral_trielement_list.size()) > 0)
	{
		this->triangle_quadrature_points = spectral_tri_element::get_triangle_quadrature(spectral_order);
		this->triangle_basis_terms = spectral_tri_element::proriol_modes(spectral_order);

		report("Triangle Element Quadrature Points Created");

		this->inv_vandermonde_matrix = spectral_tri_element::get_inverse_vandermonde_matrix(spectral_order, this->triangle_basis_terms);

		// Callback the rank and conditioning of inverse Vandermonde matrix
		report_vandermondematrix_conditioning(this->inv_vandermonde_matrix);

		// Get the gll locations and gll weights for the given spectral order 
		this->gll_locations = gll_utility::get_gll_locations(spectral_order);
		this->gll_weights = gll_utility::get_gll_weights(spectral_order, gll_locations);


		// Triangle elements
		for (auto& tri_elm_m : spec_mesh2d.spectral_trielement_list)
		{
			// get the element
			spectral_trielement_store tri_elm = tri_elm_m.second;

			//________________________________________________________________________________________________
			// Step 1: Create local node & node coordinate list
			// Build local node list _______________________________________________
			std::vector<int> elem_nodes = tri_elm.lexi_ordered_node_ids;


			// Get node coordinates ________________________________________________
			std::vector<Eigen::Vector2d> elem_coords;

			for (int nid : elem_nodes)
			{
				const auto& node = spec_mesh2d.spectral_node_list.at(nid);
				elem_coords.emplace_back(node.x_coord, node.y_coord);
			}


			//________________________________________________________________________________________________
			// Step 2: Create element k_grad matrix
			int nen = static_cast<int>(elem_nodes.size());

			Eigen::MatrixXd element_k_matrix = Eigen::MatrixXd::Zero(nen, nen); // Element k grad matrix
			Eigen::MatrixXd element_m_matrix = Eigen::MatrixXd::Zero(nen, nen); // Element mass matrix


			get_trielement_k_grad_k_mass_matrix(elem_nodes, elem_coords,
				element_k_matrix, element_m_matrix);


			//________________________________________________________________________________________________
			// Step 3: Create Element field vector
			Eigen::VectorXi element_field_BC_flag_vector = Eigen::VectorXi::Zero(nen); // Element field vector BC flag
			// Eigen::VectorXd element_field_vector = Eigen::VectorXd::Zero(nen); // Element field vector


			get_trielement_field_vector(tri_elm, element_field_BC_flag_vector);

			//________________________________________________________________________________________________
			// Step 4: Create Element source vector
			// Eigen::VectorXd element_source_vector = Eigen::VectorXd::Zero(nen); // Element source vector

			get_trielement_source_vector(tri_elm, element_field_BC_flag_vector);


			//________________________________________________________________________________________________
			// Step 5: Set the global matrix and global vector

			set_global_matrix(elem_nodes, nen,
				element_k_matrix,
				element_m_matrix,
				k_triplets,
				m_triplets);



			set_global_BC_flag_vector(elem_nodes, nen,
				element_field_BC_flag_vector, global_dirichlet_BC_flags_vector);

		}

		report("Triangle Spectral Elements Global Matrices Created");
		//
	}


	if (static_cast<int>(spec_mesh2d.spectral_quadelement_list.size()) > 0)
	{
		this->quadrilateral_quadrature_points = spectral_quad_element::get_quadrilateral_quadrature(spectral_order);

		// Get the gll locations and gll weights for the given spectral order 
		this->gll_locations = gll_utility::get_gll_locations(spectral_order);
		this->gll_weights = gll_utility::get_gll_weights(spectral_order, gll_locations);

		report("Quadrilateral Element Quadrature Points Created");

		// Quadrilateral elements
		for (auto& quad_elm_m : spec_mesh2d.spectral_quadelement_list)
		{
			// get the element
			spectral_quadelement_store quad_elm = quad_elm_m.second;

			//________________________________________________________________________________________________
			// Step 1: Create local node & node coordinate list
			// Build local node list _______________________________________________
			std::vector<int> elem_nodes = quad_elm.row_ordered_node_ids;

			// Get node coordinates ________________________________________________
			std::vector<Eigen::Vector2d> elem_coords;

			for (int nid : elem_nodes)
			{
				const auto& node = spec_mesh2d.spectral_node_list.at(nid);
				elem_coords.emplace_back(node.x_coord, node.y_coord);
			}


			//________________________________________________________________________________________________
			// Step 2: Create element k_grad matrix
			int nen = static_cast<int>(elem_nodes.size());

			Eigen::MatrixXd element_k_matrix = Eigen::MatrixXd::Zero(nen, nen); // Element k grad matrix
			Eigen::MatrixXd element_m_matrix = Eigen::MatrixXd::Zero(nen, nen); // Element k mass matrix


			get_quadelement_k_grad_k_mass_matrix(elem_nodes, elem_coords,
				element_k_matrix, element_m_matrix);


			//________________________________________________________________________________________________
			// Step 3: Create Element field vector
			Eigen::VectorXi element_field_BC_flag_vector = Eigen::VectorXi::Zero(nen); // Element field vector BC flag
			// Eigen::VectorXd element_field_vector = Eigen::VectorXd::Zero(nen); // Element field vector

			get_quadelement_field_vector(quad_elm, element_field_BC_flag_vector);

			//________________________________________________________________________________________________
			// Step 4: Create Element source vector
			// Eigen::VectorXd element_source_vector = Eigen::VectorXd::Zero(nen); // Element source vector

			get_quadelement_source_vector(quad_elm, element_field_BC_flag_vector);

			//________________________________________________________________________________________________
			// Step 5: Set the global matrix and global vector

			set_global_matrix(elem_nodes, nen,
				element_k_matrix,
				element_m_matrix,
				k_triplets,
				m_triplets);


			set_global_BC_flag_vector(elem_nodes, nen,
				element_field_BC_flag_vector, global_dirichlet_BC_flags_vector);

		}

		report("Quadrilateral Spectral Elements Global Matrices Created");
		//
	}

	//__________________________________________________________________________________________________________
	// Set the global k and m matrix
	global_k_matrix.setFromTriplets(k_triplets.begin(), k_triplets.end());
	global_m_matrix.setFromTriplets(m_triplets.begin(), m_triplets.end());

	// Create the message string and convert to const char*
	std::string sizeMsg = "Global K matrix created. Size: " +
		std::to_string(global_k_matrix.rows()) + "x" +
		std::to_string(global_k_matrix.cols()) +
		", Non-zeros: " + std::to_string(global_k_matrix.nonZeros());

	report(sizeMsg.c_str());

	sizeMsg = "Global M matrix created. Size: " +
		std::to_string(global_m_matrix.rows()) + "x" +
		std::to_string(global_m_matrix.cols()) +
		", Non-zeros: " + std::to_string(global_m_matrix.nonZeros());

	report(sizeMsg.c_str());

	//
}




void modal_spectral_solver::solve_modal_analysis(int inpt_num_modes, int solver_type)
{
	// 1) Build the free-DOF index (apply Dirichlet BC)

	std::vector<int> free_dofs;
	// free_dofs.reserve(global_dirichlet_BC_flags_vector.size());

	for (int i = 0; i < global_dirichlet_BC_flags_vector.size(); ++i)
	{
		if (global_dirichlet_BC_flags_vector[i] == 0)
			free_dofs.push_back(i);
	}



	// 2) Extract reduced matrices Kff, Mff
	int n_free = static_cast<int>(free_dofs.size());

	// Set the number of modes
	int num_modes = inpt_num_modes;
	if (inpt_num_modes > n_free)
	{
		num_modes = n_free;
	}

	std::string msg = "Number of modes: " + std::to_string(num_modes);
	report(msg.c_str());

	Eigen::SparseMatrix<double> K_ff(n_free, n_free);
	Eigen::SparseMatrix<double> M_ff(n_free, n_free);

	std::vector<Eigen::Triplet<double>> Kt, Mt;
	// Kt.reserve(n_free * 20); 
	// Mt.reserve(n_free * 20);

	// Build a map: global => reduced index
	std::vector<int> map(global_k_matrix.rows(), -1);
	for (int i = 0; i < n_free; ++i)
		map[free_dofs[i]] = i;

	// Traverse original sparse matrices
	for (int k = 0; k < global_k_matrix.outerSize(); ++k)
	{
		for (Eigen::SparseMatrix<double>::InnerIterator it(global_k_matrix, k); it; ++it)
		{
			int i = it.row();
			int j = it.col();

			if (map[i] != -1 && map[j] != -1)
				Kt.emplace_back(map[i], map[j], it.value());
		}
	}

	for (int k = 0; k < global_m_matrix.outerSize(); ++k)
	{
		for (Eigen::SparseMatrix<double>::InnerIterator it(global_m_matrix, k); it; ++it)
		{
			int i = it.row();
			int j = it.col();

			if (map[i] != -1 && map[j] != -1)
				Mt.emplace_back(map[i], map[j], it.value());
		}
	}

	K_ff.setFromTriplets(Kt.begin(), Kt.end());
	M_ff.setFromTriplets(Mt.begin(), Mt.end());

	report("K_ff, M_ff reduced matrices created");

	// 3) Solve the generalized eigenvalue problem
	// Spectra (Eigen - based)
	// ARPACK (eigsh equivalent)

	// K and M are your reduced matrices (K_ff, M_ff)

	Eigen::VectorXd eigenvalues;
	Eigen::MatrixXd eigenvectors;

	if (solver_type == 1)
	{
		// Spectra - Eigen based
		Eigen::SimplicialLLT<Eigen::SparseMatrix<double>> chol(M_ff);

		if (chol.info() != Eigen::Success)
		{
			report("Cholesky failed!");
			return;
		}

		report("Cholesky successful!");

		MinvKOp op(K_ff, chol);

		SymEigsSolver<MinvKOp> eigs(op, num_modes, 2 * num_modes);

		eigs.init();
		int nconv = eigs.compute(SortRule::SmallestAlge);

		if (eigs.info() == CompInfo::Successful)
		{
			eigenvalues = eigs.eigenvalues();
			eigenvectors = eigs.eigenvectors();
		}
		else
		{
			report("Eigen Spectra solver failed!");
			return;
		}

		report("Eigen Spectra solve successful!");
	}
	else if (solver_type == 2)
	{
		// ARPACK
		solveARPACKEigen(eigenvalues, eigenvectors,
			num_modes,
			K_ff,
			M_ff);

	}

	

	// 4) Convert eigenvalues => frequencies
	this->natural_frequencies.clear();

	for (int i = 0; i < eigenvalues.size(); ++i)
	{
		double lambda = eigenvalues[i];

		if (lambda > 0.0)
		{
			double omega = std::sqrt(lambda);
			double freq = omega / (2.0 * M_PI);
			this->natural_frequencies.push_back(freq);
		}
	}


	// 5) Normalize mode shapes
	for (int i = 0; i < eigenvectors.cols(); ++i)
	{
		Eigen::VectorXd phi = eigenvectors.col(i);

		double norm = std::sqrt(phi.transpose() * M_ff * phi);
		eigenvectors.col(i) /= norm;
	}


	// 6) Reconstruct full mode shapes
	int total_dofs = global_k_matrix.rows();
	this->natural_modes.resize(total_dofs, eigenvectors.cols());	
	this->natural_modes.setZero();

	for (int i = 0; i < n_free; ++i)
	{
		this->natural_modes.row(free_dofs[i]) = eigenvectors.row(i);
	}

	// Store results
	store_results_with_index();

	report("Eigen vectors stored!");

}





void modal_spectral_solver::get_trielement_k_grad_k_mass_matrix(const std::vector<int>& elem_nodes,
	const std::vector<Eigen::Vector2d>& elem_coords,
	Eigen::MatrixXd& element_k_matrix,
	Eigen::MatrixXd& element_m_matrix)
{
	int nen = static_cast<int>(elem_nodes.size());

	// Get quadrature points
	const auto& quadrature_points = this->triangle_quadrature_points;

	// --- 1. Loop over quadrature points ---
	for (int q = 0; q < static_cast<int>(quadrature_points.size()); q++)
	{
		double quadraturept_xi = quadrature_points[q].xi;
		double quadraturept_eta = quadrature_points[q].eta;
		double wt = quadrature_points[q].weight; // weights are normalized to 1.0

		// --- 2. Evaluate shape functions ---
		Eigen::VectorXd N(nen);
		Eigen::VectorXd dN_dxi(nen); // [dN/dxi, dN/deta]
		Eigen::VectorXd dN_deta(nen); // [dN/dxi, dN/deta]

		spectral_tri_element::evaluate_triangle_shape_functions(quadraturept_xi, quadraturept_eta, spec_mesh2d.spectral_order,
			this->inv_vandermonde_matrix, this->triangle_basis_terms,
			N, dN_dxi, dN_deta);


		// --- 3. Compute Jacobian ---
		Eigen::Matrix2d J = Eigen::Matrix2d::Zero();

		for (int i = 0; i < nen; i++)
		{
			double x = elem_coords[i].x();
			double y = elem_coords[i].y();

			J(0, 0) += dN_dxi(i) * x; // dx/dxi
			J(0, 1) += dN_deta(i) * x; // dx/deta
			J(1, 0) += dN_dxi(i) * y; // dy/dxi
			J(1, 1) += dN_deta(i) * y; // dy/deta
		}

		double detJ = J.determinant();
		Eigen::Matrix2d invJ = J.inverse();

		// --- 4. Transform gradients ---
		// [dN/dx; dN/dy] = invJ^T * [dN/dxi; dN/deta]
		Eigen::MatrixXd dN_dx(nen, 2);  // Each row: [dN_i/dx, dN_i/dy]

		for (int i = 0; i < nen; i++)
		{
			// Gradient in reference coordinates
			Eigen::Vector2d grad_ref(dN_dxi(i), dN_deta(i));

			// Transform to physical coordinates
			Eigen::Vector2d grad_phys = invJ.transpose() * grad_ref;

			dN_dx(i, 0) = grad_phys(0); // dN_i/dx
			dN_dx(i, 1) = grad_phys(1); // dN_i/dy
		}

		// Quadrature weight including Jacobian determinant
		double dV = detJ * wt;


		// --- 5. Assemble matrices ---
		for (int i = 0; i < nen; i++)
		{
			for (int j = 0; j < nen; j++)
			{
				double grad_dot = dN_dx.row(i).dot(dN_dx.row(j));

				// Gradient (stiffness)
				element_k_matrix(i, j) += grad_dot * dV;

				// Mass
				element_m_matrix(i, j) += (N(i) * N(j)) * dV;
			}
		}
	}
	//
}




void modal_spectral_solver::get_trielement_field_vector(const spectral_trielement_store& tri_elm,
	Eigen::VectorXi& dirichlet_BC_flag)
{

	for (int i = 0; i < 3; i++)
	{
		// Get the edge id
		int edge_id = tri_elm.edge_ids[i];

		const spectral_edge_store& edge = spec_mesh2d.spectral_edge_list[edge_id];

		if (edge.isboundaryedge == true && edge.isFieldBC == true)
		{
			// --- Assembly ---
			// Dirichlet (field) BC contribution
			double q_edge = edge.fieldvalue;  // field value

			int local_idx = spec_mesh2d.tri_element_id_structure.corner_nodes[i];

			// dirichlet_vector(local_idx) = q_edge;
			dirichlet_BC_flag(local_idx) = 1;

			for (const int& j : spec_mesh2d.tri_element_id_structure.edge_node_ids[i])
			{
				local_idx = j;

				// dirichlet_vector(local_idx) = q_edge;
				dirichlet_BC_flag(local_idx) = 1;
			}

			local_idx = spec_mesh2d.tri_element_id_structure.corner_nodes[(i + 1) % 3];

			// dirichlet_vector(local_idx) = q_edge;
			dirichlet_BC_flag(local_idx) = 1;
			//
		}
		//
	}
	//
}


void modal_spectral_solver::get_trielement_source_vector(const spectral_trielement_store& tri_elm,
	Eigen::VectorXi& dirichlet_BC_flag)
{
	// Get the corner nodes
	const std::vector<int>& corner_nodes = tri_elm.corner_nodes;

	for (int i = 0; i < 3; i++)
	{
		const spectral_node_store& nd = spec_mesh2d.spectral_node_list[corner_nodes[i]];

		if (nd.isboundarynode == true)
		{
			int local_idx = spec_mesh2d.tri_element_id_structure.corner_nodes[i];

			if (nd.isFieldBC == true)
			{
				// Apply field value at the node
				// dirichlet_vector(local_idx) = nd.fieldvalue;
				dirichlet_BC_flag(local_idx) = 1;
			}
			else
			{
				// Apply source value at the node
				// source_vector(local_idx) = nd.sourcevalue;
			}
		}

	}
	//
}




//________________________________________________________________________________________________

void modal_spectral_solver::get_quadelement_k_grad_k_mass_matrix(const std::vector<int>& elem_nodes,
	const std::vector<Eigen::Vector2d>& elem_coords,
	Eigen::MatrixXd& element_k_matrix,
	Eigen::MatrixXd& element_m_matrix)
{
	int nen = static_cast<int>(elem_nodes.size());

	// Get quadrature points
	const auto& quadrature_points = this->quadrilateral_quadrature_points;

	// --- 1. Loop over quadrature points ---
	for (int q = 0; q < static_cast<int>(quadrature_points.size()); q++)
	{
		double quadraturept_xi = quadrature_points[q].xi;
		double quadraturept_eta = quadrature_points[q].eta;
		double wt = quadrature_points[q].weight;

		// --- 2. Evaluate shape functions ---
		Eigen::VectorXd N(nen);
		Eigen::VectorXd dN_dxi(nen); // [dN/dxi, dN/deta]
		Eigen::VectorXd dN_deta(nen); // [dN/dxi, dN/deta]

		spectral_quad_element::evaluate_quadrilateral_shape_functions(quadraturept_xi, quadraturept_eta,
			spec_mesh2d.spectral_order, gll_locations,
			N, dN_dxi, dN_deta);

		// --- 3. Compute Jacobian ---
		Eigen::Matrix2d J = Eigen::Matrix2d::Zero();

		for (int i = 0; i < nen; i++)
		{
			double x = elem_coords[i].x();
			double y = elem_coords[i].y();

			J(0, 0) += dN_dxi(i) * x; // dx/dxi
			J(0, 1) += dN_deta(i) * x; // dx/deta
			J(1, 0) += dN_dxi(i) * y; // dy/dxi
			J(1, 1) += dN_deta(i) * y; // dy/deta
		}

		double detJ = J.determinant();
		Eigen::Matrix2d invJ = J.inverse();

		// --- 4. Transform gradients ---
		Eigen::MatrixXd dN_dx(nen, 2);

		for (int i = 0; i < nen; i++)
		{
			// Gradient in reference coordinates
			Eigen::Vector2d grad_ref(dN_dxi(i), dN_deta(i));

			// Transform to physical coordinates
			Eigen::Vector2d grad_phys = invJ.transpose() * grad_ref;

			dN_dx(i, 0) = grad_phys(0); // dN_i/dx
			dN_dx(i, 1) = grad_phys(1); // dN_i/dy
		}

		// Quadrature weight including Jacobian determinant
		double dV = detJ * wt;

		// --- 5. Assemble matrices ---
		// element_k_grad_matrix += (dN_dx * dN_dx.transpose()) * detJ * w;
		// element_k_mass_matrix += (N * N.transpose()) * detJ * w;

		for (int i = 0; i < nen; i++)
		{
			for (int j = 0; j < nen; j++)
			{
				double grad_dot = dN_dx.row(i).dot(dN_dx.row(j));

				// Gradient (stiffness)
				element_k_matrix(i, j) += grad_dot * dV;

				// Mass
				element_m_matrix(i, j) += (N(i) * N(j)) * dV;
			}
		}
	}
	//
}





void modal_spectral_solver::get_quadelement_field_vector(const spectral_quadelement_store& quad_elm,
	Eigen::VectorXi& dirichlet_BC_flag)
{

	for (int i = 0; i < 4; i++)
	{
		// Get the edge id
		int edge_id = quad_elm.edge_ids[i];

		const spectral_edge_store& edge = spec_mesh2d.spectral_edge_list[edge_id];

		if (edge.isboundaryedge == true && edge.isFieldBC == true)
		{
			// --- Assembly ---
			// Dirichlet (field) BC contribution
			double q_edge = edge.fieldvalue;  // field value

			int local_idx = spec_mesh2d.quad_element_id_structure.corner_nodes[i];

			// dirichlet_vector(local_idx) = q_edge;
			dirichlet_BC_flag(local_idx) = 1;

			for (const int& j : spec_mesh2d.quad_element_id_structure.edge_node_ids[i])
			{
				local_idx = j;

				// dirichlet_vector(local_idx) = q_edge;
				dirichlet_BC_flag(local_idx) = 1;
			}

			local_idx = spec_mesh2d.quad_element_id_structure.corner_nodes[(i + 1) % 4];

			// dirichlet_vector(local_idx) = q_edge;
			dirichlet_BC_flag(local_idx) = 1;
			//
		}
		//
	}
	//
}



void modal_spectral_solver::get_quadelement_source_vector(const spectral_quadelement_store& quad_elm,
	Eigen::VectorXi& dirichlet_BC_flag)
{
	// Get the corner nodes
	const std::vector<int>& corner_nodes = quad_elm.corner_nodes;

	for (int i = 0; i < 4; i++)
	{
		const spectral_node_store& nd = spec_mesh2d.spectral_node_list[corner_nodes[i]];

		if (nd.isboundarynode == true)
		{
			int local_idx = spec_mesh2d.quad_element_id_structure.corner_nodes[i];

			if (nd.isFieldBC == true)
			{
				// Apply field value at the node
				// dirichlet_vector(local_idx) = nd.fieldvalue;
				dirichlet_BC_flag(local_idx) = 1;
			}
			else
			{
				// Apply source value at the node
				// source_vector(local_idx) = nd.sourcevalue;
			}
		}

	}
	//
}






//________________________________________________________________________________________________


void modal_spectral_solver::set_global_matrix(const std::vector<int>& elem_nodes,
	int nen,
	const Eigen::MatrixXd& element_k_matrix,
	const Eigen::MatrixXd& element_m_matrix,
	std::vector<Eigen::Triplet<double>>& k_triplets,
	std::vector<Eigen::Triplet<double>>& m_triplets)
{

	for (int i = 0; i < nen; i++)
	{
		// get the global map id
		int i_node_map = this->nodeid_map[elem_nodes[i]];

		for (int j = 0; j < nen; j++)
		{
			// get the global map id
			int j_node_map = this->nodeid_map[elem_nodes[j]];

			double k_val = element_k_matrix(i, j);
			double m_val = element_m_matrix(i, j);


			k_triplets.emplace_back(i_node_map, j_node_map, k_val);
			m_triplets.emplace_back(i_node_map, j_node_map, m_val);

			// Note: Triplets don’t accumulate — Eigen accumulates when building the sparse matrix.
		}
	}
	//

	global_k_matrix.setFromTriplets(k_triplets.begin(), k_triplets.end());
	global_m_matrix.setFromTriplets(m_triplets.begin(), m_triplets.end());

}



void modal_spectral_solver::set_global_BC_flag_vector(const std::vector<int>& elem_nodes,
	int nen,
	const Eigen::VectorXi& element_BC_flag_vector, Eigen::VectorXi& global_BC_flag_vector)
{
	for (int i = 0; i < nen; i++)
	{
		// get the global map id
		int i_node_map = this->nodeid_map[elem_nodes[i]];

		global_BC_flag_vector(i_node_map) = element_BC_flag_vector(i);
	}
	//
}




void modal_spectral_solver::report_vandermondematrix_conditioning(const Eigen::MatrixXd& invVanderMondematrix)
{
	std::stringstream ss;

	// Compute condition number using singular value decomposition
	Eigen::JacobiSVD<Eigen::MatrixXd> svd(invVanderMondematrix);
	Eigen::VectorXd singular_values = svd.singularValues();

	double max_sv = singular_values(0);
	double min_sv = singular_values(singular_values.size() - 1);
	double cond_number = max_sv / min_sv;
	double log10_cond = std::log10(cond_number);

	// Compute numerical rank (singular values > tolerance)
	double tolerance = std::max(invVanderMondematrix.rows(), invVanderMondematrix.cols()) *
		singular_values(0) * std::numeric_limits<double>::epsilon();

	int rank = 0;
	for (int i = 0; i < singular_values.size(); ++i)
	{
		if (singular_values(i) > tolerance)
		{
			rank++;
		}
	}

	// Compute determinant (log scale to avoid overflow)
	double log_det = 0.0;
	for (int i = 0; i < rank; ++i)
	{
		log_det += std::log(singular_values(i));
	}


	int rows = invVanderMondematrix.rows();
	int cols = invVanderMondematrix.cols();

	// Additional statistics
	double min_sv_db = 20.0 * std::log10(min_sv);
	double max_sv_db = 20.0 * std::log10(max_sv);
	double dynamic_range_db = max_sv_db - min_sv_db;

	ss << "\n========================================";
	ss << "\n" << "Inverse VanderMonde Matrix Analysis : ";
	ss << "\n  Dimensions: " << rows << "x" << cols;
	ss << "\n  Rank: " << rank << "/" << std::min(rows, cols);
	ss << "\n  Condition number: " << std::scientific << std::setprecision(6) << cond_number;
	ss << " (10^" << std::fixed << std::setprecision(2) << log10_cond << ")";
	ss << "\n  Singular values:";
	ss << "\n    Max: " << std::scientific << std::setprecision(4) << max_sv;
	ss << " (" << std::fixed << std::setprecision(1) << max_sv_db << " dB)";
	ss << "\n    Min: " << std::scientific << std::setprecision(4) << min_sv;
	ss << " (" << std::fixed << std::setprecision(1) << min_sv_db << " dB)";
	ss << "\n    Dynamic range: " << std::fixed << std::setprecision(1) << dynamic_range_db << " dB";
	ss << "\n  Log determinant: " << std::scientific << std::setprecision(4) << log_det;




	// Warning for ill-conditioned matrix
	if (cond_number > 1e8)
	{
		ss << " [WARNING: Matrix is ill-conditioned!]";
	}
	else
	{
		ss << "\n  [OK] Matrix is well-conditioned";
	}
	ss << "\n========================================";


	report(ss.str().c_str());

}



void modal_spectral_solver::report(const char* msg)
{
	std::stringstream stopwatch_elapsed_str;

	stopwatch_elapsed_str << std::fixed << std::setprecision(6)
		<< this->m_stopwatch->elapsed();

	std::string final_msg = std::string(msg) + " " +
		stopwatch_elapsed_str.str() +
		" secs";

	if (m_callback)
		m_callback(final_msg.c_str());
	//
}



void modal_spectral_solver::store_results_with_index()
{
	// Open file in binary mode
	std::ofstream bin_file(output_file, std::ios::binary);

	if (!bin_file.is_open()) 
	{
		std::string error_msg = "Failed to open output file: " + this->output_file;
		report(error_msg.c_str());
		throw std::runtime_error("Failed to open output file: " + output_file);
	}

	// Get counts
	int32_t num_nodes = static_cast<int32_t>(spec_mesh2d.renderer_node_points.size());
	int32_t num_modes = static_cast<int32_t>(natural_frequencies.size());

	// Calculate offsets (will be updated after writing)
	uint64_t header_size = sizeof(BinaryFileHeader);
	uint64_t node_data_offset = header_size;

	// Node data size
	uint64_t node_data_size = num_nodes * (sizeof(int32_t) + 2 * sizeof(double));
	uint64_t edge_data_offset = node_data_offset + node_data_size;

	// Edge data size
	uint64_t edge_data_size = spec_mesh2d.renderer_edge_lines.size() * (2 * sizeof(int32_t));
	uint64_t triangle_data_offset = edge_data_offset + edge_data_size;

	// Triangle data size
	uint64_t triangle_data_size = spec_mesh2d.renderer_element_triangles.size() * (3 * sizeof(int32_t));
	uint64_t mode_index_offset = triangle_data_offset + triangle_data_size;

	// Mode index table size
	uint64_t mode_index_size = num_modes * sizeof(ModeIndexEntry);
	uint64_t mode_data_offset = mode_index_offset + mode_index_size;

	std::string success_msg = "";

	// Write header
	BinaryFileHeader header;
	std::memcpy(header.magic, "SEMF", 4);
	header.version = 2;
	header.num_modes = num_modes;
	header.num_nodes = num_nodes;
	header.mode_data_offset = mode_data_offset;
	header.mode_index_offset = mode_index_offset;

	bin_file.write(reinterpret_cast<const char*>(&header), sizeof(header));

	// Write nodes
	for (const auto& node : spec_mesh2d.renderer_node_points) 
	{
		int32_t node_id = static_cast<int32_t>(node.n_id);
		bin_file.write(reinterpret_cast<const char*>(&node_id), sizeof(int32_t));
		bin_file.write(reinterpret_cast<const char*>(&node.x), sizeof(double));
		bin_file.write(reinterpret_cast<const char*>(&node.y), sizeof(double));
	}

	report("Result mesh: Nodes written");

	// Write edges
	for (const auto& edge : spec_mesh2d.renderer_edge_lines) 
	{
		int32_t start_id = static_cast<int32_t>(edge.nstart);
		int32_t end_id = static_cast<int32_t>(edge.nend);
		bin_file.write(reinterpret_cast<const char*>(&start_id), sizeof(int32_t));
		bin_file.write(reinterpret_cast<const char*>(&end_id), sizeof(int32_t));
	}

	report("Result mesh: Edges written");

	// Write triangles
	for (const auto& tri : spec_mesh2d.renderer_element_triangles) 
	{
		int32_t n1 = static_cast<int32_t>(tri.n1);
		int32_t n2 = static_cast<int32_t>(tri.n2);
		int32_t n3 = static_cast<int32_t>(tri.n3);
		bin_file.write(reinterpret_cast<const char*>(&n1), sizeof(int32_t));
		bin_file.write(reinterpret_cast<const char*>(&n2), sizeof(int32_t));
		bin_file.write(reinterpret_cast<const char*>(&n3), sizeof(int32_t));
	}

	report("Result mesh: Triangles written");

	// Write mode index table (to be updated with offsets)
	std::vector<ModeIndexEntry> mode_index;
	uint64_t current_offset = mode_data_offset;

	for (int32_t i = 0; i < num_modes; i++) 
	{
		ModeIndexEntry entry;
		entry.mode_id = i;
		entry.frequency = natural_frequencies[i];
		entry.file_offset = current_offset;

		// Calculate data size for this mode: node_id + mode_value for each node
		entry.data_size = num_nodes * (sizeof(int32_t) + sizeof(double));

		mode_index.push_back(entry);
		current_offset += entry.data_size;
	}

	// Write mode index table
	for (const auto& entry : mode_index) 
	{
		bin_file.write(reinterpret_cast<const char*>(&entry), sizeof(ModeIndexEntry));
	}

	// Write mode data (each mode separately)
	for (int32_t mode_id = 0; mode_id < num_modes; mode_id++) 
	{
		// Write mode ID first (for redundancy)
		bin_file.write(reinterpret_cast<const char*>(&mode_id), sizeof(int32_t));

		// Write mode shape values for each node
		for (const auto& node : spec_mesh2d.renderer_node_points) 
		{
			int32_t node_id = static_cast<int32_t>(node.n_id);
			int nd_idx = nodeid_map[node.n_id];
			double mode_value = natural_modes(nd_idx, mode_id);

			bin_file.write(reinterpret_cast<const char*>(&node_id), sizeof(int32_t));
			bin_file.write(reinterpret_cast<const char*>(&mode_value), sizeof(double));
		}
	}

	bin_file.flush();
	bin_file.close();

	success_msg = "Modal results stored successfully: " + this->output_file +
				" (" + std::to_string(num_nodes) + " nodes, " +
				std::to_string(num_modes) + " modes)";

	report(success_msg.c_str());

}



void modal_spectral_solver::solveARPACKEigen(Eigen::VectorXd& eigenvalues,
	Eigen::MatrixXd& eigenvectors,
	int number_of_modes,
	const Eigen::SparseMatrix<double>& K,
	const Eigen::SparseMatrix<double>& M)
{
	int nev = number_of_modes; // std::min(static_cast<int>(K.rows()), 20); // Don't compute all eigenvalues! Use a reasonable number
	int ncv = std::min(2 * nev + 1, static_cast<int>(K.rows())); // Number of Lanczos vectors

	std::string solver_msg;

	// Matrix sizes
	solver_msg = "K size: " + std::to_string(K.rows()) + " x " + std::to_string(K.cols());
	report(solver_msg.c_str());

	solver_msg = "M size: " + std::to_string(M.rows()) + " x " + std::to_string(M.cols());
	report(solver_msg.c_str());

	// Eigenvalue info
	solver_msg = "Computing " + std::to_string(nev) + " eigenvalues";
	report(solver_msg.c_str());

	// Symmetry checks
	solver_msg = std::string("K symmetric? ") + (K.isApprox(K.transpose()) ? "true" : "false");
	report(solver_msg.c_str());

	solver_msg = std::string("M symmetric? ") + (M.isApprox(M.transpose()) ? "true" : "false");
	report(solver_msg.c_str());


	// Use the correct solver type
	Eigen::ArpackGeneralizedSelfAdjointEigenSolver<Eigen::SparseMatrix<double>> solver;

	// The compute() method expects: matrix K, matrix M, number of eigenvalues, 
	// spectrum type, and optionally ncv
	solver.compute(K, M, nev, "SM", Eigen::ComputeEigenvectors, ncv);

	// Check ARPACK's convergence status
	if (solver.info() != Eigen::Success) 
	{
		solver_msg = "ARPACK info: " + solver.info();
		report(solver_msg.c_str());

		throw std::runtime_error("ARPACK failed to converge.");
	}


	// Check if eigenvectors were computed
	if (!solver.eigenvectors().size()) 
	{
		solver_msg = "Eigenvectors not computed." + solver.info();
		report(solver_msg.c_str());

		throw std::runtime_error("Eigenvectors not computed.");
	}

	eigenvalues = solver.eigenvalues();
	eigenvectors = solver.eigenvectors();

}



//std::ofstream bin_file(this->output_file.c_str(), std::ios::binary);
//
//if (!bin_file.is_open())
//{
//	std::string error_msg = "Failed to open output file: " + this->output_file;
//	report(error_msg.c_str());
//	throw std::runtime_error(error_msg);
//}
//
//
////// Write header
////struct { char magic[4]; uint32_t version; } header = { {'S','E','M','F'}, 1 };
////bin_file.write(reinterpret_cast<const char*>(&header), sizeof(header));
//
//
//int32_t node_points_count = static_cast<int32_t>(spec_mesh2d.renderer_node_points.size());
//bin_file.write(reinterpret_cast<const char*>(&node_points_count), sizeof(int32_t));
//
//// Write the nodes
//for (const auto& node : spec_mesh2d.renderer_node_points)
//{
//	int32_t nodeid = static_cast<int32_t>(node.n_id);
//
//	// retrive the results
//	int nd_idx = nodeid_map[nodeid];
//
//	bin_file.write(reinterpret_cast<const char*>(&nodeid), sizeof(int32_t));
//	bin_file.write(reinterpret_cast<const char*>(&node.x), sizeof(double));
//	bin_file.write(reinterpret_cast<const char*>(&node.y), sizeof(double));
//}
//
//report("Result mesh: Nodes written");
//
//int32_t edge_lines_count = static_cast<int32_t>(spec_mesh2d.renderer_edge_lines.size());
//bin_file.write(reinterpret_cast<const char*>(&edge_lines_count), sizeof(int32_t));
//
//// Write the edges
//for (const auto& edge : spec_mesh2d.renderer_edge_lines)
//{
//	int32_t start_nodeid = static_cast<int32_t>(edge.nstart);
//	int32_t end_nodeid = static_cast<int32_t>(edge.nend);
//
//	bin_file.write(reinterpret_cast<const char*>(&start_nodeid), sizeof(int32_t));
//	bin_file.write(reinterpret_cast<const char*>(&end_nodeid), sizeof(int32_t));
//}
//
//report("Result mesh: Edges written");
//
//int32_t triangles_count = static_cast<int32_t>(spec_mesh2d.renderer_element_triangles.size());
//bin_file.write(reinterpret_cast<const char*>(&triangles_count), sizeof(int32_t));
//
//// Write the triangles
//for (const auto& tri : spec_mesh2d.renderer_element_triangles)
//{
//	int32_t n1 = static_cast<int32_t>(tri.n1);
//	int32_t n2 = static_cast<int32_t>(tri.n2);
//	int32_t n3 = static_cast<int32_t>(tri.n3);
//
//	bin_file.write(reinterpret_cast<const char*>(&n1), sizeof(int32_t));
//	bin_file.write(reinterpret_cast<const char*>(&n2), sizeof(int32_t));
//	bin_file.write(reinterpret_cast<const char*>(&n3), sizeof(int32_t));
//}
//
//report("Result mesh: Triangles written");
//
//
//// Report Success and file size
//std::string success_msg = "Result mesh written: " +
//this->output_file +
//" (" + std::to_string(node_points_count) + " nodes, " +
//std::to_string(triangles_count) + " triangles)";
//report(success_msg.c_str());
//
//
//// Write the Modal results
//
//// Write frequencies
//int32_t num_modes = static_cast<int32_t>(this->natural_frequencies.size());
//bin_file.write(reinterpret_cast<const char*>(&num_modes), sizeof(int32_t));
//
//
//for (int32_t i = 0; i < num_modes; i++)
//{
//	// Write the Mode ID
//	bin_file.write(reinterpret_cast<const char*>(&i), sizeof(int32_t));
//
//	// Write the Natural frequency
//	bin_file.write(reinterpret_cast<const char*>(&this->natural_frequencies[i]), sizeof(double));
//
//	// Write the mode shapes of each node
//	for (const auto& node : spec_mesh2d.renderer_node_points)
//	{
//		// retrive the results
//		int nd_idx = nodeid_map[node.n_id];
//
//		// Mode shape at node
//		double node_mode_value = this->natural_modes(nd_idx, i);
//
//		// Node id
//		int32_t node_id = static_cast<int32_t>(node.n_id);
//
//		bin_file.write(reinterpret_cast<const char*>(&node_id), sizeof(int32_t));
//		bin_file.write(reinterpret_cast<const char*>(&node_mode_value), sizeof(double));
//
//	}
//
//}
//
//
//bin_file.flush();
//
//auto file_size = bin_file.tellp();  // tellp() for output file (tellg() is for input)
//
//bin_file.close();
//
//// Report Success and file size
//success_msg = "Modal results stored successfully: " +
//this->output_file +
//" (" + std::to_string(node_points_count) + " nodes, " +
//std::to_string(num_modes) + " modes)";
//report(success_msg.c_str());










//// Write th natural frequencies to the binary file
//int32_t num_modes = static_cast<int32_t>(this->natural_frequencies.size());
//bin_file.write(reinterpret_cast<const char*>(&num_modes), sizeof(int32_t));
//
//// Write the natural frequencies
//for (double freq : this->natural_frequencies)
//{
//	bin_file.write(reinterpret_cast<const char*>(&freq), sizeof(double));
//}
//
//
//int32_t node_points_count = static_cast<int32_t>(spec_mesh2d.renderer_node_points.size());
//bin_file.write(reinterpret_cast<const char*>(&node_points_count), sizeof(int32_t));
//
//// Write the nodes
//for (const auto& node : spec_mesh2d.renderer_node_points)
//{
//	int32_t nodeid = static_cast<int32_t>(node.n_id);
//
//	// retrive the results
//	int nd_idx = nodeid_map[nodeid];
//
//	// Get the eigen vectors of the node (mode 1, mode 2, mode 3, ...)
//	Eigen::VectorXd node_modes = this->natural_modes.row(nd_idx);
//
//	// Node data (node id, x, y, mode 1 value, mode 2 value, ...)
//	bin_file.write(reinterpret_cast<const char*>(&nodeid), sizeof(int32_t));
//	bin_file.write(reinterpret_cast<const char*>(&node.x), sizeof(double));
//	bin_file.write(reinterpret_cast<const char*>(&node.y), sizeof(double));
//
//	// Write the eigen vector values for the node (mode 1, mode 2, mode 3, ...)
//	bin_file.write(reinterpret_cast<const char*>(node_modes.data()), node_modes.size() * sizeof(double));
//}
//
//report("Results: Nodes written");
//
//int32_t edge_lines_count = static_cast<int32_t>(spec_mesh2d.renderer_edge_lines.size());
//bin_file.write(reinterpret_cast<const char*>(&edge_lines_count), sizeof(int32_t));
//
//// Write the edges
//for (const auto& edge : spec_mesh2d.renderer_edge_lines)
//{
//	int32_t start_nodeid = static_cast<int32_t>(edge.nstart);
//	int32_t end_nodeid = static_cast<int32_t>(edge.nend);
//
//	bin_file.write(reinterpret_cast<const char*>(&start_nodeid), sizeof(int32_t));
//	bin_file.write(reinterpret_cast<const char*>(&end_nodeid), sizeof(int32_t));
//}
//
//report("Results: Edges written");
//
//int32_t triangles_count = static_cast<int32_t>(spec_mesh2d.renderer_element_triangles.size());
//bin_file.write(reinterpret_cast<const char*>(&triangles_count), sizeof(int32_t));
//
//// Write the triangles
//for (const auto& tri : spec_mesh2d.renderer_element_triangles)
//{
//	int32_t n1 = static_cast<int32_t>(tri.n1);
//	int32_t n2 = static_cast<int32_t>(tri.n2);
//	int32_t n3 = static_cast<int32_t>(tri.n3);
//
//	bin_file.write(reinterpret_cast<const char*>(&n1), sizeof(int32_t));
//	bin_file.write(reinterpret_cast<const char*>(&n2), sizeof(int32_t));
//	bin_file.write(reinterpret_cast<const char*>(&n3), sizeof(int32_t));
//}
//
//report("Results: Triangles written");
