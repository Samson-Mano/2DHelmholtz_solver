import numpy as np
from scipy.linalg import svd, qr
from scipy.special import eval_jacobi
import matplotlib.pyplot as plt

class TriangleSpectralElement:
    """
    Spectral Element Method on triangles using Fekete points and Dubiner polynomials.
    """
    
    def __init__(self, N, ref_coords=None):
        """
        Initialize triangular spectral element of polynomial degree N.
        """
        self.N = N
        self.Np = (N + 1) * (N + 2) // 2
        
        # Use provided reference coordinates
        if ref_coords is not None:
            self.fekete_points = np.array(ref_coords, dtype=np.float64)
            assert len(self.fekete_points) == self.Np, \
                f"Expected {self.Np} points, got {len(self.fekete_points)}"
        else:
            self.fekete_points = self._generate_equispaced_points()
        
        # Compute Vandermonde matrix using more stable method
        self.Vandermonde = self._compute_vandermonde_stable(self.fekete_points)
        
        # Compute pseudo-inverse for better stability
        self.V_inv = np.linalg.pinv(self.Vandermonde, rcond=1e-12)
        
        # Lagrange coefficients: each column is coefficients for one Lagrange polynomial
        self.lagrange_coeffs = self.V_inv.T
        
        print(f"Vandermonde condition number: {np.linalg.cond(self.Vandermonde):.2e}")
        
    def _generate_equispaced_points(self):
        """Generate equispaced points in barycentric coordinates for testing."""
        points = []
        for i in range(self.N + 1):
            for j in range(self.N - i + 1):
                # Barycentric coordinates
                a = i / self.N
                b = j / self.N
                c = 1 - a - b
                # Convert to (xi, eta) coordinates
                xi = -a + c
                eta = -a - b + c
                points.append([xi, eta])
        return np.array(points)
    
    def _dubiner_basis_normalized(self, xi, eta, p, q):
        """
        Evaluate normalized Dubiner polynomial at (xi, eta).
        Using proper normalization for better conditioning.
        """
        # Handle the singular point at eta = 1
        if abs(eta - 1.0) < 1e-12:
            a = -1.0
        else:
            a = 2.0 * (1.0 + xi) / (1.0 - eta) - 1.0
        
        b = eta
        
        # Evaluate Jacobi polynomials with proper normalization
        if p == 0:
            P_p = 1.0
        else:
            # Normalized Jacobi: sqrt((2p+1)/2) * P_p^{(0,0)}
            P_p = eval_jacobi(p, 0.0, 0.0, a)
            P_p *= np.sqrt((2*p + 1) / 2)
        
        if q == 0:
            P_q = 1.0
        else:
            # Normalized Jacobi: sqrt((2q+1)/2) * P_q^{(2p+1,0)}
            P_q = eval_jacobi(q, 2*p + 1, 0.0, b)
            P_q *= np.sqrt((2*q + 1) / 2)
        
        # Factor for the transformation
        factor = ((1.0 - b) / 2.0) ** p
        
        return P_p * P_q * factor
    
    def _compute_vandermonde_stable(self, points):
        """
        Compute Vandermonde matrix with column scaling for better conditioning.
        """
        n_points = points.shape[0]
        V = np.zeros((n_points, self.Np))
        
        idx = 0
        for p in range(self.N + 1):
            for q in range(self.N - p + 1):
                for i in range(n_points):
                    V[i, idx] = self._dubiner_basis_normalized(points[i, 0], points[i, 1], p, q)
                idx += 1
        
        # Scale columns to have unit norm for better conditioning
        col_norms = np.linalg.norm(V, axis=0)
        col_norms[col_norms < 1e-12] = 1.0
        self.column_scaling = 1.0 / col_norms
        V = V * self.column_scaling
        
        return V
    
    def interpolate(self, f_values, xi, eta):
        """
        Interpolate function at given points.
        """
        points = np.column_stack([xi.flatten(), eta.flatten()])
        
        # Evaluate normalized Dubiner basis at query points
        V_query = np.zeros((points.shape[0], self.Np))
        idx = 0
        for p in range(self.N + 1):
            for q in range(self.N - p + 1):
                for i in range(points.shape[0]):
                    V_query[i, idx] = self._dubiner_basis_normalized(points[i, 0], points[i, 1], p, q)
                idx += 1
        
        # Apply same column scaling
        V_query = V_query * self.column_scaling
        
        # Compute coefficients in Dubiner basis
        coeffs = f_values @ self.lagrange_coeffs.T
        
        # Evaluate interpolant
        return V_query @ coeffs
    
    def compute_gradient(self, f_values, xi, eta):
        """
        Compute gradient using finite differences with adaptive step size.
        """
        coeffs = f_values @ self.lagrange_coeffs.T
        
        df_dxi = np.zeros_like(xi)
        df_deta = np.zeros_like(eta)
        
        points = np.column_stack([xi.flatten(), eta.flatten()])
        
        for n in range(points.shape[0]):
            xi_n, eta_n = points[n]
            idx = 0
            
            # Adaptive step size based on position
            h_xi = max(1e-6, 1e-4 * (1 - abs(xi_n)))
            h_eta = max(1e-6, 1e-4 * (1 - abs(eta_n)))
            
            for p in range(self.N + 1):
                for q in range(self.N - p + 1):
                    # Compute basis values
                    base = self._dubiner_basis_normalized(xi_n, eta_n, p, q)
                    
                    # Forward differences for stability near boundaries
                    xi_plus = min(xi_n + h_xi, 0.999)
                    eta_plus = min(eta_n + h_eta, 0.999)
                    
                    d_xi = (self._dubiner_basis_normalized(xi_plus, eta_n, p, q) - base) / (xi_plus - xi_n)
                    d_eta = (self._dubiner_basis_normalized(xi_n, eta_plus, p, q) - base) / (eta_plus - eta_n)
                    
                    df_dxi[n] += coeffs[idx] * d_xi
                    df_deta[n] += coeffs[idx] * d_eta
                    idx += 1
        
        return df_dxi.reshape(xi.shape), df_deta.reshape(eta.shape)
    
    def compute_lagrange_at_points(self, points):
        """
        Evaluate all Lagrange basis functions at given points.
        """
        V_points = np.zeros((points.shape[0], self.Np))
        idx = 0
        for p in range(self.N + 1):
            for q in range(self.N - p + 1):
                for i in range(points.shape[0]):
                    V_points[i, idx] = self._dubiner_basis_normalized(points[i, 0], points[i, 1], p, q)
                idx += 1
        
        V_points = V_points * self.column_scaling
        return V_points @ self.lagrange_coeffs.T
    
    def mass_matrix(self):
        """
        Compute mass matrix using high-order quadrature.
        """
        from scipy.special import roots_jacobi
        
        M = np.zeros((self.Np, self.Np))
        
        # Use sufficiently high quadrature order
        n_quad = self.N + 5
        xi_quad, w_xi = roots_jacobi(n_quad, 0, 0)
        
        for i in range(n_quad):
            for j in range(n_quad - i):
                xi = xi_quad[i]
                eta = xi_quad[j]
                
                # Check if point is inside triangle
                if xi + eta <= 0 and xi >= -1 and eta >= -1:
                    # Quadrature weight for triangle
                    weight = w_xi[i] * w_xi[j] * (1 - eta) / 4
                    
                    # Evaluate basis at quadrature point
                    phi = np.zeros(self.Np)
                    idx = 0
                    for p in range(self.N + 1):
                        for q in range(self.N - p + 1):
                            phi[idx] = self._dubiner_basis_normalized(xi, eta, p, q)
                            idx += 1
                    
                    # Scale by column scaling
                    phi = phi * self.column_scaling
                    
                    # Transform to Lagrange basis
                    lagrange_vals = phi @ self.lagrange_coeffs.T
                    
                    M += weight * np.outer(lagrange_vals, lagrange_vals)
        
        return M


