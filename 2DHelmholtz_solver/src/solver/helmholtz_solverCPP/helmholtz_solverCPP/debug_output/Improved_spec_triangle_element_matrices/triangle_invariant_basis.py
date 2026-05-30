import numpy as np
from scipy.special import eval_jacobi

class TriangleInvariantBasis:
    """
    Invariant basis functions for triangles using barycentric coordinates.
    Based on the Koornwinder-Dubiner basis.
    """
    
    def __init__(self, r):
        """
        Parameters:
        -----------
        r : int
            Polynomial degree (basis functions up to total degree r)
        """
        self.r = r
        self.Np = (r + 1) * (r + 2) // 2  # Total number of basis functions
        
    def barycentric(self, x, y):
        """
        Compute barycentric coordinates for triangle with vertices:
        (0,0), (1,0), (0,1)
        
        Returns:
        --------
        lambda0, lambda1, lambda2 : barycentric coordinates
        """
        lambda0 = 1 - x - y
        lambda1 = x
        lambda2 = y
        return lambda0, lambda1, lambda2
    
    def vertex_basis(self, x, y):
        """
        Vertex basis functions (3 functions for r >= 1)
        
        Returns:
        --------
        phi : array of shape (n_points, 3)
        """
        lambda0, lambda1, lambda2 = self.barycentric(x, y)
        
        phi = np.zeros((len(x), 3))
        phi[:, 0] = lambda0  # Vertex at (0,0)
        phi[:, 1] = lambda1  # Vertex at (1,0)
        phi[:, 2] = lambda2  # Vertex at (0,1)
        
        return phi
    
    def edge_basis(self, x, y):
        """
        Edge basis functions using Jacobi polynomials P_i^{(1,1)}
        
        For each edge: lambda_a * lambda_b * P_i^{(1,1)}(lambda_b - lambda_a)
        where i ranges from 0 to r-2
        
        Returns:
        --------
        phi : array of shape (n_points, 3*(r-1))
        """
        lambda0, lambda1, lambda2 = self.barycentric(x, y)
        
        n_points = len(x)
        n_edge_funcs = 3 * (self.r - 1)  # 3 edges × (r-1) interior edge functions
        phi = np.zeros((n_points, n_edge_funcs))
        
        idx = 0
        
        # Edge 0-1: between vertices 0 and 1 (lambda2 = 0)
        # lambda0 * lambda1 * P_i^{(1,1)}(lambda1 - lambda0)
        for i in range(self.r - 1):
            # Argument: (lambda1 - lambda0) = (x - (1-x-y)) = 2x + y - 1
            arg = lambda1 - lambda0  # = 2x + y - 1
            P = eval_jacobi(i, 1, 1, arg)
            phi[:, idx] = lambda0 * lambda1 * P
            idx += 1
        
        # Edge 1-2: between vertices 1 and 2 (lambda0 = 0)
        # lambda1 * lambda2 * P_i^{(1,1)}(lambda2 - lambda1)
        for i in range(self.r - 1):
            arg = lambda2 - lambda1  # = y - x
            P = eval_jacobi(i, 1, 1, arg)
            phi[:, idx] = lambda1 * lambda2 * P
            idx += 1
        
        # Edge 2-0: between vertices 2 and 0 (lambda1 = 0)
        # lambda2 * lambda0 * P_i^{(1,1)}(lambda0 - lambda2)
        for i in range(self.r - 1):
            arg = lambda0 - lambda2  # = 1 - x - 2y
            P = eval_jacobi(i, 1, 1, arg)
            phi[:, idx] = lambda2 * lambda0 * P
            idx += 1
        
        return phi
    
    def interior_basis(self, x, y):
        """
        Interior basis functions using tensor product of Jacobi polynomials
        
        For 0 < i+j < r-3:
        lambda0 * lambda1 * lambda2 * P_i^{(1,1)}((2x/(1-y)) - 1) * (1-y)^i * P_j^{(2i+1,1)}(2y-1)
        
        Returns:
        --------
        phi : array of shape (n_points, (r-2)*(r-1)//2)
        """
        lambda0, lambda1, lambda2 = self.barycentric(x, y)
        
        n_points = len(x)
        n_int_funcs = (self.r - 2) * (self.r - 1) // 2  # Number of interior functions
        phi = np.zeros((n_points, n_int_funcs))
        
        if self.r < 3:
            return phi  # No interior functions for low degree
        
        idx = 0
        for i in range(self.r - 2):
            for j in range(self.r - 2 - i):
                # Transform coordinates
                # u = (2x/(1-y)) - 1  maps to [-1, 1] on the triangle
                # v = 2y - 1          maps to [-1, 1]
                
                # Handle y = 1 edge
                with np.errstate(divide='ignore', invalid='ignore'):
                    u = np.where(np.abs(1 - y) > 1e-12, 
                                 2 * x / (1 - y) - 1, 
                                 -1)
                    v = 2 * y - 1
                
                # Evaluate Jacobi polynomials
                P_i = eval_jacobi(i, 1, 1, u)
                P_j = eval_jacobi(j, 2*i + 1, 1, v)
                
                # Interior basis function
                phi[:, idx] = lambda0 * lambda1 * lambda2 * P_i * ((1 - y)**i) * P_j
                idx += 1
        
        return phi
    
    def evaluate_all(self, x, y):
        """
        Evaluate all basis functions (vertices + edges + interior)
        
        Returns:
        --------
        phi : array of shape (n_points, Np)
            Complete basis functions ordered by total degree
        """
        # Get individual components
        phi_vertices = self.vertex_basis(x, y)
        phi_edges = self.edge_basis(x, y)
        phi_interior = self.interior_basis(x, y)
        
        # Combine all basis functions
        phi_all = np.hstack([phi_vertices, phi_edges, phi_interior])
        
        assert phi_all.shape[1] == self.Np, \
            f"Expected {self.Np} basis functions, got {phi_all.shape[1]}"
        
        return phi_all


