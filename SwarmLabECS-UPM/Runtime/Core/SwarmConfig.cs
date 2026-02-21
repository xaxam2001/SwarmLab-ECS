using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace SwarmLabECS.Core
{
    public class SwarmConfigAuthoring : MonoBehaviour
    {
        [System.Serializable]
        public struct SpeciesSetup
        {
            public EntityAuthoring prefab;
            public int count;
            public float initialRandomVelocity;
            public float radius;
            public float3 spawnCenter;
        }

        public List<SpeciesSetup> speciesList;

        class Baker : Baker<SwarmConfigAuthoring>
        {
            public override void Bake(SwarmConfigAuthoring authoring)
            {
                Entity containerEntity = GetEntity(TransformUsageFlags.None);

                // Create a Dynamic Buffer (like a List<T>) to hold our species configs
                DynamicBuffer<SpeciesSpawnConfig> buffer = AddBuffer<SpeciesSpawnConfig>(containerEntity);

                foreach (var species in authoring.speciesList)
                {
                    // MAGIC HAPPENS HERE: 
                    // We turn the GameObject Prefab into an Entity Prefab
                    Entity entityPrefab = GetEntity(species.prefab, TransformUsageFlags.Dynamic);

                    buffer.Add(new SpeciesSpawnConfig
                    {
                        prefabEntity = entityPrefab,
                        count = species.count,
                        initialRandomVelocity = species.initialRandomVelocity,
                        spawnRadius = species.radius,
                        spawnCenter = species.spawnCenter
                    });
                }
            }
        }
    }
    
    public struct SpeciesSpawnConfig : IBufferElementData
    {
        public Entity prefabEntity; // The BAKED version of the prefab
        public int count;

        public float initialRandomVelocity;

        public float spawnRadius;
        public float3 spawnCenter;
    }
    
}
