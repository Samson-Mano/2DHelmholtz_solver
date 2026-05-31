
import numpy as np
from scipy.special import eval_legendre, eval_jacobi



def proriol_modes(N):
    modes = []
    for i in range(N + 1):
        for j in range(N + 1 - i):
            modes.append((int(i), int(j)))  # FORCE clean tuple
    return modes



def proriol(xi, eta, i, j):
    """
    Orthogonal Proriol/Dubiner basis on triangle
    xi, eta in reference triangle (0<=xi,eta, xi+eta<=1)
    """

    if eta >= 1.0:
        eta = 1.0 - 1e-15

    xi_bar = (2.0 * xi / (1.0 - eta)) - 1.0
    eta_bar = 2.0 * eta - 1.0

    # Jacobi structure
    P_i = eval_jacobi(i, 0, 0, xi_bar)
    P_j = eval_jacobi(j, 2*i + 1, 0, eta_bar)

    # weight
    w = (1.0 - eta)**i

    return P_i * P_j * w



# def proriol(xi, eta, i, j):
#     if eta > 1.0 - 1e-14:
#         eta = 1.0 - 1e-14

#     a = (2*xi/(1-eta)) - 1
#     b = 2*eta - 1

#     P_i = eval_jacobi(i, 0, 0, a)
#     P_j = eval_jacobi(j, 2*i+1, 0, b)

#     return (1-eta)**i * P_i * P_j



def build_vandermonde(coords, modes):
    Np = len(coords)
    V = np.zeros((Np, Np))

    for p, (xi, eta) in enumerate(coords):
        for q, (i, j) in enumerate(modes):
            V[p, q] = proriol(xi, eta, i, j)

    return V



def shape_functions(xi, eta, invV, modes):
    """
    Lagrange shape functions from spectral basis
    """

    phi = np.array([
        proriol(xi, eta, i, j)
        for (i, j) in modes
    ])

    N = invV @ phi

    return N


def proriol_derivatives(xi, eta, i, j, h=1e-8):
    f0 = proriol(xi, eta, i, j)

    dxi = (proriol(xi+h, eta, i, j) - proriol(xi-h, eta, i, j)) / (2*h)
    deta = (proriol(xi, eta+h, i, j) - proriol(xi, eta-h, i, j)) / (2*h)

    return dxi, deta



def spectral_triangle_element(nodes, quad, invV, modes):

    N = len(nodes)

    K = np.zeros((N, N))
    M = np.zeros((N, N))

    for (xi, eta, w) in quad:

        # shape functions in modal space
        phi = np.array([proriol(xi, eta, i, j) for (i, j) in modes])

        # derivatives (still FD but OK for now)
        dphi_x = np.array([
            proriol_derivatives(xi, eta, i, j)[0] for (i, j) in modes
        ])

        dphi_y = np.array([
            proriol_derivatives(xi, eta, i, j)[1] for (i, j) in modes
        ])

        # modal -> nodal
        Nq = invV @ phi
        dN_xi = invV @ dphi_x
        dN_eta = invV @ dphi_y

        # Jacobian
        J = np.zeros((2, 2))

        for k in range(N):
            x, y = nodes[k]
            J[0, 0] += dN_xi[k] * x
            J[0, 1] += dN_eta[k] * x
            J[1, 0] += dN_xi[k] * y
            J[1, 1] += dN_eta[k] * y

        detJ = np.linalg.det(J)
        invJ = np.linalg.inv(J)

        dN_dx = invJ[0, 0] * dN_xi + invJ[0, 1] * dN_eta
        dN_dy = invJ[1, 0] * dN_xi + invJ[1, 1] * dN_eta

        # assembly
        for i in range(N):
            for j in range(N):

                K[i, j] += (dN_dx[i]*dN_dx[j] + dN_dy[i]*dN_dy[j]) * detJ * w
                M[i, j] += Nq[i] * Nq[j] * detJ * w

    return K, M, K - wave_number**2 * M





spectral_order = 4

# Node coordinates (xi, eta) for the 15 nodes on the triangle for spectral order 4
# Lobatto triangular nodes in (xi, eta) format
reference_coords = [
    (0.0, 0.0),  # Node 1
    (1.0, 0.0),  # Node 2
    (0.0, 1.0),  # Node 3
    (0.17267316464601140, 0.0),  # Node 4
    (0.5, 0.0),  # Node 5
    (0.82732683535398865, 0.0),  # Node 6
    (0.82732683535398865, 0.17267316464601140),  # Node 7
    (0.5, 0.5),  # Node 8
    (0.17267316464601140, 0.82732683535398865),  # Node 9
    (0.0, 0.82732683535398865),  # Node 10
    (0.0, 0.5),  # Node 11
    (0.0, 0.17267316464601140),  # Node 12
    (0.22422438821533711, 0.2242243882153371),   # Node 13
    (0.55155122356932573, 0.2242243882153371),   # Node 14
    (0.22422438821533711, 0.55155122356932573)    # Node 15
]

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


wave_number = 0.041887902047863856




modes = proriol_modes(spectral_order)

V = build_vandermonde(reference_coords, modes)

invV = np.linalg.inv(V)

K,M,A = spectral_triangle_element(node_coords, triangle_quadrature_points, invV, modes)



print("Stiffness matrix K:")
print(np.round(K,10))


print("Mass matrix M:")
print(np.round(M,10))

print("Helmholtz matrix A = K - k^2 M:")
print(np.round(A,10))


from scipy.linalg import eig

eigvals, eigvecs = eig(K, M)

print("Eigenvalues of K x = lambda M x:")
print(np.round(eigvals,10))






