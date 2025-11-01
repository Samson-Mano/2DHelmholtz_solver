import numpy as np
import matplotlib.pyplot as plt
import initial_condition_profile as inlcond

# Basic input parameters
node_count = 100 # number of nodes
wire_tension = 100 # tension in Newton
wire_length = 100 #  length in mm
wire_density = 64 # wire density tons/mm
damping_ratio = 0.015 # damping ratio




# Displacement intial condition
inl_displ = inlcond.InitialConditionData(total_nodes=node_count)
inl_displ.set_initial_condition_profile(node_start=20, node_end=80, inl_cond_val=1.0)
# inl_displ.plot_profile()

# Velocity initial conidtion
inl_velo = inlcond.InitialConditionData(total_nodes=node_count)
inl_velo.set_initial_condition_profile(node_start=40, node_end=55, inl_cond_val=2.0)
# inl_velo.plot_profile()

# Pulse Force profile
force_start_time = 0.0
force_end_time = 3.0
force_profile = inlcond.InitialConditionData(total_nodes=node_count)
force_profile.set_initial_condition_profile(node_start=40, node_end=55, inl_cond_val=0.0)


# --- Derived Parameters ---
N = node_count
L = wire_length
T = wire_tension
mu = wire_density
c = np.sqrt(T / mu) # wave speed
l = L / (N - 1) # Element length

# ----------------------------------------------
# Create the Global Consistent Mass Matrix (M)
# ----------------------------------------------
M = np.zeros((N, N))
M_coeff = mu * l / 6.0 # mu*l/6

for i in range(N):
    # Diagonal terms
    if i == 0 or i == N - 1:
        M[i, i] = 2.0 * M_coeff # End nodes (2/6 contribution)
    else:
        M[i, i] = 4.0 * M_coeff # Internal nodes (4/6 contribution)

    # Off-diagonal terms
    if i < N - 1:
        M[i, i + 1] = 1.0 * M_coeff # Upper diagonal (1/6 contribution)
    if i > 0:
        M[i, i - 1] = 1.0 * M_coeff # Lower diagonal (1/6 contribution)


# ----------------------------------------------
# Create the Global Stiffness Matrix (K)
# ----------------------------------------------
K = np.zeros((N, N))
K_coeff = T / l

for i in range(N):
    # Diagonal terms
    if i == 0 or i == N - 1:
        K[i, i] = 1.0 * K_coeff # End nodes (1/l contribution)
    else:
        K[i, i] = 2.0 * K_coeff # Internal nodes (2/l contribution)

    # Off-diagonal terms
    if i < N - 1:
        K[i, i + 1] = -1.0 * K_coeff # Upper diagonal (-1/l contribution)
    if i > 0:
        K[i, i - 1] = -1.0 * K_coeff # Lower diagonal (-1/l contribution)

print("Mass Matrix M (3x3 corner):")
print(M[:3, :3])
print("\nStiffness Matrix K (3x3 corner):")
print(K[:3, :3])



# ----------------------------------------------
# Create the ABC Damping Matrix (C_ABC)
# ----------------------------------------------
C_ABC = np.zeros((N, N))

# Boundary Contribution (Coefficient 1/c)
C_coeff = 1.0 / c 

# At x=0 (Node 0): C[0, 0]
C_ABC[0, 0] = C_coeff 

# At x=L (Node N-1): C[N-1, N-1]
C_ABC[N-1, N-1] = C_coeff

print("\nABC Damping Matrix C_ABC (sparse):")
print(C_ABC[0, 0], C_ABC[N-1, N-1])






