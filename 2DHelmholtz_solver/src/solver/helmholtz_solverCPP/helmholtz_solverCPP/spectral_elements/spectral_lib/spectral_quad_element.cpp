#include "spectral_quad_element.h"




std::vector<spectral_point> spectral_quad_element::get_quadrilateral_quadrature(int spectral_order)
{
	// Gauss - Legendre points/ weights

	std::vector<spectral_point> quadrature_points;

	int n = spectral_order + 1;

	//std::vector<double> gp = get_gauss_points(n);
	//std::vector<double> gw = get_gauss_weights(n);


	GaussQuadrature q = get_gauss_quadrature(n);
	auto& gp = q.points;
	auto& gw = q.weights;


	for (int j = 0; j < n; j++)
	{
		for (int i = 0; i < n; i++)
		{
			spectral_point pt;
			pt.xi = gp[i];
			pt.eta = gp[j];
			pt.weight = gw[i] * gw[j];  // tensor product

			quadrature_points.push_back(pt);
		}
	}

	return quadrature_points;
	//

}




std::vector<spectral_point> spectral_quad_element::get_quadrilateral_spectral_element(int spectral_order)
{
	// 4-----3     
	// |     |
	// |     | 
	// |     |
	// 1-----2


	// Get the GLL points of 1D edges
	std::vector<double> gll_points = gll_utility::get_gll_locations(spectral_order);


	// Quadrilateral spectral element points
	std::vector<spectral_point> quad_spectral_points;

	// Create the corner points
	std::vector<spectral_point> corner_points;

	corner_points.emplace_back(spectral_point{ -1.0, -1.0, 1.0 }); // Point 1
	corner_points.emplace_back(spectral_point{ 1.0, -1.0, 1.0 }); // Point 2
	corner_points.emplace_back(spectral_point{ 1.0, 1.0, 1.0 }); // Point 3
	corner_points.emplace_back(spectral_point{ -1.0, 1.0, 1.0 }); // Point 4

	for (int i = 0; i < 4; i++)
	{
		spectral_point v_start = corner_points[i];
		spectral_point v_end = corner_points[(i + 1) % 4];


		// Add the corner
		quad_spectral_points.emplace_back(spectral_point{ v_start.xi, v_start.eta, v_start.weight });

		// Add the edges
		for (int j = 1; j < spectral_order; j++) // Exclude the end point -1 and 1
		{
			// Get the GLL Point -1 to 1
			double s = gll_points[j];

			double x_coord = ((1.0 - s) * v_start.xi) + (s * v_end.xi);
			double y_coord = ((1.0 - s) * v_start.eta) + (s * v_end.eta);

			quad_spectral_points.emplace_back(spectral_point{ x_coord, y_coord, 1.0 });
		}

	}


	// Create the internal nodes for the quadrilateral element using bilinear mapping
	for (int i = 1; i < spectral_order; i++)
	{
		for (int j = 1; j < spectral_order; j++)
		{

			double xi = gll_points[j];
			double eta = gll_points[i];

			// Bilinear mapping
			double x = 0.25 * (
				(1 - xi) * (1 - eta) * corner_points[0].xi +
				(1 + xi) * (1 - eta) * corner_points[1].xi +
				(1 + xi) * (1 + eta) * corner_points[2].xi +
				(1 - xi) * (1 + eta) * corner_points[3].xi
				);

			double y = 0.25 * (
				(1 - xi) * (1 - eta) * corner_points[0].eta +
				(1 + xi) * (1 - eta) * corner_points[1].eta +
				(1 + xi) * (1 + eta) * corner_points[2].eta +
				(1 - xi) * (1 + eta) * corner_points[3].eta
				);

			// Internal nodes
			quad_spectral_points.emplace_back(spectral_point{ x, y, 1.0 });
		}
		//
	}

	return quad_spectral_points;
	//
}

GaussQuadrature spectral_quad_element::get_gauss_quadrature(int n)
{
	GaussQuadrature q;
	gauss_legendre(n, q.points, q.weights);
	return q;
}


//std::vector<double> spectral_quad_element::get_gauss_points(int n)
//{
//	std::vector<double> pts, wts;
//	gauss_legendre(n, pts, wts);
//	return pts;
//}
//
//
//
//std::vector<double> spectral_quad_element::get_gauss_weights(int n)
//{
//	std::vector<double> pts, wts;
//	gauss_legendre(n, pts, wts);
//	return wts;
//}



void spectral_quad_element::gauss_legendre(int n, std::vector<double>& points, std::vector<double>& weights)
{
	//if (n <= 0)
	//	throw std::invalid_argument("Gauss order must be > 0");

	Eigen::MatrixXd J = Eigen::MatrixXd::Zero(n, n);

	// --- Build Jacobi matrix ---
	for (int i = 0; i < n - 1; i++)
	{
		double a = (i + 1.0) / std::sqrt((2.0 * i + 1.0) * (2.0 * i + 3.0));
		J(i, i + 1) = a;
		J(i + 1, i) = a;
	}

	// --- Eigen decomposition ---
	Eigen::SelfAdjointEigenSolver<Eigen::MatrixXd> solver(J);

	//if (solver.info() != Eigen::Success)
	//	throw std::runtime_error("Eigen decomposition failed");

	Eigen::VectorXd x = solver.eigenvalues();
	Eigen::MatrixXd V = solver.eigenvectors();

	points.resize(n);
	weights.resize(n);

	// --- Sort + weights ---
	for (int i = 0; i < n; i++)
	{
		points[i] = x(i);
		weights[i] = 2.0 * V(0, i) * V(0, i);
	}

	// --- Ensure ordering (-1 to 1 increasing) ---
	std::vector<int> idx(n);
	std::iota(idx.begin(), idx.end(), 0);

	std::sort(idx.begin(), idx.end(),
		[&](int a, int b) { return points[a] < points[b]; });

	std::vector<double> p_sorted(n), w_sorted(n);

	for (int i = 0; i < n; i++)
	{
		p_sorted[i] = points[idx[i]];
		w_sorted[i] = weights[idx[i]];
	}

	points = p_sorted;
	weights = w_sorted;
}

