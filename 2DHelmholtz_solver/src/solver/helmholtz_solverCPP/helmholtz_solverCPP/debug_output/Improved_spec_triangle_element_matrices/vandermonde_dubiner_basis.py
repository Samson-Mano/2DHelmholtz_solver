import numpy as np
from scipy.special import eval_jacobi

# -------------------------------
# Orthonormal Dubiner basis
# -------------------------------

def safe_dubiner_coordinates(L1, L2):
    """
    Stable collapsed coordinate transform for triangle spectral basis
    """

    # Clamp ONLY for numerical safety at vertex
    eps = 1e-15

    one_minus_L2 = 1.0 - L2

    if one_minus_L2 < eps:
        # We are at vertex (0,1,0) in barycentric coords
        # Return limiting values of Jacobi coordinates
        xi = -1.0
    else:
        xi = 2.0 * L1 / one_minus_L2 - 1.0

    eta = 2.0 * L2 - 1.0

    return xi, eta


def dubiner_basis_orthonormal(L1, L2, p, q):
    """
    Orthonormal Dubiner basis on reference triangle
    L1, L2 barycentric coordinates
    """

    a, b = safe_dubiner_coordinates(L1, L2)

    # Jacobi polynomials
    Pp = eval_jacobi(p, 0, 0, a)
    Pq = eval_jacobi(q, 2*p + 1, 0, b)

    # standard Dubiner weight
    w = (1.0 - L2)**p

    # -------------------------------
    # ORTHONORMALIZATION FACTOR
    # -------------------------------
    norm = np.sqrt((2*p + 1) * (p + q + 1))

    return norm * Pp * Pq * w



def dubiner_modes(order):
    return [(p, q)
            for p in range(order + 1)
            for q in range(order + 1 - p)]


def build_vandermonde(coords, modes):
    V = np.zeros((len(coords), len(modes)))

    for i, (L1, L2) in enumerate(coords):
        for j, (p, q) in enumerate(modes):
            V[i, j] = dubiner_basis_orthonormal(L1, L2, p, q)

    return V



def stable_inverse(V):
    return np.linalg.pinv(V, rcond=1e-14)

spectral_order = 4

# Triangle element reference coordinates (xi, eta) for the 15 nodes
reference_coords = [
    (-1.0, -1.0),  # Node 1
    (1.0, -1.0),  # Node 2
    (-1.0, 1.0),  # Node 3
    (-0.654653670707978, -1.0),  # Node 4
    (0.0, -1.0),  # Node 5
    (0.654653670707978, -1.0),  # Node 6
    (0.654653670707978, -0.654653670707978),  # Node 7
    (0.0, 0.0),  # Node 8
    (-0.654653670707978, 0.654653670707978),  # Node 9
    (-1.0, 0.654653670707978),  # Node 10
    (-1.0, 0.0),  # Node 11
    (-1.0, -0.654653670707978),  # Node 12
    (-0.551551223569326, -0.551551223569326),   # Node 13
    (0.55155122356932573, -0.551551223569326),   # Node 14
    (-0.551551223569326, 0.55155122356932573)    # Node 15
]

def test_orthonormality(modes, quad_points):
    n = len(modes)
    M = np.zeros((n, n))

    for L1, L2, w in quad_points:
        phi = np.array([
            dubiner_basis_orthonormal(L1, L2, p, q)
            for (p, q) in modes
        ])

        M += np.outer(phi, phi) * w * 0.5

    return M

spectral_order = 4

modes = dubiner_modes(spectral_order)  # order 4 -> 15 modes

V = build_vandermonde(reference_coords, modes)

M = V.T @ V  # should be identity-like

# print(M)

# Triangle quadrature points and weights
# Properly ordered for a 2D triangle element with 15 nodes

triangle_quadrature_points = [
    (0.108103018168070,0.445948490915965, 0.223381589678011),  # Point 1
    (0.445948490915965, 0.445948490915965, 0.223381589678011),  # Point 2
    (0.445948490915965, 0.108103018168070, 0.223381589678011),  # Point 3
    (0.816847572980459, 0.091576213509771, 0.109951743655322),  # Point 4
    (0.091576213509771, 0.091576213509771, 0.109951743655322),  # Point 5
    (0.091576213509771, 0.816847572980459, 0.109951743655322),  # Point 6
]



# M = test_orthonormality(modes, triangle_quadrature_points)
print(np.max(np.abs(M - np.eye(len(modes)))))

# ~ 1e-12 to 1e-14




