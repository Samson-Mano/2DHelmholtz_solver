import numpy as np


def get_gll_nodes(N, tol=1e-12, max_iter=100):
    """
    Compute Gauss-Lobatto-Legendre (GLL) nodes for a given spectral order N.

    Parameters
    ----------
    N : int
        Spectral order (number of points = N+1)
    tol : float
        Convergence tolerance for Newton iteration
    max_iter : int
        Maximum Newton iterations

    Returns
    -------
    xi : ndarray
        GLL nodes in [-1, 1]
    """

    xi = np.zeros(N + 1)

    # Endpoints
    xi[0] = -1.0
    xi[N] = 1.0

    if N == 1:
        return xi

    # Loop over interior points
    for i in range(1, N):
        # Initial guess (Chebyshev-Gauss-Lobatto)
        x = -np.cos(np.pi * i / N)

        for _ in range(max_iter):
            # Compute Legendre P_N(x) and P_{N-1}(x)
            Pnm1 = 1.0   # P0
            Pn = x       # P1

            for k in range(2, N + 1):
                Pkp1 = ((2*k - 1)*x*Pn - (k - 1)*Pnm1) / k
                Pnm1, Pn = Pn, Pkp1

            # First derivative P'_N
            dPn = (N * (Pnm1 - x * Pn)) / (1.0 - x**2)

            # Second derivative P''_N
            ddPn = (2.0 * x * dPn - N * (N + 1) * Pn) / (1.0 - x**2)

            # Newton update
            dx = -dPn / ddPn
            x += dx

            if abs(dx) < tol:
                break

        xi[i] = x

    return xi

