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


	// Set zeros
	global_k_matrix.setZero(numDOF, numDOF);
	global_kI_matrix.setZero(numDOF, numDOF);

	global_field_vector.setZero(numDOF);
	global_normalderivfield_vector.setZero(numDOF);
	global_source_vector.setZero(numDOF);

	global_dirichlet_BC_flags_vector.setZero(numDOF);

	// Triangle elements
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

		// get the three edge ids of the triangle element
		int edge1_id = get_edge_id(nd1_id, nd2_id); // Edge 1
		int edge2_id = get_edge_id(nd2_id, nd3_id); // Edge 2
		int edge3_id = get_edge_id(nd3_id, nd1_id); // Edge 3

		// get the edge lengths of triangle element
		double edge1_length = get_line_length(nd1_id, nd2_id);
		double edge2_length = get_line_length(nd2_id, nd3_id);
		double edge3_length = get_line_length(nd3_id, nd1_id);

		// get the material parameters of this element
		double trielm_area = get_triangle_area(nd1_id, nd2_id, nd3_id);
		double wave_number = helmholtz_2dsystem.material_list[tri_elm.materialid].wave_number; // get the material wave number

		//________________________________________________________________________________________________
		// Step 2: Create element k_grad matrix
		Eigen::Matrix3d element_k_grad_matrix; // Element k_grad matrix
		Eigen::Matrix3d element_k_mass_matrix; // Element k_mass matrix

		get_trielement_k_grad_k_mass_matrix(nd1_id, nd2_id, nd3_id, trielm_area, element_k_grad_matrix, element_k_mass_matrix);

		
		Eigen::Matrix3d element_k_matrix;
		element_k_matrix.setZero();
		
		element_k_matrix = element_k_grad_matrix - ((wave_number * wave_number) * element_k_mass_matrix);

		//________________________________________________________________________________________________
		// Step 3: Create Sommerfield Absorbtion Boundary Condition matrix
		Eigen::Matrix3d element_kI_matrix; // Element kI matrix

		get_trielement_kI_matrix(edge1_id, edge2_id, edge3_id,
			edge1_length, edge2_length, edge3_length, wave_number, element_kI_matrix);

		//________________________________________________________________________________________________
		// Step 4: Create Element field vector
		Eigen::Vector3d element_field_vector; // Element field vector
		Eigen::Vector3i element_dirichlet_BC_flags_vector; // Element BC Vector to track the Dirichlet boundary conidtion (For elimination)

		get_trielement_field_vector(nd1_id, nd2_id, nd3_id,
			edge1_id, edge2_id, edge3_id,
			edge1_length, edge2_length, edge3_length, 
			element_dirichlet_BC_flags_vector, element_field_vector);


		//________________________________________________________________________________________________
		// Step 5: Create Element normal derivative field vector
		Eigen::Vector3d element_normderivfield_vector; // Element normal derivative field vector

		get_trielement_normderivfield_vector(edge1_id, edge2_id, edge3_id,
			edge1_length, edge2_length, edge3_length, element_normderivfield_vector);

		//________________________________________________________________________________________________
		// Step 6: Create Element source vector
		Eigen::Vector3d element_source_vector; // Element source vector

		get_trielement_source_vector(nd1_id, nd2_id, nd3_id, element_source_vector);


		//________________________________________________________________________________________________
		// Step 7: Set the global matrix and global vector

		set_trielement_global_matrix(nd1_id, nd2_id, nd3_id,
			element_k_matrix, global_k_matrix);

		set_trielement_global_matrix(nd1_id, nd2_id, nd3_id,
			element_kI_matrix, global_kI_matrix);


		set_trielement_global_vector(nd1_id, nd2_id, nd3_id,
			element_field_vector, global_field_vector);

		set_trielement_global_vector(nd1_id, nd2_id, nd3_id,
			element_normderivfield_vector, global_normalderivfield_vector);

		set_trielement_global_vector(nd1_id, nd2_id, nd3_id,
			element_source_vector, global_source_vector);

		// BC Vector to track prescribed field vector
		set_trielement_global_BCvector(nd1_id, nd2_id, nd3_id,
			element_dirichlet_BC_flags_vector, global_dirichlet_BC_flags_vector);


	}



	// Quadrialteral elements
	for (auto& quad_elm_m : helmholtz_2dsystem.quadelement_list)
	{
		// get the element
		quadelement_store quad_elm = quad_elm_m.second;

		//_______________________________________________________________________________________________
		// Step: 1 Get the element data
		// set the element ID
		int elm_id = quad_elm.quad_id;

		// get the node ids of the element
		int nd1_id = quad_elm.nodeid1; // Node id 1
		int nd2_id = quad_elm.nodeid2; // Node id 2
		int nd3_id = quad_elm.nodeid3; // Node id 3
		int nd4_id = quad_elm.nodeid4; // Node id 3

		// get the Four edge ids of the quadrilateral element
		int edge1_id = get_edge_id(nd1_id, nd2_id); // Edge 1
		int edge2_id = get_edge_id(nd2_id, nd3_id); // Edge 2
		int edge3_id = get_edge_id(nd3_id, nd4_id); // Edge 3
		int edge4_id = get_edge_id(nd4_id, nd1_id); // Edge 4

		// get the edge lengths of quadrilateral element
		double edge1_length = get_line_length(nd1_id, nd2_id);
		double edge2_length = get_line_length(nd2_id, nd3_id);
		double edge3_length = get_line_length(nd3_id, nd4_id);
		double edge4_length = get_line_length(nd4_id, nd1_id);

		// get the material parameters of this element
		double wave_number = helmholtz_2dsystem.material_list[quad_elm.materialid].wave_number; // get the material wave number

		//________________________________________________________________________________________________
		// Step 2: Create element k & m matrix
		Eigen::Matrix4d element_k_grad_matrix; // Element k_grad matrix
		Eigen::Matrix4d element_k_mass_matrix; // Element k_mass matrix

		get_quadelement_k_grad_k_mass_matrix(nd1_id, nd2_id, nd3_id, nd4_id, 
			element_k_grad_matrix, element_k_mass_matrix);


		Eigen::Matrix4d element_k_matrix;
		element_k_matrix.setZero();

		element_k_matrix = element_k_grad_matrix - ((wave_number * wave_number) * element_k_mass_matrix);


		//________________________________________________________________________________________________
		// Step 3: Create Sommerfield Absorbtion Boundary Condition matrix
		Eigen::Matrix4d element_kI_matrix; // Element kI matrix

		get_quadelement_kI_matrix(edge1_id, edge2_id, edge3_id, edge4_id,
			edge1_length, edge2_length, edge3_length, edge4_length,
			wave_number, element_kI_matrix);

		//________________________________________________________________________________________________
		// Step 4: Create Element field vector
		Eigen::Vector4d element_field_vector; // Element field vector
		Eigen::Vector4i element_dirichlet_BC_flags_vector; // Element BC Vector to track the Dirichlet boundary conidtion (For elimination)

		get_quadelement_field_vector(nd1_id, nd2_id, nd3_id, nd4_id,
			edge1_id, edge2_id, edge3_id, edge4_id,
			edge1_length, edge2_length, edge3_length, edge4_length,
			element_dirichlet_BC_flags_vector, element_field_vector);


		//________________________________________________________________________________________________
		// Step 5: Create Element normal derivative field vector
		Eigen::Vector4d element_normderivfield_vector; // Element normal derivative field vector

		get_quadelement_normderivfield_vector(edge1_id, edge2_id, edge3_id, edge4_id,
			edge1_length, edge2_length, edge3_length, edge4_length,
			element_normderivfield_vector);

		//________________________________________________________________________________________________
		// Step 6: Create Element source vector
		Eigen::Vector4d element_source_vector; // Element source vector

		get_quadelement_source_vector(nd1_id, nd2_id, nd3_id, nd4_id, element_source_vector);


		//________________________________________________________________________________________________
		// Step 7: Set the global matrix and global vector

		set_quadelement_global_matrix(nd1_id, nd2_id, nd3_id, nd4_id,
			element_k_matrix, global_k_matrix);

		set_quadelement_global_matrix(nd1_id, nd2_id, nd3_id, nd4_id,
			element_kI_matrix, global_kI_matrix);


		set_quadelement_global_vector(nd1_id, nd2_id, nd3_id, nd4_id,
			element_field_vector, global_field_vector);

		set_quadelement_global_vector(nd1_id, nd2_id, nd3_id, nd4_id,
			element_normderivfield_vector, global_normalderivfield_vector);

		set_quadelement_global_vector(nd1_id, nd2_id, nd3_id, nd4_id,
			element_source_vector, global_source_vector);

		// BC Vector to track prescribed field vector
		set_quadelement_global_BCvector(nd1_id, nd2_id, nd3_id, nd4_id,
			element_dirichlet_BC_flags_vector, global_dirichlet_BC_flags_vector);


	}

	// Matrix formation end


}


