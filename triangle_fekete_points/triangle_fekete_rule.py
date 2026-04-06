import numpy as np
import fekete_subrule_func


def fekete_degree(rule):
    """
    Return the polynomial degree of exactness of a Fekete rule
    for the triangle.

    Parameters
    ----------
    rule : int
        Index of the Fekete rule.

    Returns
    -------
    int
        Degree of exactness.

    Raises
    ------
    ValueError
        If the rule index is invalid.
    """

    rule_degrees = {
        1: 3,
        2: 6,
        3: 9,
        4: 12,
        5: 12,
        6: 15,
        7: 18,
    }
    try:
        return rule_degrees[rule]
    except KeyError:
        raise ValueError(f"FEKETE_DEGREE - Illegal RULE = {rule}")



def fekete_suborder_num(rule):
    """
    Return the number of suborders for a Fekete rule.

    Parameters
    ----------
    rule : int
        Index of the Fekete rule.

    Returns
    -------
    int
        Number of suborders.

    Raises
    ------
    ValueError
        If the rule index is invalid.
    """
    suborder_counts = {
        1: 3,
        2: 7,
        3: 12,
        4: 19,
        5: 21,
        6: 28,
        7: 38,
    }

    try:
        return suborder_counts[rule]
    except KeyError:
        raise ValueError(f"FEKETE_SUBORDER_NUM - Illegal RULE = {rule}")
    


def fekete_suborder(rule, suborder_num):
    """
    Return the suborders for a Fekete rule.

    Parameters
    ----------
    rule : int
        Index of the Fekete rule.
    suborder_num : int
        Expected number of suborders.

    Returns
    -------
    list of int
        The suborders of the rule.

    Raises
    ------
    ValueError
        If the rule index is invalid or the expected suborder count does not match.
    """
    suborders_dict = {
        1: [1, 3, 6], # 3 suborders
        2: [1] + [3]*3 + [6]*3,   # 7 suborders
        3: [1] + [3]*4 + [6]*7,   # 12 suborders
        4: [1] + [3]*6 + [6]*12,  # 19 suborders 
        5: [1] + [3]*10 + [6]*10, # 21 suborders
        6: [1] + [3]*9 + [6]*18,  # 28 suborders
        7: [1] + [3]*11 + [6]*26  # 38 suborders
    }

    try:
        suborder = suborders_dict[rule]
    except KeyError:
        raise ValueError(f"FEKETE_SUBORDER - Illegal RULE = {rule}")

    if len(suborder) != suborder_num:
        raise ValueError("Mismatch in suborder_num and actual suborder length")

    return suborder



def fekete_order_num(rule):
    """
    Return the order (number of points) of a Fekete rule
    for the triangle.

    Parameters
    ----------
    rule : int
        Index of the rule.

    Returns
    -------
    int
        Total number of points in the rule.
    """

    suborder_num = fekete_suborder_num(rule)
    suborder = fekete_suborder(rule, suborder_num)

    order_num = 0
    for order in range(suborder_num):
        order_num += suborder[order]

    return order_num



def fekete_rule(rule):
    """
    Return the Fekete points (XY) and weights for a given rule.

    Parameters
    ----------
    rule : int
        Index of the Fekete rule.

    Returns
    -------
    xy : np.ndarray
        Array of shape (N,2) with the XY coordinates of all points.
    w : np.ndarray
        Array of shape (N,) with the weights of all points.
    """

    # Get suborders
    suborder_num = fekete_suborder_num(rule)
    suborders = fekete_suborder(rule, suborder_num)

    # Get suborder points (barycentric coordinates) and weights
    suborder_xyz, suborder_w = fekete_subrule_func.fekete_subrule(rule)

    xy_list = []
    w_list = []

    for s, so in enumerate(suborders):
        l1, l2, l3 = suborder_xyz[s]

        if so == 1:
            xy_list.append([l1, l2])
            w_list.append(suborder_w[s])

        elif so == 3:
            perms = [(l1, l2), (l2, l3), (l3, l1)]
            for p in perms:
                xy_list.append(p)
                w_list.append(suborder_w[s])

        elif so == 6:
            perms = [
                (l1, l2), (l2, l3), (l3, l1),
                (l2, l1), (l3, l2), (l1, l3)
            ]
            for p in perms:
                xy_list.append(p)
                w_list.append(suborder_w[s])
        else:
            raise ValueError(f"Invalid suborder: {so}")

    # Convert to NumPy arrays for convenience
    xy = np.array(xy_list)
    w = np.array(w_list)

    return xy, w


