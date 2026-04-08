#include "helmholtz2d_spectral_solver.h"

helmholtz2d_spectral_solver::helmholtz2d_spectral_solver()
{
	// Empty constructor

}


void helmholtz2d_spectral_solver::init(helmholtz_system_store* helmholtz_2dsystem_ptr,
	const char* output_file,
	void(*callback)(const char*))
{
	// Set the initialized system ptr
	this->helmholtz_2dsystem_ptr = helmholtz_2dsystem_ptr;

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




}


void helmholtz2d_spectral_solver::solve_helmholtz_matrices(const int& solver_type)
{
	// open the bin file
	bin_file.open(output_file, std::ios::binary);



	// close the bin file
	bin_file.close();
//
}




void helmholtz2d_spectral_solver::report(const char* msg)
{
	if (m_callback)
		m_callback(msg);
	//
}

