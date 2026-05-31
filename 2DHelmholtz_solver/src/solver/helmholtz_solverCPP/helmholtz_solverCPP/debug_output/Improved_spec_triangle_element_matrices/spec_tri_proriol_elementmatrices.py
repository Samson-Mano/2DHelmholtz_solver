import numpy as np
from scipy.special import eval_legendre, eval_jacobi

def proriol_modes(N):
    return [(i, j)
            for i in range(N + 1)
            for j in range(N + 1 - i)]



def proriol_basis(xi, eta, i, j):
    eps = 1e-10
    den = max(1.0 - eta, eps)

    # collapsed coordinates
    xi_bar = ((2.0 * xi) / den) - 1.0
    eta_bar = (2.0 * eta) - 1.0

    # polynomials
    Li = eval_legendre(i, xi_bar)
    Pj = eval_jacobi(j, (2.0*i) + 1.0, 0, eta_bar)

    # weight (stable form)
    w = ((1.0 - eta_bar) / 2.0)**i

    # normalization
    c = np.sqrt(((2.0 * i)+ 1.0) * (i + j + 1.0) * 0.5)

    return c * Li * Pj * w



def build_vandermonde(coords, modes):
    """Build Vandermonde matrix V where V_{p,q} = phi_q(xi_p, eta_p)"""
    Np = len(coords)
    n_modes = len(modes)
    
    V = np.zeros((Np, n_modes))
    
    for p, (xi, eta) in enumerate(coords):
        for q, (i, j) in enumerate(modes):
            V[p, q] = proriol_basis(xi, eta, i, j)
    
    return V



def evaluate_shape_functions_and_derivatives(xi, eta, invV, modes):
    """
    Evaluate shape functions and their derivatives at point (xi, eta).
    
    The shape functions N(xi, eta) are defined such that:
    N(xi_p, eta_p) = e_p (unit vector) at node p
    
    We have: V * c = f (where c are modal coefficients, f are nodal values)
    So for a given nodal values f, the modal coefficients are: c = invV @ f
    
    For shape function p (which is 1 at node p and 0 elsewhere),
    the nodal values are e_p, so the modal coefficients are invV[:, p].
    
    Therefore, shape function p at (xi, eta) is:
    N_p(xi, eta) = sum_q (invV[q, p] * phi_q(xi, eta))
                = (basis_vals @ invV)[p]
    """
    n = len(modes)
    
    # Evaluate all basis functions at the point
    basis_vals = np.zeros(n)
    for q, (i, j) in enumerate(modes):
        basis_vals[q] = proriol_basis(xi, eta, i, j)


    # Shape functions: N = basis_vals @ invV
    N = basis_vals @ invV
    
    # Compute derivatives of shape functions using finite differences
    eps = 1e-8
    dN_dxi = np.zeros(n)
    dN_deta = np.zeros(n)
    
    # Perturb in xi direction
    basis_vals_xi_plus = np.zeros(n)
    for q, (i, j) in enumerate(modes):
        basis_vals_xi_plus[q] = proriol_basis(xi + eps, eta, i, j)
    N_xi_plus = basis_vals_xi_plus @ invV
    
    if abs(xi) < eps:
        dN_dxi = (N_xi_plus - N) / eps
    elif abs(xi - 1.0) < eps:
        basis_vals_xi_minus = np.zeros(n)
        for q, (i, j) in enumerate(modes):
            basis_vals_xi_minus[q] = proriol_basis(xi - eps, eta, i, j)
        N_xi_minus = basis_vals_xi_minus @ invV
        dN_dxi = (N - N_xi_minus) / eps
    else:
        basis_vals_xi_minus = np.zeros(n)
        for q, (i, j) in enumerate(modes):
            basis_vals_xi_minus[q] = proriol_basis(xi - eps, eta, i, j)
        N_xi_minus = basis_vals_xi_minus @ invV
        dN_dxi = (N_xi_plus - N_xi_minus) / (2 * eps)
    
    # Perturb in eta direction
    basis_vals_eta_plus = np.zeros(n)
    for q, (i, j) in enumerate(modes):
        basis_vals_eta_plus[q] = proriol_basis(xi, eta + eps, i, j)
    N_eta_plus = basis_vals_eta_plus @ invV
    
    if abs(eta) < eps:
        dN_deta = (N_eta_plus - N) / eps
    elif abs(eta - 1.0) < eps:
        basis_vals_eta_minus = np.zeros(n)
        for q, (i, j) in enumerate(modes):
            basis_vals_eta_minus[q] = proriol_basis(xi, eta - eps, i, j)
        N_eta_minus = basis_vals_eta_minus @ invV
        dN_deta = (N - N_eta_minus) / eps
    else:
        basis_vals_eta_minus = np.zeros(n)
        for q, (i, j) in enumerate(modes):
            basis_vals_eta_minus[q] = proriol_basis(xi, eta - eps, i, j)
        N_eta_minus = basis_vals_eta_minus @ invV
        dN_deta = (N_eta_plus - N_eta_minus) / (2 * eps)
    
    return N, dN_dxi, dN_deta



