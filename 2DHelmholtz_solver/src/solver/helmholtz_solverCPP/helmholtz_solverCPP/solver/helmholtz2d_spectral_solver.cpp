#include "helmholtz2d_spectral_solver.h"

helmholtz2d_spectral_solver::helmholtz2d_spectral_solver()
{
	// Empty constructor

}


void helmholtz2d_spectral_solver::init(helmholtz_system_store* helmholtz_2dsystem_ptr,
	const char* output_file_char,
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
		// CRITICAL: Copy the string to std::string for permanent storage
	this->output_file = std::string(output_file_char);

	std::string msg = "Output file set to: " + this->output_file;
	report(msg.c_str());

}


void helmholtz2d_spectral_solver::create_global_matrices()
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


	// Global k Matrix (Ke - k^2 * Me) + kI Global kI Matrix Boundary impedance matrix (Absorbing Boundary condition - Sommerfield)
	global_system_matrix.resize(numDOF, numDOF);
	global_system_matrix.setZero();


	// std::vector<Eigen::Triplet<double>> triplets_K;
	std::vector<Eigen::Triplet<std::complex<double>>> triplets_system;

	global_field_vector.setZero(numDOF); // Global field Vector
	global_normalderivfield_vector.setZero(numDOF); // Global derivative normal field Vector
	global_source_vector.setZero(numDOF); // Global source Vector

	global_dirichlet_BC_flags_vector.setZero(numDOF); // Global boundary condition Vector (To track the nodes where prescribed field is applied)


	// get the quadrature points and the basis term
	int spectral_order = spec_mesh2d.spectral_order;


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
			Eigen::VectorXi element_field_BC_flag_vector = Eigen::VectorXi::Zero(nen); // Element field vector BC flag
			Eigen::VectorXd element_field_vector = Eigen::VectorXd::Zero(nen); // Element field vector


			get_trielement_field_vector(tri_elm, element_field_BC_flag_vector, element_field_vector);


			//________________________________________________________________________________________________
			// Step 5: Create Element normal derivative field vector
			Eigen::VectorXd element_normderivfield_vector = Eigen::VectorXd::Zero(nen); // Element normal derivative field vector

			get_trielement_normderivfield_vector(tri_elm, elem_coords, nen, element_normderivfield_vector);

			//________________________________________________________________________________________________
			// Step 6: Create Element source vector
			Eigen::VectorXd element_source_vector = Eigen::VectorXd::Zero(nen); // Element source vector

			get_trielement_source_vector(tri_elm, element_field_BC_flag_vector,
				element_field_vector, element_source_vector);


			//________________________________________________________________________________________________
			// Step 7: Set the global matrix and global vector

			set_complex_global_matrix(elem_nodes, nen,
				element_k_matrix,
				element_kI_matrix,
				triplets_system);


			set_global_vector(elem_nodes, nen,
				element_field_vector, global_field_vector);

			set_global_vector(elem_nodes, nen,
				element_normderivfield_vector, global_normalderivfield_vector);

			set_global_vector(elem_nodes, nen,
				element_source_vector, global_source_vector);


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
			Eigen::VectorXi element_field_BC_flag_vector = Eigen::VectorXi::Zero(nen); // Element field vector BC flag
			Eigen::VectorXd element_field_vector = Eigen::VectorXd::Zero(nen); // Element field vector

			get_quadelement_field_vector(quad_elm, element_field_BC_flag_vector, element_field_vector);


			//________________________________________________________________________________________________
			// Step 5: Create Element normal derivative field vector
			Eigen::VectorXd element_normderivfield_vector = Eigen::VectorXd::Zero(nen); // Element normal derivative field vector

			get_quadelement_normderivfield_vector(quad_elm, elem_coords, nen, element_normderivfield_vector);

			//________________________________________________________________________________________________
			// Step 6: Create Element source vector
			Eigen::VectorXd element_source_vector = Eigen::VectorXd::Zero(nen); // Element source vector

			get_quadelement_source_vector(quad_elm, element_field_BC_flag_vector,
				element_field_vector, element_source_vector);

			//________________________________________________________________________________________________
			// Step 7: Set the global matrix and global vector

			set_complex_global_matrix(elem_nodes, nen,
				element_k_matrix,
				element_kI_matrix,
				triplets_system);


			set_global_vector(elem_nodes, nen,
				element_field_vector, global_field_vector);

			set_global_vector(elem_nodes, nen,
				element_normderivfield_vector, global_normalderivfield_vector);

			set_global_vector(elem_nodes, nen,
				element_source_vector, global_source_vector);


			set_global_BC_flag_vector(elem_nodes, nen,
				element_field_BC_flag_vector, global_dirichlet_BC_flags_vector);

		}

		report("Quadrilateral Spectral Elements Global Matrices Created");
		//
	}

	// Set the global sparse matrix
	global_system_matrix.setFromTriplets(triplets_system.begin(), triplets_system.end());

	// Create the message string and convert to const char*
	std::string sizeMsg = "Global system matrix created. Size: " +
		std::to_string(global_system_matrix.rows()) + "x" +
		std::to_string(global_system_matrix.cols()) +
		", Non-zeros: " + std::to_string(global_system_matrix.nonZeros());

	report(sizeMsg.c_str());

}


