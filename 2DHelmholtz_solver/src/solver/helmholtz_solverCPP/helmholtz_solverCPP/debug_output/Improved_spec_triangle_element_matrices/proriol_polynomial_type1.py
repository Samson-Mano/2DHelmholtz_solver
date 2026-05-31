
import numpy as np
from scipy.special import eval_legendre, eval_jacobi



def proriol_modes(N):
    return [(i, j)
            for i in range(N + 1)
            for j in range(N + 1 - i)]



def proriol_basis(xi, eta, i, j):
    eps = 1e-14
    den = max(1.0 - eta, eps)

    # collapsed coordinates
    xi_bar = 2.0 * xi / den - 1.0
    eta_bar = 2.0 * eta - 1.0

    # polynomials
    Li = eval_legendre(i, xi_bar)
    Pj = eval_jacobi(j, 2*i + 1, 0, eta_bar)

    # weight (stable form)
    w = ((1.0 - eta_bar) / 2.0)**i

    # normalization
    c = np.sqrt((2*i + 1.0) * (i + j + 1.0) * 0.5)

    return c * Li * Pj * w


def build_vandermonde(coords, modes):
    Np = len(coords)
    V = np.zeros((Np, Np))

    for p, (xi, eta) in enumerate(coords):
        for q, (i, j) in enumerate(modes):
            V[p, q] = proriol_basis(xi, eta, i, j)

    return V

def test_vandermonde(V):
    # rank
    rank = np.linalg.matrix_rank(V)

    # conditioning
    cond = np.linalg.cond(V)

    # singular values
    s = np.linalg.svd(V, compute_uv=False)

    print("Rank:", rank)
    print("Condition number: {:.2e}".format(cond))
    print("Min singular value:", s[-1])
    print("Max singular value:", s[0])

    return rank, cond, s


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



# reference_coords = [
#     (0.0, 0.0),  # Node 1
#     (0.17267316464601140, 0.0),  # Node 4
#     (0.5, 0.0),  # Node 5
#     (0.82732683535398865, 0.0),  # Node 6
#     (1.0, 0.0),  # Node 2
#     (0.0, 0.17267316464601140),  # Node 12
#     (0.22422438821533711, 0.2242243882153371),   # Node 13
#     (0.55155122356932573, 0.2242243882153371),   # Node 14
#     (0.82732683535398865, 0.17267316464601140),  # Node 7
#     (0.0, 0.5),  # Node 11
#     (0.22422438821533711, 0.55155122356932573),    # Node 15
#     (0.5, 0.5),  # Node 8
#     (0.0, 0.82732683535398865),  # Node 10
#     (0.17267316464601140, 0.82732683535398865),  # Node 9
#     (0.0, 1.0),  # Node 3
# ]


def test_orthonormality(V):
    M = V.T @ V
    print("Condition number of M = V^T V:", np.linalg.cond(M))
    I_error = np.linalg.norm(M - np.eye(len(M)), 'fro')
    print("Orthonormality error (Frobenius): {:.2e}".format(I_error))
    Vinvtest = np.linalg.norm(V @ invV - np.eye(len(modes)))
    print("V @ invV - I norm:", Vinvtest)


modes = proriol_modes(spectral_order)

V = build_vandermonde(reference_coords, modes)

rank, cond, s = test_vandermonde(V)


invV = np.linalg.inv(V)

test_orthonormality(V)













