import numpy as np
from numpy.polynomial.legendre import leggauss


def lobatto_nodes_weights(N):
    """Gauss-Lobatto nodes (includes endpoints)"""
    if N == 1:
        return np.array([-1.0, 1.0]), np.array([1.0, 1.0])

    x, w = leggauss(N - 1)
    nodes = np.zeros(N + 1)
    nodes[0] = -1
    nodes[-1] = 1
    nodes[1:-1] = x

    # simple weights (good enough for FEM assembly demo)
    weights = np.ones(N + 1)
    return nodes, weights


def lagrange_1d(xi, nodes, i):
    """Evaluate Lagrange basis L_i at xi"""
    val = 1.0
    for j, xj in enumerate(nodes):
        if j != i:
            val *= (xi - xj) / (nodes[i] - xj)
    return val


def lagrange_1d_derivative(xi, nodes, i):
    """Numerical derivative (stable enough for FEM prototype)"""
    h = 1e-8
    return (lagrange_1d(xi + h, nodes, i) -
            lagrange_1d(xi - h, nodes, i)) / (2*h)



def square_to_triangle(xi, eta):
    """
    Maps [-1,1]^2 → reference triangle
    """
    L1 = 0.5 * (1 + xi) * (1 - eta)
    L2 = 0.5 * (1 + eta)
    L3 = 1 - L1 - L2
    return L1, L2, L3


def evaluate_shape_functions(N, xi, eta):
    """
    Tensor-product LGL basis on square mapped to triangle
    """
    nodes, _ = lobatto_nodes_weights(N)

    phi = []
    dphi_dxi = []
    dphi_deta = []

    for j in range(N + 1):
        for i in range(N + 1):

            # basis
            Lxi = lagrange_1d(xi, nodes, i)
            Leta = lagrange_1d(eta, nodes, j)
            phi.append(Lxi * Leta)

            # derivatives (tensor product)
            dLxi = lagrange_1d_derivative(xi, nodes, i)
            dLeta = lagrange_1d_derivative(eta, nodes, j)

            dphi_dxi.append(dLxi * Leta)
            dphi_deta.append(Lxi * dLeta)

    return np.array(phi), np.array(dphi_dxi), np.array(dphi_deta)



def triangle_jacobian(node_coords, dN_dxi, dN_deta):
    """
    Compute J = dx/d(xi,eta)
    """
    J = np.zeros((2, 2))

    for i, (x, y) in enumerate(node_coords):
        J[0, 0] += dN_dxi[i] * x
        J[0, 1] += dN_deta[i] * x
        J[1, 0] += dN_dxi[i] * y
        J[1, 1] += dN_deta[i] * y

    return J


def spectral_triangle_element(node_coords, N, quad_points):
    """
    Full spectral triangle element (stable formulation)
    """
    nen = (N + 1)**2

    K = np.zeros((nen, nen))
    M = np.zeros((nen, nen))

    for (xi, eta, w) in quad_points:

        phi, dphi_dxi, dphi_deta = evaluate_shape_functions(N, xi, eta)

        J = triangle_jacobian(node_coords, dphi_dxi, dphi_deta)
        detJ = np.linalg.det(J)
        invJ = np.linalg.inv(J)

        # gradients in physical space
        dphi_dx = np.zeros(nen)
        dphi_dy = np.zeros(nen)

        for i in range(nen):
            dphi_dx[i] = invJ[0,0]*dphi_dxi[i] + invJ[0,1]*dphi_deta[i]
            dphi_dy[i] = invJ[1,0]*dphi_dxi[i] + invJ[1,1]*dphi_deta[i]

        # assembly
        for i in range(nen):
            for j in range(nen):

                K[i,j] += (dphi_dx[i]*dphi_dx[j] +
                           dphi_dy[i]*dphi_dy[j]) * detJ * w

                M[i,j] += phi[i]*phi[j] * detJ * w

    return K, M



# Node data
node_coords = [
    (-10.0, -10.0),  # Node 1
    (10.0, -10.0),  # Node 2
    (0.0, 10.0),  # Node 3
    (-6.5465367070797731, -10.0),  # Node 4
    (0.0, -10.0),  # Node 5
    (6.5465367070797731, -10.0),  # Node 6
    (8.2732683535398870, -6.5465367070797731),  # Node 7
    (5.0, 0.0),  # Node 8
    (1.7267316464601139, 6.5465367070797731),  # Node 9
    (-1.7267316464601139, 6.5465367070797731),  # Node 10
    (-5.0, 0.0),  # Node 11
    (-8.2732683535398870, -6.5465367070797731),  # Node 12
    (-3.2732683535398861, -5.5155122356932562),  # Node 13
    (3.2732683535398861, -5.5155122356932562),  # Node 14
    (0.0, 1.0310244713865160),  # Node 15
]


spectral_order = 4

# Quadrature points and weights for triangle 
from quadrilateral_quadrature_pts import get_quadrilateral_quadrature_points
quadrature_points = get_quadrilateral_quadrature_points(spectral_order)



K, M = spectral_triangle_element(node_coords, spectral_order, quadrature_points)






