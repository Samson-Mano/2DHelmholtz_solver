

quadrature_points = [
    (1.0 / 3.0, 1.0 / 3.0, -0.5625),
    (0.2, 0.6, 0.520833333333333),
    (0.2, 0.2, 0.520833333333333),
    (0.6, 0.6, 0.520833333333333)
]



def sort_quadrature_points(points):
    # Sort points based on the first coordinate (x), then by the second coordinate (y)
    sorted_points = sorted(points, key=lambda p: (p[0], p[1]))
    return sorted_points



sorted_quadrature_points = sort_quadrature_points(quadrature_points)

# Print the sorted quadrature points
for point in sorted_quadrature_points:
    print(point)


    