void helmholtz2d_spectral_solver::solve_helmholtz_matrices(const int& solver_type)
{

	// Matrix formation end
	// Solve the helmholtz equation
	//  ([A] + i[B]) [u] = [f] + [du/dn]


	Eigen::VectorXcd u;

	if (solver_type == 0)
	{
		solve_dirichlet_BCs_elimination_method(u);

	}
	else
	{
		solve_dirichlet_BCs_lagrange_method(u);

	}

	
	// post - process
	this->u_complex = u;
	this->u_real.resize(numDOF);
	this->u_imag.resize(numDOF);

	for (int i = 0; i < numDOF; i++)
	{
		double real_part = std::real(u(i));
		double imag_part = std::imag(u(i));

		u_real(i) = real_part;
		u_imag(i) = imag_part;

	}


	// Write the results
	store_results();

}


void helmholtz2d_spectral_solver::get_trielement_k_grad_k_mass_matrix(const std::vector<int>& elem_nodes,
	const std::vector<Eigen::Vector2d>& elem_coords,
	Eigen::MatrixXd& element_k_grad_matrix,
	Eigen::MatrixXd& element_k_mass_matrix)
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
				element_k_grad_matrix(i, j) += grad_dot * dV;

				// Mass
				element_k_mass_matrix(i, j) += (N(i) * N(j)) * dV;
			}
		}
	}
	//
}


