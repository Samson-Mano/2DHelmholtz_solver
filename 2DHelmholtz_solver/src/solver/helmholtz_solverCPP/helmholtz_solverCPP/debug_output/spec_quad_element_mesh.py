


# Single Quadrilateral element spectral mesh with 16 nodes 
# x----x----x----x
# |              |
# x    x    x    x
# |              |
# x    x    x    x
# |              |
# x----x----x----x


# Node data
node_coords = [
    (-10.0, -10.0),  # Node 1
    (-4.47214, -10.0),  # Node 2
    (4.47214, -10.0),  # Node 3
    (10.0, -10.0),  # Node 4
    (-10.0, -4.47214),  # Node 5
    (-4.47214, -4.47214),  # Node 6
    (4.47214, -4.47214),  # Node 7
    (10.0, -4.47214),  # Node 8
    (-10.0, 4.47214),  # Node 9
    (-4.47214, 4.47214),  # Node 10
    (4.47214, 4.47214),  # Node 11
    (10.0, 4.47214),  # Node 12
    (-10.0, 10.0),  # Node 13
    (-4.47214, 10.0),  # Node 14
    (4.47214, 10.0),  # Node 15
    (10.0, 10.0),  # Node 16
]


#Quadrature points and weights for 4-point Gauss-Legendre quadrature
# Properly ordered for a 2D quadrilateral element with 16 nodes (4x4 grid)
gauss_quadrature_points = [
    (-0.8611363115940526, -0.8611363115940526, 0.12100299328560192),  # Point 1
    (-0.3399810435848563, -0.8611363115940526, 0.22685185185185164),  # Point 2
    (0.3399810435848563, -0.8611363115940526, 0.22685185185185164),   # Point 3
    (0.8611363115940526, -0.8611363115940526, 0.12100299328560192),    # Point 4
    (-0.8611363115940526, -0.3399810435848563, 0.12100299328560192),  # Point 5
    (-0.3399810435848563, -0.3399810435848563, 0.22685185185185164),  # Point 6
    (0.3399810435848563, -0.3399810435848563, 0.22685185185185164),   # Point 7
    (0.8611363115940526, -0.3399810435848563, 0.12100299328560192),    # Point 8
    (-0.8611363115940526, 0.3399810435848563, 0.12100299328560192),  # Point 9
    (-0.3399810435848563, 0.3399810435848563, 0.22685185185185164),  # Point 10
    (0.3399810435848563, 0.3399810435848563, 0.22685185185185164),   # Point 11
    (0.8611363115940526, 0.3399810435848563, 0.12100299328560192),    # Point 12
    (-0.8611363115940526, 0.8611363115940526, 0.12100299328560192),  # Point 13
    (-0.3399810435848563, 0.8611363115940526, 0.22685185185185164),  # Point 14
    (0.3399810435848563, 0.8611363115940526, 0.22685185185185164),   # Point 15
    (0.8611363115940526, 0.8611363115940526, 0.12100299328560192)    # Point 16
]


nen = 16 # number of element nodes

wave_number = 2.0943951023931926 # wave number (k) for the Helmholtz equation, corresponding to a wavelength of xx units in the medium

# GLL Locations
gll_locations = [
    -1.0,  # Node 1
    -0.44721359549995793,  # Node 2
    0.44721359549995793,  # Node 3
    1.0,  # Node 4
]

spectral_order = 3 # Spectral order of the element (number of nodes per side - 1)


def evaluate_lagrange_1D(xi):
    # Placeholder function to compute 1D Lagrange shape functions
    n = len(gll_locations)
    L = [0.0] * n  # Shape function values
    dL = [0.0] * n  # Derivative values

    for i in range(n):
        li = 1.0
        dli = 0.0

        # Calculate li
        for j in range(n):
            if j != i:
                li *= (xi - gll_locations[j]) / (gll_locations[i] - gll_locations[j])

        # Calculate dli
        for j in range(n):
            if j != i:
                term = 1.0 / (gll_locations[i] - gll_locations[j])

                for k in range(n):
                    if k != i and k != j:
                        term *= (xi - gll_locations[k]) / (gll_locations[i] - gll_locations[k])
                
                dli += term

        L[i]   = li
        dL[i]  = dli

    return L, dL