# Example usage
def demo_triangle_basis():
    """Demonstrate the triangle invariant basis functions."""
    
    print("=" * 60)
    print("Triangle Invariant Basis Functions")
    print("=" * 60)
    
    # Create basis for polynomial degree r=4
    r = 4
    basis = TriangleInvariantBasis(r)
    
    print(f"Polynomial degree: r = {r}")
    print(f"Number of basis functions: Np = {basis.Np}")
    print(f"  - Vertex functions: 3")
    print(f"  - Edge functions: {3*(r-1)}")
    print(f"  - Interior functions: {(r-2)*(r-1)//2}")
    
    # Create grid of points in triangle
    n_points = 20
    x = np.linspace(0, 1, n_points)
    y = np.linspace(0, 1, n_points)
    X, Y = np.meshgrid(x, y)
    
    # Only keep points inside triangle (x + y <= 1)
    mask = X + Y <= 1
    X_inside = X[mask]
    Y_inside = Y[mask]
    
    # Evaluate all basis functions
    phi = basis.evaluate_all(X_inside, Y_inside)
    
    print(phi)
    print(f"\nGrid points inside triangle: {len(X_inside)}")
    print(f"Basis matrix shape: {phi.shape}")
    
    # Test orthogonality (approximate using numerical integration)
    print("\nTesting approximate orthogonality...")
    
    # Compute mass matrix using simple quadrature
    M = phi.T @ phi  # Approximation (should use proper quadrature)
    M = M / len(X_inside)  # Normalize
    
    # Check orthogonality
    off_diag = M - np.diag(np.diag(M))
    orthogonality_error = np.max(np.abs(off_diag))
    
    print(f"Max off-diagonal entry in mass matrix: {orthogonality_error:.2e}")
    
    # Plot first few basis functions
    try:
        import matplotlib.pyplot as plt
        from matplotlib.tri import Triangulation
        
        fig, axes = plt.subplots(2, 3, figsize=(12, 8))
        
        # Create triangulation for plotting
        tri = Triangulation(X_inside, Y_inside)
        
        basis_names = [
            ('Vertex 0', 0), ('Vertex 1', 1), ('Vertex 2', 2),
            ('Edge 0-1', 3), ('Edge 1-2', 4), ('Edge 2-0', 5)
        ]
        
        for ax, (name, idx) in zip(axes.flat, basis_names):
            if idx < phi.shape[1]:
                scatter = ax.tripcolor(tri, phi[:, idx], shading='gouraud', cmap='viridis')
                ax.set_title(f'{name} basis')
                ax.set_xlabel('x')
                ax.set_ylabel('y')
                ax.set_aspect('equal')
                plt.colorbar(scatter, ax=ax)
        
        plt.tight_layout()
        plt.show()
        
    except ImportError:
        print("Matplotlib not available for plotting")
    
    return basis, phi


def evaluate_at_points(x, y, r=4):
    """
    Simple interface to evaluate triangle basis at given points.
    
    Parameters:
    -----------
    x, y : arrays
        Coordinates of points (must be inside triangle: x>=0, y>=0, x+y<=1)
    r : int
        Polynomial degree
    
    Returns:
    --------
    phi : array of shape (n_points, Np)
        Basis function values
    """
    basis = TriangleInvariantBasis(r)
    return basis.evaluate_all(x, y)


# Test with specific points
if __name__ == "__main__":
    # Create basis
    basis, phi = demo_triangle_basis()
    
    # Test at specific points
    print("\n" + "=" * 60)
    print("Basis values at specific points")
    print("=" * 60)
    
    # Test points
    test_points = [
    (-1.0, -1.0),  # Node 1
    (1.0, -1.0),  # Node 2
    (-1.0, 1.0),  # Node 3
    (-0.654653670707978, -1.0),  # Node 4
    (0.0, -1.0),  # Node 5
    (0.654653670707978, -1.0),  # Node 6
    (0.654653670707978, -0.654653670707978),  # Node 7
    (0.0, 0.0),  # Node 8
    (-0.654653670707978, 0.654653670707978),  # Node 9
    (-1.0, 0.654653670707978),  # Node 10
    (-1.0, 0.0),  # Node 11
    (-1.0, -0.654653670707978),  # Node 12
    (-0.551551223569326, -0.551551223569326),   # Node 13
    (0.55155122356932573, -0.551551223569326),   # Node 14
    (-0.551551223569326, 0.55155122356932573)    # Node 15
]

    for x, y in test_points:
        if x >= 0 and y >= 0 and x + y <= 1:
            phi_vals = evaluate_at_points(np.array([x]), np.array([y]), r=4)
            print(f"\nPoint ({x:.3f}, {y:.3f}):")
            print(f"  Vertex basis: {phi_vals[0, :3]}")
            print(f"  Sum of vertices: {np.sum(phi_vals[0, :3]):.3f}")
        else:
            print(f"\nPoint ({x:.3f}, {y:.3f}) is outside triangle")


