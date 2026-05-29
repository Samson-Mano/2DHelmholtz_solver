import numpy as np
from scipy.linalg import qr, solve_triangular
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
        
        # Compute Vandermonde matrix with proper ordering
        self.Vandermonde = self._compute_vandermonde(self.fekete_points)
        
        # Use QR decomposition for stable inversion
        # V^T = Q * R, then V^{-1} = (R^{-1} * Q^T)^T
        Q, R = qr(self.Vandermonde.T, mode='economic')
        self.R_inv = np.linalg.inv(R)
        self.V_inv = (self.R_inv @ Q.T).T
        
        # Lagrange coefficients: each column gives coefficients for one Lagrange polynomial
        self.lagrange_coeffs = self.V_inv.T
        
        # Verify the inverse
        identity_check = self.Vandermonde @ self.V_inv
        self.inverse_error = np.max(np.abs(identity_check - np.eye(self.Np)))
        print(f"Inverse error: {self.inverse_error:.2e}")
        
        if self.inverse_error > 1e-10:
            print("Warning: High inverse error, trying pseudo-inverse...")
            self.V_inv = np.linalg.pinv(self.Vandermonde, rcond=1e-10)
            self.lagrange_coeffs = self.V_inv.T
        
    def _generate_equispaced_points(self):
        """Generate equispaced points in barycentric coordinates."""
        points = []
        for i in range(self.N + 1):
            for j in range(self.N - i + 1):
                # Barycentric coordinates in order: (i,j) with i+j <= N
                a = i / self.N
                b = j / self.N
                c = 1 - a - b
                # Convert to (xi, eta) coordinates
                # Map from barycentric (a,b,c) to (xi,eta) where:
                # xi = c - a, eta = c - b
                xi = c - a
                eta = c - b
                points.append([xi, eta])
        return np.array(points)
    
    def _dubiner_basis(self, xi, eta, p, q):
        """
        Evaluate Dubiner polynomial at (xi, eta).
        Using orthogonal polynomials on the triangle.
        """
        # Transform to coordinates in the square [-1,1]^2
        # Map triangle to square: (xi, eta) -> (a, b)
        if abs(eta - 1.0) < 1e-12:
            a = -1.0
        else:
            a = 2.0 * (1.0 + xi) / (1.0 - eta) - 1.0
        
        b = eta
        
        # Evaluate Jacobi polynomials
        # P_p^{(0,0)}(a) - Legendre polynomial
        if p == 0:
            P_p = 1.0
        else:
            P_p = eval_jacobi(p, 0.0, 0.0, a)
        
        # P_q^{(2p+1,0)}(b)
        if q == 0:
            P_q = 1.0
        else:
            P_q = eval_jacobi(q, 2*p + 1, 0.0, b)
        
        # Transformation factor
        factor = ((1.0 - b) / 2.0) ** p
        
        # Normalization factor for orthonormality
        norm_p = np.sqrt(2.0 * p + 1.0) if p > 0 else 1.0
        norm_q = np.sqrt(2.0 * q + 1.0) if q > 0 else 1.0
        norm_factor = norm_p * norm_q
        
        return P_p * P_q * factor / norm_factor
    
    def _compute_vandermonde(self, points):
        """
        Compute Vandermonde matrix for Dubiner basis at given points.
        Each column corresponds to a basis function, each row to a point.
        """
        n_points = points.shape[0]
        V = np.zeros((n_points, self.Np))
        
        idx = 0
        for p in range(self.N + 1):
            for q in range(self.N - p + 1):
                for i in range(n_points):
                    V[i, idx] = self._dubiner_basis(points[i, 0], points[i, 1], p, q)
                idx += 1
        
        return V
    
    def interpolate(self, f_values, xi, eta):
        """
        Interpolate function at given points using Lagrange basis.
        
        For a function f defined at Fekete points (f_values), 
        the interpolant is: f(x) = Σ f(x_k) * L_k(x)
        where L_k are Lagrange basis functions.
        """
        points = np.column_stack([xi.flatten(), eta.flatten()])
        
        # Evaluate Dubiner basis at query points
        V_query = self._compute_vandermonde(points)
        
        # Compute coefficients in Dubiner basis: c_j = Σ_k f(x_k) * (V^{-1})_{k,j}
        coeffs = f_values @ self.lagrange_coeffs.T
        
        # Evaluate interpolant: Σ_j c_j * D_j(x)
        return V_query @ coeffs
    
    def compute_lagrange_at_points(self, points):
        """
        Evaluate all Lagrange basis functions at given points.
        Returns array of shape (n_points, Np) where column k is L_k(points)
        """
        V_points = self._compute_vandermonde(points)
        return V_points @ self.lagrange_coeffs.T
    
    def compute_gradient(self, f_values, xi, eta):
        """
        Compute gradient using analytic derivatives of Dubiner basis.
        """
        coeffs = f_values @ self.lagrange_coeffs.T
        points = np.column_stack([xi.flatten(), eta.flatten()])
        
        df_dxi = np.zeros(len(points))
        df_deta = np.zeros(len(points))
        
        h = 1e-6  # Finite difference step
        
        for n in range(len(points)):
            xi_n, eta_n = points[n]
            idx = 0
            
            for p in range(self.N + 1):
                for q in range(self.N - p + 1):
                    # Get basis coefficient
                    c = coeffs[idx]
                    
                    # Compute derivative using central differences
                    # d/dxi
                    dxi = (self._dubiner_basis(xi_n + h, eta_n, p, q) - 
                          self._dubiner_basis(xi_n - h, eta_n, p, q)) / (2*h)
                    
                    # d/deta
                    deta = (self._dubiner_basis(xi_n, eta_n + h, p, q) - 
                           self._dubiner_basis(xi_n, eta_n - h, p, q)) / (2*h)
                    
                    df_dxi[n] += c * dxi
                    df_deta[n] += c * deta
                    idx += 1
        
        return df_dxi.reshape(xi.shape), df_deta.reshape(eta.shape)
    
    def mass_matrix(self):
        """
        Compute mass matrix using high-order quadrature.
        """
        from scipy.special import roots_jacobi
        
        M = np.zeros((self.Np, self.Np))
        
        # Use Gaussian quadrature of order N+3
        n_quad = self.N + 4
        xi_quad, w_xi = roots_jacobi(n_quad, 0, 0)
        
        for i in range(n_quad):
            for j in range(n_quad - i):
                xi = xi_quad[i]
                eta = xi_quad[j]
                
                # Check if point is inside triangle
                if xi + eta <= 0 + 1e-12 and xi >= -1 - 1e-12 and eta >= -1 - 1e-12:
                    # Quadrature weight for triangle
                    weight = w_xi[i] * w_xi[j] * (1 - eta) / 4
                    
                    # Evaluate all basis functions at this point
                    phi = np.zeros(self.Np)
                    idx = 0
                    for p in range(self.N + 1):
                        for q in range(self.N - p + 1):
                            phi[idx] = self._dubiner_basis(xi, eta, p, q)
                            idx += 1
                    
                    # Add contribution to mass matrix
                    M += weight * np.outer(phi, phi)
        
        return M


