#pragma once
#include <vector>
#include <cmath>

struct spectral_point
{
	double xi;
	double eta;
	double weight;
};


class gll_utility
{
public:

	static std::vector<double> get_gll_locations(int spectral_order);

	static std::vector<double> get_gll_weights(int spectral_order, const std::vector<double>& gll_points_xi);

	static std::vector<spectral_point> get_triangle_spectral_element(int spectral_order);
	
	static std::vector<spectral_point> get_quadrilateral_spectral_element(int spectral_order);

	static std::vector<spectral_point> get_triangle_quadrature(int spectral_order);


private:
	static constexpr double tol = 1e-12;
	static constexpr int max_iter = 100;
	static constexpr double m_pi = 3.1415926535897932384626433832795028841971; // 3.1415926535897932384626433832795028841971



};


