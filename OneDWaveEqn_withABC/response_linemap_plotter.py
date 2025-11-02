import matplotlib.pyplot as plt
import numpy as np
from mpl_toolkits.mplot3d.art3d import Line3DCollection
from matplotlib import cm


def plot_linemap(parsed_results, num_samples = 10, plot_type = 'd'):
    """
    Generates a 3D "waterfall" plot (series of line maps) to visualize the 
    time-spatial response of a system.
    
    The input data (displacement, velocity, acceleration) is assumed to be 
    in the shape (numDOF, numTimeSteps).
    
    Args:
        parsed_results (dict): Dictionary containing 'time', 'displacement', 
                               'velocity', and 'acceleration' arrays.
        num_samples (int): Number of time steps to sample and plot.
        plot_type (str): 'd' for displacement, 'v' for velocity, 'a' for acceleration.
    """

    time = parsed_results['time']  # shape: (numTimeSteps,)
    displacement = parsed_results['displacement']  # shape: (numDOF, numTimeSteps)
    velocity = parsed_results['velocity']  # shape: (numDOF, numTimeSteps)
    acceleration = parsed_results['acceleration']  # shape: (numDOF, numTimeSteps)
    numDOF, numTimeSteps = displacement.shape

    # Extract the data
    x_label = ''
    if(plot_type == 'd'):
        z_data = displacement 
        x_label = 'Displacement'
    elif (plot_type == 'v'):
        z_data = velocity
        x_label = 'Velocity'
    elif (plot_type == 'a'):
        z_data = acceleration
        x_label = 'Acceleration'
    

    # Create spatial positions along the string
    spatial_positions = np.linspace(0, numDOF - 1, numDOF)

    # Plot 3D line plot with rainbow contour
    fig = plt.figure(figsize=(12, 8))
    ax = fig.add_subplot(111, projection='3d')

    # Normalize displacement values for color mapping
    norm = plt.Normalize(z_data.min(), z_data.max())
    cmap = cm.get_cmap('rainbow')


    # Choose 10 equally spaced indices from the time steps
    sample_indices = np.linspace(0, numTimeSteps - 1, num_samples, dtype=int)


    for i in sample_indices:
        x = np.full_like(spatial_positions, time[i])  # Time axis
        y = spatial_positions  # Spatial axis
        z = z_data[:, i]  # Value (Displacement, Velocity, or Acceleration)

        # Create segments for line collection
        points = np.array([x, y, z]).T.reshape(-1, 1, 3)
        segments = np.concatenate([points[:-1], points[1:]], axis=1)

        # Color based on displacement
        colors = cmap(norm(z[:-1]))
        lc = Line3DCollection(segments, colors=colors, linewidth=2)
        ax.add_collection(lc)

    # Set axis limits and labels
    ax.set_xlim(time[0], time[-1])
    ax.set_ylim(0, numDOF - 1)
    ax.set_zlim(z_data.min(), z_data.max())

    ax.set_box_aspect([4, 1, 0.2])
    ax.set_xlabel('Time (s)')
    ax.set_ylabel('Degrees of Freedom (spatial index)')
    ax.set_zlabel(x_label)
    ax.set_title(f'3D Line Plot of String {x_label} Over Time')

    # Add colorbar
    mappable = cm.ScalarMappable(norm=norm, cmap=cmap)
    mappable.set_array(z_data)
    cbar = plt.colorbar(mappable, ax=ax, shrink=0.6, pad=0.1)
    cbar.set_label(x_label)

    plt.tight_layout()
    plt.show()







