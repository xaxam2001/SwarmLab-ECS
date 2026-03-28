# SwarmLab-ECS

## Overview
**SwarmLabECS** is a highly optimized, modular Unity package designed for simulating massive, complex swarm behaviors (flocking, schooling, and crowd movement) using Unity's Data-Oriented Technology Stack (DOTS).

Built entirely with the Entity Component System (ECS), the Burst Compiler, and C# Job System, SwarmLabECS can handle tens of thousands of entities at 60+ FPS. It utilizes Spatial Hash Grids for heavily optimized $O(N)$ neighborhood lookups and integrates cleanly with Unity Physics for terrain adherence.

### Key Features
- **Performance**: Simulate 30,000+ boids smoothly via Burst-compiled, multithreaded Jobs and Spatial Partitioning.
- **Interaction Matrix**: Support for multi-species ecosystems. Fine-tune Cohesion, Alignment, and Separation weights independently for how each species reacts to itself and others.
- **Environment Adherence**: Built-in "Hovercraft" physics using Hooke's Law (Spring and Damping) via ECS Raycasts to smoothly snap entities to complex terrain and colliders.
- **Flexible Spawning**: Uniform volumetric spawning supporting both spherical and cubic zones.
- **Data-Oriented Architecture**: Clean separation of Authoring components and ECS Runtime systems, making it highly modular and safe to drop into any project.

## System Requirements
- **Unity Version**: 6000.2 or later.
- **Dependencies**:
    - `com.unity.entities`
    - `com.unity.burst`
    - `com.unity.mathematics`
    - `com.unity.physics`
    - `https://github.com/dbrizov/NaughtyAttributes.git#upm`

## Installation Instructions

### Via Package Manager (Git)
1. Open the Unity Package Manager.
2. Click the `+` button in the top left.
3. Select "Add package from git URL..."
4. Paste the following URL: `https://github.com/xaxam2001/SwarmLab-ECS.git?path=/SwarmLabECS-UPM`
5. **NAUGHTY ATTRIBUTES**: please note that naughty attribute is a necessary dependency for this package you can install via this link: `https://github.com/dbrizov/NaughtyAttributes.git#upm`

### Via Source Code (Local)
If you want to modify the package source code:
1. Download the repository as a ZIP or Clone it.
2. Copy the `SwarmLabECS-UPM` folder into your Unity project's `Packages` folder (or anywhere in your project assets).
3. Open the Unity Package Manager.
4. Click the `+` button in the top left.
5. Select **"Add package from disk..."**.
6. Select the `package.json` file inside the folder you just copied.

## Quick Start
1. Add an empty Unity subscene into your scene.
2. Create a Prefab and add the `EntityAuthoring` script to it. Ensure it has a Mesh and Material.
2. Create an Empty GameObject in your subscene and add the `SwarmManager` and `SwarmConfigAuthoring` scripts.
3. In the `SwarmConfigAuthoring` component, add a new Species, assign your Prefab, and configure the spawn count, max speed, and interaction rules.
4. Ensure your environment has a Unity Physics Collider attached and is baked into an ECS SubScene (if using Gravity/Raycasting).
5. Enter Play Mode and click **"Generate Swarm"** in the SwarmManager Inspector.

## Documentation
See [DOCUMENTATION.md](Documentation~/DOCUMENTATION.md) for full details on ECS workflows, component data structures, and advanced configuration.