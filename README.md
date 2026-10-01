# 2D Helmholtz Solver

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-Framework%2F.NET-blue)](https://dotnet.microsoft.com/)
[![C++](https://img.shields.io/badge/C%2B%2B-20-blue.svg)](https://isocpp.org/)
[![OpenTK](https://img.shields.io/badge/OpenTK-3.3-green)](https://opentk.net/)

A C# front-end for a 2D Helmholtz spectral element solver written in C++. The solver supports higher-order spectral elements up to **10th order**, uses **Abaqus-style input**, and provides **interactive OpenTK visualization**. A dedicated **modal analysis module** visualizes mode shapes of 2D domains. Applications include acoustics, electromagnetics, and related wave propagation fields.

---

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Visualization Controls](#visualization-controls)
- [Visualization](#visualization)
- [How to Use the Software](#how-to-use-the-software)
  - [Step 1: Importing the Model](#step-1-importing-the-model)
  - [Step 2: Applying Nodal or Edge Constraints](#step-2-applying-nodal-or-edge-constraints)
  - [Step 3: Material Update](#step-3-material-update)
  - [Step 4: Helmholtz Solve](#step-4-helmholtz-solve)
  - [Step 5: Post-Processing](#step-5-post-processing)
- [Examples](#examples)
- [Modal Analysis Module](#modal-analysis-module)
- [Build Instructions](#build-instructions)
- [Repository](#repository)
- [License](#license)

---

## Overview

This repository contains a C# implementation of a 2D Helmholtz solver that acts as a front-end for:

- Model import
- Node and edge constraint application
- Material assignment
- Post-processing of results

The numerical solver is developed in **C++** and linked to the C# front-end via a **DLL**. A **modal analysis module** is also included to visualize the mode shapes of 2D domains.

The solver uses a **spectral element formulation** for both **Quad** and **Tria** elements, supporting spectral orders up to **10**. It supports both **elimination** and **Lagrange augmentation** methods for constraint enforcement.

Input models follow the **Abaqus format**, and example models are provided in the repository.

Post-processing allows visualization of:
- Real field value
- Imaginary field value
- Magnitude of field
- Phase of field
- Mode shape visualization via modal analysis

---

## Features

- Spectral element formulation of the 2D Helmholtz equation
- Higher-order spectral elements up to **order 10**
- **Absorbing boundary conditions (ABC)** at edges (first-order)
- **Abaqus-style input** mesh format
- Interactive visualization using **OpenTK 3.3**
- Post-processing of field values and mode shapes
- Modal analysis module using **ARPACK**

---

## Visualization Controls

| Action                      | Shortcut                  |
| --------------------------- | ------------------------- |
| Zoom In / Out               | `Ctrl` + Scroll Wheel     |
| Pan                         | `Ctrl` + Right Click Drag |
| Zoom to Fit                 | `Ctrl` + `F`              |
| Select Nodes / Elements     | `Shift` + Left Click Drag |
| Deselect / Refine Selection | `Shift` + Right Click Drag|

---

## Visualization

- OpenTK 3.3 based rendering
- Contour plots of field values

---

## How to Use the Software

### Step 1: Importing the Model
The model format is in Abaqus `*.inp` format. Example models are located in the repository at:

```
/2DHelmholtz_solver/Example_model/
```

The mesh text file (`*.txt`) follows the format shown below:

```
**
**   Template:  2D Helmholtz Solver
**
*NODE
         1,  -100.0   ,  -100.0   ,  0.0
         2,   100.0   ,  -100.0   ,  0.0
         3,   100.0   ,   100.0   ,  0.0
         4,  -100.0   ,   100.0   ,  0.0
         ......

*ELEMENT,TYPE=S4
        12,    23,    13,     7,    20
        13,    18,    23,    20,     9
        14,     9,    20,    24,    21
        15,    20,     7,    14,    24
        .......

*ELEMENT,TYPE=S3
         0,     0,     1,     2
         1,     2,     1,     3
         3,     2,     3,     4
         .......
```

### Step 2: Applying Nodal or Edge Constraints

Open the **Node Constraint** form or **Edge Constraint** form from the **Constraint** menu.

- Use `Shift` + Left Click Drag to select nodes/edges.
- Use `Shift` + Right Click to deselect/refine the selection.
- Apply the prescribed field values at the nodes or edges.
- Apply first-order absorbing boundary condition (ABC) at the edges.
- Apply source values at the nodes.

### Step 3: Material Update

A default material is applied to the mesh. A new material can be created and applied to meshes by selecting/deselecting using `Shift` + Left Click Drag and `Shift` + Right Click.

> **Note:** Deleting a material will revert the mesh it was applied to back to the default material.

### Step 4: Helmholtz Solve

The Helmholtz solver window is accessed through the **Solve** menu. The solver window allows the DLL connection to the C++ solver to perform the solve.

- Select the spectral order — up to **order 10** is available.
- Input the wave field frequency.
- Optionally select **Extend the constraints intermediate spectral nodes**.

> **Note:** Selecting a higher spectral order will increase the computing cost. Be mindful when selecting the spectral order.

### Step 5: Post-Processing

**Solve → Show Results** menu allows the selection of various result options:

- Real Field Value
- Imaginary Field Value
- Magnitude of Field Value
- Phase of Field Value

**Update Contour Range** option allows the contour plot maximum and minimum range to be adjusted. Concentrated field values at a few nodes might skew the contour plot and show a uniform color in most locations. By adjusting the maximum and minimum range (which varies between `1.0` and `0.0`), for example by selecting `0.8` for maximum contour range, values above `0.8 × maximum` will not be plotted, allowing for better visualization of the field values.

> **Tip:** `Help → General Instruction` gives a few more instructions on how to use the tool.

---

## Examples

### Problem 1: Circular Domain Scattering

![Problem 1 Model](Images/prob1_circlescattering_model.png)
![Problem 1 Real Field](Images/prob1_circlescattering_realfield.png)
![Problem 1 Magnitude Field](Images/prob1_circlescattering_magfield.png)

### Problem 2: Helmholtz Resonator

![Problem 2 Model](Images/prob2_helmholtzresonnator_model.png)
![Problem 2 Real Field](Images/prob2_helmholtzresonnator_fieldvalues.png)

### Problem 3: Cascading Amplifier

![Problem 3 Model](Images/prob3_cascadingamplifier_model.png)
![Problem 3 Real Field](Images/prob3_cascadingamplifier_fieldvalue.png)
![Problem 3 Magnitude Field](Images/prob3_cascadingamplifier_magvalue.png)

### Problem 4: Gabriel Horn Amplifier

![Problem 4 Model](Images/prob4_gabehorn_model.png)
![Problem 4 Real Field](Images/prob4_gabehorn_fieldvalues.png)

### Problem 5: Double Slit

![Problem 5 Model](Images/prob5_doubleslit_model.png)
![Problem 5 Real Field](Images/prob5_doubleslit_fieldvalue.png)
![Problem 5 Magnitude Field](Images/prob5_doubleslit_magvalue.png)

### Problem 6: Silencer Trap

![Problem 6 Model](Images/prob6_silencertrap_model.png)
![Problem 6 Real Field](Images/prob6_silencertrap_fieldvalues.png)

---

## Modal Analysis Module

A dedicated modal analysis module is included to perform modal analysis of the domain.

- **Solve → 2D Modal Solve** allows access to the modal analysis DLL package, which uses **ARPACK** to perform the modal analysis.
- **Show Results → Modal Results** allows visualization of the modal results.
- **Mode Result Settings** option allows changing the mode shapes and animation control.

Below are some of the **Chladni-like patterns** generated using the modal analysis module:

### Circle

![Chladni Circle](Images/chladni_circle.png)

### Ellipse

![Chladni Ellipse 1](Images/chladni_ellipse_1.png)
![Chladni Ellipse 2](Images/chladni_ellipse_2.png)

### Triangle

![Chladni Triangle 1](Images/chladni_triangle_1.png)
![Chladni Triangle 2](Images/chladni_triangle_2.png)

### Square

![Chladni Square 1](Images/chladni_square_1.png)
![Chladni Square 2](Images/chladni_square_2.png)

### Pentagon

![Chladni Pentagon 1](Images/chladni_pentagon_1.png)
![Chladni Pentagon 2](Images/chladni_pentagon_2.png)

### Hexagon

![Chladni Hexagon](Images/chladni_hexagon.png)

---

## Build Instructions

**1. Clone the repository:**

```bash
git clone https://github.com/Samson-Mano/2DHelmholtz_solver.git
cd 2DHelmholtz_solver
```

**2. Download the pre-built software:**

Use [https://download-directory.github.io/](https://download-directory.github.io/) and download the `Release` folder to run the software:

```
2DHelmholtz_solver/2DHelmholtz_solver/bin/x64/Release/
```

---

## Repository

- **GitHub:** [https://github.com/Samson-Mano/2DHelmholtz_solver](https://github.com/Samson-Mano/2DHelmholtz_solver)

---

## License

This project is licensed under the **MIT License**.

```
MIT License

Copyright (c) 2025 Samson Mano

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```




