import numpy as np
from scipy.special import eval_jacobi



def koornwinder_dubiner_mode(spectral_order):
    # Koornwinder-Dubiner modes on reference triangle
    # 0 <= i, j and i + j <= order
    return [(i, j)
            for j in range(spectral_order + 1)
            for i in range(spectral_order + 1 - j)]


def koornwinder_dubiner_basis(x, y, i, j):
    # Koornwinder-Dubiner basis function
    # Jacobi polynomial i
    if abs(1.0 - y) < 1e-8:
        xbar = -1.0
    else:
        xbar = ((2.0 * x) + y + 1.0)/(1.0 - y)

    P_i = eval_jacobi(i, 0, 0, xbar)

    # Jacobi polynomial j
    P_j = eval_jacobi(j, ((2.0 * i) + 1.0),  0,  y)

    # Weight function
    wt = ((1.0 - y) / 2.0)**i

    # Coefficient c_ij
    c_val = ((2.0 * i) + 1.0) * (i + j + 1.0)
    c_ij = np.sqrt(c_val / 2.0)

    return c_ij * P_i * P_j * wt


def Hesthaven_Warburton_dubiner_basis(x, y, i, j):
    # Hesthaven-Warburton Dubiner basis function
    # Jacobi polynomial i
    if abs(1.0 - y) < 1e-8:
        xbar = -1.0
    else:
        xbar = (2.0 *(1.0 + x) / (1.0 - y)) - 1.0   

    P_i = eval_jacobi(i, 0, 0, xbar)

    # Jacobi polynomial j
    P_j = eval_jacobi(j, ((2.0 * i) + 1.0),  0,  y)

    # Weight function
    wt = (1.0 - y)**i
    coeff = np.sqrt(2.0)

    return coeff * P_i * P_j * wt


def build_vandermonde(coords, KD_modes):
    Np = len(coords)

    # Build Vandermonde matrix for given coordinates
    # Note that coordinates are in (xi, eta) format and i, j are the mode indices
    V = np.zeros((Np, Np))

    for p, (x, y) in enumerate(coords):
        for q, (i, j) in enumerate(KD_modes):
            V[p, q] = koornwinder_dubiner_basis(x, y, i, j)

    return V



spectral_order = 4

Number_of_nodes = ((spectral_order + 1) * (spectral_order + 2)) / 2

print("Number of nodes for spectral order {} = {}".format(spectral_order, int(Number_of_nodes)))

KD_modes = koornwinder_dubiner_mode(spectral_order)

print(KD_modes)


# # Node coordinates (xi, eta) for the 15 nodes on the triangle for spectral order 4
# # Each tuple contains (i, j, xi, eta) where (i, j) are the mode indices and (xi, eta) are the coordinates
# node_coords = [
#     (0, 0, -1.0, -1.0),  # Node 1
#     (1, 0, -0.654653670707978, -1.0),  # Node 4
#     (2, 0, 0.0, -1.0),  # Node 5
#     (3, 0, 0.654653670707978, -1.0),  # Node 6
#     (4, 0, 1.0, -1.0),  # Node 2
#     (0, 1, -1.0, -0.654653670707978),  # Node 12
#     (1, 1, -0.551551223569326, -0.551551223569326),   # Node 13
#     (2, 1, 0.55155122356932573, -0.551551223569326),   # Node 14
#     (3, 1, 0.654653670707978, -0.654653670707978),  # Node 7
#     (0, 2, -1.0, 0.0),  # Node 11
#     (1, 2, -0.551551223569326, 0.55155122356932573),    # Node 15
#     (2, 2, 0.0, 0.0),  # Node 8
#     (0, 3,-1.0, 0.654653670707978),  # Node 10
#     (1, 3,-0.654653670707978, 0.654653670707978),  # Node 9
#     (0, 4, -1.0, 0.9999),  # Node 3
# ]
    # (-0.551784777779014,-0.551784777779014),
    # (0.551784777779014,-0.551784777779014),
    # (-0.333333333333333,-0.333333333333333)

node_coords = [
    (-1,-1),(1,-1),(-1,0.9999),

    (-0.6546536707079771,-1),
    (0.0,-1),
    (0.6546536707079771,-1),

    (-1,-0.6546536707079771),
    (-1,0.0),
    (-1,0.6546536707079771),

    (0.6546536707079771,-0.6546536707079771),
    (0.0,0.0),
    (-0.6546536707079771,0.6546536707079771),

    (-0.551784777779014,-0.551784777779014),
    (0.551784777779014,0.551784777779014),
    (-0.333333333333333,-0.333333333333333)
]



V = build_vandermonde(node_coords, KD_modes)
cond_num = np.linalg.cond(V)
print(f"Vandermonde condition number: {cond_num:.2e}")

print(f"Vandermonde matrix rank: {np.linalg.matrix_rank(V)}")

U,S,VT = np.linalg.svd(V)

print("Null mode 1")
print(np.round(VT[-1],4))

print("Null mode 2")
print(np.round(VT[-2],4))


# print("Singular values:")
# print(S)

print("Min singular value:",S[-1])
print("Max singular value:",S[0])


# Inverse of Vandermonde matrix
invV = np.linalg.inv(V)



def evaluate_shape_functions(x,y,modes,invV):

    D = np.array([
        koornwinder_dubiner_basis(x,y,i,j)
        for i,j in modes
    ])

    N = invV @ D

    return N

for p, (xp,yp) in enumerate(node_coords):

    N = evaluate_shape_functions(
        xp,
        yp,
        KD_modes,
        invV
    )

    # print(np.round(N,10))



