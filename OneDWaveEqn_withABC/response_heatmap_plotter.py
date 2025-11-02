import matplotlib.pyplot as plt
import numpy as np

def plot_heatmap(parsed_results, plot_type='d'):
    """
    Generates a 2D heatmap to visualize the time-spatial response of a system.
    
    The input data (displacement, velocity, acceleration) is assumed to be 
    in the shape (numDOF, numTimeSteps).
    
    Args:
        parsed_results (dict): Dictionary containing 'time', 'displ', 
                               'velo', and 'accel' arrays.
        plot_type (str): 'd' for displacement, 'v' for velocity, 'a' for acceleration.
    """
        
    time = parsed_results['time']  # shape: (numTimeSteps,)
    displacement = parsed_results['displacement'] # shape: (numDOF, numTimeSteps)
    velocity = parsed_results['velocity']  # shape: (numDOF, numTimeSteps)
    acceleration = parsed_results['acceleration']  # shape: (numDOF, numTimeSteps)

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

    numDOF= z_data.shape[0]
    numTimeSteps = z_data.shape[1]
    
    Z_transposed = z_data.T

    plt.figure(figsize=(10, 6))
    plt.imshow(Z_transposed, 
            aspect='auto',      # Scale correctly along both axes
            cmap='rainbow',     # Rainbow colormap
            origin='lower',     # Keep time increasing upwards
            extent=[ 0, numDOF - 1, time[0], time[-1]])  # Match axes to indices

    plt.colorbar(label = x_label)
    plt.xlabel("Degrees of Freedom (spatial index)")
    plt.ylabel("Time (s)")
    plt.title(f"{x_label} Heat Map (String in Tension)")
    plt.show()


