#include "gll_utility.h"

std::vector<double> gll_utility::get_gll_locations(int spectral_order)
{
	// Pn(xi) Legendre Polynomial & Pn'(xi) Derivative of Legendre polynomial
	// spectral order, Pn, Pn'
	// 1, xi, 1
	// 2, 1/2 (3*xi^2 - 1), 3*xi
	// 3, 1/2 (5*xi^3 - 3*xi), 1/2 (15*xi^2 - 3)

	// GLL Points
	// Gauss - Lobatto - Legendre (GLL) Points are the roots of (1- xi^2) * Pn'

	int N = spectral_order;

	std::vector<double> xi(N + 1);

	// Endpoints
	xi[0] = -1.0;
	xi[N] = 1.0;

	if (N == 1)
		return xi;


	const double h = 0.0001;

	// Loop over interior points
	for (int i = 1; i < N; i++)
	{
		// Initial guess (Chebyshev-Gauss-Lobatto nodes)
		double x = -cos(m_pi * (i / static_cast<double>(N)));

		for (int iter = 0; iter < max_iter; iter++)
		{
			// Compute Pn and Pn-1 using recurrence
			double Pnm1 = 1.0;   // P0
			double Pn = x;       // P1

			for (int k = 2; k <= N; k++)
			{
				double Pkp1 = ((2.0 * k - 1.0) * x * Pn - (k - 1.0) * Pnm1) / k;
				Pnm1 = Pn;
				Pn = Pkp1;
			}

			// Derivative Pn'
			double dPn = (N * (Pnm1 - x * Pn)) / (1.0 - x * x);

			// Second derivative (needed for Newton step)
			double ddPn = (2.0 * x * dPn - N * (N + 1.0) * Pn) / (1.0 - x * x);

			// Newton update
			double dx = -dPn / ddPn;
			x += dx;

			if (std::abs(dx) < tol)
				break;
		}

		xi[i] = x;
	}

	return xi;
	//
}




std::vector<double> gll_utility::get_gll_weights(int spectral_order, const std::vector<double>& gll_points_xi)
{

	// Find the spectral polynomial values
	int N = spectral_order;

	std::vector<double> wi(N + 1);

	for (int i = 0; i < N + 1; i++)
	{
		// Get the Legendre polynomial
		double Pn_xi_i = std::legendre(N, gll_points_xi[i]);

		// Weights w_i = 2.0 / (N * (N + 1)) * Pn_xi_i * Pn_xi_i

		wi[i] = 2.0 / (N * (N + 1) * Pn_xi_i * Pn_xi_i);

	}

	return wi;
	//
}



