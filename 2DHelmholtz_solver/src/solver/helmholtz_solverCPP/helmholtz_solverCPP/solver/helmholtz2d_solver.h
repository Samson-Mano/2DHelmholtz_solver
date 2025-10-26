#pragma once


#include <cmath>

#pragma warning(push)
#pragma warning (disable : 26451)
#pragma warning (disable : 26495)
#pragma warning (disable : 6255)
#pragma warning (disable : 6294)
#pragma warning (disable : 26813)
#pragma warning (disable : 26454)

// Optimization for Eigen Library
// 1) OpenMP (Yes (/openmp)
//	 Solution Explorer->Configuration Properties -> C/C++ -> Language -> Open MP Support
// 2) For -march=native, choose "AVX2" or the latest supported instruction set.
//   Solution Explorer->Configuration Properties -> C/C++ -> Code Generation -> Enable Enhanced Instruction Set 

#include <Eigen/Dense>
#include <Eigen/Sparse>
#include <Eigen/SparseLU>
#include <Eigen/Eigenvalues>
// Define the sparse matrix type for the reduced global stiffness matrix
typedef Eigen::SparseMatrix<double> SparseMatrix;
#pragma warning(pop)

#include "../system_store/helmholtz_system_store.h"


class helmholtz2d_solver
{
public:
	helmholtz2d_solver();
	~helmholtz2d_solver() = default;

	void init(helmholtz_system_store* helmholtz_2dsystem_ptr);
	void create_global_matrices();


private:
	helmholtz_system_store* helmholtz_2dsystem_ptr;

	int numDOF = 0;
	std::unordered_map<int, int> nodeid_map; // Node ID map


	Eigen::MatrixXd global_k_matrix; // Global k Matrix
	Eigen::MatrixXd global_m_matrix; // Global m Matrix
	Eigen::MatrixXd global_kI_matrix; // Global kI Matrix Boundary impedance matrix (Absorbing Boundary condition - Sommerfield)
	Eigen::VectorXd global_field_vector; // Global field Vector
	Eigen::VectorXd global_normalderivfield_vector; // Global derivative normal field Vector
	Eigen::VectorXd global_source_vector; // Global source Vector

	Eigen::VectorXi global_BC_vector; // Global boundary condition Vector (To track the nodes where prescribed field is applied)


	int get_edge_id(const int& startNode_id, const int& endNode_id);

	double get_line_length(const int& nd1_id, const int& nd2_id);

	double get_triangle_area(const int& nd1_id, const int& nd2_id, const int& nd3_id);

	double get_quadrilateral_area(const int& nd1_id, const int& nd2_id, const int& nd3_id, const int& nd4_id);

	void get_trielement_k_matrix(const int& nd1_id, const int& nd2_id, const int& nd3_id,
		const double& trielm_area, Eigen::Matrix3d& element_k_matrix);


	void get_trielement_m_matrix(const int& nd1_id, const int& nd2_id, const int& nd3_id,
		const double& trielm_area, Eigen::Matrix3d& element_m_matrix);


	void get_trielement_kI_matrix(const int& edge1_id, const int& edge2_id, const int& edge3_id,
		const double& edge1_length, const double& edge2_length, const double& edge3_length,
		const double& wave_number, Eigen::Matrix3d& element_kI_matrix);


	void get_trielement_field_vector(const int& nd1_id, const int& nd2_id, const int& nd3_id,
		const int& edge1_id, const int& edge2_id, const int& edge3_id,
		const double& edge1_length, const double& edge2_length, const double& edge3_length,
		Eigen::Vector3i& dirichlet_BC,
		Eigen::Vector3d& dirichlet_vector);


	void get_trielement_normderivfield_vector(const int& edge1_id, const int& edge2_id, const int& edge3_id,
		const double& edge1_length, const double& edge2_length, const double& edge3_length,
		Eigen::Vector3d& neumann_vector);


	void get_trielement_source_vector(const int& nd1_id, const int& nd2_id, const int& nd3_id,
		Eigen::Vector3d& source_vector);


	void set_trielement_global_matrix(const int& nd1_id, const int& nd2_id, const int& nd3_id,
		const Eigen::Matrix3d& element_matrix, Eigen::MatrixXd& global_matrix);

	void set_trielement_global_vector(const int& nd1_id, const int& nd2_id, const int& nd3_id,
		const Eigen::Vector3d& element_vector, Eigen::VectorXd& global_vector);

	void set_trielement_global_BCvector(const int& nd1_id, const int& nd2_id, const int& nd3_id,
		const Eigen::Vector3i& element_BC_vector, Eigen::VectorXi& global_BC_vector);

	//__________________________________________________________________________________________________________

	void get_quadelement_k_m_matrix(const int& nd1_id, const int& nd2_id,
		const int& nd3_id, const int& nd4_id,
		Eigen::Matrix4d& element_k_matrix,
		Eigen::Matrix4d& element_m_matrix);



	void get_quadelement_kI_matrix(const node_store& nd1, const node_store& nd2, 
		const node_store& nd3, const node_store& nd4,
		const int& edge1_id, const int& edge2_id, const int& edge3_id, const int& edge4_id,
		const double& edge1_length, const double& edge2_length, 
		const double& edge3_length, const double& edge4_length,
		const double& k, Eigen::MatrixXd& element_kI_matrix);


	void get_quadelement_field_vector(const node_store& nd1, const node_store& nd2, 
		const node_store& nd3, const node_store& nd4,
		const int& edge1_id, const int& edge2_id, const int& edge3_id, const int& edge4_id,
		const double& edge1_length, const double& edge2_length, 
		const double& edge3_length, const double& edge4_length,
		Eigen::VectorXd& dirichlet_vector);


	void get_quadelement_normderivfield_vector(const node_store& nd1, const node_store& nd2,
		const node_store& nd3, const node_store& nd4,
		const int& edge1_id, const int& edge2_id, const int& edge3_id, const int& edge4_id,
		const double& edge1_length, const double& edge2_length,
		const double& edge3_length, const double& edge4_length,
		Eigen::VectorXd& dirichlet_vector);


	void get_quadelement_source_vector(const node_store& nd1, const node_store& nd2, 
		const node_store& nd3, const node_store& nd4,
		Eigen::VectorXd& source_vector);





};