def create_spectral_triangle_element(node_coords, quadrature_points, 
                                     V, modes, wave_number):
    """
    Compute element stiffness and mass matrices for a spectral triangle element.
    
    Parameters:
    -----------
    node_coords : list of tuples
        Physical coordinates of nodes (x, y) for each node
    quadrature_points : list of tuples
        Quadrature points as (L1, L2, weight) in barycentric coordinates
    V : numpy array
        Vandermonde matrix for the reference element
    modes : list of tuples
        Prorial mode indices (i, j)
    wave_number : float
        Wave number (omega) for Helmholtz equation
    
    Returns:
    --------
    K : numpy array
        Stiffness matrix
    M : numpy array
        Mass matrix
    A : numpy array
        System matrix (K - ω²M)
    """
    nen = len(node_coords)
    n_quad = len(quadrature_points)
    
    K = np.zeros((nen, nen))
    M = np.zeros((nen, nen))
    
    invV = np.linalg.inv(V)  # Compute inverse Vandermonde matrix

    print(f"Assembling element with {nen} nodes and {n_quad} quadrature points")
    
    for q_idx, (L1, L2, weight) in enumerate(quadrature_points):
        L3 = 1.0 - L1 - L2
        
        # Skip if point is outside triangle (shouldn't happen with proper quadrature)
        if L1 < -1e-12 or L2 < -1e-12 or L3 < -1e-12:
            print(f"Warning: Quadrature point {q_idx} is outside triangle: ({L1:.3f}, {L2:.3f}, {L3:.3f})")
            continue
        
        # Evaluate shape functions and derivatives in reference coordinates
        # Note: Passing V (not invV) as per corrected implementation
        N, dN_dL1, dN_dL2 = evaluate_shape_functions_and_derivatives(L1, L2, invV, modes)
        
        # Compute Jacobian matrix for mapping from reference to physical coordinates
        # J = [dx/dL1, dx/dL2; dy/dL1, dy/dL2]
        J = np.zeros((2, 2))
        for i in range(nen):
            x, y = node_coords[i]
            J[0, 0] += dN_dL1[i] * x   # dx/dL1
            J[0, 1] += dN_dL2[i] * x   # dx/dL2
            J[1, 0] += dN_dL1[i] * y   # dy/dL1
            J[1, 1] += dN_dL2[i] * y   # dy/dL2
        
        detJ = np.linalg.det(J)
        
        # Check for invalid Jacobian
        if detJ <= 0:
            print(f"Warning: Non-positive Jacobian determinant {detJ:.3e} at quadrature point {q_idx}")
            continue
            
        invJ = np.linalg.inv(J)
        
        # Transform derivatives from reference to physical coordinates
        # [dN/dx; dN/dy] = invJ^T * [dN/dL1; dN/dL2]
        # But careful: We have dN/dL1 and dN/dL2 as separate arrays
        # For each shape function i:
        # dN_dx[i] = invJ[0,0] * dN_dL1[i] + invJ[0,1] * dN_dL2[i]
        # dN_dy[i] = invJ[1,0] * dN_dL1[i] + invJ[1,1] * dN_dL2[i]
        dN_dx = np.zeros(nen)
        dN_dy = np.zeros(nen)
        for i in range(nen):
            dN_dx[i] = invJ[0, 0] * dN_dL1[i] + invJ[0, 1] * dN_dL2[i]
            dN_dy[i] = invJ[1, 0] * dN_dL1[i] + invJ[1, 1] * dN_dL2[i]
        
        # Quadrature weight including Jacobian determinant
        dV = detJ * weight
        
        # Assemble stiffness and mass matrices
        # Using symmetric assembly (only compute upper triangle and mirror)
        for i in range(nen):
            for j in range(i, nen):  # Only compute upper triangle
                # Stiffness: ∫∇N_i·∇N_j dΩ
                K_ij = (dN_dx[i] * dN_dx[j] + dN_dy[i] * dN_dy[j]) * dV
                K[i, j] += K_ij
                if i != j:
                    K[j, i] += K_ij
                
                # Mass: ∫N_i N_j dΩ
                M_ij = N[i] * N[j] * dV
                M[i, j] += M_ij
                if i != j:
                    M[j, i] += M_ij
        
        # Progress indicator
        if (q_idx + 1) % max(1, n_quad // 4) == 0:
            print(f"  Processed {q_idx+1}/{n_quad} quadrature points")
    
    # Compute system matrix: K - ω²M
    omega_sq = wave_number ** 2
    A = K - omega_sq * M
    
    # Print matrix statistics
    print(f"\nElement matrix statistics:")
    print(f"  Stiffness matrix condition: {np.linalg.cond(K):.2e}")
    print(f"  Mass matrix condition: {np.linalg.cond(M):.2e}")
    print(f"  System matrix condition: {np.linalg.cond(A):.2e}")
    
    return K, M, A




spectral_order = 4

wave_number = 0.41887902047863856


# Reference coordinates
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

triangle_quadrature_points = [
    (0.108103018168070,0.445948490915965, 0.223381589678011),  # Point 1
    (0.445948490915965, 0.445948490915965, 0.223381589678011),  # Point 2
    (0.445948490915965, 0.108103018168070, 0.223381589678011),  # Point 3
    (0.816847572980459, 0.091576213509771, 0.109951743655322),  # Point 4
    (0.091576213509771, 0.091576213509771, 0.109951743655322),  # Point 5
    (0.091576213509771, 0.816847572980459, 0.109951743655322),  # Point 6
]

modes = proriol_modes(spectral_order)  # order 4 -> 15 modes
V = build_vandermonde(reference_coords, modes)

K, M, A = create_spectral_triangle_element(node_coords, triangle_quadrature_points, V, modes, wave_number)

print("\nFinal matrices:")
print(f"K shape: {K.shape}")
print(f"M shape: {M.shape}")
print(f"A shape: {A.shape}")

print(f"A matrix stats: min={A.min():.3e}, max={A.max():.3e}, mean={A.mean():.3e}, cond={np.linalg.cond(A):.2e}")
print(f"A matrix:\n{A}")

# Check symmetry
sym_error_K = np.linalg.norm(K - K.T) / np.linalg.norm(K)
sym_error_M = np.linalg.norm(M - M.T) / np.linalg.norm(M)
print(f"K symmetry error: {sym_error_K:.2e}")
print(f"M symmetry error: {sym_error_M:.2e}")