std::vector<spectral_point> gll_utility::get_triangle_spectral_element(int spectral_order)
{
	// 3
	// |\
    // | \
    // |  \
    // 1---2


	// Get the GLL points of 1D edges
	std::vector<double> gll_points = get_gll_locations(spectral_order);


	// Triangle spectral element points
	std::vector<spectral_point> tri_spectral_points;

	// Create the corner points
	std::vector<spectral_point> corner_points;

	corner_points.emplace_back(spectral_point{ 0.0, 0.0, 1.0 }); // Point 1
	corner_points.emplace_back(spectral_point{ 1.0, 0.0, 1.0 }); // Point 2
	corner_points.emplace_back(spectral_point{0.0, 1.0, 1.0 }); // Point 3

	// Add the corner
	tri_spectral_points.emplace_back(spectral_point{ corner_points[0].xi, corner_points[0].eta, corner_points[0].weight });
	tri_spectral_points.emplace_back(spectral_point{ corner_points[1].xi, corner_points[1].eta, corner_points[1].weight });
	tri_spectral_points.emplace_back(spectral_point{ corner_points[2].xi, corner_points[2].eta, corner_points[2].weight });

	for (int i = 0; i < 3; i++)
	{
		spectral_point v_start = corner_points[i];
		spectral_point v_end = corner_points[(i + 1) % 3];

		// Add the edges
		for (int j = 1; j < spectral_order; j++) // Exclude the end point -1 and 1
		{
			// Map [-1, 1] to [0, 1]
			double s = (gll_points[j] + 1.0) / 2.0;

			double x_coord = ((1.0 - s) * v_start.xi) + (s * v_end.xi);
			double y_coord = ((1.0 - s) * v_start.eta) + (s * v_end.eta);

			tri_spectral_points.emplace_back(spectral_point{ x_coord, y_coord, 1.0 });
		}

	}

	// Populate Interior(Lobatto interpolation)
	// IMA Journal of Applied Mathematics Advance Access published March 16, 2005
	// A Lobatto interpolation grid over the triangle
	// M.G.Blyth and C.Pozrikidis

	if (spectral_order > 2)
	{
		for (int i = 1; i < spectral_order; i++)
		{
			for (int j = 1; j < spectral_order - i; j++)
			{
				int k = spectral_order - i - j;

				double vi = (gll_points[i] + 1.0) / 2.0;
				double vj = (gll_points[j] + 1.0) / 2.0;
				double vk = (gll_points[k] + 1.0) / 2.0;

				double x_coord = (1.0 / 3.0) * (1.0 + (2.0 * vj) - vi - vk);
				double y_coord = (1.0 / 3.0) * (1.0 + (2.0 * vk) - vi - vj);

				tri_spectral_points.emplace_back(spectral_point{ x_coord, y_coord, 1.0 });
			}
		}
		//
	}

	return tri_spectral_points;
	//
}






