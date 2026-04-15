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


	// get the quadrature points and the basis term
	int spectral_order = spec_mesh2d.spectral_order;
	this->triangle_quadrature_points = gll_utility::get_triangle_quadrature(spectral_order);
	this->triangle_basis_terms = gll_utility::build_basis_terms(spectral_order);
	this->inv_vandermonde_matrix = gll_utility::get_inverse_vandermonde_matrix(spectral_order);

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

		Eigen::MatrixXd element_k_grad_matrix = Eigen::MatrixXd::Zero(nen, nen); // Element k grad matrix
		Eigen::MatrixXd element_k_mass_matrix = Eigen::MatrixXd::Zero(nen, nen); // Element k mass matrix


		get_trielement_k_grad_k_mass_matrix(elem_nodes, elem_coords,
			element_k_grad_matrix, element_k_mass_matrix);

		double wave_number = spec_mesh2d.material_list[tri_elm.materialid].wave_number; // get the material wave number

		// Helmholtz operator
		Eigen::MatrixXd element_k_matrix = Eigen::MatrixXd::Zero(nen, nen);

		element_k_matrix = element_k_grad_matrix - ((wave_number * wave_number) * element_k_mass_matrix);

		//________________________________________________________________________________________________
		// Step 3: Create Sommerfield Absorbtion Boundary Condition matrix
		Eigen::MatrixXcd element_kI_matrix = Eigen::MatrixXcd::Zero(nen, nen); // Element kI matrix

		get_trielement_kI_matrix(tri_elm, elem_coords, nen, wave_number, element_kI_matrix);

		//________________________________________________________________________________________________
		// Step 4: Create Element field vector
		Eigen::VectorXd element_field_vector = Eigen::VectorXd::Zero(nen); // Element field vector
	
		get_trielement_field_vector(tri_elm, element_field_vector);


		//________________________________________________________________________________________________
		// Step 5: Create Element normal derivative field vector
		Eigen::VectorXd element_normderivfield_vector = Eigen::VectorXd::Zero(nen); // Element normal derivative field vector

		get_trielement_normderivfield_vector(tri_elm, elem_coords , nen, element_normderivfield_vector);

		//________________________________________________________________________________________________
		// Step 6: Create Element source vector
		Eigen::VectorXd element_source_vector = Eigen::VectorXd::Zero(nen); // Element source vector

		get_trielement_source_vector(tri_elm, element_field_vector, element_source_vector);





	}

	this->quadrilateral_quadrature_points = gll_utility::get_quadrilateral_quadrature(spectral_order);


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

		Eigen::MatrixXd element_k_grad_matrix = Eigen::MatrixXd::Zero(nen, nen); // Element k grad matrix
		Eigen::MatrixXd element_k_mass_matrix = Eigen::MatrixXd::Zero(nen, nen); // Element k mass matrix


		get_quadelement_k_grad_k_mass_matrix(elem_nodes, elem_coords,
			element_k_grad_matrix, element_k_mass_matrix);

		double wave_number = spec_mesh2d.material_list[quad_elm.materialid].wave_number; // get the material wave number

		// Helmholtz operator
		Eigen::MatrixXd element_k_matrix = Eigen::MatrixXd::Zero(nen, nen);

		element_k_matrix = element_k_grad_matrix - ((wave_number * wave_number) * element_k_mass_matrix);

		//________________________________________________________________________________________________
		// Step 3: Create Sommerfield Absorbtion Boundary Condition matrix
		Eigen::MatrixXcd element_kI_matrix = Eigen::MatrixXcd::Zero(nen, nen); // Element kI matrix

		get_quadelement_kI_matrix(quad_elm, elem_coords, nen, wave_number, element_kI_matrix);

		//________________________________________________________________________________________________
		// Step 4: Create Element field vector
		Eigen::VectorXd element_field_vector = Eigen::VectorXd::Zero(nen); // Element field vector

		get_quadelement_field_vector(quad_elm, element_field_vector);


		//________________________________________________________________________________________________
		// Step 5: Create Element normal derivative field vector
		Eigen::VectorXd element_normderivfield_vector = Eigen::VectorXd::Zero(nen); // Element normal derivative field vector

		get_quadelement_normderivfield_vector(quad_elm, elem_coords, nen, element_normderivfield_vector);

		//________________________________________________________________________________________________
		// Step 6: Create Element source vector
		Eigen::VectorXd element_source_vector = Eigen::VectorXd::Zero(nen); // Element source vector

		get_quadelement_source_vector(quad_elm, element_field_vector, element_source_vector);





	}




	// Write the results
	store_results();


	// close the bin file
	bin_file.close();
	//
}


