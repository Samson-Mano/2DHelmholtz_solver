
import numpy as np


# Single Triangle element spectral mesh with 15 nodes 
#
#   x
#   | \
#   |   \ 
#   @     @
#   |       \
#   |    0    \
#   @           @
#   |             \
#   |    0     0    \  
#   @                 @
#   |                   \
#   |                     \
#   x --- @ --- @ --- @ --- x


# Spectral order = 4

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



# Triangle element reference coordinates (xi, eta) for the 15 nodes
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



nen = 15
wave_number = 0.41887902047863856
spectral_order = 4




def create_triangle_basis_terms(spectral_order):
    """Create monomial basis terms up to given order"""
    basis_terms = []
    for a in range(spectral_order + 1):
        for b in range(spectral_order + 1 - a):
            basis_terms.append((a, b))
    return basis_terms



def create_inverse_vandermonde_matrix(spectral_order, reference_coords):
    """Build Vandermonde matrix and compute its inverse"""
    basis_terms = create_triangle_basis_terms(spectral_order)
    nen = len(reference_coords)
    
    # Build Vandermonde matrix
    V = np.zeros((nen, nen))
    for i in range(nen):
        xi, eta = reference_coords[i]
        for j, (a, b) in enumerate(basis_terms):
            V[i, j] = (xi ** a) * (eta ** b)
    
    # Compute inverse (using pseudo-inverse for stability)
    invV = np.linalg.pinv(V)
    
    # Verify
    print(f"Vandermonde condition number: {np.linalg.cond(V):.2e}")

    return invV, V, basis_terms



def evaluate_shape_functions(xi, eta, invV, basis_terms):
    """Evaluate shape functions and their derivatives at (xi, eta)"""
    n = len(basis_terms)
    
    # Compute monomial values
    phi = np.zeros(n)
    dphi_dxi = np.zeros(n)
    dphi_deta = np.zeros(n)
    
    for j, (a, b) in enumerate(basis_terms):
        phi[j] = (xi ** a) * (eta ** b)
        
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
                                     invV, basis_terms, wave_number):
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
        N, dN_dL1, dN_dL2 = evaluate_shape_functions(L1, L2, invV, basis_terms)
        
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




# Main execution
print("="*60)
print("Triangle Spectral Element Assembly")
print("="*60)

# Create basis and inverse Vandermonde
invV, V, basis_terms = create_inverse_vandermonde_matrix(spectral_order, reference_coords)

# Assemble element matrices
K, M, A = create_spectral_triangle_element(node_coords, triangle_quadrature_points, 
                                           invV, basis_terms, wave_number)



