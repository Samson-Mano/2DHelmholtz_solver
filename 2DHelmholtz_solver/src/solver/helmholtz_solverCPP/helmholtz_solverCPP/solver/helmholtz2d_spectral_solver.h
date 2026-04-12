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
#include "../spectral_elements/spectral_mesh2d.h"
#include "../system_store/stopwatch_events.h"

#include "../spectral_elements/gll_utility.h"

#include <fstream>

#include <iomanip> // to get std::setprecision()



class helmholtz2d_spectral_solver
{
public:
	helmholtz2d_spectral_solver();
	~helmholtz2d_spectral_solver() = default;

	void init(helmholtz_system_store* helmholtz_2dsystem_ptr,
		const char* output_file,
		stopwatch_events* stopwatch,
		void(*callback)(const char*));

	void create_global_matrices();
	void solve_helmholtz_matrices(const int& solver_type);




private:
	helmholtz_system_store* helmholtz_2dsystem_ptr;
	spectral_mesh2d spec_mesh2d;

	stopwatch_events* m_stopwatch;

	const char* output_file = nullptr;
	std::ofstream bin_file;


	int numDOF = 0;
	std::unordered_map<int, int> nodeid_map; // Node ID map


	Eigen::MatrixXd global_k_matrix; // Global k Matrix (Ke - k^2 * Me)
	Eigen::MatrixXd global_kI_matrix; // Global kI Matrix Boundary impedance matrix (Absorbing Boundary condition - Sommerfield)

	Eigen::VectorXd global_field_vector; // Global field Vector
	Eigen::VectorXd global_normalderivfield_vector; // Global derivative normal field Vector
	Eigen::VectorXd global_source_vector; // Global source Vector

	Eigen::VectorXi global_dirichlet_BC_flags_vector; // Global boundary condition Vector (To track the nodes where prescribed field is applied)

	// Solution
	Eigen::VectorXd u_real;
	Eigen::VectorXd u_imag;


	// Quadrature points
	std::vector<spectral_point> triangle_quadrature_points;


	void get_trielement_k_grad_k_mass_matrix(const std::vector<int>& elem_nodes,
		const std::vector<Eigen::Vector2d>& elem_coords,
		const double& tri_area,
		Eigen::MatrixXd& element_k_grad_matrix, 
		Eigen::MatrixXd& element_k_mass_matrix);

	void evaluate_triangle_shape_functions(double xi, double eta,
		const std::vector<Eigen::Vector2d>& elem_coords,
		Eigen::VectorXd& N,
		Eigen::MatrixXd& dN_dxi);



	void store_results();


	void(*m_callback)(const char*) = nullptr;

	void report(const char* msg);


};