void helmholtz2d_spectral_solver::get_trielement_k_grad_k_mass_matrix(const std::vector<int>& elem_nodes,
	const std::vector<Eigen::Vector2d>& elem_coords,
	Eigen::MatrixXd& element_k_grad_matrix,
	Eigen::MatrixXd& element_k_mass_matrix)
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
				element_k_grad_matrix(i, j) += (dN_dx.row(i).dot(dN_dx.row(j))) * detJ * w;

				// Mass
				element_k_mass_matrix(i, j) += (N(i) * N(j)) * detJ * w;
			}
		}
	}
	//
}


void helmholtz2d_spectral_solver::evaluate_triangle_shape_functions(double quadraturept_xi,
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


void helmholtz2d_spectral_solver::get_trielement_kI_matrix(const spectral_trielement_store& tri_elm,
	const std::vector<Eigen::Vector2d>& elem_coords,
	int nen,
	double wave_number,
	Eigen::MatrixXcd& element_kI_matrix)
{

	for (int i = 0; i < 3; i++)
	{
		// Get the edge id
		int edge_id = tri_elm.edge_ids[i];

		const spectral_edge_store& edge = spec_mesh2d.spectral_edge_list[edge_id];

		if (edge.isboundaryedge == true && edge.isSommerfieldBC == true)
		{
			for (int j = 0; j < spec_mesh2d.spectral_order + 1; j++)
			{
				// Map the 1D GLL point to [0, 1]
				double s = (this->gll_locations[j] + 1.0) * 0.5;

				// Get the weigths
				double w = this->gll_weights[j] * 0.5;

				double xi, eta;

				// Map the edge
				if (i == 0)
				{
					// Edge 1 (0,0) to (1,0)
					xi = s;
					eta = 0.0;
				}
				else if (i == 1)
				{
					// Edge 2 (1,0) to (0, 1)
					xi = 1.0 - s;
					eta = s;
				}
				else if (i == 2)
				{
					// Edge 3 (0,1) to (0,0)
					xi = 0.0;
					eta = 1.0 - s;
				}

				// --- Shape functions ---
				Eigen::VectorXd N;
				Eigen::MatrixXd dN_dxi;

				evaluate_triangle_shape_functions(xi, eta, nen, N, dN_dxi);

				// --- Compute physical edge length Jacobian ---
				Eigen::Vector2d dx_ds = Eigen::Vector2d::Zero();

				// chain rule: dx/ds = dx/dxi * dxi/ds + dx/deta * deta/ds
				double dxi_ds, deta_ds;

				if (edge_id == 0) { dxi_ds = 1.0;  deta_ds = 0.0; }
				else if (edge_id == 1) { dxi_ds = -1.0; deta_ds = 1.0; }
				else if (edge_id == 2) { dxi_ds = 0.0;  deta_ds = -1.0; }

				for (int k = 0; k < nen; k++)
				{
					double dN_ds = dN_dxi(k, 0) * dxi_ds + dN_dxi(k, 1) * deta_ds;

					dx_ds += dN_ds * elem_coords[k];
				}

				double J_edge = dx_ds.norm();

				// --- Assembly ---
				for (int a = 0; a < nen; a++)
				{
					for (int b = 0; b < nen; b++)
					{
						element_kI_matrix(a, b) += std::complex<double>(0, wave_number) *
							N(a) * N(b) * J_edge * w;
					}
				}
				//
			}
			//
		}
		//
	}
	//
}



void helmholtz2d_spectral_solver::get_trielement_field_vector(const spectral_trielement_store& tri_elm,
	Eigen::VectorXd& dirichlet_vector)
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
				dirichlet_vector(local_idx) = q_edge;
			}
			//
		}
		//
	}
	//
}


