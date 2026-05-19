#pragma once



#include <cmath>

#pragma warning(push)
#pragma warning (disable : 26451)
#pragma warning (disable : 26495)
#pragma warning (disable : 6255)
#pragma warning (disable : 6294)
#pragma warning (disable : 26813)
#pragma warning (disable : 26454)
#pragma warning (disable : 4244)


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

#include <Eigen/Core>
#include <Eigen/SparseCore>

// Spectra
#include <Spectra/SymEigsSolver.h>
#include <Spectra/MatOp/SparseSymMatProd.h>
#include <Spectra/MatOp/SparseCholesky.h>

#include <Spectra/GenEigsSolver.h>

using namespace Spectra;

#pragma warning(pop)


#include "../system_store/helmholtz_system_store.h"
#include "../spectral_elements/spectral_mesh2d.h"
#include "../system_store/stopwatch_events.h"

#include "../spectral_elements/gll_utility.h"

#include <fstream>

#include <iomanip> // to get std::setprecision()


#include "MinvKOp.h"



class modal_spectral_solver
{
public:
	modal_spectral_solver();
	~modal_spectral_solver() = default;

	void init(helmholtz_system_store* helmholtz_2dsystem_ptr,
		const char* output_file_char,
		stopwatch_events* stopwatch,
		void(*callback)(const char*));

	void create_global_matrices();

	void solve_modal_analysis(int inpt_num_modes);




private:
	const double M_PI = 3.14159265;


	helmholtz_system_store* helmholtz_2dsystem_ptr;
	spectral_mesh2d spec_mesh2d;

	stopwatch_events* m_stopwatch;

	std::string output_file;


	int numDOF = 0;
	std::unordered_map<int, int> nodeid_map; // Node ID map


	 Eigen::SparseMatrix<double> global_k_matrix; // Global ke Matrix [Ke]
	 Eigen::SparseMatrix<double> global_m_matrix; // Global ke Matrix [Me]

	Eigen::VectorXi global_dirichlet_BC_flags_vector; // Global boundary condition Vector (To track the nodes where prescribed field is applied)

	// Solution
	std::vector<double> natural_frequencies;
	Eigen::MatrixXd natural_modes;



	// Quadrature points for triangle element
	std::vector<spectral_point> triangle_quadrature_points;

	// Triangle basis term
	std::vector<basis_term> triangle_basis_terms;

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
		Eigen::MatrixXd& element_k_matrix,
		Eigen::MatrixXd& element_m_matrix);


	void evaluate_triangle_shape_functions(double quadraturept_xi,
		double quadraturept_eta, int nen,
		Eigen::VectorXd& N,
		Eigen::MatrixXd& dN_dxi);



	void get_trielement_field_vector(const spectral_trielement_store& tri_elm,
		Eigen::VectorXi& dirichlet_BC_flag);


	void get_trielement_source_vector(const spectral_trielement_store& tri_elm,
		Eigen::VectorXi& dirichlet_BC_flag);


	//________________________________________________________________________________________________

	void set_global_matrix(const std::vector<int>& elem_nodes,
		int nen,
		const Eigen::MatrixXd& element_k_matrix,
		const Eigen::MatrixXd& element_m_matrix,
		std::vector<Eigen::Triplet<double>>& k_triplets,
		std::vector<Eigen::Triplet<double>>& m_triplets);



	void set_global_BC_flag_vector(const std::vector<int>& elem_nodes,
		int nen,
		const Eigen::VectorXi& element_BC_flag_vector, Eigen::VectorXi& global_BC_flag_vector);



	//________________________________________________________________________________________________

	void get_quadelement_k_grad_k_mass_matrix(const std::vector<int>& elem_nodes,
		const std::vector<Eigen::Vector2d>& elem_coords,
		Eigen::MatrixXd& element_k_matrix,
		Eigen::MatrixXd& element_m_matrix);


	void evaluate_quadrilateral_shape_functions(double quadraturept_xi,
		double quadraturept_eta, int nen,
		Eigen::VectorXd& N,
		Eigen::MatrixXd& dN_dxi);



	void get_quadelement_field_vector(const spectral_quadelement_store& quad_elm,
		Eigen::VectorXi& dirichlet_BC_flag);



	void get_quadelement_source_vector(const spectral_quadelement_store& quad_elm,
		Eigen::VectorXi& dirichlet_BC_flag);




	void store_results();


	void(*m_callback)(const char*) = nullptr;

	void report(const char* msg);






};