void helmholtz2d_solver::solve_helmholtz_matrices(const int& solver_type)
{
	// Solve the helmholtz equation
	//  ([A] + i[B]) [u] = [f] + [du/dn]

	//  Solve using
	// | [A] -[B] | |u_r| = [f] + [du/dn]
	// | [B]  [A] | |u_i|   [0]   [0]

	const int& numDOF = this->numDOF;

	Eigen::MatrixXd& A_matrix = global_k_matrix;  
	Eigen::MatrixXd& B_matrix = global_kI_matrix;  // Imaginary coupling (from Sommerfeld/ABC)

	Eigen::VectorXd& f_vector = global_source_vector; // Source vector (applied at nodes)
	Eigen::VectorXd& du_vector = global_normalderivfield_vector; // Neumann boundary condition vector


	// --- Construct combined real system ---
	Eigen::MatrixXd RealSystem(2 * numDOF, 2 * numDOF);
	Eigen::VectorXd RHS(2 * numDOF);

	RealSystem.setZero();
	RHS.setZero();

	// Fill block matrix
	RealSystem.topLeftCorner(numDOF, numDOF) = A_matrix;
	RealSystem.topRightCorner(numDOF, numDOF) = -B_matrix;
	RealSystem.bottomLeftCorner(numDOF, numDOF) = B_matrix;
	RealSystem.bottomRightCorner(numDOF, numDOF) = A_matrix;

	// Construct RHS
	RHS.head(numDOF) = f_vector + du_vector;  // Real part (includes Neumann BC)
	RHS.tail(numDOF) = Eigen::VectorXd::Zero(numDOF);  // Imaginary part (no imaginary forcing)


	// Intialize the solution vector   
	this->u_real = Eigen::VectorXd::Zero(numDOF);
	this->u_imag = Eigen::VectorXd::Zero(numDOF);


	if (solver_type == 0)
		apply_dirichlet_BCs_elimination_method(RealSystem, RHS);
	else
		apply_dirichlet_BCs_lagrange_method(RealSystem, RHS);

	Eigen::SparseMatrix<double> K_sparse = RealSystem.sparseView();
	Eigen::SimplicialLDLT<Eigen::SparseMatrix<double>> solver;
	solver.compute(K_sparse);
	Eigen::VectorXd solution = solver.solve(RHS);

	this->u_real = solution.head(numDOF);
	this->u_imag = solution.segment(numDOF, numDOF);

}