def classify_fekete_points(xy_bary):
    """
    Classify Fekete points into corners, edges, and interior points.

    Parameters
    ----------
    xy_bary : list of tuples
        List of barycentric coordinates (l1, l2, l3) for each point.

    Returns
    -------
    corners : list of tuples
        Points on the corners (l_i = 1)
    edges : list of tuples
        Points on the edges (one l_i = 0)
    interior : list of tuples
        Points strictly inside the triangle (all l_i > 0)
    """
    corners = []
    edges = []
    interior = []

    tol = 1e-12  # tolerance to account for floating-point errors

    for l1, l2 in xy_bary:
        l3 = 1 - l1- l2
        if abs(l1 - 1.0) < tol or abs(l2 - 1.0) < tol or abs(l3 - 1.0) < tol:
            corners.append((l1, l2, l3))
        elif abs(l1) < tol or abs(l2) < tol or abs(l3) < tol:
            edges.append((l1, l2, l3))
        else:
            interior.append((l1, l2, l3))

    return corners, edges, interior



def pretty_print_points(points, decimals=6):
    return [
        tuple(round(float(c), decimals) for c in p)
        for p in points
    ]


def fekete_points_tri(rule):
    """
    Return Fekete points for a triangle in xi, eta coordinates with weights,
    classified as corners, edges, and interior points.

    Parameters
    ----------
    rule : int
        Fekete rule number (1..7).

    Returns
    -------
    corners, edges, interior : list of tuples
        Each tuple is (xi, eta, w)
    """
    # --- Get suborders ---
    suborder_num = fekete_suborder_num(rule)
    suborders = fekete_suborder(rule, suborder_num)

    # --- Get barycentric points and weights from subrule function ---
    suborder_xyz, suborder_w = fekete_subrule_func.fekete_subrule(rule)

    # --- Expand points according to suborder multiplicity ---
    bary_points = []
    weights = []

    for s, so in enumerate(suborders):
        l1, l2, l3 = suborder_xyz[s]
        w = suborder_w[s]

        if so == 1:  # corner
            bary_points.append((l1, l2, l3))
            weights.append(w)

        elif so == 3:  # edge
            perms = [(l1, l2, l3), (l2, l3, l1), (l3, l1, l2)]
            bary_points.extend(perms)
            weights.extend([w]*3)

        elif so == 6:  # interior
            perms = [
                (l1, l2, l3), (l2, l3, l1), (l3, l1, l2),
                (l2, l1, l3), (l3, l2, l1), (l1, l3, l2)
            ]
            bary_points.extend(perms)
            weights.extend([w]*6)

        else:
            raise ValueError(f"Invalid suborder: {so}")

    # --- Classify points ---
    corners = []
    edges = []
    interior = []

    tol = 1e-12  # numerical tolerance
    for (l1, l2, l3), w in zip(bary_points, weights):
        xi = l2
        eta = l3

        if abs(l1-1) < tol or abs(l2-1) < tol or abs(l3-1) < tol:
            corners.append((xi, eta, w))
        elif abs(l1) < tol or abs(l2) < tol or abs(l3) < tol:
            edges.append((xi, eta, w))
        else:
            interior.append((xi, eta, w))

    return corners, edges, interior


# Rule 1: Spectral order = 2
# Rule 2: Spectral order = 5
# Rule 3: Spectral order = 8
# Rule 4: Spectral order = 11
# Rule 5: Spectral order = 11
# Rule 6: Spectral order = 14
# Rule 7: Spectral order = 17

rule = 2  # example
# xy_bary, w = fekete_rule(rule)

# corners, edges, interior = classify_fekete_points(xy_bary)
corners, edges, interior = fekete_points_tri(rule)


print("Corner points:", pretty_print_points(corners))
print("Edge points:", pretty_print_points(edges))
print("Interior points:", pretty_print_points(interior))



# def print_fekete_points(rule):
#     """
#     Print Fekete points and weights for a given rule index.
#     """
#     suborder_num = fekete_suborder_num(rule)
#     xy, w = fekete_rule(rule)

#     print(f"Fekete points for rule {rule} (total {len(xy)} points):\n")
#     for i, (pt, weight) in enumerate(zip(xy, w)):
#         print(f"Point {i+1}: x = {pt[0]:.10f}, y = {pt[1]:.10f}, weight = {weight:.10f}")

# # Example: print points for rule 4
# print_fekete_points(1)