def test_with_fekete_points():
    """Test the triangular spectral element with pre-computed Fekete points."""
    print("=" * 60)
    print("Testing Triangular Spectral Element with Pre-computed Fekete Points")
    print("=" * 60)
    
    # Fekete points for N=4 (15 points) - reordered to match polynomial basis ordering
    ref_coords = [
        (-1.0, -1.0),   # Vertex 1
        (1.0, -1.0),    # Vertex 2  
        (-1.0, 1.0),    # Vertex 3
        (-1.0, -0.654653670707978),  # Edge points (reordered)
        (-1.0, 0.0),
        (-1.0, 0.654653670707978),
        (-0.654653670707978, -1.0),
        (0.0, -1.0),
        (0.654653670707978, -1.0),
        (-0.654653670707978, -0.654653670707978),  # Interior points
        (0.0, 0.0),
        (0.654653670707978, -0.654653670707978),
        (-0.551551223569326, -0.551551223569326),
        (0.551551223569326, -0.551551223569326),
        (-0.551551223569326, 0.551551223569326)
    ]
    
    # Check if we have correct number of points for N=4
    expected_np = (4 + 1) * (4 + 2) // 2  # = 15
    print(f"\nExpected number of points for N=4: {expected_np}")
    print(f"Provided points: {len(ref_coords)}")
    
    # Create element
    N = 4
    element = TriangleSpectralElement(N, ref_coords)
    
    # Test interpolation with a simple polynomial (should be exact)
    print("\n" + "=" * 40)
    print("Test 1: Interpolation of polynomial function")
    print("=" * 40)
    
    # Test with a polynomial of degree <= N
    def test_polynomial(xi, eta):
        return 1 + xi + eta + xi**2 + xi*eta + eta**2
    
    # Evaluate at Fekete points
    f_at_nodes = np.array([test_polynomial(xi, eta) for xi, eta in element.fekete_points])
    
    # Interpolate back
    f_interp = element.interpolate(f_at_nodes, 
                                   element.fekete_points[:, 0], 
                                   element.fekete_points[:, 1])
    
    error = np.max(np.abs(f_at_nodes - f_interp))
    print(f"Interpolation error: {error:.2e}")
    
    # Test Lagrange basis at nodes
    print("\n" + "=" * 40)
    print("Test 2: Lagrange Basis Properties")
    print("=" * 40)
    
    # Check Lagrange basis at Fekete points
    phi_at_nodes = element.compute_lagrange_at_points(element.fekete_points)
    
    # Should be identity matrix
    identity_error = np.max(np.abs(phi_at_nodes - np.eye(element.Np)))
    print(f"Φ_i(x_j) = δ_ij error: {identity_error:.2e}")
    
    # Check partition of unity at test points
    test_points = np.array([
        [0.0, 0.0],
        [0.3, -0.2],
        [-0.4, 0.1],
        [0.5, -0.5],
        [-0.3, -0.3]
    ])
    
    phi_test = element.compute_lagrange_at_points(test_points)
    sum_phi = np.sum(phi_test, axis=1)
    
    print(f"Partition of unity at test points:")
    for i, s in enumerate(sum_phi):
        print(f"  Point {i}: sum = {s:.6f} (error = {abs(s-1):.2e})")
    
    # Test gradient with a simple function
    print("\n" + "=" * 40)
    print("Test 3: Gradient Computation")
    print("=" * 40)
    
    def linear_function(xi, eta):
        return 2*xi + 3*eta
    
    f_linear = np.array([linear_function(xi, eta) for xi, eta in element.fekete_points])
    
    # Test at several points
    test_grad_points = np.array([[0.0, 0.0], [0.3, -0.2], [-0.4, 0.1]])
    
    for xi_test, eta_test in test_grad_points:
        df_dxi, df_deta = element.compute_gradient(f_linear, np.array([xi_test]), np.array([eta_test]))
        
        print(f"\n  At point ({xi_test:.2f}, {eta_test:.2f}):")
        print(f"    ∂f/∂ξ: numerical = {df_dxi[0]:.6f}, exact = 2.0, error = {abs(df_dxi[0] - 2.0):.2e}")
        print(f"    ∂f/∂η: numerical = {df_deta[0]:.6f}, exact = 3.0, error = {abs(df_deta[0] - 3.0):.2e}")
    
    # Compute mass matrix
    print("\n" + "=" * 40)
    print("Test 4: Mass Matrix")
    print("=" * 40)
    
    M = element.mass_matrix()
    print(f"Mass matrix shape: {M.shape}")
    print(f"Mass matrix condition number: {np.linalg.cond(M):.2e}")
    
    # Check if mass matrix is positive definite
    eigenvals = np.linalg.eigvalsh(M)
    print(f"Smallest eigenvalue: {np.min(eigenvals):.2e}")
    print(f"Largest eigenvalue: {np.max(eigenvals):.2e}")
    print(f"Positive definite: {'✓' if np.min(eigenvals) > 0 else '✗'}")
    
    return element


