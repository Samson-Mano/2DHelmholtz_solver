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




dubiner_basis_terms = get_dubiner_basis_terms(spectral_order)
V = build_vandermonde_dubiner(reference_coords, dubiner_basis_terms)
invV = np.linalg.pinv(V, rcond=1e-12)



# def dubiner_basis_derivatives(L1, L2, a, b):
#     """
#     Compute derivatives of Dubiner basis with respect to L1 and L2
#     Using finite differences for simplicity (you can optimize later)
#     """
#     h = 1e-8
    
#     # Value at point
#     f0 = dubiner_basis(L1, L2, a, b)
    
#     # Derivative w.r.t L1
#     f_plus = dubiner_basis(L1 + h, L2, a, b)
#     df_dL1 = (f_plus - f0) / h
    
#     # Derivative w.r.t L2
#     f_plus = dubiner_basis(L1, L2 + h, a, b)
#     df_dL2 = (f_plus - f0) / h
    
#     return df_dL1, df_dL2


def evaluate_shape_functions_dubiner(L1, L2, invV, dubiner_basis_terms):

    n = len(dubiner_basis_terms)

    phi = np.zeros(n)
    dphi_dL1 = np.zeros(n)
    dphi_dL2 = np.zeros(n)

    for j, (a, b) in enumerate(dubiner_basis_terms):

        phi[j] = dubiner_basis(L1, L2, a, b)

        # replace finite difference with complex-step
        dphi_dL1[j] = np.imag(dubiner_basis(L1 + 1j*1e-20, L2, a, b)) / 1e-20

        dphi_dL2[j] = np.imag(dubiner_basis(L1, L2 + 1j*1e-20, a, b)) / 1e-20

    N = invV @ phi
    dN_dL1 = invV @ dphi_dL1
    dN_dL2 = invV @ dphi_dL2

    return N, dN_dL1, dN_dL2




def create_spectral_triangle_element(node_coords, quadrature_points, 
                                     invV, dubiner_basis_terms, wave_number):
    """Compute element stiffness and mass matrices"""
    nen = len(node_coords)
    n_quad = len(quadrature_points)

    K = np.zeros((nen, nen))
    M = np.zeros((nen, nen))

    print(f"Assembling element with {nen} nodes and {n_quad} quadrature points")

    # Precompute constant Jacobian structure is NOT valid for curved elements,
    # but for linear triangle we can precompute nodal gradients per quad

    for q in range(n_quad):

        L1, L2, weight = quadrature_points[q]

        N, dN_dL1, dN_dL2 = evaluate_shape_functions_dubiner(
            L1, L2, invV, dubiner_basis_terms
        )

        # Jacobian
        x = np.array([p[0] for p in node_coords])
        y = np.array([p[1] for p in node_coords])

        dx_dL1 = np.dot(dN_dL1, x)
        dx_dL2 = np.dot(dN_dL2, x)
        dy_dL1 = np.dot(dN_dL1, y)
        dy_dL2 = np.dot(dN_dL2, y)

        J = np.array([[dx_dL1, dx_dL2],
                      [dy_dL1, dy_dL2]])

        detJ = np.linalg.det(J)

        invJ = np.linalg.inv(J)

        dN_dx = invJ[0,0]*dN_dL1 + invJ[0,1]*dN_dL2
        dN_dy = invJ[1,0]*dN_dL1 + invJ[1,1]*dN_dL2

        for i in range(nen):
            for j in range(nen):

                K[i, j] += (dN_dx[i]*dN_dx[j] + dN_dy[i]*dN_dy[j]) * detJ * weight
                M[i, j] += N[i]*N[j] * detJ * weight

    A = K - wave_number**2 * M

    return K, M, A


def test_element_matrices(K, M, A):
    print("\n================ ELEMENT MATRIX CHECKS ================")
    
    print(f"K shape: {K.shape}")
    print(f"M shape: {M.shape}")
    print(f"A shape: {A.shape}")
    
    sym_K = np.linalg.norm(K - K.T)
    sym_M = np.linalg.norm(M - M.T)
    
    print(f"K symmetry error: {sym_K:.2e}")
    print(f"M symmetry error: {sym_M:.2e}")
    
    eig_M = np.linalg.eigvalsh(M)
    eig_K = np.linalg.eigvalsh(K)
    
    print(f"Min eigenvalue of M: {eig_M.min():.2e}")
    print(f"Min eigenvalue of K: {eig_K.min():.2e}")

    u = np.ones(K.shape[0])

    Ku = K @ u
    Mu = M @ u

    print("\n===== CONSTANT FIELD TEST =====")
    print("||K * 1||:", np.linalg.norm(Ku))
    print("M * 1 (should be mass of element):", Mu)

    u = np.random.rand(K.shape[0])

    energy_K = u @ K @ u
    energy_M = u @ M @ u

    print("\n===== ENERGY TEST =====")
    print("u^T K u:", energy_K)
    print("u^T M u:", energy_M)

    eig_A = np.linalg.eigvalsh(A)

    print("\n===== HELMHOLTZ MATRIX CHECK =====")
    print("Min eigenvalue of A:", eig_A.min())
    print("Max eigenvalue of A:", eig_A.max())

    print("\n===== SCALING CHECK =====")
    print("trace(M):", np.trace(M))
    print("trace(K):", np.trace(K))

    coords = np.array(node_coords)
    u = coords[:,0] + coords[:,1]

    Ku = u @ K @ u
    Mu = u @ M @ u

    print("\n===== POLYNOMIAL ENERGY TEST =====")
    print("u^T K u:", Ku)
    print("u^T M u:", Mu)



# Assemble element matrices
K, M, A = create_spectral_triangle_element(node_coords, triangle_quadrature_points, 
                                           invV, dubiner_basis_terms, wave_number)




test_element_matrices(K, M, A)










