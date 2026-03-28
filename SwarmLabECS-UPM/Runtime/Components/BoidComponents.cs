using Unity.Entities;
using Unity.Mathematics;

namespace SwarmLabECS.Components
{
    public struct EntityGravity : IComponentData
    {
        public float Value;
        public float RaycastStartHeight;
        public float RaycastLength;
        
        // Hooke's Law settings
        public float HoverHeight;
        public float SpringStrength;
        public float Damping;
    }
    
    public struct EntityVelocity : IComponentData
    {
        public float3 Value;
    }
    
    public struct EntitySettings : IComponentData
    {
        public int SpeciesId;
        public float MaxSpeed;
    }
    
    public struct BoidTag : IComponentData {}
    
    public struct NewEntity : IComponentData, IEnableableComponent {}
    
    // use to fast spawning for the different species
    public struct BoidSpawnSetup : IComponentData
    {
        public bool UseCubeSpawn;
        public float3 Center;
        public float Radius;
        public float3 CubeSize;
        public float InitialSpeed;
    }
}