def check_common_issues(node_coords, reference_coords, quadrature_points, invV, basis_terms):
    """Check for common implementation issues"""
    
    print("\n" + "="*60)
    print("COMMON ISSUES CHECK")
    print("="*60)
    
    # 1. Check node ordering (should be counter-clockwise)
    corners = node_coords[:3]
    x = [c[0] for c in corners]
    y = [c[1] for c in corners]
    area = 0.5 * ((x[1]-x[0])*(y[2]-y[0]) - (x[2]-x[0])*(y[1]-y[0]))
    print(f"\n1. Node ordering:")
    print(f"   Signed area: {area:.6f}")
    if area < 0:
        print(f"   WARNING: Nodes are in clockwise order! Should be counter-clockwise.")
        print(f"   FIX: Reverse node order or swap two corner nodes.")
    
    # 2. Check reference coordinates for interior nodes
    print(f"\n2. Reference coordinates:")
    for i, (L1, L2) in enumerate(reference_coords):
        L3 = 1.0 - L1 - L2
        if i >= 3:  # Skip corners
            print(f"   Node {i}: L1={L1:.4f}, L2={L2:.4f}, L3={L3:.4f}")
            if not (0 <= L1 <= 1 and 0 <= L2 <= 1 and 0 <= L3 <= 1):
                print(f"     ERROR: Node {i} outside reference triangle!")
    
    # 3. Check inverse Vandermonde accuracy
    print(f"\n3. Vandermonde matrix:")
    nen = len(reference_coords)
    V = np.zeros((nen, nen))
    for i in range(nen):
        xi, eta = reference_coords[i]
        for j, (a, b) in enumerate(basis_terms):
            V[i, j] = (xi ** a) * (eta ** b)
    
    cond_num = np.linalg.cond(V)
    print(f"   Condition number: {cond_num:.2e}")
    if cond_num > 1e10:
        print(f"   WARNING: Ill-conditioned! Need to use orthogonal polynomials.")
    
    # 4. Check that sum of shape functions equals 1
    print(f"\n4. Partition of unity test:")
    test_points = [(0.2, 0.3), (0.5, 0.3), (0.3, 0.5)]
    for L1, L2 in test_points:
        N, _, _ = evaluate_shape_functions(L1, L2, invV, basis_terms)
        sum_N = np.sum(N)
        print(f"   At (L1={L1:.2f}, L2={L2:.2f}): sum(N) = {sum_N:.6f}")
        if abs(sum_N - 1.0) > 1e-6:
            print(f"     ERROR: Partition of unity violated!")
    
    # 5. Check that shape functions are non-negative in element interior
    print(f"\n5. Positivity test:")
    test_points = [(0.33, 0.33), (0.2, 0.2), (0.5, 0.3)]
    for L1, L2 in test_points:
        N, _, _ = evaluate_shape_functions(L1, L2, invV, basis_terms)
        min_N = np.min(N)
        print(f"   At (L1={L1:.2f}, L2={L2:.2f}): min(N) = {min_N:.6f}")
        if min_N < -1e-6:
            print(f"     WARNING: Negative shape function values!")

# Run common issues check
check_common_issues(node_coords, reference_coords, triangle_quadrature_points, invV, basis_terms)


# Print results
def print_matrix_stats(name, matrix, nen):
    print(f"\n{name}:")
    print(f"  Shape: {matrix.shape}")
    print(f"  Condition number: {np.linalg.cond(matrix):.2e}")
    print(f"  Min diagonal: {np.min(np.diag(matrix)):.6e}")
    print(f"  Max diagonal: {np.max(np.diag(matrix)):.6e}")
    print(f"  Symmetric: {np.allclose(matrix, matrix.T, atol=1e-10)}")
    
    # For small matrices, print the full matrix
    if nen <= 15:
        print(f"  Full matrix:")
        for i in range(min(15, nen)):
            row_str = "    "
            for j in range(min(15, nen)):
                row_str += f"{matrix[i, j]:12.4e} "
            print(row_str)
        if nen > 15:
            print(f"    ... (showing first 15x15)")

print_matrix_stats("Inverse Vandermonde Matrix (V⁻¹)", invV, nen)
print_matrix_stats("Vandermonde Matrix (V)", V, nen)
print_matrix_stats("Stiffness Matrix (K)", K, nen)
print_matrix_stats("Mass Matrix (M)", M, nen)
print_matrix_stats("System Matrix (A = K - ω²M)", A, nen)

# Save to file for comparison with C++
np.savetxt("frmpy_stiffness_matrix_python.txt", K, fmt="%.12e", delimiter=" ")
np.savetxt("frmpy_mass_matrix_python.txt", M, fmt="%.12e", delimiter=" ")
np.savetxt("frmpy_system_matrix_python.txt", A, fmt="%.12e", delimiter=" ")

print(f"\nMatrices saved to:")
print(f"  stiffness_matrix_python.txt")
print(f"  mass_matrix_python.txt")
print(f"  system_matrix_python.txt")

# Verify matrix properties
print(f"\nVerification:")
print(f"  Stiffness positive definite: {np.all(np.linalg.eigvals(K) > 0)}")
print(f"  Mass positive definite: {np.all(np.linalg.eigvals(M) > 0)}")
print(f"  System matrix positive definite: {np.all(np.linalg.eigvals(A) > 0)}")










