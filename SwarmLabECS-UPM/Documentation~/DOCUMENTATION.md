# SwarmLabECS Documentation

## Overview
SwarmLabECS is a Unity package designed to simplify the creation and management of rule-based particle systems (swarms) within the runtime environment. It allows for the simulation of complex autonomous behaviors such as flocking, following, and avoiding.

Rebuilt from the ground up using Unity's Data-Oriented Technology Stack (DOTS), this version leverages the Burst Compiler and C# Job System to deliver unprecedented performance and modularity.


## Package contents
The package is organized as follows:
- **Runtime**: Contains the core logic for the simulation, cleanly divided into `Authoring` (Data Injectors), `Components` (Pure ECS Data), `Systems` (Burst Jobs), and `Utils` (Math/Grid helpers).
- **Samples**: An example scene to demonstrate a cuick two species boid setup.

## Installation instructions

### Via Package Manager (Git)
1. Open the Unity Package Manager.
2. Click the `+` button in the top left.
3. Select "Add package from git URL..."
4. Paste the following URL: `https://github.com/xaxam2001/SwarmLab-ECS.git?path=/SwarmLabECS-UPM`

### Via Source Code (Local)
If you want to modify the package source code:
1. Download the repository as a ZIP or Clone it.
2. Copy the `SwarmLabECS-UPM` folder into your Unity project's `Packages` folder (or anywhere in your project assets).
3. Open the Unity Package Manager.
4. Click the `+` button in the top left.
5. Select **"Add package from disk..."**.
6. Select the `package.json` file inside the folder you just copied.

## Requirements
- **Unity Version**: 6000.2 or higher.
- **Dependencies**: `com.unity.entities`, `com.unity.burst`, `com.unity.mathematics`, `com.unity.physics`, `naughty attributes`.

## Performance & Architecture
- **Spatial Partitioning**: The legacy O(N²) interaction algorithm has been replaced. SwarmLabECS utilizes a Native Multi-Hash Map to create a 3D Spatial Grid. This reduces neighborhood lookups to $O(N)$ constant time, safely allowing for **30,000+ entities** at stable frame rates.
- **Physics**: The system uses a custom velocity/position integration for horizontal swarm movement, combined with Unity Physics Raycasts for vertical terrain adherence.


## Workflows

### 1. Creating an Entity Template
SwarmLabECS uses a "Template and Injector" architecture. Prefabs act as empty structural shells.
1. Create a 3D Model or simple Mesh GameObject in your scene.
2. Add the `EntityAuthoring` script to it.
3. Drag the GameObject into your Project window to make it a Prefab.

### 2. Setting up the Swarm Manager
The `SwarmManager` is the brain of the simulation and acts as the Data Injector for your templates.
1. Create an empty GameObject in your scene named "SwarmManager".
2. Add the `SwarmManager` and `SwarmConfigAuthoring` components.
3. In the `SwarmConfigAuthoring` inspector, add a new element to the **Species List**.
4. Assign your Prefab to the Species element.

### 3. Configuring Species & Rules
Within the `SwarmConfigAuthoring`, you can define exact parameters for mass-spawning:
- **Spawn Zones**: Choose between perfectly uniform Spherical or Cubic spawn areas.
- **Gravity & Terrain**: Enable `Has Gravity` to utilize the Hovercraft physics system. Entities will cast rays downward to detect Unity Physics colliders, applying Hooke's Law (Spring Strength and Damping) to smoothly glide over uneven terrain.


- **Interaction Matrix**: For each species, you can define specific **Steering Rules** against every other species in the simulation. This allows you to define that "Species A" ignores "Species B" but is strongly attracted to "Species C".
Let's say we want to set up the behavior of Species 0 towards Species 1:
    - **Separation Radius**: From which Radius Species 0 starts to go away from Species 1
    - **Separation Weight**: With which Force Species 0 is repulsed from Species 1
    - **Flocking Radius**: From which Radius Species 0 aligns and keep close to Species 1
    - **Alignment Weight**: With which Force Species 0 try to align with Species 1 direction
    - **COhesion Weight**: With which Force Species 0 try to keep close to Species 1

### 4. Running the Simulation
1. Add an empty Unity subscene into your scene.
2. Create a Prefab and add the `EntityAuthoring` script to it. Ensure it has a Mesh and Material.
2. Create an Empty GameObject in your subscene and add the `SwarmManager` and `SwarmConfigAuthoring` scripts.
3. In the `SwarmConfigAuthoring` component, add a new Species, assign your Prefab, and configure the spawn count, max speed, and interaction rules.
4. Ensure your environment has a Unity Physics Collider attached and is baked into an ECS SubScene (if using Gravity/Raycasting).
5. Enter Play Mode and click **"Generate Swarm"** in the SwarmManager Inspector.

## Samples
The package includes one sample.
This sample contains a simple implementation of a classic two species Boids configuration for a quick hands on.