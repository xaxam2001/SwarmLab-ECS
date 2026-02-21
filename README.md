# SwarmLab

## Overview
**SwarmLab** is a modular and reliable Unity package designed for simulating complex swarm behaviors like flocking, schooling, and leader following. It operates effectively in **Runtime**, offering a flexible rule-based system.

<p align="center">
    <img src="SwarmLab-UPM/Documentation~/demo-aquarium.gif" alt="Demo of the sample project" width="50%" />
    <br>  
    <em>Demonstration of 3D sample - Aquarium Demo (classic boids)</em>
    <br>
    <img src="SwarmLab-UPM/Documentation~/demo-zombies.gif" alt="Demo of the sample project" width="50%" />
    <br>
    <em>Demonstration of 2D sample - Zombies Demo (Prey VS Predator Boids)</em>
</p>

### Key Features
- **Adjustable Behaviors**: Fine-tune customizable steering rules to create unique swarm dynamics.
- **Multi-Species Support**: Create complex ecosystems where different species interacting with custom weights.
- **Dual Simulation Modes**: seamless support for **Volumetric (3D)** and **Planar (2D)** environments.
- **Extensible Architecture**: Easily add custom `SteeringRule` classes to define unique behaviors.

## System Requirements
- **Unity Version**: 6000.2 or later.


## Installation instructions
### Via Package Manager (Git)
1. Open the Unity Package Manager.
2. Click the `+` button in the top left.
3. Select "Add package from git URL..."
4. Paste the following URL: `https://github.com/xaxam2001/SwarmLab.git?path=/SwarmLab-UPM`

### Via Source Code (Local)
If you want to modify the package source code:
1. Download the repository as a ZIP or Clone it.
2. Copy the `SwarmLab-UP` folder into your Unity project's `Packages` folder (or anywhere in your project assets).
3. Open the Unity Package Manager.
4. Click the `+` button in the top left.
5. Select **"Add package from disk..."**.
6. Select the `package.json` file inside the `SwarmLab-UP` folder you just copied.


Documentation
---
See [Documentation](SwarmLab-UPM/Documentation~/DOCUMENTATION.md) for full details on workflows, configuration.
