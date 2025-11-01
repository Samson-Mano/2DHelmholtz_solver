import numpy as np
import matplotlib.pyplot as plt

class InitialConditionData:
    def __init__(self, total_nodes):
        """
        Initialize with the total number of nodes.
        """
        self.total_nodes = total_nodes
        self.initial_condition_profile = np.zeros(total_nodes)

    def half_sine_interpolation(self, pt1, pt2, pt3, t):
        """
        Perform half-sine interpolation.

        Parameters:
        - pt1, pt2, pt3: Control points in [x, y] form.
        - t: Normalized position between 0 and 1.

        Returns:
        - [x, y] interpolated point.
        """
        x = (1 - np.cos(np.pi * t)) / 2  # Not used further but kept for clarity
        y = pt2[1] * np.sin(np.pi * t)
        return [x, y]

    def set_initial_condition_profile(self, node_start, node_end, inl_cond_val):
        """
        Apply sine-shaped initial condition between node_start and node_end.

        Parameters:
        - node_start: Starting node index
        - node_end: Ending node index
        - inl_cond_val: Peak value (e.g., displacement or velocity)
        """
        if node_start < 0 or node_end >= self.total_nodes or node_start >= node_end:
            raise ValueError("Invalid node range")

        spread_length = node_end - node_start
        pt1 = [0, 0]
        pt2 = [spread_length / 2.0, inl_cond_val]
        pt3 = [spread_length, 0]

        for i in range(node_start, node_end + 1):
            t = (i - node_start) / spread_length
            _, y = self.half_sine_interpolation(pt1, pt2, pt3, t)
            self.initial_condition_profile[i] = y

    def get_initial_condition_profile(self):
        """
        Returns the profile array.
        """
        return self.initial_condition_profile

    def plot_profile(self, title="Initial Condition Profile"):
        """
        Plot the current profile using matplotlib.
        """
        plt.figure(figsize=(8, 4))
        plt.plot(self.initial_condition_profile, label='Initial Condition')
        plt.xlabel("Node Index")
        plt.ylabel("Value")
        plt.title(title)
        plt.grid(True)
        plt.legend()
        plt.tight_layout()
        plt.show()






        