double helmholtz2d_solver::get_result_ureal(const int& node_id)
{
	return this->u_real[this->nodeid_map[node_id]];
}


double helmholtz2d_solver::get_result_uimag(const int& node_id)
{
	return this->u_imag[this->nodeid_map[node_id]];

}


void helmholtz2d_solver::apply_dirichlet_BCs_elimination_method(Eigen::MatrixXd& K, Eigen::VectorXd& F)
{
	const int& numDOF = this->numDOF;

	// Elimination method
	for (int i = 0; i < numDOF; ++i)
	{
		if (global_dirichlet_BC_flags_vector(i) == 1)
		{
			double prescribed_value = global_field_vector(i);

			// Real component
			K.row(i).setZero();
			K.col(i).setZero();
			K(i, i) = 1.0;
			F(i) = prescribed_value;

			// Imaginary component
			int imag_idx = i + numDOF;
			K.row(imag_idx).setZero();
			K.col(imag_idx).setZero();
			K(imag_idx, imag_idx) = 1.0;
			F(imag_idx) = 0.0; // 0 imaginary field for Dirichlet

		}
	}
}


void helmholtz2d_solver::apply_dirichlet_BCs_lagrange_method(Eigen::MatrixXd& K, Eigen::VectorXd& F)
{
	// Lagrange Augmentation method
	const int& numDOF = this->numDOF;

	// --- Step 1: Count Dirichlet constraints ---
	int dirichlet_count = (global_dirichlet_BC_flags_vector.array() == 1).count();
	if (dirichlet_count == 0)
		return; // No Dirichlet BCs => nothing to do


	// --- Step 2: Build the constraint matrix (A) ---
	Eigen::MatrixXd SPC_Matrix = Eigen::MatrixXd::Zero(dirichlet_count, 2 * numDOF);
	Eigen::VectorXd prescribed_field_values(dirichlet_count);

	int j = 0;
	for (int i = 0; i < numDOF; ++i)
	{
		if (global_dirichlet_BC_flags_vector(i) == 1)
		{
			// Real constraint
			SPC_Matrix(j, i) = 1.0;

			// Imaginary constraint
			SPC_Matrix(j, i + numDOF) = 0.0;  // Typically constrain imaginary to 0

			prescribed_field_values(j) = global_field_vector(i);
			j++;
		}
	}

	// --- Step 3: Create augmented matrices ---
	int augSize = (2 * numDOF) + dirichlet_count;
	Eigen::MatrixXd K_aug = Eigen::MatrixXd::Zero(augSize, augSize);
	Eigen::VectorXd F_aug = Eigen::VectorXd::Zero(augSize);

	// Block assignments
	K_aug.topLeftCorner(2 * numDOF, 2 * numDOF) = K;                        // K
	K_aug.topRightCorner(2 * numDOF, dirichlet_count) = SPC_Matrix.transpose(); // A^T
	K_aug.bottomLeftCorner(dirichlet_count, 2 * numDOF) = SPC_Matrix;   // A
	// bottom-right block remains zero

	// RHS vector
	F_aug.head(2 * numDOF) = F;                             // Original forces
	F_aug.tail(dirichlet_count) = prescribed_field_values; // Constraint RHS (b)

	// --- Step 4: Assign augmented matrices ---
	K = std::move(K_aug);
	F = std::move(F_aug);

}



int helmholtz2d_solver::get_edge_id(const int& startnodeid, const int& endnodeid)
{

	helmholtz_system_store& helmholtz_2dsystem = (*this->helmholtz_2dsystem_ptr);

	// Get the connected edges to start node
	const std::vector<int>& connected_edges = helmholtz_2dsystem.node_edge_map[startnodeid];

	for (const int& edge_id : connected_edges)
	{
		const auto& edge = helmholtz_2dsystem.edge_list[edge_id];
		if ((edge.startnodeid == startnodeid && edge.endnodeid == endnodeid) ||
			(edge.startnodeid == endnodeid && edge.endnodeid == startnodeid))
		{
			// Line with the same start and end nodes
			return edge_id;
		}
			
	}
	
	return -1;

}



double helmholtz2d_solver::get_line_length(const int& nd1_id, const int& nd2_id)
{
	helmholtz_system_store& helmholtz_2dsystem = (*this->helmholtz_2dsystem_ptr);

	node_store& nd1 = helmholtz_2dsystem.node_list[nd1_id];
	node_store& nd2 = helmholtz_2dsystem.node_list[nd2_id];

	// Length of line
	double length = std::sqrt(std::pow(nd1.x_coord - nd2.x_coord, 2) + std::pow(nd1.y_coord - nd2.y_coord, 2));

	return length;
}