void helmholtz2d_spectral_solver::get_trielement_normderivfield_vector(const spectral_trielement_store& tri_elm,
	const std::vector<Eigen::Vector2d>& elem_coords,
	int nen,
	Eigen::VectorXd& neumann_vector)
{
	for (int i = 0; i < 3; i++)
	{
		// Get the edge id
		int edge_id = tri_elm.edge_ids[i];

		const spectral_edge_store& edge = spec_mesh2d.spectral_edge_list[edge_id];

		if (edge.isboundaryedge == true && edge.isDerivFieldBC == true)
		{
			for (int j = 0; j < spec_mesh2d.spectral_order + 1; j++)
			{
				// Map the 1D GLL point to [0, 1]
				double s = (this->gll_locations[j] + 1.0) * 0.5;

				// Get the weigths
				double w = this->gll_weights[j] * 0.5;

				double xi, eta;

				// Map the edge
				if (i == 0)
				{
					// Edge 1 (0,0) to (1,0)
					xi = s;
					eta = 0.0;
				}
				else if (i == 1)
				{
					// Edge 2 (1,0) to (0, 1)
					xi = 1.0 - s;
					eta = s;
				}
				else if (i == 2)
				{
					// Edge 3 (0,1) to (0,0)
					xi = 0.0;
					eta = 1.0 - s;
				}

				// --- Shape functions ---
				Eigen::VectorXd N;
				Eigen::MatrixXd dN_dxi;

				evaluate_triangle_shape_functions(xi, eta, nen, N, dN_dxi);

				// --- Compute physical edge length Jacobian ---
				Eigen::Vector2d dx_ds = Eigen::Vector2d::Zero();

				// chain rule: dx/ds = dx/dxi * dxi/ds + dx/deta * deta/ds
				double dxi_ds = 0.0;
				double deta_ds = 0.0;

				if (edge_id == 0) { dxi_ds = 1.0;  deta_ds = 0.0; }
				else if (edge_id == 1) { dxi_ds = -1.0; deta_ds = 1.0; }
				else if (edge_id == 2) { dxi_ds = 0.0;  deta_ds = -1.0; }

				for (int k = 0; k < nen; k++)
				{
					double dN_ds = dN_dxi(k, 0) * dxi_ds + dN_dxi(k, 1) * deta_ds;

					dx_ds += dN_ds * elem_coords[k];
				}

				double J_edge = dx_ds.norm();

				// --- Assembly ---
				// Neumann (flux) BC contribution
				double dq_edge = edge.normalderivfieldvalue;  // flux value

				for (int a = 0; a < nen; a++)
				{
					neumann_vector(a) += dq_edge * N(a) * J_edge * w;
				}
				//
			}
			//
		}
		//
	}
	//
}


void helmholtz2d_spectral_solver::get_trielement_source_vector(const spectral_trielement_store& tri_elm,
	Eigen::VectorXd& dirichlet_vector,
	Eigen::VectorXd& source_vector)
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
				dirichlet_vector(local_idx) = nd.fieldvalue;
			}
			else
			{
				// Apply source value at the node
				source_vector(local_idx) = nd.sourcevalue;
			}
		}

	}
	//
}




//________________________________________________________________________________________________

void helmholtz2d_spectral_solver::get_quadelement_k_grad_k_mass_matrix(const std::vector<int>& elem_nodes,
	const std::vector<Eigen::Vector2d>& elem_coords,
	Eigen::MatrixXd& element_k_grad_matrix,
	Eigen::MatrixXd& element_k_mass_matrix)
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
				element_k_grad_matrix(i, j) += (dN_dx.row(i).dot(dN_dx.row(j))) * detJ * w;

				// Mass
				element_k_mass_matrix(i, j) += (N(i) * N(j)) * detJ * w;
			}
		}
	}
	//
}



void helmholtz2d_spectral_solver::evaluate_quadrilateral_shape_functions(double quadraturept_xi,
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


void helmholtz2d_spectral_solver::get_quadelement_kI_matrix(const spectral_quadelement_store& quad_elm,
	const std::vector<Eigen::Vector2d>& elem_coords,
	int nen,
	double wave_number,
	Eigen::MatrixXcd& element_kI_matrix)
{

}


void helmholtz2d_spectral_solver::get_quadelement_field_vector(const spectral_quadelement_store& quad_elm,
	Eigen::VectorXd& dirichlet_vector)
{

}

void helmholtz2d_spectral_solver::get_quadelement_normderivfield_vector(const spectral_quadelement_store& quad_elm,
	const std::vector<Eigen::Vector2d>& elem_coords,
	int nen,
	Eigen::VectorXd& neumann_vector)
{

}


void helmholtz2d_spectral_solver::get_quadelement_source_vector(const spectral_quadelement_store& quad_elm,
	Eigen::VectorXd& dirichlet_vector,
	Eigen::VectorXd& source_vector)
{


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

