
from tsem_points_store import TSEMLibrary
from tsem_points_store import TSEMPoint
from get_gll_nodes import get_gll_nodes
import numpy as np
import random

def populate_tsem(order):
    lib = TSEMLibrary(order)

    # 1. Corners (Barycentric: (1,0,0), (0,1,0), (0,0,1))
    lib.corners = [
        TSEMPoint(0.0, 0.0, 1.0), # V1
        TSEMPoint(1.0, 0.0, 1.0), # V2
        TSEMPoint(0.0, 1.0, 1.0)  # V3
    ]

    if order < 1: return lib

    # 2. Get GLL points for the 1D edges
    gll = get_gll_nodes(order)

    # 3. Populate Edges (Interior points of each edge only)
    # Edge 0: V1 -> V2, Edge 1: V2 -> V3, Edge 2: V3 -> V1
    for i in range(3):
        v_start = lib.corners[i]
        v_end = lib.corners[(i + 1) % 3]

        for j in range(1, order): # Exclude endpoints -1 and 1
            # Map [-1, 1] to [0, 1]
            s = (gll[j] + 1.0) / 2.0

            x = (1 - s) * v_start.xi + s * v_end.xi
            y = (1 - s) * v_start.eta + s * v_end.eta
            lib.edges[i].append(TSEMPoint(x, y, 1.0))

    # 4. Populate Interior (Lobatto interpolation)
    # IMA Journal of Applied Mathematics Advance Access published March 16, 2005
    # A Lobatto interpolation grid over the triangle
    # M.G. Blyth and C. Pozrikidis

    if order >= 3:
        for i in range(1, order):
            for j in range(1, order - i):
                k = order - i - j

                vi = (gll[i] + 1)*0.5
                vj = (gll[j] + 1)*0.5
                vk = (gll[k] + 1)*0.5

                xi  = (1/3.0)*(1 + 2*vj - vi - vk)
                eta = (1/3.0)*(1 + 2*vk - vi - vj)

                lib.interior.append(TSEMPoint(xi, eta, 1.0))

    return lib



def plot_random_triangle(lib):
    # Random triangle
    x1, y1 = random.uniform(-10,10), random.uniform(-10,10)
    x2, y2 = random.uniform(-10,10), random.uniform(-10,10)
    x3, y3 = random.uniform(-10,10), random.uniform(-10,10)

    order = lib.order
    rand_tri = TSEMLibrary(order)

    corners = [
        (x1, y1),  # V1
        (x2, y2),  # V2
        (x3, y3)   # V3
    ]

    rand_tri.corners = [TSEMPoint(x, y, 1.0) for (x, y) in corners]

    # Edge definitions
    for i in range(3):
        rand_tri.edges[i] = []

        for j in range(0, order-1):  # skip corners if needed
            xi   = lib.edges[i][j].xi
            eta  = lib.edges[i][j].eta
            l1   = 1.0 - xi - eta
            l2   = xi
            l3   = eta

            x = l1*x1 + l2*x2 + l3*x3
            y = l1*y1 + l2*y2 + l3*y3

            rand_tri.edges[i].append(TSEMPoint(x, y, 1.0))

    # Interior definitions
    for i in range(len(lib.interior)):
        xi = lib.interior[i].xi
        eta= lib.interior[i].eta
        l3   = 1.0 - xi - eta
        l2   = xi
        l1   = eta

        x = l1*x1 + l2*x2 + l3*x3
        y = l1*y1 + l2*y2 + l3*y3

        rand_tri.interior.append(TSEMPoint(x, y, 1.0))

    
    rand_tri.plot_tsem_library()






lib = populate_tsem(3)
# lib.plot_tsem_library()

plot_random_triangle(lib)