double helmholtz2d_solver::get_triangle_area(const int& nd1_id, const int& nd2_id, const int& nd3_id)
{
	helmholtz_system_store& helmholtz_2dsystem = (*this->helmholtz_2dsystem_ptr);

	node_store& nd1 = helmholtz_2dsystem.node_list[nd1_id];
	node_store& nd2 = helmholtz_2dsystem.node_list[nd2_id];
	node_store& nd3 = helmholtz_2dsystem.node_list[nd3_id];

	double x1 = nd1.x_coord;
	double y1 = nd1.y_coord;
	double x2 = nd2.x_coord;
	double y2 = nd2.y_coord;
	double x3 = nd3.x_coord;
	double y3 = nd3.y_coord;

	// Shoelace formula
	double area = 0.5 * (x1 * (y2 - y3) + x2 * (y3 - y1) + x3 * (y1 - y2));

	return std::abs(area);

}


double helmholtz2d_solver::get_quadrilateral_area(const int& nd1_id, const int& nd2_id, const int& nd3_id, const int& nd4_id)
{
	helmholtz_system_store& helmholtz_2dsystem = (*this->helmholtz_2dsystem_ptr);

	node_store& nd1 = helmholtz_2dsystem.node_list[nd1_id];
	node_store& nd2 = helmholtz_2dsystem.node_list[nd2_id];
	node_store& nd3 = helmholtz_2dsystem.node_list[nd3_id];
	node_store& nd4 = helmholtz_2dsystem.node_list[nd4_id];

	double x1 = nd1.x_coord;
	double y1 = nd1.y_coord;
	double x2 = nd2.x_coord;
	double y2 = nd2.y_coord;
	double x3 = nd3.x_coord;
	double y3 = nd3.y_coord;
	double x4 = nd4.x_coord;
	double y4 = nd4.y_coord;

	// Shoelace formula for polygon area
	double area = 0.5 * (((x1 * y2) + (x2 * y3) + (x3 * y4) + (x4 * y1)) - ((y1 * x2) + (y2 * x3) + (y3 * x4) + (y4 * x1)));

	return std::abs(area);

}



void helmholtz2d_solver::get_trielement_k_grad_k_mass_matrix(const int& nd1_id, const int& nd2_id, const int& nd3_id,
	const double& trielm_area, Eigen::Matrix3d& element_k_grad_matrix, Eigen::Matrix3d& element_k_mass_matrix)
{
	// Element k_grad matrix
	element_k_grad_matrix = Eigen::Matrix3d::Zero();

	helmholtz_system_store& helmholtz_2dsystem = (*this->helmholtz_2dsystem_ptr);

	node_store& nd1 = helmholtz_2dsystem.node_list[nd1_id];
	node_store& nd2 = helmholtz_2dsystem.node_list[nd2_id];
	node_store& nd3 = helmholtz_2dsystem.node_list[nd3_id];

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
	element_k_grad_matrix = (1.0 / (4.0 * trielm_area)) * (B.transpose() * B);


	//_____________________________________________________________________________________
	// Element k_mass matrix
	element_k_mass_matrix = Eigen::Matrix3d::Zero();
	element_k_mass_matrix << 2, 1, 1,
		1, 2, 1,
		1, 1, 2;

	element_k_mass_matrix = (trielm_area / 12.0) * element_k_mass_matrix;

}


void helmholtz2d_solver::get_trielement_kI_matrix(const int& edge1_id, const int& edge2_id, const int& edge3_id,
	const double& edge1_length, const double& edge2_length, const double& edge3_length,
	const double& wave_number, Eigen::Matrix3d& element_kI_matrix)
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

				edge_matrix *= (wave_number * edge_length / 6.0);

				element_kI_matrix += edge_matrix;
			}
		};

	// Triangle edges (local node pairs)
	add_edge_contribution(edge1_id, edge1_length, 0, 1); // Edge between node 1 and 2
	add_edge_contribution(edge2_id, edge2_length, 1, 2); // Edge between node 2 and 3
	add_edge_contribution(edge3_id, edge3_length, 2, 0); // Edge between node 3 and 1

}