void helmholtz2d_spectral_solver::get_trielement_kI_matrix(const spectral_trielement_store& tri_elm,
	const std::vector<Eigen::Vector2d>& elem_coords,
	int nen,
	double wave_number,
	Eigen::MatrixXcd& element_kI_matrix)
{

	const std::complex<double> factor(0.0, wave_number);

	for (int i = 0; i < 3; i++)
	{
		// Get the edge id
		int edge_id = tri_elm.edge_ids[i];

		const spectral_edge_store& edge = spec_mesh2d.spectral_edge_list[edge_id];

		if (edge.isboundaryedge == true && edge.isSommerfieldBC == true)
		{

			int n_gll_points = spec_mesh2d.spectral_order + 1;

			// Pre-compute edge mapping parameters
			auto get_edge_coordinates = [i](double s, double& xi, double& eta,
				double& dxi_ds, double& deta_ds) 
				{
					if (i == 0) 
					{      // Bottom edge
						xi = s;  eta = 0.0;
						dxi_ds = 1.0;  deta_ds = 0.0;
					}
					else if (i == 1) 
					{ // Diagonal edge
						xi = 1.0 - s;  eta = s;
						dxi_ds = -1.0;  deta_ds = 1.0;
					}
					else 
					{             // Left edge
						xi = 0.0;  eta = 1.0 - s;
						dxi_ds = 0.0;  deta_ds = -1.0;
					}
				};


			for (int j = 0; j < n_gll_points; j++)
			{
				// Map the 1D GLL point to [0, 1]
				double s = (this->gll_locations[j] + 1.0) * 0.5;

				// Get the weigths
				double wt = this->gll_weights[j] * 0.5;

				double xi, eta, dxi_ds, deta_ds;
				get_edge_coordinates(s, xi, eta, dxi_ds, deta_ds);


				// --- Shape functions ---
				Eigen::VectorXd N(nen);
				Eigen::VectorXd dN_dxi(nen); // [dN/dxi, dN/deta]
				Eigen::VectorXd dN_deta(nen); // [dN/dxi, dN/deta]

				spectral_tri_element::evaluate_triangle_shape_functions(xi, eta, spec_mesh2d.spectral_order,
					this->inv_vandermonde_matrix, this->triangle_basis_terms,
					N, dN_dxi, dN_deta);


				// --- Compute physical edge length Jacobian ---
				Eigen::Vector2d dx_ds = Eigen::Vector2d::Zero();

				for (int k = 0; k < nen; k++)
				{
					double dN_ds = dN_dxi(k) * dxi_ds + dN_deta(k) * deta_ds;

					dx_ds += dN_ds * elem_coords[k];
				}

				double J_edge = dx_ds.norm();
				double dV = J_edge * wt;

				// --- Assembly ---
				for (int a = 0; a < nen; a++)
				{
					for (int b = 0; b < nen; b++)
					{
						element_kI_matrix(a, b) += factor * N(a) * N(b) * dV;
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
	Eigen::VectorXi& dirichlet_BC_flag,
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

			//for (int j = 0; j < spec_mesh2d.spectral_order + 1; j++)
			//{
			//	int local_idx = (i * spec_mesh2d.spectral_order) + j;
			//	dirichlet_vector(local_idx) = q_edge;
			//	dirichlet_BC_flag(local_idx) = 1;
			//}


			int local_idx = spec_mesh2d.tri_element_id_structure.corner_nodes[i];

			dirichlet_vector(local_idx) = q_edge;
			dirichlet_BC_flag(local_idx) = 1;

			for (const int& j : spec_mesh2d.tri_element_id_structure.edge_node_ids[i])
			{
				local_idx = j;

				dirichlet_vector(local_idx) = q_edge;
				dirichlet_BC_flag(local_idx) = 1;
			}

			local_idx = spec_mesh2d.tri_element_id_structure.corner_nodes[(i + 1) % 3];

			dirichlet_vector(local_idx) = q_edge;
			dirichlet_BC_flag(local_idx) = 1;


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
			
			int n_gll_points = spec_mesh2d.spectral_order + 1;

			// Pre-compute edge mapping parameters
			auto get_edge_coordinates = [i](double s, double& xi, double& eta,
				double& dxi_ds, double& deta_ds)
				{
					if (i == 0)
					{      // Bottom edge
						xi = s;  eta = 0.0;
						dxi_ds = 1.0;  deta_ds = 0.0;
					}
					else if (i == 1)
					{ // Diagonal edge
						xi = 1.0 - s;  eta = s;
						dxi_ds = -1.0;  deta_ds = 1.0;
					}
					else
					{             // Left edge
						xi = 0.0;  eta = 1.0 - s;
						dxi_ds = 0.0;  deta_ds = -1.0;
					}
				};



			for (int j = 0; j < n_gll_points; j++)
			{
				// Map the 1D GLL point to [0, 1]
				double s = (this->gll_locations[j] + 1.0) * 0.5;

				// Get the weigths
				double wt = this->gll_weights[j] * 0.5;

				double xi, eta, dxi_ds, deta_ds;
				get_edge_coordinates(s, xi, eta, dxi_ds, deta_ds);

			
				// --- Shape functions ---
				Eigen::VectorXd N(nen);
				Eigen::VectorXd dN_dxi(nen); // [dN/dxi, dN/deta]
				Eigen::VectorXd dN_deta(nen); // [dN/dxi, dN/deta]

				spectral_tri_element::evaluate_triangle_shape_functions(xi, eta, spec_mesh2d.spectral_order,
					this->inv_vandermonde_matrix, this->triangle_basis_terms,
					N, dN_dxi, dN_deta);


				// --- Compute physical edge length Jacobian ---
				Eigen::Vector2d dx_ds = Eigen::Vector2d::Zero();

				for (int k = 0; k < nen; k++)
				{
					double dN_ds = dN_dxi(k) * dxi_ds + dN_deta(k) * deta_ds;

					dx_ds += dN_ds * elem_coords[k];
				}

				double J_edge = dx_ds.norm();
				double dV = J_edge * wt;

				// --- Assembly ---
				// Neumann (flux) BC contribution
				double dq_edge = edge.normalderivfieldvalue;  // flux value

				for (int a = 0; a < nen; a++)
				{
					neumann_vector(a) += dq_edge * N(a) * dV;
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
	Eigen::VectorXi& dirichlet_BC_flag,
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
			// int local_idx = (i * spec_mesh2d.spectral_order);
			int local_idx = spec_mesh2d.tri_element_id_structure.corner_nodes[i];

			if (nd.isFieldBC == true)
			{
				// Apply field value at the node
				dirichlet_vector(local_idx) = nd.fieldvalue;
				dirichlet_BC_flag(local_idx) = 1;
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
				element_k_grad_matrix(i, j) += grad_dot * dV;

				// Mass
				element_k_mass_matrix(i, j) += (N(i) * N(j)) * dV;
			}
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

	const std::complex<double> factor(0.0, wave_number);

	for (int i = 0; i < 4; i++)
	{
		// Get the edge id
		int edge_id = quad_elm.edge_ids[i];

		const spectral_edge_store& edge = spec_mesh2d.spectral_edge_list[edge_id];

		if (edge.isboundaryedge == true && edge.isSommerfieldBC == true)
		{
			int n_gll_points = spec_mesh2d.spectral_order + 1;

			// Pre-compute edge mapping parameters
			auto get_edge_coordinates = [i](double s, double& xi, double& eta,
				double& dxi_ds, double& deta_ds) 
				{
					if (i == 0) 
					{      // Edge 1 (-1,-1) to (1,-1) bottom edge
						xi = s;  eta = -1.0;
						dxi_ds = 1.0;  deta_ds = 0.0;
					}
					else if (i == 1) 
					{	// Edge 2 (1,-1) to (1, 1) right edge
						xi = 1.0;  eta = s;
						dxi_ds = 0.0;  deta_ds = 1.0;
					}
					else if (i == 2)
					{  // Edge 3 (1,1) to (-1,1) top edge
						xi = -s;  eta = 1.0;
						dxi_ds = -1.0;  deta_ds = 0.0;
					}
					else
					{  // Edge 4 (-1,1) to (-1,-1) left edge
						xi = -1.0;  eta = -s;
						dxi_ds = 0.0;  deta_ds = -1.0;
					}
				};


			for (int j = 0; j < n_gll_points; j++)
			{
				// Get the 1D GLL point [-1, 1]
				double s = this->gll_locations[j];

				// Get the weigths
				double w = this->gll_weights[j];

				double xi, eta, dxi_ds, deta_ds;
				get_edge_coordinates(s, xi, eta, dxi_ds, deta_ds);

				// --- Shape functions ---
				Eigen::VectorXd N(nen);
				Eigen::VectorXd dN_dxi(nen);
				Eigen::VectorXd dN_deta(nen);

				spectral_quad_element::evaluate_quadrilateral_shape_functions(xi, eta, 
					spec_mesh2d.spectral_order, gll_locations, 
					N, dN_dxi, dN_deta);


				// --- Compute physical edge length Jacobian ---
				Eigen::Vector2d dx_ds = Eigen::Vector2d::Zero();

				for (int k = 0; k < nen; k++)
				{
					double dN_ds = dN_dxi(k) * dxi_ds + dN_deta(k) * deta_ds;

					dx_ds += dN_ds * elem_coords[k];
				}

				double J_edge = dx_ds.norm();
				double dV = J_edge * w;

				// --- Assembly ---
				for (int a = 0; a < nen; a++)
				{
					for (int b = 0; b < nen; b++)
					{
						element_kI_matrix(a, b) += factor *	N(a) * N(b) * dV;
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


void helmholtz2d_spectral_solver::get_quadelement_field_vector(const spectral_quadelement_store& quad_elm,
	Eigen::VectorXi& dirichlet_BC_flag,
	Eigen::VectorXd& dirichlet_vector)
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

			//for (int j = 0; j < spec_mesh2d.spectral_order + 1; j++)
			//{
			//	int local_idx = (i * spec_mesh2d.spectral_order) + j;
			//	dirichlet_vector(local_idx) = q_edge;
			//	dirichlet_BC_flag(local_idx) = 1;
			//}
			
			int local_idx = spec_mesh2d.quad_element_id_structure.corner_nodes[i];

			dirichlet_vector(local_idx) = q_edge;
			dirichlet_BC_flag(local_idx) = 1;

			for (const int& j : spec_mesh2d.quad_element_id_structure.edge_node_ids[i])
			{
				local_idx = j;

				dirichlet_vector(local_idx) = q_edge;
				dirichlet_BC_flag(local_idx) = 1;
			}

			local_idx = spec_mesh2d.quad_element_id_structure.corner_nodes[(i + 1)%4];

			dirichlet_vector(local_idx) = q_edge;
			dirichlet_BC_flag(local_idx) = 1;

			//
		}
		//
	}
	//
}

void helmholtz2d_spectral_solver::get_quadelement_normderivfield_vector(const spectral_quadelement_store& quad_elm,
	const std::vector<Eigen::Vector2d>& elem_coords,
	int nen,
	Eigen::VectorXd& neumann_vector)
{
	for (int i = 0; i < 4; i++)
	{
		// Get the edge id
		int edge_id = quad_elm.edge_ids[i];

		const spectral_edge_store& edge = spec_mesh2d.spectral_edge_list[edge_id];

		if (edge.isboundaryedge == true && edge.isDerivFieldBC == true)
		{
			int n_gll_points = spec_mesh2d.spectral_order + 1;

			// Pre-compute edge mapping parameters
			auto get_edge_coordinates = [i](double s, double& xi, double& eta,
				double& dxi_ds, double& deta_ds)
				{
					if (i == 0)
					{      // Edge 1 (-1,-1) to (1,-1) bottom edge
						xi = s;  eta = -1.0;
						dxi_ds = 1.0;  deta_ds = 0.0;
					}
					else if (i == 1)
					{	// Edge 2 (1,-1) to (1, 1) right edge
						xi = 1.0;  eta = s;
						dxi_ds = 0.0;  deta_ds = 1.0;
					}
					else if (i == 2)
					{  // Edge 3 (1,1) to (-1,1) top edge
						xi = -s;  eta = 1.0;
						dxi_ds = -1.0;  deta_ds = 0.0;
					}
					else
					{  // Edge 4 (-1,1) to (-1,-1) left edge
						xi = -1.0;  eta = -s;
						dxi_ds = 0.0;  deta_ds = -1.0;
					}
				};


			for (int j = 0; j < n_gll_points; j++)
			{
				// Get the 1D GLL point [-1, 1]
				double s = this->gll_locations[j];

				// Get the weigths
				double wt = this->gll_weights[j];

				double xi, eta, dxi_ds, deta_ds;
				get_edge_coordinates(s, xi, eta, dxi_ds, deta_ds);

				// --- Shape functions ---
				Eigen::VectorXd N(nen);
				Eigen::VectorXd dN_dxi(nen);
				Eigen::VectorXd dN_deta(nen);

				spectral_quad_element::evaluate_quadrilateral_shape_functions(xi, eta, spec_mesh2d.spectral_order, gll_locations,
					N, dN_dxi, dN_deta);

				// --- Compute physical edge length Jacobian ---
				Eigen::Vector2d dx_ds = Eigen::Vector2d::Zero();

				for (int k = 0; k < nen; k++)
				{
					double dN_ds = dN_dxi(k) * dxi_ds + dN_deta(k) * deta_ds;

					dx_ds += dN_ds * elem_coords[k];
				}

				double J_edge = dx_ds.norm();
				double dV = J_edge * wt;

				// --- Assembly ---
				// Neumann (flux) BC contribution
				double dq_edge = edge.normalderivfieldvalue;  // flux value

				for (int a = 0; a < nen; a++)
				{
					neumann_vector(a) += dq_edge * N(a) * dV;
				}
				//
			}
			//
		}
		//
	}
	//
}


void helmholtz2d_spectral_solver::get_quadelement_source_vector(const spectral_quadelement_store& quad_elm,
	Eigen::VectorXi& dirichlet_BC_flag,
	Eigen::VectorXd& dirichlet_vector,
	Eigen::VectorXd& source_vector)
{
	// Get the corner nodes
	const std::vector<int>& corner_nodes = quad_elm.corner_nodes;

	for (int i = 0; i < 4; i++)
	{
		const spectral_node_store& nd = spec_mesh2d.spectral_node_list[corner_nodes[i]];

		if (nd.isboundarynode == true)
		{
			// int local_idx = (i * spec_mesh2d.spectral_order);
			int local_idx = spec_mesh2d.quad_element_id_structure.corner_nodes[i];

			if (nd.isFieldBC == true)
			{
				// Apply field value at the node
				dirichlet_vector(local_idx) = nd.fieldvalue;
				dirichlet_BC_flag(local_idx) = 1;
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



void helmholtz2d_spectral_solver::set_complex_global_matrix(const std::vector<int>& elem_nodes,
	int nen,
	const Eigen::MatrixXd& element_k_matrix,
	const Eigen::MatrixXcd& element_kI_matrix,
	std::vector<Eigen::Triplet<std::complex<double>>>& triplets_system)
{
	for (int i = 0; i < nen; i++)
	{
		// get the global map id
		int i_node_map = this->nodeid_map[elem_nodes[i]];

		for (int j = 0; j < nen; j++)
		{
			// get the global map id
			int j_node_map = this->nodeid_map[elem_nodes[j]];

			std::complex<double> val = element_k_matrix(i, j) + element_kI_matrix(i, j);

			triplets_system.emplace_back(i_node_map, j_node_map, val);

			// Note: Triplets don’t accumulate — Eigen accumulates when building the sparse matrix.
		}
	}
	//
}


void helmholtz2d_spectral_solver::set_global_vector(const std::vector<int>& elem_nodes,
	int nen,
	const Eigen::VectorXd& element_vector, Eigen::VectorXd& global_vector)
{
	for (int i = 0; i < nen; i++)
	{
		// get the global map id
		int i_node_map = this->nodeid_map[elem_nodes[i]];

		global_vector(i_node_map) += element_vector(i);
	}
	//
}


void helmholtz2d_spectral_solver::set_global_BC_flag_vector(const std::vector<int>& elem_nodes,
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



void helmholtz2d_spectral_solver::solve_dirichlet_BCs_elimination_method(Eigen::VectorXcd& u)
{
	// Elimination method

	// Find the fixed and free nodes dof
	std::vector<int> free_dofs;
	std::vector<int> fixed_dofs;

	for (int i = 0; i < this->numDOF; ++i)
	{
		if (global_dirichlet_BC_flags_vector(i))
			fixed_dofs.push_back(i);
		else
			free_dofs.push_back(i);
	}

	// Map free DOF to Local indices
	std::unordered_map<int, int> free_map;

	for (int i = 0; i < free_dofs.size(); ++i)
	{
		free_map[free_dofs[i]] = i;
	}



	std::vector<Eigen::Triplet<std::complex<double>>> triplets_ff;

	for (int k = 0; k < global_system_matrix.outerSize(); ++k)
	{
		for (Eigen::SparseMatrix<std::complex<double>>::InnerIterator it(global_system_matrix, k); it; ++it)
		{
			int i = it.row();
			int j = it.col();

			// keep only free-free block
			if (global_dirichlet_BC_flags_vector(i) == 0 &&
				global_dirichlet_BC_flags_vector(j) == 0)
			{

				// Get the local indices of free nodes
				int ii = free_map[i];
				int jj = free_map[j];

				triplets_ff.emplace_back(ii, jj, it.value());
			}
		}
	}


	// Main system
	this->K_ff.resize(free_dofs.size(), free_dofs.size());	// Main system
	this->K_ff.setFromTriplets(triplets_ff.begin(), triplets_ff.end());



	Eigen::VectorXcd F = global_source_vector.cast<std::complex<double>>()
		+ global_normalderivfield_vector.cast<std::complex<double>>();


	for (int i : fixed_dofs)
	{
		std::complex<double> ui = global_field_vector(i);

		for (Eigen::SparseMatrix<std::complex<double>>::InnerIterator it(global_system_matrix, i); it; ++it)
		{
			int row = it.row();
			int col = it.col();

			// only affect free rows
			if (global_dirichlet_BC_flags_vector(row) == 0)
			{
				F(row) -= it.value() * ui;
			}
		}
	}


	this->F_f.resize(free_dofs.size()); // Source + Deriv Field

	for (int i = 0; i < free_dofs.size(); ++i)
	{
		this->F_f(i) = F(free_dofs[i]);
	}


	// Perform solve
	Eigen::SparseLU<Eigen::SparseMatrix<std::complex<double>>> solver;
	solver.compute(K_ff);

	Eigen::VectorXcd u_f = solver.solve(F_f);

	// Final solution
	u.resize(numDOF);
	u.setZero();

	// Free DOF assign the result
	for (int i = 0; i < free_dofs.size(); ++i)
	{
		u(free_dofs[i]) = u_f(i);
	}

	// Fixed DOF assign the prescribed field value
	for (int i : fixed_dofs)
	{
		u(i) = global_field_vector(i);
	}
	//
}



void helmholtz2d_spectral_solver::solve_dirichlet_BCs_lagrange_method(Eigen::VectorXcd& u)
{
	// Lagrange method

		// Find the fixed and free nodes dof
	std::vector<int> free_dofs;
	std::vector<int> fixed_dofs;

	for (int i = 0; i < this->numDOF; ++i)
	{
		if (global_dirichlet_BC_flags_vector(i))
			fixed_dofs.push_back(i);
		else
			free_dofs.push_back(i);
	}

	int m = fixed_dofs.size();

	int N = numDOF;

	// Buld the Augmented K matrix
	 // Copy the original system to the left corner
	std::vector<Eigen::Triplet<std::complex<double>>> triplets_K;

	for (int k = 0; k < global_system_matrix.outerSize(); ++k)
	{
		for (Eigen::SparseMatrix<std::complex<double>>::InnerIterator it(global_system_matrix, k); it; ++it)
		{
			int i = it.row();
			int j = it.col();

			triplets_K.emplace_back(i, j, it.value());

		}
	}

	// Build constraint matrix C
	for (int j = 0; j < m; ++j)
	{
		int dof = fixed_dofs[j];

		// C row j has 1 at DOF position
		triplets_K.emplace_back(N + j, dof, 1.0);
		triplets_K.emplace_back(dof, N + j, 1.0);
	}

	this->K_aug.resize(N + m, N + m);
	this->K_aug.setFromTriplets(triplets_K.begin(), triplets_K.end());


	// Buld the Augmented F matrix
	this->F_aug.resize(N + m);
	this->F_aug.setZero();
	// Eigen::VectorXcd F_aug = Eigen::VectorXcd::Zero(N + m);

	Eigen::VectorXcd F = global_source_vector.cast<std::complex<double>>()
		+ global_normalderivfield_vector.cast<std::complex<double>>();

	F_aug.head(N) = F;

	// Add the global force vector
	for (int j = 0; j < m; ++j)
	{
		int dof = fixed_dofs[j];
		F_aug(N + j) = global_field_vector(dof);
	}


	// Solve the system
	Eigen::SparseLU<Eigen::SparseMatrix<std::complex<double>>> solver;
	solver.compute(K_aug);

	Eigen::VectorXcd sol = solver.solve(F_aug);


	// Final solution
	u.resize(numDOF);
	u.setZero();

	for (int i = 0; i < numDOF; ++i)
	{
		u(i) = sol(i);
	}
	//
}


void helmholtz2d_spectral_solver::store_results()
{

	std::ofstream bin_file(this->output_file.c_str(), std::ios::binary);

	if (!bin_file.is_open())
	{
		std::string error_msg = "Failed to open output file: " + this->output_file;
		report(error_msg.c_str());
		throw std::runtime_error(error_msg);
	}


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


	bin_file.flush();

	auto file_size = bin_file.tellp();  // tellp() for output file (tellg() is for input)

	bin_file.close();

	// Report Success and file size
	std::string success_msg = "Results stored successfully: " +
		this->output_file +
		" (" + std::to_string(node_points_count) + " nodes, " +
		std::to_string(triangles_count) + " triangles)";
	report(success_msg.c_str());



	//
}


void helmholtz2d_spectral_solver::report_vandermondematrix_conditioning(const Eigen::MatrixXd& invVanderMondematrix)
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


void helmholtz2d_spectral_solver::report(const char* msg)
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







