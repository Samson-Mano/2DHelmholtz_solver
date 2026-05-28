import numpy as np

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





def get_dubiner_basis_terms(order):
    """Return list of (a,b) pairs for Dubiner basis where a+b <= order"""
    terms = []
    for a in range(order + 1):
        for b in range(order + 1 - a):
            terms.append((a, b))
    return terms



from scipy.special import eval_jacobi

def dubiner_basis(L1, L2, a, b):
    xi = L1
    eta = L2
    
    # Transform
    if abs(1.0 - eta) < 1e-12:
        x = -1.0
    else:
        x = 2.0 * xi / (1 - eta) - 1.0
    
    y = 2.0 * eta - 1.0

    # Proper Dubiner
    P_a = eval_jacobi(a, 0, 0, x)
    P_b = eval_jacobi(b, 2*a + 1, 0, y)
    
    # Weight factor
    w = ((1.0 - y) / 2.0) ** a

    return P_a * P_b * w



def build_vandermonde_dubiner(reference_coords, dubiner_basis_terms):
    """Build Vandermonde matrix using Dubiner orthogonal basis"""
    nen = len(reference_coords)
    n_basis = len(dubiner_basis_terms)
    
    print(f"Number of nodes: {nen}")
    print(f"Number of basis terms: {n_basis}")
    
    V = np.zeros((nen, n_basis))
    
    for i, (L1, L2) in enumerate(reference_coords):
        for j, (a, b) in enumerate(dubiner_basis_terms):
            V[i, j] = dubiner_basis(L1, L2, a, b)
    
    return V

def test_vandermonde(V):
    print("\n================ VANDERMONDE DIAGNOSTICS ================")
    
    # Shape check
    n, m = V.shape
    print(f"Shape: {n} x {m}")
    
    # Condition number
    cond_V = np.linalg.cond(V)
    print(f"Condition number: {cond_V:.2e}")
    
    # Rank check
    rank = np.linalg.matrix_rank(V)
    print(f"Rank: {rank} (expected {n})")
    
    # Singular values
    U, S, Vt = np.linalg.svd(V)
    print(f"Min singular value: {S.min():.2e}")
    print(f"Max singular value: {S.max():.2e}")
    
    return cond_V


def compute_inverse(V):
    print("\n================ INVERSE TEST ================")
    
    invV = np.linalg.pinv(V, rcond=1e-12)
    
    I_check = V @ invV
    error = np.max(np.abs(I_check - np.eye(V.shape[0])))
    
    print(f"V @ invV error: {error:.2e}")
    
    return invV, error


def test_polynomial_reproduction(V, invV, reference_coords):
    print("\n================ POLYNOMIAL TEST ================")
    
    # Test polynomial (must be ≤ order 4)
    def f(x, y):
        return x**4 + y**4 + x*y + x**2*y
    
    f_vals = np.array([f(x, y) for x, y in reference_coords])
    
    coeffs = invV @ f_vals
    f_reconstructed = V @ coeffs
    
    error = np.max(np.abs(f_vals - f_reconstructed))
    print(f"Reconstruction error: {error:.2e}")
    
    return error



dubiner_basis_terms = get_dubiner_basis_terms(spectral_order)
V = build_vandermonde_dubiner(reference_coords, dubiner_basis_terms)



cond_V = test_vandermonde(V)
invV, inv_error = compute_inverse(V)
poly_error = test_polynomial_reproduction(V, invV, reference_coords)


