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


	void store_results();


	void(*m_callback)(const char*) = nullptr;

	void report(const char* msg);


};