def evaluate_quadrilateral_shape_functions(xi, eta, nen):
    n1d = spectral_order + 1  # Number of nodes per side

    lx, dlx = evaluate_lagrange_1D(xi)
    ly, dly = evaluate_lagrange_1D(eta)
    
    # Alocate shape function array
    N = [0.0] * nen
    dN_dxi = [0.0] * nen
    dN_deta = [0.0] * nen

    idx = 0
    for i in range(n1d):
        for j in range(n1d):
            N[idx] = lx[i] * ly[j]

            dN_dxi[idx] = dlx[i] * ly[j]
            dN_deta[idx] = lx[i] * dly[j]
            idx += 1

    return N, dN_dxi, dN_deta


def create_spectral_quadrilateral_element_mesh(node_coords, gauss_quadrature_points, nen):

    element_stiffness_matrix = [[0.0] * nen for _ in range(nen)]
    element_mass_matrix = [[0.0] * nen for _ in range(nen)]    

    for i in range(nen):
        xi = gauss_quadrature_points[i][0]
        eta = gauss_quadrature_points[i][1]
        weight = gauss_quadrature_points[i][2]

        # Evaluate shape functions at the quadrature point (xi, eta)
        N, dN_dxi, dN_deta = evaluate_quadrilateral_shape_functions(xi, eta, nen)

        J = [[0.0, 0.0], [0.0, 0.0]]  # Initialize Jacobian matrix

        # Compute the Jacobian 
        for j in range(nen):
            xj, yj = node_coords[j]
            # Compute the Jacobian components
            J[0][0] += dN_dxi[j] * xj  # J11
            J[0][1] += dN_dxi[j] * yj  # J12
            J[1][0] += dN_deta[j] * xj  # J21
            J[1][1] += dN_deta[j] * yj  # J22
        
        # Compute the determinant of the Jacobian
        detJ = J[0][0] * J[1][1] - J[0][1] * J[1][0]

        # Compute the inverse of the Jacobian
        invJ = [[0.0, 0.0], [0.0, 0.0]]
        invJ[0][0] = J[1][1] / detJ  # J22 / detJ
        invJ[0][1] = -J[0][1] / detJ  # -J12 / detJ
        invJ[1][0] = -J[1][0] / detJ  # -J21 / detJ
        invJ[1][1] = J[0][0] / detJ  # J11 / detJ

        # Transform shape function derivatives to physical coordinates
        dN_dx = [0.0] * nen
        dN_dy = [0.0] * nen

        for j in range(nen):
            dN_dx[j] = invJ[0][0] * dN_dxi[j] + invJ[0][1] * dN_deta[j]
            dN_dy[j] = invJ[1][0] * dN_dxi[j] + invJ[1][1] * dN_deta[j]

        # Assemble the element stiffness matrix and element mass matrix using the shape functions and their derivatives

        for m in range(nen):
            for n in range(nen):
                # Stiffness matrix contribution
                element_stiffness_matrix[m][n] += (dN_dx[m] * dN_dx[n] + dN_dy[m] * dN_dy[n]) * detJ * weight

                # Mass matrix contribution
                element_mass_matrix[m][n] += N[m] * N[n] * detJ * weight

    return element_stiffness_matrix, element_mass_matrix
        



elemeent_stiffness_matrix, element_mass_matrix = create_spectral_quadrilateral_element_mesh(node_coords, gauss_quadrature_points, nen)

element_matrix = [[0.0] * nen for _ in range(nen)]

for i in range(nen):
    for j in range(nen):
        element_matrix[i][j] = elemeent_stiffness_matrix[i][j] - wave_number**2 * element_mass_matrix[i][j]


# Print the resulting element stiffness matrix and mass matrix
# print("Element Stiffness Matrix:")
# for row in elemeent_stiffness_matrix:
#     print(row)  

# print("\nElement Mass Matrix:")
# for row in element_mass_matrix:
#     print(row)



# 16 x 16 Matrix (Stiffness - k^2 * Mass)

# Print to a text file
with open("py_element_matrices.txt", "w") as f:
    f.write("Element Stiffness Matrix:\n")
    for row in elemeent_stiffness_matrix:
        f.write(" ".join(f"{val:.6e}" for val in row) + "\n")

    f.write("\nElement Mass Matrix:\n")
    for row in element_mass_matrix:
        f.write(" ".join(f"{val:.6e}" for val in row) + "\n")

    f.write("\nElement Matrix (Stiffness - k^2 * Mass):\n")
    for row in element_matrix:
        f.write(" ".join(f"{val:.6e}" for val in row) + "\n")





