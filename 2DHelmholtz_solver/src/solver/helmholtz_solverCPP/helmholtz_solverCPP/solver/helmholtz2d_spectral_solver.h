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

#include "../spectral_elements/spectral_lib/gll_utility.h"
#include "../spectral_elements/spectral_lib/spectral_quad_element.h"
#include "../spectral_elements/spectral_lib/spectral_tri_element.h"


#include <fstream>

#include <iomanip> // to get std::setprecision()



class helmholtz2d_spectral_solver
{
public:
	helmholtz2d_spectral_solver();
	~helmholtz2d_spectral_solver() = default;

	void init(helmholtz_system_store* helmholtz_2dsystem_ptr,
		const char* output_file_char,
		stopwatch_events* stopwatch,
		void(*callback)(const char*));

	void create_global_matrices();
	void solve_helmholtz_matrices(const int& solver_type);


	void store_k_m_matrices_text_debug();

	void store_matrices_text_debug();


private:
	helmholtz_system_store* helmholtz_2dsystem_ptr;
	
	stopwatch_events* m_stopwatch;

	std::string output_file;


	Eigen::SparseMatrix<double> global_k_matrix; // Global ke Matrix [Ke]
	Eigen::SparseMatrix<double> global_m_matrix; // Global ke Matrix [Me]



	//___________________________________________________
	spectral_mesh2d spec_mesh2d;

	int numDOF = 0;
	std::unordered_map<int, int> nodeid_map; // Node ID map


	// Eigen::SparseMatrix<double> global_k_matrix; // Global k Matrix (Ke - k^2 * Me)
	// Eigen::SparseMatrix<std::complex<double>> global_kI_matrix; // kI Global kI Matrix Boundary impedance matrix (Absorbing Boundary condition - Sommerfield)
	Eigen::SparseMatrix<std::complex<double>> global_system_matrix; // (Ke - k^2 * Me) + kI 

	Eigen::VectorXd global_field_vector; // Global field Vector
	Eigen::VectorXd global_normalderivfield_vector; // Global derivative normal field Vector
	Eigen::VectorXd global_source_vector; // Global source Vector

	Eigen::VectorXi global_dirichlet_BC_flags_vector; // Global boundary condition Vector (To track the nodes where prescribed field is applied)


	Eigen::SparseMatrix<std::complex<double>> K_ff;
	Eigen::VectorXcd F_f;

	Eigen::SparseMatrix<std::complex<double>> K_aug;
	Eigen::VectorXcd F_aug;



	// Solution
	Eigen::VectorXcd u_complex;
	Eigen::VectorXd u_real;
	Eigen::VectorXd u_imag;


	// Quadrature points for triangle element
	std::vector<spectral_point> triangle_quadrature_points;

	// Triangle basis term
	std::vector<proriol_basis_term> triangle_basis_terms;

	// Inverse Vandermonde matrix
	Eigen::MatrixXd inv_vandermonde_matrix;

	// 1D GLL points
	std::vector<double> gll_locations;
	std::vector<double> gll_weights;


	// Quadrature points for quadrilateral element
	std::vector<spectral_point> quadrilateral_quadrature_points;


	//________________________________________________________________________________________________

	void get_trielement_k_grad_k_mass_matrix(const std::vector<int>& elem_nodes,
		const std::vector<Eigen::Vector2d>& elem_coords,
		Eigen::MatrixXd& element_k_grad_matrix, 
		Eigen::MatrixXd& element_k_mass_matrix);


	void get_trielement_kI_matrix(const spectral_trielement_store& tri_elm,
		const std::vector<Eigen::Vector2d>& elem_coords,
		int nen,
		double wave_number, 
		Eigen::MatrixXcd& element_kI_matrix);
	

	void get_trielement_field_vector(const spectral_trielement_store& tri_elm,
		Eigen::VectorXi& dirichlet_BC_flag,
		Eigen::VectorXd& dirichlet_vector);

	void get_trielement_normderivfield_vector(const spectral_trielement_store& tri_elm,
		const std::vector<Eigen::Vector2d>& elem_coords,
		int nen,
		Eigen::VectorXd& neumann_vector);


	void get_trielement_source_vector(const spectral_trielement_store& tri_elm,
		Eigen::VectorXi& dirichlet_BC_flag,
		Eigen::VectorXd& dirichlet_vector,
		Eigen::VectorXd& source_vector);





	void set_global_matrix(const std::vector<int>& elem_nodes,
		int nen,
		const Eigen::MatrixXd& element_k_matrix,
		const Eigen::MatrixXd& element_m_matrix,
		std::vector<Eigen::Triplet<double>>& k_triplets,
		std::vector<Eigen::Triplet<double>>& m_triplets);


	void set_complex_global_matrix(const std::vector<int>& elem_nodes,
		int nen,
		const Eigen::MatrixXd& element_k_matrix,
		const Eigen::MatrixXcd& element_kI_matrix,
		std::vector<Eigen::Triplet<std::complex<double>>>& triplets_system);



	void set_global_vector(const std::vector<int>& elem_nodes,
		int nen,
		const Eigen::VectorXd& element_vector, Eigen::VectorXd& global_vector);


	void set_global_BC_flag_vector(const std::vector<int>& elem_nodes,
		int nen,
		const Eigen::VectorXi& element_BC_flag_vector, Eigen::VectorXi& global_BC_flag_vector);
	


	void report_vandermondematrix_conditioning(const Eigen::MatrixXd& invVanderMondematrix);


	//________________________________________________________________________________________________

	void get_quadelement_k_grad_k_mass_matrix(const std::vector<int>& elem_nodes,
		const std::vector<Eigen::Vector2d>& elem_coords,
		Eigen::MatrixXd& element_k_grad_matrix,
		Eigen::MatrixXd& element_k_mass_matrix);


	void get_quadelement_kI_matrix(const spectral_quadelement_store& quad_elm,
		const std::vector<Eigen::Vector2d>& elem_coords,
		int nen,
		double wave_number,
		Eigen::MatrixXcd& element_kI_matrix);


	void get_quadelement_field_vector(const spectral_quadelement_store& quad_elm,
		Eigen::VectorXi& dirichlet_BC_flag,
		Eigen::VectorXd& dirichlet_vector);

	void get_quadelement_normderivfield_vector(const spectral_quadelement_store& quad_elm,
		const std::vector<Eigen::Vector2d>& elem_coords,
		int nen,
		Eigen::VectorXd& neumann_vector);


	void get_quadelement_source_vector(const spectral_quadelement_store& quad_elm,
		Eigen::VectorXi& dirichlet_BC_flag,
		Eigen::VectorXd& dirichlet_vector,
		Eigen::VectorXd& source_vector);



	//________________________________________________________________________________________________


	void solve_dirichlet_BCs_elimination_method(Eigen::VectorXcd& u);


	void solve_dirichlet_BCs_lagrange_method(Eigen::VectorXcd& u);



	void store_results();


	void(*m_callback)(const char*) = nullptr;

	void report(const char* msg);


};
