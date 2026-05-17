#include "modal_spectral_solver.h"

modal_spectral_solver::modal_spectral_solver()
{
	// Empty constructor
}

void modal_spectral_solver::init(helmholtz_system_store * helmholtz_2dsystem_ptr, 
	const char* output_file, stopwatch_events * stopwatch, void(*callback)(const char*))
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
		this->triangle_quadrature_points = gll_utility::get_triangle_quadrature(spectral_order);
		this->triangle_basis_terms = gll_utility::build_basis_terms(spectral_order);
		this->inv_vandermonde_matrix = gll_utility::get_inverse_vandermonde_matrix(spectral_order);

		// Get the gll locations and gll weights for the given spectral order 
		this->gll_locations = gll_utility::get_gll_locations(spectral_order);
		this->gll_weights = gll_utility::get_gll_weights(spectral_order, gll_locations);

		report("Triangle Element Quadrature Points Created");

		// Triangle elements
		for (auto& tri_elm_m : spec_mesh2d.spectral_trielement_list)
		{
			// get the element
			spectral_trielement_store tri_elm = tri_elm_m.second;

			//________________________________________________________________________________________________
			// Step 1: Create local node & node coordinate list
			// Build local node list _______________________________________________
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
		this->quadrilateral_quadrature_points = gll_utility::get_quadrilateral_quadrature(spectral_order);

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
			std::vector<int> elem_nodes;

			for (int i = 0; i < 4; i++)
			{
				// Add the corner i (0, 1, 2, 3)
				elem_nodes.push_back(quad_elm.corner_nodes[i]);

				// Then the edge i (0, 1, 2, 3)
				for (const auto& edge1_ndid : quad_elm.edge_node_ids[i])
				{
					elem_nodes.push_back(edge1_ndid);
				}
				//
			}

			// internal nodes
			for (int internal_ndid : quad_elm.internal_nodes)
			{
				elem_nodes.push_back(internal_ndid);
			}


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

	//
}




void modal_spectral_solver::solve_modal_analysis(int inpt_num_modes)
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

	Eigen::VectorXd eigenvalues;
	Eigen::MatrixXd eigenvectors;


	if (eigs.info() == CompInfo::Successful)
	{
		eigenvalues = eigs.eigenvalues();
		eigenvectors = eigs.eigenvectors();
	}
	else
	{
		report("Eigen solver failed!");
		return;
	}

	report("Eigen solve successful!");

	// 4) Convert eigenvalues => frequencies
	std::vector<double> frequencies;

	for (int i = 0; i < eigenvalues.size(); ++i)
	{
		double lambda = eigenvalues[i];

		if (lambda > 0.0)
		{
			double omega = std::sqrt(lambda);
			double freq = omega / (2.0 * M_PI);
			frequencies.push_back(freq);
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
	Eigen::MatrixXd full_modes = Eigen::MatrixXd::Zero(total_dofs, eigenvectors.cols());

	for (int i = 0; i < n_free; ++i)
	{
		full_modes.row(free_dofs[i]) = eigenvectors.row(i);
	}



	report("Eigen vectors stored!");

}










void modal_spectral_solver::get_trielement_k_grad_k_mass_matrix(const std::vector<int>& elem_nodes,
	const std::vector<Eigen::Vector2d>& elem_coords,
	Eigen::MatrixXd& element_k_matrix,
	Eigen::MatrixXd& element_m_matrix)
{
	int nen = static_cast<int>(elem_nodes.size());

	// --- 1. Loop over quadrature points ---
	for (int q = 0; q < static_cast<int>(triangle_quadrature_points.size()); q++)
	{
		double quadraturept_xi = triangle_quadrature_points[q].xi;
		double quadraturept_eta = triangle_quadrature_points[q].eta;
		double w = triangle_quadrature_points[q].weight; // weights are normalized to 1.0

		// --- 2. Evaluate shape functions ---
		Eigen::VectorXd N(nen);
		Eigen::MatrixXd dN_dxi(nen, 2); // [dN/dxi, dN/deta]

		evaluate_triangle_shape_functions(quadraturept_xi, quadraturept_eta, nen,
			N, dN_dxi);

		// --- 3. Compute Jacobian ---
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
				element_k_matrix(i, j) += (dN_dx.row(i).dot(dN_dx.row(j))) * detJ * w;

				// Mass
				element_m_matrix(i, j) += (N(i) * N(j)) * detJ * w;
			}
		}
	}
	//
}


