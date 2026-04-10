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

	
	// Write the results
	store_results();


	// close the bin file
	bin_file.close();
//
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