std::vector<spectral_point> gll_utility::get_quadrilateral_spectral_element(int spectral_order)
{
	// 4-----3     
	// |     |
	// |     | 
	// |     |
	// 1-----2


	// Get the GLL points of 1D edges
	std::vector<double> gll_points = get_gll_locations(spectral_order);


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


std::vector<spectral_point> gll_utility::get_triangle_quadrature(int spectral_order)
{
	std::vector<spectral_point> quadrature_points;

	// Map spectral order to Dunavant rule number
	int rule = get_dunavant_rule_for_order(spectral_order);

	// Get the number of quadrature points for this rule
	int order_num = dunavant_order_num(rule);

	// Allocate arrays for points and weights
	// Note: xy array needs to be of size 2*order_num for x and y coordinates
	std::vector<double> xy(2 * order_num);
	std::vector<double> w(order_num);

	// Call dunavant_rule
	dunavant_rule(rule, order_num, xy.data(), w.data());

	// Convert to spectral_point format
	for (int i = 0; i < order_num; i++) 
	{
		double L1 = xy[2 * i];      // x coordinate (L1)
		double L2 = xy[2 * i + 1];  // y coordinate (L2)
		double L3 = 1.0 - L1 - L2; // L3 is determined

		quadrature_points.push_back({ L1, L2, w[i] });
	}

	return quadrature_points;
}


int gll_utility::get_dunavant_rule_for_order(int spectral_order)
{
	// Minimum rule numbers needed for exact integration of polynomials of degree 2p
	// where p = spectral_order
	switch (spectral_order) 
	{
	case 1:  return 1;   // 1 point, degree 1
	case 2:  return 2;   // 3 points, degree 2  
	case 3:  return 3;   // 4 points, degree 3
	case 4:  return 4;   // 6 points, degree 4
	case 5:  return 5;   // 7 points, degree 5
	case 6:  return 6;   // 12 points, degree 6
	case 7:  return 7;   // 16 points, degree 7
	case 8:  return 8;   // 19 points, degree 8
	case 9:  return 9;   // 25 points, degree 9
	case 10: return 10;  // 31 points, degree 10
	case 11: return 11;  // 37 points, degree 11
	case 12: return 12;  // 43 points, degree 12
	case 13: return 13;  // 49 points, degree 13
	case 14: return 14;  // 55 points, degree 14
	case 15: return 15;  // 61 points, degree 15
	case 16: return 16;  // 67 points, degree 16
	case 17: return 17;  // 73 points, degree 17
	case 18: return 18;  // 79 points, degree 18
	case 19: return 19;  // 85 points, degree 19
	case 20: return 20;  // 91 points, degree 20
	default: return 10;  // Default to order 10 rule, 31 points, degree 10 
	}
	//
}


std::vector<spectral_point> gll_utility::get_triangle_quadrature_manual(int spectral_order)
{
	auto quadrature_points = get_unsorted_triangle_quadrature(spectral_order);  // Your existing function

	// Sort lexicographically for consistency
	std::sort(quadrature_points.begin(), quadrature_points.end(),
		[](const spectral_point& a, const spectral_point& b) {
			if (a.xi != b.xi) return a.xi < b.xi;
			return a.eta < b.eta;
		});

	return quadrature_points;

	//
}



std::vector<spectral_point> gll_utility::get_unsorted_triangle_quadrature(int spectral_order)
{

	// Dunavant Quadrature for Area Coordinate Triangle

	std::vector<spectral_point> quadrature_points;

	if (spectral_order == 1)
	{
		quadrature_points = {
			{1.0 / 3.0, 1.0 / 3.0, 1.0}
		};
	}
	else if (spectral_order == 2)
	{
		quadrature_points = {
			{1.0 / 6.0, 2.0 / 3.0, 1.0 / 3.0},
			{1.0 / 6.0, 1.0 / 6.0, 1.0 / 3.0},
			{2.0 / 3.0, 1.0 / 6.0, 1.0 / 3.0}
		};
	}
	else if (spectral_order == 3)
	{
		quadrature_points = {
			{1.0 / 3.0, 1.0 / 3.0, -0.5625},
			{0.2, 0.6, 0.520833333333333},
			{0.2, 0.2, 0.520833333333333},
			{0.6, 0.6, 0.520833333333333}
		};
	}
	else if (spectral_order == 4)
	{
		quadrature_points = {
			{0.445948490915965, 0.108103018168070, 0.223381589678011},
			{0.445948490915965, 0.445948490915965, 0.223381589678011},
			{0.108103018168070, 0.445948490915965, 0.223381589678011},
			{0.091576213509771, 0.816847572980459, 0.109951743655322},
			{0.091576213509771, 0.091576213509771, 0.109951743655322},
			{0.816847572980459, 0.091576213509771, 0.109951743655322},
		};
	}
	else if (spectral_order == 5)
	{
		quadrature_points = {
			{ 0.333333333333333,	0.333333333333333,	0.225000000000000 },
			{ 0.470142064105115,	0.059715871789770,	0.132394152788506 },
			{ 0.470142064105115,	0.470142064105115,	0.132394152788506 },
			{ 0.059715871789770,	0.470142064105115,	0.132394152788506 },
			{ 0.101286507323456,	0.797426985353087,	0.125939180544827 },
			{ 0.101286507323456,	0.101286507323456,	0.125939180544827 },
			{ 0.797426985353087,	0.101286507323456,	0.125939180544827 }
		};
	}
	else if (spectral_order == 6)
	{
		quadrature_points = {
			{	0.063089014491502,	0.873821971016996,	0.050844906370207	}	,
			{	0.063089014491502,	0.063089014491502,	0.050844906370207	}	,
			{	0.873821971016996,	0.063089014491502,	0.050844906370207	}	,
			{	0.053145049844817,	0.636502499121399,	0.082851075618374	}	,
			{	0.310352451033784,	0.053145049844817,	0.082851075618374	}	,
			{	0.636502499121399,	0.310352451033784,	0.082851075618374	}	,
			{	0.310352451033784,	0.636502499121399,	0.082851075618374	}	,
			{	0.053145049844817,	0.310352451033784,	0.082851075618374	}	,
			{	0.636502499121399,	0.053145049844817,	0.082851075618374	}	,
			{	0.249286745170910,	0.501426509658179,	0.116786275726379	}	,
			{	0.249286745170910,	0.249286745170910,	0.116786275726379	}	,
			{	0.501426509658179,	0.249286745170910,	0.116786275726379	}
		};
	}
	else if (spectral_order == 7)
	{
		quadrature_points = {
			{	0.333333333333333,	0.333333333333333,	-0.149570044467682	}	,
			{	0.065130102902216,	0.869739794195568,	0.053347235608838	}	,
			{	0.065130102902216,	0.065130102902216,	0.053347235608838	}	,
			{	0.869739794195568,	0.065130102902216,	0.053347235608838	}	,
			{	0.048690315425316,	0.638444188569810,	0.077113760890257	}	,
			{	0.312865496004874,	0.048690315425316,	0.077113760890257	}	,
			{	0.638444188569810,	0.312865496004874,	0.077113760890257	}	,
			{	0.312865496004874,	0.638444188569810,	0.077113760890257	}	,
			{	0.048690315425316,	0.312865496004874,	0.077113760890257	}	,
			{	0.638444188569810,	0.048690315425316,	0.077113760890257	}	,
			{	0.260345966079040,	0.479308067841920,	0.175615257433208	}	,
			{	0.260345966079040,	0.260345966079040,	0.175615257433208	}	,
			{	0.479308067841920,	0.260345966079040,	0.175615257433208	}
		};
	}
	else if (spectral_order == 8)
	{
		quadrature_points = {
			{	0.333333333333333,	0.333333333333333,	0.144315607677787	}	,
			{	0.459292588292723,	0.081414823414554,	0.095091634267285	}	,
			{	0.459292588292723,	0.459292588292723,	0.095091634267285	}	,
			{	0.081414823414554,	0.459292588292723,	0.095091634267285	}	,
			{	0.170569307751760,	0.658861384496480,	0.103217370534718	}	,
			{	0.170569307751760,	0.170569307751760,	0.103217370534718	}	,
			{	0.658861384496480,	0.170569307751760,	0.103217370534718	}	,
			{	0.008394777409958,	0.728492392955404,	0.027230314174435	}	,
			{	0.263112829634638,	0.008394777409958,	0.027230314174435	}	,
			{	0.728492392955404,	0.263112829634638,	0.027230314174435	}	,
			{	0.263112829634638,	0.728492392955404,	0.027230314174435	}	,
			{	0.008394777409958,	0.263112829634638,	0.027230314174435	}	,
			{	0.728492392955404,	0.008394777409958,	0.027230314174435	}	,
			{	0.050547228317031,	0.898905543365938,	0.032458497623198	}	,
			{	0.050547228317031,	0.050547228317031,	0.032458497623198	}	,
			{	0.898905543365938,	0.050547228317031,	0.032458497623198	}
		};
	}
	else if (spectral_order == 9)
	{
		quadrature_points = {
			{	0.333333333333333,	0.333333333333333,	0.097135796282799	}	,
			{	0.489682519198738,	0.020634961602525,	0.031334700227139	}	,
			{	0.489682519198738,	0.489682519198738,	0.031334700227139	}	,
			{	0.020634961602525,	0.489682519198738,	0.031334700227139	}	,
			{	0.437089591492937,	0.125820817014127,	0.077827541004774	}	,
			{	0.437089591492937,	0.437089591492937,	0.077827541004774	}	,
			{	0.125820817014127,	0.437089591492937,	0.077827541004774	}	,
			{	0.188203535619033,	0.623592928761935,	0.079647738927210	}	,
			{	0.188203535619033,	0.188203535619033,	0.079647738927210	}	,
			{	0.623592928761935,	0.188203535619033,	0.079647738927210	}	,
			{	0.036838412054736,	0.741198598784498,	0.043283539377289	}	,
			{	0.221962989160766,	0.036838412054736,	0.043283539377289	}	,
			{	0.741198598784498,	0.221962989160766,	0.043283539377289	}	,
			{	0.221962989160766,	0.741198598784498,	0.043283539377289	}	,
			{	0.036838412054736,	0.221962989160766,	0.043283539377289	}	,
			{	0.741198598784498,	0.036838412054736,	0.043283539377289	}	,
			{	0.044729513394453,	0.910540973211095,	0.025577675658698	}	,
			{	0.044729513394453,	0.044729513394453,	0.025577675658698	}	,
			{	0.910540973211095,	0.044729513394453,	0.025577675658698	}
		};
	}
	else
	{
		// order 10 and above
		quadrature_points = {
			{	0.333333333333333,	0.333333333333333,	0.090817990382754	}	,
			{	0.485577633383657,	0.028844733232685,	0.036725957756467	}	,
			{	0.485577633383657,	0.485577633383657,	0.036725957756467	}	,
			{	0.028844733232685,	0.485577633383657,	0.036725957756467	}	,
			{	0.141707219414880,	0.550352941820999,	0.072757916845420	}	,
			{	0.307939838764121,	0.141707219414880,	0.072757916845420	}	,
			{	0.550352941820999,	0.307939838764121,	0.072757916845420	}	,
			{	0.307939838764121,	0.550352941820999,	0.072757916845420	}	,
			{	0.141707219414880,	0.307939838764121,	0.072757916845420	}	,
			{	0.550352941820999,	0.141707219414880,	0.072757916845420	}	,
			{	0.025003534762686,	0.728323904597411,	0.028327242531057	}	,
			{	0.246672560639903,	0.025003534762686,	0.028327242531057	}	,
			{	0.728323904597411,	0.246672560639903,	0.028327242531057	}	,
			{	0.246672560639903,	0.728323904597411,	0.028327242531057	}	,
			{	0.025003534762686,	0.246672560639903,	0.028327242531057	}	,
			{	0.728323904597411,	0.025003534762686,	0.028327242531057	}	,
			{	0.009540815400299,	0.923655933587500,	0.009421666963733	}	,
			{	0.066803251012200,	0.009540815400299,	0.009421666963733	}	,
			{	0.923655933587500,	0.066803251012200,	0.009421666963733	}	,
			{	0.066803251012200,	0.923655933587500,	0.009421666963733	}	,
			{	0.009540815400299,	0.066803251012200,	0.009421666963733	}	,
			{	0.923655933587500,	0.009540815400299,	0.009421666963733	}	,
			{	0.109481575485037,	0.781036849029926,	0.045321059435528	}	,
			{	0.109481575485037,	0.109481575485037,	0.045321059435528	}	,
			{	0.781036849029926,	0.109481575485037,	0.045321059435528	}
		};
	}


	return quadrature_points;
	//
}


std::vector<spectral_point> gll_utility::get_quadrilateral_quadrature(int spectral_order)
{
	// Gauss - Legendre points/ weights

	std::vector<spectral_point> quadrature_points;

	int n = spectral_order + 1;

	std::vector<double> gp = get_gauss_points(n);
	std::vector<double> gw = get_gauss_weights(n);


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



Eigen::MatrixXd gll_utility::get_inverse_vandermonde_matrix(int spectral_order)
{

	// Build Vandermonde Matrix
	int nen = ((spectral_order + 1) * (spectral_order + 2)) / 2;

	Eigen::MatrixXd vandermonde_matrix(nen, nen);

	// Get the reference triangle element for spectral order
	std::vector<spectral_point> elem_ref_coords = get_triangle_spectral_element(spectral_order);

	// Get the triangle basis terms
	std::vector<basis_term> triangle_basis_terms = build_basis_terms(spectral_order);

	for (int i = 0; i < nen; i++)
	{
		double xi = elem_ref_coords[i].xi; // reference triangle node x coord
		double eta = elem_ref_coords[i].eta; // reference triangle node y coord

		for (int j = 0; j < nen; j++)
		{
			int a = triangle_basis_terms[j].a;
			int b = triangle_basis_terms[j].b;

			vandermonde_matrix(i, j) = pow(xi, a) * pow(eta, b);
		}
	}

	Eigen::MatrixXd inv_vandermonde_matrix = vandermonde_matrix.inverse();

	// Eigen::MatrixXd inv_vandermonde_matrix = vandermonde_matrix.colPivHouseholderQr().solve(
	//	Eigen::MatrixXd::Identity(nen, nen));

	return inv_vandermonde_matrix;
}



void gll_utility::evaluate_basis_phi(double xi, double eta,
	const std::vector<basis_term>& basis_terms,
	Eigen::VectorXd& phi)
{

	int n = static_cast<int>(basis_terms.size());

	phi.resize(n);

	for (int i = 0; i < n; i++)
	{
		int a = basis_terms[i].a;
		int b = basis_terms[i].b;

		phi(i) = std::pow(xi, a) * std::pow(eta, b);
	}
	//
}


void gll_utility::evaluate_basis_derivatives(
	double xi,
	double eta,
	const std::vector<basis_term>& basis_terms,
	Eigen::VectorXd& dphi_dxi,
	Eigen::VectorXd& dphi_deta)
{
	int n = static_cast<int>(basis_terms.size());

	dphi_dxi.resize(n);
	dphi_deta.resize(n);

	for (int i = 0; i < n; i++)
	{
		int a = basis_terms[i].a;
		int b = basis_terms[i].b;

		// d/dxi
		if (a == 0)
			dphi_dxi(i) = 0.0;
		else
			dphi_dxi(i) = a * std::pow(xi, a - 1) * std::pow(eta, b);

		// d/deta
		if (b == 0)
			dphi_deta(i) = 0.0;
		else
			dphi_deta(i) = b * std::pow(xi, a) * std::pow(eta, b - 1);
	}

}


void gll_utility::evaluate_lagrange_1D(double x,
	const std::vector<double>& nodes,
	std::vector<double>& L,
	std::vector<double>& dL)
{
	int n = nodes.size();

	L.resize(n);
	dL.resize(n);

	for (int i = 0; i < n; i++)
	{
		double li = 1.0;
		double dli = 0.0;

		for (int j = 0; j < n; j++)
		{
			if (j == i) continue;

			double denom = nodes[i] - nodes[j];
			li *= (x - nodes[j]) / denom;
		}

		// Derivative
		for (int j = 0; j < n; j++)
		{
			if (j == i) continue;

			double term = 1.0 / (nodes[i] - nodes[j]);

			for (int k = 0; k < n; k++)
			{
				if (k == i || k == j) continue;

				term *= (x - nodes[k]) / (nodes[i] - nodes[k]);
			}

			dli += term;
		}

		L[i] = li;
		dL[i] = dli;
	}

}


std::vector<basis_term> gll_utility::build_basis_terms(int spectral_order)
{
	// Monomial basis
	// Phi_k = (xi^a) (eta^b) with a+b <= spectral order

	std::vector<basis_term> basis_terms;

	for (int a = 0; a <= spectral_order; a++)
	{
		for (int b = 0; b <= spectral_order - a; b++)
		{
			basis_terms.push_back({ a, b });
		}
	}

	return basis_terms;
}


std::vector<double> gll_utility::get_gauss_points(int n)
{
	std::vector<double> pts, wts;
	gauss_legendre(n, pts, wts);
	return pts;
}



std::vector<double> gll_utility::get_gauss_weights(int n)
{
	std::vector<double> pts, wts;
	gauss_legendre(n, pts, wts);
	return wts;
}



void gll_utility::gauss_legendre(int n,	std::vector<double>& points, std::vector<double>& weights)
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