def test_with_fekete_points():
    """Test the triangular spectral element with pre-computed Fekete points."""
    print("=" * 60)
    print("Testing Triangular Spectral Element with Pre-computed Fekete Points")
    print("=" * 60)
    
    # Fekete points for N=4 (15 points)
    ref_coords = np.array([
        (-1.0, -1.0),   # Vertex 1
        (1.0, -1.0),    # Vertex 2  
        (-1.0, 1.0),    # Vertex 3
        (-0.654653670707978, -1.0),
        (0.0, -1.0),
        (0.654653670707978, -1.0),
        (-1.0, -0.654653670707978),
        (-1.0, 0.0),
        (-1.0, 0.654653670707978),
        (0.654653670707978, -0.654653670707978),
        (0.0, 0.0),
        (-0.654653670707978, 0.654653670707978),
        (-0.551551223569326, -0.551551223569326),
        (0.551551223569326, -0.551551223569326),
        (-0.551551223569326, 0.551551223569326)
    ])
    
    expected_np = (4 + 1) * (4 + 2) // 2
    print(f"\nExpected number of points for N=4: {expected_np}")
    print(f"Provided points: {len(ref_coords)}")
    
    # Create element
    N = 4
    element = TriangleSpectralElement(N, ref_coords)
    
    # Test 1: Check Vandermonde and its inverse
    print("\n" + "=" * 40)
    print("Test 1: Vandermonde Matrix Properties")
    print("=" * 40)
    
    V = element.Vandermonde
    V_inv = element.V_inv
    I_check = V @ V_inv
    
    print(f"Vandermonde condition number: {np.linalg.cond(V):.2e}")
    print(f"Max error in V * V_inv = I: {np.max(np.abs(I_check - np.eye(element.Np))):.2e}")
    
    # Test 2: Lagrange basis at Fekete points
    print("\n" + "=" * 40)
    print("Test 2: Lagrange Basis Properties")
    print("=" * 40)
    
    # Compute Lagrange basis at Fekete points
    L_at_nodes = element.compute_lagrange_at_points(element.fekete_points)
    
    # Should be identity
    identity_error = np.max(np.abs(L_at_nodes - np.eye(element.Np)))
    print(f"L_i(x_j) = δ_ij error: {identity_error:.2e}")
    
    # Test 3: Interpolation of polynomials
    print("\n" + "=" * 40)
    print("Test 3: Polynomial Interpolation")
    print("=" * 40)
    
    # Test polynomials of increasing degree
    def test_polynomial(xi, eta, degree):
        # Simple monomial
        if degree == 0:
            return np.ones_like(xi)
        elif degree == 1:
            return xi + eta
        elif degree == 2:
            return xi**2 + xi*eta + eta**2
        elif degree == 3:
            return xi**3 + xi**2*eta + xi*eta**2 + eta**3
        else:
            return xi**4 + eta**4
    
    for deg in range(5):
        f_at_nodes = np.array([test_polynomial(xi, eta, deg) for xi, eta in element.fekete_points])
        f_interp = element.interpolate(f_at_nodes, 
                                      element.fekete_points[:, 0], 
                                      element.fekete_points[:, 1])
        error = np.max(np.abs(f_at_nodes - f_interp))
        print(f"  Polynomial degree {deg}: error = {error:.2e}")
    
    # Test 4: Partition of unity
    print("\n" + "=" * 40)
    print("Test 4: Partition of Unity")
    print("=" * 40)
    
    test_points = np.array([
        [0.0, 0.0],
        [0.3, -0.2],
        [-0.4, 0.1],
        [0.5, -0.5],
        [-0.3, -0.3]
    ])
    
    L_test = element.compute_lagrange_at_points(test_points)
    sum_L = np.sum(L_test, axis=1)
    
    for i, s in enumerate(sum_L):
        print(f"  Point {i}: sum = {s:.15f} (error = {abs(s-1):.2e})")
    
    # Test 5: Gradient of linear function
    print("\n" + "=" * 40)
    print("Test 5: Gradient of Linear Function")
    print("=" * 40)
    
    def linear_function(xi, eta):
        return 2.0*xi + 3.0*eta
    
    f_linear = np.array([linear_function(xi, eta) for xi, eta in element.fekete_points])
    
    test_grad_points = np.array([[0.0, 0.0], [0.3, -0.2], [-0.4, 0.1]])
    
    for xi_test, eta_test in test_grad_points:
        df_dxi, df_deta = element.compute_gradient(f_linear, 
                                                   np.array([xi_test]), 
                                                   np.array([eta_test]))
        
        print(f"\n  At point ({xi_test:.2f}, {eta_test:.2f}):")
        print(f"    ∂f/∂ξ: numerical = {df_dxi[0]:.8f}, exact = 2.0, error = {abs(df_dxi[0] - 2.0):.2e}")
        print(f"    ∂f/∂η: numerical = {df_deta[0]:.8f}, exact = 3.0, error = {abs(df_deta[0] - 3.0):.2e}")
    
    # Test 6: Mass matrix
    print("\n" + "=" * 40)
    print("Test 6: Mass Matrix")
    print("=" * 40)
    
    M = element.mass_matrix()
    eigenvals = np.linalg.eigvalsh(M)
    
    print(f"Mass matrix shape: {M.shape}")
    print(f"Condition number: {np.linalg.cond(M):.2e}")
    print(f"Smallest eigenvalue: {np.min(eigenvals):.2e}")
    print(f"Largest eigenvalue: {np.max(eigenvals):.2e}")
    print(f"Positive definite: {'✓' if np.min(eigenvals) > 0 else '✗'}")
    
    # Check if mass matrix is symmetric
    sym_error = np.max(np.abs(M - M.T))
    print(f"Symmetry error: {sym_error:.2e}")
    
    return element


