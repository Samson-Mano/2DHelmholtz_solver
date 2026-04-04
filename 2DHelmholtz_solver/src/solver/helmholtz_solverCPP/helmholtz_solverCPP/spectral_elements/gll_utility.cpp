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





