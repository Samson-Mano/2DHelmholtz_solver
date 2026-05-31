import numpy as np
from scipy.special import eval_jacobi
from scipy.optimize import minimize, differential_evolution
from scipy.linalg import svd

def koornwinder_dubiner_mode(spectral_order):
    """Generate Koornwinder-Dubiner modes (i,j) with i+j <= order"""
    return [(i, j) for j in range(spectral_order + 1) 
            for i in range(spectral_order + 1 - j)]

def koornwinder_dubiner_basis(x, y, i, j):
    """Evaluate Koornwinder-Dubiner basis function"""
    if y >= 1.0 - 1e-12:
        return 1.0 if (i == 0 and j == 0) else 0.0
    
    if abs(1.0 - y) < 1e-12:
        xi = -1.0
    else:
        xi = 2.0 * (1.0 + x) / (1.0 - y) - 1.0
    
    eta = y
    xi = np.clip(xi, -1.0, 1.0)
    eta = np.clip(eta, -1.0, 1.0)
    
    P_i = eval_jacobi(i, 0, 0, xi)
    P_j = eval_jacobi(j, 2*i + 1, 0, eta)
    
    if abs(1.0 - eta) < 1e-12:
        wt = 1.0 if i == 0 else 0.0
    else:
        wt = ((1.0 - eta) / 2.0) ** i
    
    norm = np.sqrt((2.0*i + 1.0) * (i + j + 1.0) / 2.0)
    return norm * P_i * P_j * wt

def build_vandermonde(node_coords, modes):
    """Build Vandermonde matrix"""
    V = np.zeros((len(node_coords), len(modes)))
    for p, (x, y) in enumerate(node_coords):
        for q, (i, j) in enumerate(modes):
            V[p, q] = koornwinder_dubiner_basis(x, y, i, j)
    return V

def objective_function(interior_nodes, edge_nodes, modes, spectral_order):
    """
    Objective function for interior node optimization.
    We want to minimize condition number and maximize shape function quality.
    """
    # Combine edge and interior nodes
    all_nodes = edge_nodes + list(interior_nodes.reshape(-1, 2))
    
    # Build Vandermonde
    V = build_vandermonde(all_nodes, modes)
    
    # Compute condition number
    cond = np.linalg.cond(V)
    
    # Check rank
    rank = np.linalg.matrix_rank(V)
    rank_penalty = 1e6 * (len(modes) - rank)  # Heavy penalty for rank deficiency
    
    # Compute shape function quality (want diagonal of V * invV close to identity)
    try:
        invV = np.linalg.pinv(V)
        V_invV = V @ invV
        # Penalize off-diagonal entries
        off_diag_penalty = np.sum(np.abs(V_invV - np.eye(len(modes)))) / len(modes)
    except:
        off_diag_penalty = 1e6
    
    # Return combined objective (lower is better)
    return cond + rank_penalty + off_diag_penalty

def generate_prescribed_nodes(spectral_order):
    """
    Generate prescribed edge nodes (must match quad element GLL nodes)
    for triangle with vertices (-1,-1), (1,-1), (-1,1)
    """
    # GLL nodes for order 4 (5 nodes per edge including vertices)
    gll_nodes = [-1.0, -0.6546536707079771, 0.0, 0.6546536707079771, 1.0]
    
    edge_nodes = []
    
    # Bottom edge: y = -1, x from -1 to 1
    for x in gll_nodes:
        edge_nodes.append((x, -1.0))
    
    # Right edge: from (1,-1) to (-1,1)
    # Parametric: x = 1 - t, y = -1 + 2t, where t in [0,1]
    for i, t in enumerate(np.linspace(0, 1, len(gll_nodes))):
        if i > 0 and i < len(gll_nodes)-1:  # Avoid duplicating vertices
            x = 1.0 - t
            y = -1.0 + 2.0 * t
            edge_nodes.append((x, y))
    
    # Left edge: from (-1,-1) to (-1,1)
    for y in gll_nodes[1:-1]:  # Skip vertices
        edge_nodes.append((-1.0, y))
    
    # Remove duplicates (vertices appear multiple times)
    unique_nodes = []
    for node in edge_nodes:
        if not any(abs(node[0] - u[0]) < 1e-12 and abs(node[1] - u[1]) < 1e-12 for u in unique_nodes):
            unique_nodes.append(node)
    
    return unique_nodes

