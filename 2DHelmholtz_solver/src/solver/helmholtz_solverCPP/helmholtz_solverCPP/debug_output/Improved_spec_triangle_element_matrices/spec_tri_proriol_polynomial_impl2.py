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



def evaluate_shape_functions_and_derivatives(xi, eta, V, modes):
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
    
    # Compute inverse of Vandermonde once (outside this function)
    # For now, we'll compute it here, but should be precomputed
    invV = np.linalg.inv(V)
    
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

# ============= TESTING CODE =============

def test_shape_functions():
    """Test shape functions and their derivatives"""
    
    # Parameters
    spectral_order = 4
    
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
    
    # Build Vandermonde
    modes = proriol_modes(spectral_order)
    V = build_vandermonde(reference_coords, modes)
    invV = np.linalg.inv(V)
    
    print("=" * 60)
    print("TESTING SHAPE FUNCTIONS")
    print("=" * 60)
    print(f"Number of modes: {len(modes)}")
    print(f"Vandermonde condition number: {np.linalg.cond(V):.2e}\n")
    
    # Test 1: Kronecker delta property
    print("TEST 1: Kronecker delta property")
    print("-" * 40)
    max_error = 0
    for node_idx, (xi, eta) in enumerate(reference_coords):
        N, _, _ = evaluate_shape_functions_and_derivatives(xi, eta, V, modes)
        
        # Check N[node_idx] should be 1, others should be 0
        for i in range(len(N)):
            expected = 1.0 if i == node_idx else 0.0
            error = abs(N[i] - expected)
            if error > max_error:
                max_error = error
        
        # Find the maximum value (should be at node_idx)
        max_val = np.max(N)
        max_pos = np.argmax(N)
        
        print(f"  Node {node_idx+1:2d} (xi={xi:.3f}, eta={eta:.3f}): "
              f"N[{node_idx+1}] = {N[node_idx]:.6f} (should be 1.0), "
              f"max at {max_pos+1} = {max_val:.6f}")
    
    print(f"\n  Maximum Kronecker delta error: {max_error:.2e}")
    if max_error < 1e-10:
        print("  ✓ PASSED")
    else:
        print("  ✗ FAILED")
    
    # Test 2: Partition of unity
    print("\nTEST 2: Partition of unity")
    print("-" * 40)
    test_points = [
        (0.0, 0.0), (1.0, 0.0), (0.0, 1.0),
        (0.3, 0.3), (0.2, 0.5), (0.5, 0.2),
        (0.1, 0.1), (0.4, 0.4), (0.25, 0.25),
        (0.75, 0.15), (0.15, 0.75)
    ]
    
    max_pu_error = 0
    for xi, eta in test_points:
        N, _, _ = evaluate_shape_functions_and_derivatives(xi, eta, V, modes)
        sum_N = np.sum(N)
        error = abs(sum_N - 1.0)
        max_pu_error = max(max_pu_error, error)
        print(f"  Point ({xi:.3f}, {eta:.3f}): sum(N) = {sum_N:.6f} (error = {error:.2e})")
    
    print(f"\n  Maximum partition of unity error: {max_pu_error:.2e}")
    if max_pu_error < 1e-10:
        print("  ✓ PASSED")
    else:
        print("  ✗ FAILED")
    
    # Test 3: Derivative consistency
    print("\nTEST 3: Derivative consistency")
    print("-" * 40)
    
    eps_fd = 1e-6
    test_points_deriv = [(0.2, 0.3), (0.4, 0.2), (0.3, 0.4)]
    
    max_deriv_error_xi = 0
    max_deriv_error_eta = 0
    
    for xi, eta in test_points_deriv:
        # Get derivatives from our function
        N, dN_dxi, dN_deta = evaluate_shape_functions_and_derivatives(xi, eta, V, modes)
        
        # Compute derivatives using finite difference on shape functions
        N_plus_xi, _, _ = evaluate_shape_functions_and_derivatives(xi + eps_fd, eta, V, modes)
        N_minus_xi, _, _ = evaluate_shape_functions_and_derivatives(xi - eps_fd, eta, V, modes)
        fd_dN_dxi = (N_plus_xi - N_minus_xi) / (2 * eps_fd)
        
        N_plus_eta, _, _ = evaluate_shape_functions_and_derivatives(xi, eta + eps_fd, V, modes)
        N_minus_eta, _, _ = evaluate_shape_functions_and_derivatives(xi, eta - eps_fd, V, modes)
        fd_dN_deta = (N_plus_eta - N_minus_eta) / (2 * eps_fd)
        
        # Compare
        error_xi = np.max(np.abs(dN_dxi - fd_dN_dxi))
        error_eta = np.max(np.abs(dN_deta - fd_dN_deta))
        
        max_deriv_error_xi = max(max_deriv_error_xi, error_xi)
        max_deriv_error_eta = max(max_deriv_error_eta, error_eta)
        
        print(f"  Point ({xi:.3f}, {eta:.3f}):")
        print(f"    dN/dxi error: {error_xi:.2e}")
        print(f"    dN/deta error: {error_eta:.2e}")
    
    print(f"\n  Maximum derivative error (xi): {max_deriv_error_xi:.2e}")
    print(f"  Maximum derivative error (eta): {max_deriv_error_eta:.2e}")
    if max_deriv_error_xi < 1e-6 and max_deriv_error_eta < 1e-6:
        print("  ✓ PASSED")
    else:
        print("  ✗ FAILED")
    
    # Test 4: Visual inspection
    print("\nTEST 4: Shape function values at selected points")
    print("-" * 40)
    
    test_vis_points = [
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
        (0.22422438821533711, 0.55155122356932573),    # Node 15
        (0.3, 0.3), (0.2, 0.5), (0.5, 0.2),
        (0.1, 0.1), (0.4, 0.4), (0.25, 0.25),
        (0.75, 0.15), (0.15, 0.75)
    ]
    
    print("\nShape function values for first 5 nodes at various points:")
    print("Point (xi, eta)     ", end="")
    for i in range(5):
        print(f"  N[{i+1:2d}]     ", end="")
    print()
    
    for xi, eta in test_vis_points:
        N, _, _ = evaluate_shape_functions_and_derivatives(xi, eta, V, modes)
        print(f"({xi:.3f}, {eta:.3f})    ", end="")
        for i in range(15):
            print(f"{N[i]:8.4f} ", end="")
        print()
    
    print("\n" + "=" * 60)
    print("TESTING COMPLETE")
    print("=" * 60)

# Run the tests
if __name__ == "__main__":
    test_shape_functions()












