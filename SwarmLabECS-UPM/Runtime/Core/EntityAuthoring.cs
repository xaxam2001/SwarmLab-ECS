using Unity.Mathematics;
using Unity.Entities;
using UnityEngine;

namespace SwarmLabECS.Core
{
    public class EntityAuthoring : MonoBehaviour
    {
        [SerializeField] private SpeciesDefinition speciesDefinition;
        
        private class EntityBaker : Baker<EntityAuthoring>
        {
            public override void Bake(EntityAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent(entity, new BoidTag());
                AddComponent(entity, new NewEntity()); // adding the new tag (will be disabled when spawned by the spawning system
                AddComponent(entity, new BoidSpawnSetup()); // associated spawning data
                
                AddComponent(entity, new EntityVelocity
                {
                    value = float3.zero
                });

                if (authoring.speciesDefinition != null)
                {
                    AddComponent(entity, new EntitySettings
                    {
                        maxSpeed = authoring.speciesDefinition.maxSpeed
                    });
                }
            }
        }   
    }
    
    public struct EntityVelocity : IComponentData
    {
        public float3 value;
    }
    
    public struct EntitySettings : IComponentData
    {
        public float maxSpeed;
    }
    
    public struct BoidTag : IComponentData {}
    
    public struct NewEntity : IComponentData, IEnableableComponent {}
    
    // use to fast spawning for the different species
    public struct BoidSpawnSetup : IComponentData
    {
        public float3 center;
        public float radius;
        public float initialSpeed;
    }
}