def visualize_results(element):
    """Visualize the Fekete points and a Lagrange basis function."""
    fig, axes = plt.subplots(1, 3, figsize=(15, 5))
    
    # Plot 1: Fekete points
    xi, eta = element.fekete_points[:, 0], element.fekete_points[:, 1]
    axes[0].scatter(xi, eta, c='red', s=50, zorder=5)
    triangle = plt.Polygon([[-1, -1], [1, -1], [-1, 1]], 
                           fill=False, edgecolor='black', linewidth=2)
    axes[0].add_patch(triangle)
    axes[0].set_xlabel(r'$\xi$')
    axes[0].set_ylabel(r'$\eta$')
    axes[0].set_title(f'Fekete Points (N={element.N})')
    axes[0].axis('equal')
    axes[0].grid(True, alpha=0.3)
    axes[0].set_xlim(-1.1, 1.1)
    axes[0].set_ylim(-1.1, 1.1)
    
    # Plot 2: Lagrange basis function for a boundary node
    n_plot = 50
    xi_plot = np.linspace(-1, 1, n_plot)
    eta_plot = np.linspace(-1, 1, n_plot)
    Xi, Eta = np.meshgrid(xi_plot, eta_plot)
    
    # Mask points outside triangle
    mask = (Xi + Eta <= 0) & (Xi >= -1) & (Eta >= -1)
    
    # Pick a boundary node (e.g., vertex)
    node_idx = 0
    
    points = np.column_stack([Xi.flatten(), Eta.flatten()])
    phi = element.compute_lagrange_at_points(points)
    phi_node = phi[:, node_idx].reshape(Xi.shape)
    phi_node[~mask] = np.nan
    
    contour = axes[1].contourf(Xi, Eta, phi_node, levels=20, cmap='viridis')
    axes[1].add_patch(plt.Polygon([[-1, -1], [1, -1], [-1, 1]], 
                                   fill=False, edgecolor='black', linewidth=2))
    axes[1].scatter(element.fekete_points[node_idx, 0], 
                   element.fekete_points[node_idx, 1], 
                   c='red', s=100, marker='*', zorder=5)
    axes[1].set_xlabel(r'$\xi$')
    axes[1].set_ylabel(r'$\eta$')
    axes[1].set_title(f'Lagrange Basis (Node {node_idx})')
    axes[1].axis('equal')
    plt.colorbar(contour, ax=axes[1])
    
    # Plot 3: Lagrange basis for an interior node
    node_idx = element.Np // 2
    
    phi_node = phi[:, node_idx].reshape(Xi.shape)
    phi_node[~mask] = np.nan
    
    contour = axes[2].contourf(Xi, Eta, phi_node, levels=20, cmap='viridis')
    axes[2].add_patch(plt.Polygon([[-1, -1], [1, -1], [-1, 1]], 
                                   fill=False, edgecolor='black', linewidth=2))
    axes[2].scatter(element.fekete_points[node_idx, 0], 
                   element.fekete_points[node_idx, 1], 
                   c='red', s=100, marker='*', zorder=5)
    axes[2].set_xlabel(r'$\xi$')
    axes[2].set_ylabel(r'$\eta$')
    axes[2].set_title(f'Lagrange Basis (Node {node_idx})')
    axes[2].axis('equal')
    plt.colorbar(contour, ax=axes[2])
    
    plt.tight_layout()
    plt.show()


if __name__ == "__main__":
    # Test with pre-computed Fekete points
    element = test_with_fekete_points()
    
    # Visualize
    try:
        visualize_results(element)
    except Exception as e:
        print(f"\nVisualization error: {e}")




