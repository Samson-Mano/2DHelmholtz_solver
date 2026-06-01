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




def evaluate_shape_functions(xi, eta, invV, modes):
    """Evaluate shape functions and their derivatives at (xi, eta)"""
    n = len(modes)
    
    # Compute monomial values
    phi = np.zeros(n)
    dphi_dxi = np.zeros(n)
    dphi_deta = np.zeros(n)
    
    for j, (a, b) in enumerate(modes):
        phi[j] = proriol_basis(xi, eta, a, b)
        
        # Derivatives of monomials
        if a > 0:
            dphi_dxi[j] = a * (xi ** (a-1)) * (eta ** b)
        else:
            dphi_dxi[j] = 0.0
            
        if b > 0:
            dphi_deta[j] = b * (xi ** a) * (eta ** (b-1))
        else:
            dphi_deta[j] = 0.0
    
    # Transform to shape functions using inverse Vandermonde
    N = invV @ phi
    dN_dxi = invV @ dphi_dxi
    dN_deta = invV @ dphi_deta
    
    return N, dN_dxi, dN_deta


def create_spectral_triangle_element(node_coords, quadrature_points, 
                                     invV, modes, wave_number):
    """Compute element stiffness and mass matrices"""
    nen = len(node_coords)
    n_quad = len(quadrature_points)
    
    K = np.zeros((nen, nen))
    M = np.zeros((nen, nen))
    
    print(f"Assembling element with {nen} nodes and {n_quad} quadrature points")
    
    for q in range(n_quad):
        L1, L2, weight = quadrature_points[q]
        L3 = 1.0 - L1 - L2  # Third barycentric coordinate
        
        # Evaluate shape functions at quadrature point
        N, dN_dL1, dN_dL2 = evaluate_shape_functions(L1, L2, invV, modes)
        
        # Compute Jacobian for mapping from reference to physical coordinates
        # For triangle: [dx/dL1, dx/dL2; dy/dL1, dy/dL2]
        J = np.zeros((2, 2))
        for i in range(nen):
            x, y = node_coords[i]
            J[0, 0] += dN_dL1[i] * x
            J[0, 1] += dN_dL2[i] * x
            J[1, 0] += dN_dL1[i] * y
            J[1, 1] += dN_dL2[i] * y
        
        detJ = np.linalg.det(J)
        invJ = np.linalg.inv(J)
        
        # Transform derivatives to physical coordinates
        dN_dx = np.zeros(nen)
        dN_dy = np.zeros(nen)
        for i in range(nen):
            dN_dx[i] = invJ[0, 0] * dN_dL1[i] + invJ[0, 1] * dN_dL2[i]
            dN_dy[i] = invJ[1, 0] * dN_dL1[i] + invJ[1, 1] * dN_dL2[i]
        
        # Assemble matrices
        for i in range(nen):
            for j in range(nen):
                # Stiffness: ∫∇N_i·∇N_j dΩ
                K[i, j] += (dN_dx[i] * dN_dx[j] + dN_dy[i] * dN_dy[j]) * detJ * weight
                
                # Mass: ∫N_i N_j dΩ
                M[i, j] += N[i] * N[j] * detJ * weight
        
        # Progress indicator (optional)
        if (q + 1) % 4 == 0:
            print(f"  Processed {q+1}/{n_quad} quadrature points")
    
    # Compute system matrix: K - ω²M
    omega_sq = wave_number ** 2
    A = K - omega_sq * M
    
    return K, M, A




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





