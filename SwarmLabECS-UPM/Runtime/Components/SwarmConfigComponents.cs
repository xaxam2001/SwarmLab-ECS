using Unity.Entities;
using Unity.Mathematics;

namespace SwarmLabECS.Components
{    
    public struct SpeciesSpawnConfig : IBufferElementData
    {
        public Entity PrefabEntity; // The BAKED version of the prefab
        public int Count;
        public float InitialRandomVelocity;
        
        // Speed related param
        public float MaxSpeed;

        // Spawn area related params
        public bool UseCubeSpawnZone;
        public float SpawnRadius;
        public float3 SpawnCubeSize;
        public float3 SpawnCenter;

        // Gravity related params
        public bool HasGravity;
        
        public float GravityValue;
        public float RaycastStartHeight;
        public float RaycastLength;
        
        // Hooke's Law settings
        public float HoverHeight;
        public float SpringStrength;
        public float Damping;
    }
    
    public struct InteractionRule : IBufferElementData
    {
        public float SeparationRadius;
        public float FlockingRadius;
    
        public float SeparationWeight;
        public float AlignmentWeight;
        public float CohesionWeight;
    }
    
    public struct SwarmGlobalSettings : IComponentData
    {
        public int TotalSpeciesCount;
        public float CellSize;
    }
    
    public struct SpawnSwarmRequest : IComponentData
    {
    }
}