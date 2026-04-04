#pragma once
#include <vector>
#include <cmath>


class gll_utility
{
public:

	static std::vector<double> get_gll_locations(int spectral_order);

	static std::vector<double> get_gll_weights(int spectral_order, const std::vector<double>& gll_points_xi);

	
private:
	static constexpr double tol = 1e-12;
	static constexpr int max_iter = 100;
	static constexpr double m_pi = 3.1415926535897932384626433832795028841971; // 3.1415926535897932384626433832795028841971



};