void helmholtz2d_solver::get_trielement_field_vector(const int& nd1_id, const int& nd2_id, const int& nd3_id,
	const int& edge1_id, const int& edge2_id, const int& edge3_id,
	const double& edge1_length, const double& edge2_length, const double& edge3_length,
	Eigen::Vector3i& dirichlet_BC,
	Eigen::Vector3d& dirichlet_vector)
{
	// Element field vector
	dirichlet_vector = Eigen::Vector3d::Zero();
	dirichlet_BC = Eigen::Vector3i::Zero();

	helmholtz_system_store& helmholtz_2dsystem = (*this->helmholtz_2dsystem_ptr);

	// Lambda for edge field contribution (Dirichlet-type BC)
	auto add_edge_contribution = [&](int edge_id, double edge_length, int n1, int n2)
		{
			const auto& edge = helmholtz_2dsystem.edge_list[edge_id];
			if (edge.isboundaryedge && edge.isFieldBC)
			{
				// Dirichlet (field) BC contribution
				double q_edge = edge.fieldvalue;  // field value

				Eigen::Vector3d edge_vec = Eigen::Vector3d::Zero();
				edge_vec(n1) += 1.0;
				edge_vec(n2) += 1.0;

				// Integral over edge:int( N^T * q d(gamma)) = q * L/2 * [1, 1]
				edge_vec *= (q_edge * edge_length / 2.0);

				dirichlet_vector += edge_vec;

				// Set the Dirichlet BC to track the index where prescribed field is present
				dirichlet_BC(n1) = 1;
				dirichlet_BC(n2) = 1;
			}
		};

	// Apply edge contributions Triangle element
	add_edge_contribution(edge1_id, edge1_length, 0, 1); // Edge 1-2
	add_edge_contribution(edge2_id, edge2_length, 1, 2); // Edge 2-3
	add_edge_contribution(edge3_id, edge3_length, 2, 0); // Edge 3-1

	// --- Dirichlet-type (prescribed field) nodes ---
	// These are handled differently: typically, we *replace* the corresponding rows/columns
	// in the global system matrix, but here you can still accumulate their vector form
	// if you’re forming a right-hand side prior to modification.
	Eigen::Vector3d node_vec = Eigen::Vector3d::Zero();

	node_store& nd1 = helmholtz_2dsystem.node_list[nd1_id];
	node_store& nd2 = helmholtz_2dsystem.node_list[nd2_id];
	node_store& nd3 = helmholtz_2dsystem.node_list[nd3_id];

	if (nd1.isboundarynode && nd1.isFieldBC)
	{
		node_vec(0) = nd1.fieldvalue;
		dirichlet_BC(0) = 1;
	}

	if (nd2.isboundarynode && nd2.isFieldBC)
	{
		node_vec(1) = nd2.fieldvalue;
		dirichlet_BC(1) = 1;
	}

	if (nd3.isboundarynode && nd3.isFieldBC)
	{
		node_vec(2) = nd3.fieldvalue;
		dirichlet_BC(2) = 1;
	}


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
			if (edge.isboundaryedge && edge.isDerivFieldBC)
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

	// Apply edge contributions Triangle element
	add_edge_contribution(edge1_id, edge1_length, 0, 1); // Edge 1-2
	add_edge_contribution(edge2_id, edge2_length, 1, 2); // Edge 2-3
	add_edge_contribution(edge3_id, edge3_length, 2, 0); // Edge 3-1

}

void helmholtz2d_solver::get_trielement_source_vector(const int& nd1_id, const int& nd2_id, const int& nd3_id,
	Eigen::Vector3d& source_vector)
{

	// --- Source (prescribed source) nodes ---
	source_vector = Eigen::Vector3d::Zero();

	helmholtz_system_store& helmholtz_2dsystem = (*this->helmholtz_2dsystem_ptr);

	node_store& nd1 = helmholtz_2dsystem.node_list[nd1_id];
	node_store& nd2 = helmholtz_2dsystem.node_list[nd2_id];
	node_store& nd3 = helmholtz_2dsystem.node_list[nd3_id];

	// Add nodal source components
	if (nd1.isboundarynode && !nd1.isFieldBC)
		source_vector(0) = nd1.sourcevalue;
	if (nd2.isboundarynode && !nd2.isFieldBC)
		source_vector(1) = nd2.sourcevalue;
	if (nd3.isboundarynode && !nd3.isFieldBC)
		source_vector(2) = nd3.sourcevalue;


}


void helmholtz2d_solver::set_trielement_global_matrix(const int& nd1_id, const int& nd2_id, const int& nd3_id,
	const Eigen::Matrix3d& element_matrix, Eigen::MatrixXd& global_matrix)
{
	// Set the global matrix

	// Set the row 1
	global_matrix.coeffRef(nodeid_map[nd1_id], nodeid_map[nd1_id]) += element_matrix.coeff(0, 0);
	global_matrix.coeffRef(nodeid_map[nd1_id], nodeid_map[nd2_id]) += element_matrix.coeff(0, 1);
	global_matrix.coeffRef(nodeid_map[nd1_id], nodeid_map[nd3_id]) += element_matrix.coeff(0, 2);

	// Set the row 2
	global_matrix.coeffRef(nodeid_map[nd2_id], nodeid_map[nd1_id]) += element_matrix.coeff(1, 0);
	global_matrix.coeffRef(nodeid_map[nd2_id], nodeid_map[nd2_id]) += element_matrix.coeff(1, 1);
	global_matrix.coeffRef(nodeid_map[nd2_id], nodeid_map[nd3_id]) += element_matrix.coeff(1, 2);

	// Set the row 3
	global_matrix.coeffRef(nodeid_map[nd3_id], nodeid_map[nd1_id]) += element_matrix.coeff(2, 0);
	global_matrix.coeffRef(nodeid_map[nd3_id], nodeid_map[nd2_id]) += element_matrix.coeff(2, 1);
	global_matrix.coeffRef(nodeid_map[nd3_id], nodeid_map[nd3_id]) += element_matrix.coeff(2, 2);

}


void helmholtz2d_solver::set_trielement_global_vector(const int& nd1_id, const int& nd2_id, const int& nd3_id,
	const Eigen::Vector3d& element_vector, Eigen::VectorXd& global_vector)
{
	// Set the global vector

	global_vector.coeffRef(nodeid_map[nd1_id]) += element_vector.coeff(0);
	global_vector.coeffRef(nodeid_map[nd2_id]) += element_vector.coeff(1);
	global_vector.coeffRef(nodeid_map[nd3_id]) += element_vector.coeff(2);

}