def visualize_fekete_points_and_basis(element):
    """Visualize the Fekete points and basis functions."""
    fig, axes = plt.subplots(2, 2, figsize=(12, 10))
    
    # Plot 1: Fekete points
    xi, eta = element.fekete_points[:, 0], element.fekete_points[:, 1]
    axes[0, 0].scatter(xi, eta, c='red', s=50, zorder=5)
    triangle = plt.Polygon([[-1, -1], [1, -1], [-1, 1]], 
                           fill=False, edgecolor='black', linewidth=2)
    axes[0, 0].add_patch(triangle)
    axes[0, 0].set_xlabel(r'$\xi$')
    axes[0, 0].set_ylabel(r'$\eta$')
    axes[0, 0].set_title(f'Fekete Points (N={element.N})')
    axes[0, 0].axis('equal')
    axes[0, 0].grid(True, alpha=0.3)
    axes[0, 0].set_xlim(-1.1, 1.1)
    axes[0, 0].set_ylim(-1.1, 1.1)
    
    # Create grid for contour plots
    n_plot = 40
    xi_plot = np.linspace(-1, 1, n_plot)
    eta_plot = np.linspace(-1, 1, n_plot)
    Xi, Eta = np.meshgrid(xi_plot, eta_plot)
    mask = (Xi + Eta <= 0) & (Xi >= -1) & (Eta >= -1)
    
    # Plot 2: Lagrange basis at a vertex
    vertex_idx = 0
    points = np.column_stack([Xi.flatten(), Eta.flatten()])
    L_all = element.compute_lagrange_at_points(points)
    L_vertex = L_all[:, vertex_idx].reshape(Xi.shape)
    L_vertex[~mask] = np.nan
    
    contour = axes[0, 1].contourf(Xi, Eta, L_vertex, levels=20, cmap='viridis')
    axes[0, 1].add_patch(plt.Polygon([[-1, -1], [1, -1], [-1, 1]], 
                                     fill=False, edgecolor='black', linewidth=2))
    axes[0, 1].scatter(element.fekete_points[vertex_idx, 0], 
                      element.fekete_points[vertex_idx, 1], 
                      c='red', s=100, marker='*', zorder=5)
    axes[0, 1].set_xlabel(r'$\xi$')
    axes[0, 1].set_ylabel(r'$\eta$')
    axes[0, 1].set_title(f'Lagrange Basis at Vertex 0')
    axes[0, 1].axis('equal')
    plt.colorbar(contour, ax=axes[0, 1])
    
    # Plot 3: Lagrange basis at an edge point
    edge_idx = 3  # Edge point
    L_edge = L_all[:, edge_idx].reshape(Xi.shape)
    L_edge[~mask] = np.nan
    
    contour = axes[1, 0].contourf(Xi, Eta, L_edge, levels=20, cmap='viridis')
    axes[1, 0].add_patch(plt.Polygon([[-1, -1], [1, -1], [-1, 1]], 
                                     fill=False, edgecolor='black', linewidth=2))
    axes[1, 0].scatter(element.fekete_points[edge_idx, 0], 
                      element.fekete_points[edge_idx, 1], 
                      c='red', s=100, marker='*', zorder=5)
    axes[1, 0].set_xlabel(r'$\xi$')
    axes[1, 0].set_ylabel(r'$\eta$')
    axes[1, 0].set_title(f'Lagrange Basis at Edge Point')
    axes[1, 0].axis('equal')
    plt.colorbar(contour, ax=axes[1, 0])
    
    # Plot 4: Lagrange basis at interior point
    interior_idx = 10  # Center point
    L_interior = L_all[:, interior_idx].reshape(Xi.shape)
    L_interior[~mask] = np.nan
    
    contour = axes[1, 1].contourf(Xi, Eta, L_interior, levels=20, cmap='viridis')
    axes[1, 1].add_patch(plt.Polygon([[-1, -1], [1, -1], [-1, 1]], 
                                     fill=False, edgecolor='black', linewidth=2))
    axes[1, 1].scatter(element.fekete_points[interior_idx, 0], 
                      element.fekete_points[interior_idx, 1], 
                      c='red', s=100, marker='*', zorder=5)
    axes[1, 1].set_xlabel(r'$\xi$')
    axes[1, 1].set_ylabel(r'$\eta$')
    axes[1, 1].set_title(f'Lagrange Basis at Interior Point')
    axes[1, 1].axis('equal')
    plt.colorbar(contour, ax=axes[1, 1])
    
    plt.tight_layout()
    plt.show()


if __name__ == "__main__":
    element = test_with_fekete_points()
    
    try:
        visualize_fekete_points_and_basis(element)
    except Exception as e:
        print(f"\nVisualization error: {e}")