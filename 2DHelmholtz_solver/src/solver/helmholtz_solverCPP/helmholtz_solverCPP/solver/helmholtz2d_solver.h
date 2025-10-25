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

	Eigen::MatrixXd k_matrix; // Element k matrix
	Eigen::MatrixXd m_matrix; // Element m matrix
	Eigen::MatrixXd kI_matrix; // Boundary impedance matrix (Absorbing Boundary condition - Sommerfield)

	Eigen::VectorXd dirichlet_vector; // field vector
	Eigen::VectorXd neumann_vector; // normal derivative field vector
	Eigen::VectorXd source_vector; // source vector

	int get_edge_id(const int& startNode_id, const int& endNode_id);

	double get_line_length(const node_store& nd1, const node_store& nd2);

	double get_triangle_area(const node_store& nd1, const node_store& nd2, const node_store& nd3);


	void get_trielement_k_matrix(const node_store& nd1, const node_store& nd2, const node_store& nd3,
		const double& k, Eigen::Matrix3d& element_k_matrix);


	void get_trielement_m_matrix(const node_store& nd1, const node_store& nd2, const node_store& nd3,
		const double& k, Eigen::Matrix3d& element_m_matrix);


	void get_trielement_kI_matrix(const int& edge1_id, const int& edge2_id, const int& edge3_id,
		const double& edge1_length, const double& edge2_length, const double& edge3_length,
		const double& k, Eigen::Matrix3d& element_kI_matrix);


	void get_trielement_field_vector(const node_store& nd1, const node_store& nd2, const node_store& nd3,
		const int& edge1_id, const int& edge2_id, const int& edge3_id,
		const double& edge1_length, const double& edge2_length, const double& edge3_length,
		Eigen::Vector3d& dirichlet_vector);


	void get_trielement_normderivfield_vector(const int& edge1_id, const int& edge2_id, const int& edge3_id,
		const double& edge1_length, const double& edge2_length, const double& edge3_length,
		Eigen::Vector3d& neumann_vector);


	void get_trielement_source_vector(const node_store& nd1, const node_store& nd2, const node_store& nd3,
		Eigen::Vector3d& source_vector);




	void get_quadelement_k_matrix(const node_store& nd1, const node_store& nd2, 
		const node_store& nd3, const node_store& nd4,
		const double& k, Eigen::MatrixXd& element_k_matrix);


	void get_quadelement_m_matrix(const node_store& nd1, const node_store& nd2, 
		const node_store& nd3, const node_store& nd4,
		const double& k, Eigen::MatrixXd& element_m_matrix);


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