void helmholtz2d_solver::set_trielement_global_BCvector(const int& nd1_id, const int& nd2_id, const int& nd3_id,
	const Eigen::Vector3i& element_BC_vector, Eigen::VectorXi& global_BC_vector)
{
	// Set the global vector to track where the Prescribed boundary conditions are applied.

	global_BC_vector.coeffRef(nodeid_map[nd1_id]) = element_BC_vector.coeff(0);
	global_BC_vector.coeffRef(nodeid_map[nd2_id]) = element_BC_vector.coeff(1);
	global_BC_vector.coeffRef(nodeid_map[nd3_id]) = element_BC_vector.coeff(2);

}


//__________________________________________________________________________________________________________


void helmholtz2d_solver::get_quadelement_k_grad_k_mass_matrix(const int& nd1_id, const int& nd2_id,
	const int& nd3_id, const int& nd4_id, 
	Eigen::Matrix4d& element_k_grad_matrix,
	Eigen::Matrix4d& element_k_mass_matrix)
{
	// Element k_grad, k_mass matrix
	element_k_grad_matrix.setZero();
	element_k_mass_matrix.setZero();

	helmholtz_system_store& sys = (*this->helmholtz_2dsystem_ptr);

	// Node coordinates
	const node_store& n1 = sys.node_list[nd1_id];
	const node_store& n2 = sys.node_list[nd2_id];
	const node_store& n3 = sys.node_list[nd3_id];
	const node_store& n4 = sys.node_list[nd4_id];

	double x[4] = { n1.x_coord, n2.x_coord, n3.x_coord, n4.x_coord };
	double y[4] = { n1.y_coord, n2.y_coord, n3.y_coord, n4.y_coord };

	// 2x2 Gauss quadrature
	const double gp[2] = { -1.0 / std::sqrt(3.0), 1.0 / std::sqrt(3.0) };

	for (int i = 0; i < 2; ++i)
	{
		for (int j = 0; j < 2; ++j)
		{
			double s = gp[i];
			double t = gp[j];

			// Shape functions (bilinear)
			Eigen::Vector4d N;
			N << 0.25 * (1 - s) * (1 - t),
				0.25 * (1 + s) * (1 - t),
				0.25 * (1 + s) * (1 + t),
				0.25 * (1 - s) * (1 + t);

			// Derivatives wrt s and t
			Eigen::Vector4d dNds, dNdt;
			dNds << -0.25 * (1 - t), 0.25 * (1 - t),
				0.25 * (1 + t), -0.25 * (1 + t);
			dNdt << -0.25 * (1 - s), -0.25 * (1 + s),
				0.25 * (1 + s), 0.25 * (1 - s);

			// Jacobian matrix
			Eigen::Matrix2d J;
			J(0, 0) = dNds.dot(Eigen::Vector4d(x[0], x[1], x[2], x[3])); // dx/ds
			J(0, 1) = dNdt.dot(Eigen::Vector4d(x[0], x[1], x[2], x[3])); // dx/dt
			J(1, 0) = dNds.dot(Eigen::Vector4d(y[0], y[1], y[2], y[3])); // dy/ds
			J(1, 1) = dNdt.dot(Eigen::Vector4d(y[0], y[1], y[2], y[3])); // dy/dt

			double detJ = J.determinant();

			//if (detJ <= 0)
			//	throw std::runtime_error("Jacobian determinant is non-positive!");

			// Inverse of Jacobian
			Eigen::Matrix2d Jinv = J.inverse();

			// Derivatives wrt x,y
			Eigen::Matrix<double, 2, 4> dN;
			dN.row(0) = (Jinv(0, 0) * dNds + Jinv(0, 1) * dNdt).transpose();
			dN.row(1) = (Jinv(1, 0) * dNds + Jinv(1, 1) * dNdt).transpose();

			// B matrix
			Eigen::Matrix<double, 2, 4> B = dN;

			// Gradient part
			Eigen::Matrix4d k_grad = (B.transpose() * B) * detJ;

			element_k_grad_matrix += k_grad;

			// Mass part (Helmholtz term)
			Eigen::Matrix4d k_mass = (N * N.transpose()) * detJ;

			element_k_mass_matrix += k_mass;
			
		}

	}

}


void helmholtz2d_solver::get_quadelement_kI_matrix(const int& edge1_id, const int& edge2_id, 
	const int& edge3_id, const int& edge4_id, const double& edge1_length, const double& edge2_length, 
	const double& edge3_length, const double& edge4_length, 
	const double& wave_number, Eigen::Matrix4d& element_kI_matrix)
{
	// Element ABC matrix (Sommerfield absorbtion boundary condition)
	element_kI_matrix = Eigen::Matrix4d::Zero();

	helmholtz_system_store& helmholtz_2dsystem = (*this->helmholtz_2dsystem_ptr);

	auto add_edge_contribution = [&](int edge_id, double edge_length, int n1, int n2)
		{
			if (helmholtz_2dsystem.edge_list[edge_id].isboundaryedge &&
				helmholtz_2dsystem.edge_list[edge_id].isSommerfieldBC)
			{
				Eigen::Matrix4d edge_matrix = Eigen::Matrix4d::Zero();

				edge_matrix(n1, n1) += 2.0;
				edge_matrix(n1, n2) += 1.0;
				edge_matrix(n2, n1) += 1.0;
				edge_matrix(n2, n2) += 2.0;

				edge_matrix *= (wave_number * edge_length / 6.0);

				element_kI_matrix += edge_matrix;

			}
		};

	// Quadrilateral edges (local node pairs)
	add_edge_contribution(edge1_id, edge1_length, 0, 1); // Edge between node 1 and 2
	add_edge_contribution(edge2_id, edge2_length, 1, 2); // Edge between node 2 and 3
	add_edge_contribution(edge3_id, edge3_length, 2, 3); // Edge between node 3 and 4
	add_edge_contribution(edge4_id, edge4_length, 3, 0); // Edge between node 4 and 1

}


