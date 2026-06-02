def calculate_required_quadrature(p, basis_type="proriol"):
    """
    Calculate minimum quadrature points needed for accurate integration.
    For Proriol basis, integrand degree ≈ 2p
    """
    # For stiffness matrix: ∇N_i · ∇N_j has degree 2p
    # For mass matrix: N_i * N_j has degree 2p
    required_degree = 2 * p
    
    # Quadrature rules for triangles (Dunavant's rules)
    quad_rules = {
        1: 1,   # Degree 1
        2: 3,   # Degree 2
        3: 4,   # Degree 3
        4: 6,   # Degree 4
        5: 7,   # Degree 5
        6: 12,  # Degree 6
        7: 16,  # Degree 7
        8: 19,  # Degree 8
        9: 25,  # Degree 9
        10: 31, # Degree 10
        11: 37, # Degree 11
        12: 43, # Degree 12
        13: 49, # Degree 13
        14: 55, # Degree 14
        15: 61, # Degree 15
    }
    
    for degree, n_points in quad_rules.items():
        if degree >= required_degree:
            print(f"p={p}: Need degree {required_degree} quadrature -> {n_points} points")
            return n_points
    
    return 61  # Fallback

# For p=4: required_degree = 8
# Minimum points needed: 19 (not 6!)


def get_quadrature_for_spectral_triangle(p):
    """
    Get appropriate quadrature rule for given spectral order.
    """
    # For p=4, use at least 19-point rule (degree 8)
    # For testing, use 16 points (degree 7) - slightly less accurate but ok
    # For production, use 19+ points
    
    quad_rules = {
        1: ("3-point", 3, 3),    # degree 3
        2: ("4-point", 4, 4),    # degree 4
        3: ("7-point", 7, 5),    # degree 5
        4: ("19-point", 19, 8),  # degree 8 - RECOMMENDED
        5: ("25-point", 25, 9),  # degree 9
        6: ("31-point", 31, 10), # degree 10
    }
    
    if p <= 4:
        name, n_points, degree = quad_rules[4]  # Use 19-point for p=4
    else:
        name, n_points, degree = quad_rules.get(p, ("31-point", 31, 10))
    
    print(f"Using {name} quadrature ({n_points} points, degree {degree}) for p={p}")
    
    # Return quadrature points from Dunavant's tables
    return get_dunavant_quadrature(n_points, degree)



def get_dunavant_quadrature(n_points, degree):
    """
    Get Dunavant quadrature points for triangles.
    You can pre-compute these tables or use a library.
    """
    # For p=4 (degree 8), use 19-point Dunavant rule
    if n_points == 19 and degree == 8:
        # Dunavant's 19-point rule (degree 8)
        # Barycentric coordinates (L1, L2, L3) and weights
        points = [
            # Points and weights from Dunavant (2000)
            # [L1, L2, weight]
            [0.333333333333333, 0.333333333333333, 0.030998162062248],
            [0.020634961602525, 0.489682519198738, 0.032991048487255],
            [0.489682519198738, 0.020634961602525, 0.032991048487255],
            [0.489682519198738, 0.489682519198738, 0.032991048487255],
            [0.125820817014127, 0.437089591492937, 0.033416281208640],
            [0.437089591492937, 0.125820817014127, 0.033416281208640],
            [0.437089591492937, 0.437089591492937, 0.033416281208640],
            [0.623592928761935, 0.188203535619032, 0.016298273175133],
            [0.188203535619032, 0.623592928761935, 0.016298273175133],
            [0.188203535619032, 0.188203535619032, 0.016298273175133],
            [0.910540973211095, 0.044729513394453, 0.001217210296702],
            [0.044729513394453, 0.910540973211095, 0.001217210296702],
            [0.044729513394453, 0.044729513394453, 0.001217210296702],
            [0.741198598784498, 0.221962989160765, 0.010582153574615],
            [0.741198598784498, 0.036838412054736, 0.010582153574615],
            [0.221962989160765, 0.741198598784498, 0.010582153574615],
            [0.036838412054736, 0.741198598784498, 0.010582153574615],
            [0.221962989160765, 0.036838412054736, 0.010582153574615],
            [0.036838412054736, 0.221962989160765, 0.010582153574615],
        ]
        
        # Convert to (xi, eta) collapsed coordinates if needed
        quadrature = []
        for L1, L2, w in points:
            L3 = 1.0 - L1 - L2
            # Convert barycentric to collapsed coordinates
            xi = L2 / (1.0 - L1) if L1 < 0.9999 else 0.0
            eta = L1
            quadrature.append((xi, eta, w))
        
        return quadrature
    
    # Fallback for other rules
    return get_simple_quadrature(n_points)


