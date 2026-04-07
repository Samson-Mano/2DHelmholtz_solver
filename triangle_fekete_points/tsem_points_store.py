import matplotlib.pyplot as plt

class TSEMPoint:
  def __init__(self, xi, eta, weight):
    self.xi = xi
    self.eta = eta
    self.weight = weight

class TSEMLibrary:
  def __init__(self, order):
    self.order = order
    # Separate storage allows easy boundary condition application
    self.corners = []   # List of 3 TSEMPoint
    self.edges = [[], [], []] # Lists of points for each of the 3 edges
    self.interior = []   # List of strictly interior TSEMPoint

  @property
  def total_nodes(self):
    # Total nodes for order N is (N+1)(N+2)/2
    return (self.order + 1) * (self.order + 2) // 2


  def print_results(self):
    print(f"--- Order {self.order} ({self.total_nodes} nodes) ---")
    print("Corners:", [(round(p.xi, 4), round(p.eta, 4)) for p in self.corners])
    for i, edge in enumerate(self.edges):
      print(f"Edge {i+1}:", [(round(p.xi, 4), round(p.eta, 4)) for p in edge])
    print("Interior:", [(round(p.xi, 4), round(p.eta, 4)) for p in self.interior])
    print()

  def plot_tsem_library(self):
    lib = self
    plt.figure(figsize=(8, 8))

    # 1. Extract coordinates for each group
    c_xi, c_eta = [p.xi for p in lib.corners], [p.eta for p in lib.corners]

    e_xi, e_eta = [], []
    for edge in lib.edges:
      e_xi.extend([p.xi for p in edge])
      e_eta.extend([p.eta for p in edge])

    i_xi, i_eta = [p.xi for p in lib.interior], [p.eta for p in lib.interior]

    # 2. Plot with distinct colors and sizes
    plt.scatter(c_xi, c_eta, c='red', s=100, label='Corners', zorder=5)
    plt.scatter(e_xi, e_eta, c='blue', s=60, label='Edges', zorder=4)
    plt.scatter(i_xi, i_eta, c='green', s=40, label='Interior', zorder=3)

    # 3. Draw the triangle boundary for context
    # Close the triangle by repeating the first point
    c_xi.append(c_xi[0])
    c_eta.append(c_eta[0])

    plt.plot(c_xi, c_eta, 'k--', alpha=0.6)

    # 4. Styling
    plt.title(f"TSEM Nodes - Lobatto interpolation grid over the triangle - Spectral Order {lib.order}")
    plt.xlabel(r"$\xi$")
    plt.ylabel(r"$\eta$")
    plt.legend(loc='upper right')
    plt.gca().set_aspect('equal')
    plt.grid(True, linestyle=':', alpha=0.6)
    plt.show()





