


def get_quadrilateral_quadrature_points(spectral_order):
    """
    Get quadrature points and weights for a quadrilateral element
    using tensor product of 1D Gauss-Lobatto-Legendre points.
    """
    from scipy.special import roots_legendre

    n1d = spectral_order + 1
    gll_points, gll_weights = roots_legendre(n1d)

    quadrature_points = []
    for j in range(n1d):
        for i in range(n1d):
            xi = gll_points[i]
            eta = gll_points[j]
            weight = gll_weights[i] * gll_weights[j]
            quadrature_points.append((xi, eta, weight))

    return quadrature_points


# spectral_order = 3
# quadrilateral_quadrature_points = get_quadrilateral_quadrature_points(spectral_order)

# print("Quadrilateral quadrature points and weights:")
# for xi, eta, weight in quadrilateral_quadrature_points:
#     print(f"xi: {xi:.6f}, eta: {eta:.6f}, weight: {weight:.6f}")



