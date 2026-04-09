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

      corner_node_id = 0  # Corner node ids

      # --- Corners ---
      for p in lib.corners:
          plt.scatter(p.xi, p.eta, c='red', s=100, zorder=5)
          plt.text(p.xi, p.eta, str(corner_node_id), fontsize=10,
                  ha='right', va='bottom')
          corner_node_id += 1

      edge_node_id = 0 # Edge node ids

      # --- Edges ---
      for edge in lib.edges:
          for p in edge:
              plt.scatter(p.xi, p.eta, c='blue', s=60, zorder=4)
              plt.text(p.xi, p.eta, str(edge_node_id), fontsize=9,
                      ha='right', va='bottom')
              edge_node_id += 1


      interior_node_ids = 0 # Interior node ids

      # --- Interior ---
      for p in lib.interior:
          plt.scatter(p.xi, p.eta, c='green', s=40, zorder=3)
          plt.text(p.xi, p.eta, str(interior_node_ids), fontsize=8,
                  ha='right', va='bottom')
          interior_node_ids += 1

      # --- Triangle boundary ---
      c_xi  = [p.xi for p in lib.corners]
      c_eta = [p.eta for p in lib.corners]

      plt.plot(c_xi + [c_xi[0]], c_eta + [c_eta[0]], 'k--', alpha=0.6)

      # --- Styling ---
      plt.title(f"TSEM Nodes (Order {lib.order})")
      plt.xlabel(r"$\xi$")
      plt.ylabel(r"$\eta$")
      plt.gca().set_aspect('equal')
      plt.grid(True, linestyle=':', alpha=0.6)

      plt.show()