void helmholtz2d_solver::get_quadelement_field_vector(const int& nd1_id, const int& nd2_id, 
	const int& nd3_id, const int& nd4_id, const int& edge1_id, const int& edge2_id, 
	const int& edge3_id, const int& edge4_id, const double& edge1_length, const double& edge2_length, 
	const double& edge3_length, const double& edge4_length, 
	Eigen::Vector4i& dirichlet_BC, Eigen::Vector4d& dirichlet_vector)
{
	// Element field vector
	dirichlet_vector = Eigen::Vector4d::Zero();
	dirichlet_BC = Eigen::Vector4i::Zero();

	helmholtz_system_store& helmholtz_2dsystem = (*this->helmholtz_2dsystem_ptr);

	// Lambda for edge field contribution (Dirichlet-type BC)
	auto add_edge_contribution = [&](int edge_id, double edge_length, int n1, int n2)
		{
			const auto& edge = helmholtz_2dsystem.edge_list[edge_id];
			if (edge.isboundaryedge && edge.isFieldBC)
			{
				// Dirichlet (field) BC contribution
				double q_edge = edge.fieldvalue;  // field value

				Eigen::Vector4d edge_vec = Eigen::Vector4d::Zero();
				edge_vec(n1) += 1.0;
				edge_vec(n2) += 1.0;

				// Integral over edge:int( N^T * q d(gamma)) = q * L/2 * [1, 1]
				edge_vec *= (q_edge * edge_length / 2.0);

				dirichlet_vector += edge_vec;

				// Set the Dirichlet BC to track the index where prescribed field is present
				dirichlet_BC(n1) = 1;
				dirichlet_BC(n2) = 1;

			}
		};

	// Apply edge contributions of Quadrilateral Element
	add_edge_contribution(edge1_id, edge1_length, 0, 1); // Edge 1-2
	add_edge_contribution(edge2_id, edge2_length, 1, 2); // Edge 2-3
	add_edge_contribution(edge3_id, edge3_length, 2, 3); // Edge 3-4
	add_edge_contribution(edge4_id, edge4_length, 3, 0); // Edge 4-1

	// --- Dirichlet-type (prescribed field) nodes ---
	Eigen::Vector4d node_vec = Eigen::Vector4d::Zero();

	node_store& nd1 = helmholtz_2dsystem.node_list[nd1_id];
	node_store& nd2 = helmholtz_2dsystem.node_list[nd2_id];
	node_store& nd3 = helmholtz_2dsystem.node_list[nd3_id];
	node_store& nd4 = helmholtz_2dsystem.node_list[nd4_id];

	if (nd1.isboundarynode && nd1.isFieldBC)
	{
		node_vec(0) = nd1.fieldvalue;
		dirichlet_BC(0) = 1;
	}

	if (nd2.isboundarynode && nd2.isFieldBC)
	{
		node_vec(1) = nd2.fieldvalue;
		dirichlet_BC(1) = 1;
	}

	if (nd3.isboundarynode && nd3.isFieldBC)
	{
		node_vec(2) = nd3.fieldvalue;
		dirichlet_BC(2) = 1;
	}

	if (nd4.isboundarynode && nd4.isFieldBC)
	{
		node_vec(3) = nd4.fieldvalue;
		dirichlet_BC(3) = 1;
	}


	// Add nodal (Dirichlet) field components
	dirichlet_vector += node_vec;

}


void helmholtz2d_solver::get_quadelement_normderivfield_vector(const int& edge1_id, const int& edge2_id, 
	const int& edge3_id, const int& edge4_id, 
	const double& edge1_length, const double& edge2_length, 
	const double& edge3_length, const double& edge4_length, Eigen::Vector4d& neumann_vector)
{

	neumann_vector = Eigen::Vector4d::Zero();

	helmholtz_system_store& helmholtz_2dsystem = (*this->helmholtz_2dsystem_ptr);

	// Lambda for edge field contribution (Neumann-type BC)
	auto add_edge_contribution = [&](int edge_id, double edge_length, int n1, int n2)
		{
			const auto& edge = helmholtz_2dsystem.edge_list[edge_id];
			if (edge.isboundaryedge && edge.isDerivFieldBC)
			{
				// Neumann (field flux) BC contribution
				double q_edge = edge.normalderivfieldvalue;  // prescribed flux value

				Eigen::Vector4d edge_vec = Eigen::Vector4d::Zero();
				edge_vec(n1) += 1.0;
				edge_vec(n2) += 1.0;

				// Integral over edge:int( N^T * q d(gamma)) = q * L/2 * [1, 1]
				edge_vec *= (q_edge * edge_length / 2.0);

				neumann_vector += edge_vec;

			}
		};

	// Apply edge contributions Quadrilateral Element
	add_edge_contribution(edge1_id, edge1_length, 0, 1); // Edge 1-2
	add_edge_contribution(edge2_id, edge2_length, 1, 2); // Edge 2-3
	add_edge_contribution(edge3_id, edge3_length, 2, 3); // Edge 3-4
	add_edge_contribution(edge4_id, edge4_length, 3, 0); // Edge 4-1

}