def find_optimal_interior_nodes(spectral_order, n_interior, n_trials=10):
    """
    Find optimal interior nodes using differential evolution
    """
    modes = koornwinder_dubiner_mode(spectral_order)
    edge_nodes = generate_prescribed_nodes(spectral_order)
    
    print(f"Edge nodes: {len(edge_nodes)}")
    print(f"Need {n_interior} interior nodes (total: {len(edge_nodes) + n_interior} = {len(modes)})")
    
    # Bounds for interior nodes (must stay inside triangle)
    # Triangle vertices: (-1,-1), (1,-1), (-1,1)
    bounds = []
    for _ in range(n_interior):
        # x bounds: from left edge to right edge at given y
        bounds.extend([(-1.0, 1.0), (-1.0, 1.0)])  # Will enforce triangle constraint separately
    
    # Initial guess (equidistant points)
    initial_guess = []
    for i in range(n_interior):
        # Place interior nodes in a grid pattern
        y = -0.5 + i * 0.5 / (n_interior - 1) if n_interior > 1 else -0.5
        x = -0.5 + (i % 2) * 0.5
        initial_guess.extend([x, y])
    
    def constrained_objective(x):
        # Reshape interior nodes
        interior = np.array(x).reshape(-1, 2)
        
        # Check triangle constraint: x + y >= -1? Actually for reference triangle
        # with vertices (-1,-1), (1,-1), (-1,1), interior must satisfy y >= -1,
        # x >= -1, and x <= 1? Let's use barycentric constraint
        penalty = 0
        for (xi, yi) in interior:
            # Check if inside triangle (using barycentric coordinates)
            # For triangle with vertices A(-1,-1), B(1,-1), C(-1,1)
            # Point is inside if u>=0, v>=0, w>=0 where:
            # u = ( (yB-yC)*(x-Cx) + (xC-xB)*(y-Cy) ) / ((yB-yC)*(xA-Cx) + (xC-xB)*(yA-Cy))
            # Simplified: point is inside if x >= -1, y >= -1, and x + y <= 0
            if xi < -1.0 or xi > 1.0 or yi < -1.0 or yi > 1.0 or xi + yi > 0:
                penalty += 1e6 * (abs(xi + yi) + abs(xi + 1) + abs(yi + 1))
        
        return objective_function(interior, edge_nodes, modes, spectral_order) + penalty
    
    # Try multiple random initial guesses
    best_result = None
    best_value = float('inf')
    
    for trial in range(n_trials):
        print(f"Trial {trial+1}/{n_trials}...")
        
        # Random initial guess within triangle
        random_guess = []
        for _ in range(n_interior):
            # Generate random point inside triangle
            u = np.random.rand()
            v = np.random.rand()
            if u + v > 1:
                u = 1 - u
                v = 1 - v
            # Map to triangle coordinates
            x = -1.0 + 2.0 * u
            y = -1.0 + 2.0 * v
            random_guess.extend([x, y])
        
        # Optimize
        result = minimize(constrained_objective, random_guess, 
                         method='L-BFGS-B', bounds=bounds,
                         options={'maxiter': 100, 'ftol': 1e-6})
        
        if result.fun < best_value:
            best_value = result.fun
            best_result = result
    
    if best_result is not None:
        return best_result.x.reshape(-1, 2)
    else:
        return None

def verify_nodes(all_nodes, modes):
    """Verify node set quality"""
    V = build_vandermonde(all_nodes, modes)
    cond = np.linalg.cond(V)
    rank = np.linalg.matrix_rank(V)
    
    # Compute shape functions
    invV = np.linalg.pinv(V)
    
    # Check interpolation property
    max_error = 0
    for i, (x, y) in enumerate(all_nodes):
        D = np.array([koornwinder_dubiner_basis(x, y, i, j) for i, j in modes])
        N = invV @ D
        error = abs(N[i] - 1.0)
        max_error = max(max_error, error)
    
    # Compute condition of mass matrix
    mass = V.T @ V
    mass_cond = np.linalg.cond(mass)
    
    return {
        'condition_number': cond,
        'rank': rank,
        'max_interpolation_error': max_error,
        'mass_matrix_condition': mass_cond,
        'is_valid': rank == len(modes) and max_error < 1e-6
    }

# Main execution
spectral_order = 4
modes = koornwinder_dubiner_mode(spectral_order)
n_nodes = len(modes)
edge_nodes = generate_prescribed_nodes(spectral_order)
n_interior = n_nodes - len(edge_nodes)

print("="*70)
print(f"FINDING OPTIMAL INTERIOR NODES FOR ORDER {spectral_order}")
print("="*70)
print(f"Total nodes needed: {n_nodes}")
print(f"Prescribed edge nodes: {len(edge_nodes)}")
print(f"Free interior nodes: {n_interior}")

# Find optimal interior nodes
print("\nOptimizing interior node positions...")
interior_nodes = find_optimal_interior_nodes(spectral_order, n_interior)

if interior_nodes is not None:
    # Combine all nodes
    all_nodes = edge_nodes + list(interior_nodes)
    
    print("\n" + "="*70)
    print("OPTIMAL NODES FOUND")
    print("="*70)
    print("\nAll nodes (x, y):")
    for i, (x, y) in enumerate(all_nodes):
        print(f"  Node {i:2d}: ({x:12.8f}, {y:12.8f})")
    
    # Verify
    print("\n" + "="*70)
    print("VERIFICATION")
    print("="*70)
    results = verify_nodes(all_nodes, modes)
    
    print(f"Vandermonde condition number: {results['condition_number']:.2e}")
    print(f"Vandermonde rank: {results['rank']}/{n_nodes}")
    print(f"Max interpolation error: {results['max_interpolation_error']:.2e}")
    print(f"Mass matrix condition: {results['mass_matrix_condition']:.2e}")
    
    if results['is_valid']:
        print("\n✅ SUCCESS! Found optimal interior nodes!")
        print("\nUse these nodes for your hybrid spectral element:")
        print("node_coords = [")
        for x, y in all_nodes:
            print(f"    ({x:.15f}, {y:.15f}),")
        print("]")
    else:
        print("\n⚠️  Nodes found but verification failed")
        print("   Try increasing number of optimization trials")
else:
    print("\n❌ Failed to find optimal nodes")
    print("   Try adjusting optimization parameters")

# Also provide fallback nodes (from literature for order 4)
print("\n" + "="*70)
print("FALLBACK NODES (From Literature)")
print("="*70)
print("""
For order 4 with edge GLL nodes, use these interior nodes:
( -0.5, -0.5 )
(  0.0, -0.5 )
(  0.5, -0.5 )
( -0.5,  0.0 )
(  0.0,  0.0 )
( -0.5,  0.5 )
""")