void modal_spectral_solver::evaluate_triangle_shape_functions(double quadraturept_xi,
	double quadraturept_eta, int nen,
	Eigen::VectorXd& N,
	Eigen::MatrixXd& dN_dxi)
{
	// Calculate the polynomial basis function
	Eigen::VectorXd phi, dphi_dxi, dphi_deta;

	gll_utility::evaluate_basis_phi(quadraturept_xi, quadraturept_eta, this->triangle_basis_terms, phi);
	gll_utility::evaluate_basis_derivatives(quadraturept_xi, quadraturept_eta, this->triangle_basis_terms, dphi_dxi, dphi_deta);

	// Shape functions
	N = this->inv_vandermonde_matrix * phi;

	// Derivatives of Shape functions
	Eigen::VectorXd dN_dxi_vec = this->inv_vandermonde_matrix * dphi_dxi;
	Eigen::VectorXd dN_deta_vec = this->inv_vandermonde_matrix * dphi_deta;

	for (int i = 0; i < nen; i++)
	{
		dN_dxi(i, 0) = dN_dxi_vec(i);
		dN_dxi(i, 1) = dN_deta_vec(i);
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

			for (int j = 0; j < spec_mesh2d.spectral_order + 1; j++)
			{
				int local_idx = (i * spec_mesh2d.spectral_order) + j;
				// dirichlet_vector(local_idx) = q_edge;
				dirichlet_BC_flag(local_idx) = 1;
			}
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
			int local_idx = (i * spec_mesh2d.spectral_order);

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

	// --- 1. Loop over quadrature points ---
	for (int q = 0; q < static_cast<int>(quadrilateral_quadrature_points.size()); q++)
	{
		double quadraturept_xi = quadrilateral_quadrature_points[q].xi;
		double quadraturept_eta = quadrilateral_quadrature_points[q].eta;
		double w = quadrilateral_quadrature_points[q].weight;

		// --- 2. Evaluate shape functions ---
		Eigen::VectorXd N(nen);
		Eigen::MatrixXd dN_dxi(nen, 2); // [dN/dxi, dN/deta]

		evaluate_quadrilateral_shape_functions(quadraturept_xi, quadraturept_eta, nen,
			N, dN_dxi);

		// --- 3. Compute Jacobian ---
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
		// element_k_grad_matrix += (dN_dx * dN_dx.transpose()) * detJ * w;
		// element_k_mass_matrix += (N * N.transpose()) * detJ * w;

		for (int i = 0; i < nen; i++)
		{
			for (int j = 0; j < nen; j++)
			{
				// Gradient (stiffness)
				element_k_matrix(i, j) += (dN_dx.row(i).dot(dN_dx.row(j))) * detJ * w;

				// Mass
				element_m_matrix(i, j) += (N(i) * N(j)) * detJ * w;
			}
		}
	}
	//
}



void modal_spectral_solver::evaluate_quadrilateral_shape_functions(double quadraturept_xi,
	double quadraturept_eta, int nen,
	Eigen::VectorXd& N,
	Eigen::MatrixXd& dN_dxi)
{
	int p = spec_mesh2d.spectral_order;
	int n1d = p + 1;

	// --- 1D shape functions ---
	std::vector<double> lx(n1d), d_lx(n1d);
	std::vector<double> ly(n1d), d_ly(n1d);

	// Evaluate 1D Lagrange basis at xi and eta
	gll_utility::evaluate_lagrange_1D(quadraturept_xi, gll_locations, lx, d_lx);
	gll_utility::evaluate_lagrange_1D(quadraturept_eta, gll_locations, ly, d_ly);

	// --- Allocate ---
	N.resize(nen);
	dN_dxi.resize(nen, 2);

	// --- Tensor product assembly ---
	int idx = 0;

	for (int j = 0; j < n1d; j++)
	{
		for (int i = 0; i < n1d; i++)
		{
			// Shape function
			N(idx) = lx[i] * ly[j];

			// Derivatives
			dN_dxi(idx, 0) = d_lx[i] * ly[j]; // d/dxi
			dN_dxi(idx, 1) = lx[i] * d_ly[j]; // d/deta

			idx++;
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

			for (int j = 0; j < spec_mesh2d.spectral_order + 1; j++)
			{
				int local_idx = (i * spec_mesh2d.spectral_order) + j;
				// dirichlet_vector(local_idx) = q_edge;
				dirichlet_BC_flag(local_idx) = 1;
			}
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
			int local_idx = (i * spec_mesh2d.spectral_order);

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






void modal_spectral_solver::store_results()
{

	int32_t node_points_count = static_cast<int32_t>(spec_mesh2d.renderer_node_points.size());
	bin_file.write(reinterpret_cast<const char*>(&node_points_count), sizeof(int32_t));

	// Write the nodes
	for (const auto& node : spec_mesh2d.renderer_node_points)
	{
		int32_t nodeid = static_cast<int32_t>(node.n_id);
		// double rand_result = std::sin(node.x * 10.0) * std::cos(node.y * 10.0); // Random value between 0 and 1

		// retrive the results
		int nd_idx = nodeid_map[nodeid];
		double uR = this->u_real(nd_idx);
		double uI = this->u_imag(nd_idx);
		double uMag = std::abs(this->u_complex(nd_idx));
		double uPhase = std::arg(this->u_complex(nd_idx));

		bin_file.write(reinterpret_cast<const char*>(&nodeid), sizeof(int32_t));
		bin_file.write(reinterpret_cast<const char*>(&node.x), sizeof(double));
		bin_file.write(reinterpret_cast<const char*>(&node.y), sizeof(double));
		bin_file.write(reinterpret_cast<const char*>(&uR), sizeof(double));
		bin_file.write(reinterpret_cast<const char*>(&uI), sizeof(double));
		bin_file.write(reinterpret_cast<const char*>(&uMag), sizeof(double));
		bin_file.write(reinterpret_cast<const char*>(&uPhase), sizeof(double));
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