void helmholtz2d_solver::get_quadelement_source_vector(const int& nd1_id, const int& nd2_id, 
	const int& nd3_id, const int& nd4_id, Eigen::Vector4d& source_vector)
{

	// --- Source (prescribed source) nodes ---
	source_vector = Eigen::Vector4d::Zero();

	helmholtz_system_store& helmholtz_2dsystem = (*this->helmholtz_2dsystem_ptr);

	node_store& nd1 = helmholtz_2dsystem.node_list[nd1_id];
	node_store& nd2 = helmholtz_2dsystem.node_list[nd2_id];
	node_store& nd3 = helmholtz_2dsystem.node_list[nd3_id];
	node_store& nd4 = helmholtz_2dsystem.node_list[nd4_id];

	// Add nodal source components
	if (nd1.isboundarynode && !nd1.isFieldBC)
		source_vector(0) = nd1.sourcevalue;
	if (nd2.isboundarynode && !nd2.isFieldBC)
		source_vector(1) = nd2.sourcevalue;
	if (nd3.isboundarynode && !nd3.isFieldBC)
		source_vector(2) = nd3.sourcevalue;
	if (nd4.isboundarynode && !nd4.isFieldBC)
		source_vector(3) = nd4.sourcevalue;


}




void helmholtz2d_solver::set_quadelement_global_matrix(const int& nd1_id, const int& nd2_id, 
	const int& nd3_id, const int& nd4_id,
	const Eigen::Matrix4d& element_matrix, Eigen::MatrixXd& global_matrix)
{
	// Set the global matrix

	// Set the row 1
	global_matrix.coeffRef(nodeid_map[nd1_id], nodeid_map[nd1_id]) += element_matrix.coeff(0, 0);
	global_matrix.coeffRef(nodeid_map[nd1_id], nodeid_map[nd2_id]) += element_matrix.coeff(0, 1);
	global_matrix.coeffRef(nodeid_map[nd1_id], nodeid_map[nd3_id]) += element_matrix.coeff(0, 2);
	global_matrix.coeffRef(nodeid_map[nd1_id], nodeid_map[nd4_id]) += element_matrix.coeff(0, 3);

	// Set the row 2
	global_matrix.coeffRef(nodeid_map[nd2_id], nodeid_map[nd1_id]) += element_matrix.coeff(1, 0);
	global_matrix.coeffRef(nodeid_map[nd2_id], nodeid_map[nd2_id]) += element_matrix.coeff(1, 1);
	global_matrix.coeffRef(nodeid_map[nd2_id], nodeid_map[nd3_id]) += element_matrix.coeff(1, 2);
	global_matrix.coeffRef(nodeid_map[nd2_id], nodeid_map[nd4_id]) += element_matrix.coeff(1, 3);

	// Set the row 3
	global_matrix.coeffRef(nodeid_map[nd3_id], nodeid_map[nd1_id]) += element_matrix.coeff(2, 0);
	global_matrix.coeffRef(nodeid_map[nd3_id], nodeid_map[nd2_id]) += element_matrix.coeff(2, 1);
	global_matrix.coeffRef(nodeid_map[nd3_id], nodeid_map[nd3_id]) += element_matrix.coeff(2, 2);
	global_matrix.coeffRef(nodeid_map[nd3_id], nodeid_map[nd4_id]) += element_matrix.coeff(2, 3);

	// Set the row 4
	global_matrix.coeffRef(nodeid_map[nd4_id], nodeid_map[nd1_id]) += element_matrix.coeff(3, 0);
	global_matrix.coeffRef(nodeid_map[nd4_id], nodeid_map[nd2_id]) += element_matrix.coeff(3, 1);
	global_matrix.coeffRef(nodeid_map[nd4_id], nodeid_map[nd3_id]) += element_matrix.coeff(3, 2);
	global_matrix.coeffRef(nodeid_map[nd4_id], nodeid_map[nd4_id]) += element_matrix.coeff(3, 3);


}


void helmholtz2d_solver::set_quadelement_global_vector(const int& nd1_id, const int& nd2_id,
	const int& nd3_id, const int& nd4_id,
	const Eigen::Vector4d& element_vector, Eigen::VectorXd& global_vector)
{
	// Set the global vector

	global_vector.coeffRef(nodeid_map[nd1_id]) += element_vector.coeff(0);
	global_vector.coeffRef(nodeid_map[nd2_id]) += element_vector.coeff(1);
	global_vector.coeffRef(nodeid_map[nd3_id]) += element_vector.coeff(2);
	global_vector.coeffRef(nodeid_map[nd4_id]) += element_vector.coeff(3);

}


void helmholtz2d_solver::set_quadelement_global_BCvector(const int& nd1_id, const int& nd2_id, 
	const int& nd3_id, const int& nd4_id,
	const Eigen::Vector4i& element_BC_vector, Eigen::VectorXi& global_BC_vector)
{
	// Set the global vector to track where the Prescribed boundary conditions are applied.

	global_BC_vector.coeffRef(nodeid_map[nd1_id]) = element_BC_vector.coeff(0);
	global_BC_vector.coeffRef(nodeid_map[nd2_id]) = element_BC_vector.coeff(1);
	global_BC_vector.coeffRef(nodeid_map[nd3_id]) = element_BC_vector.coeff(2);
	global_BC_vector.coeffRef(nodeid_map[nd4_id]) = element_BC_vector.coeff(3);

}









