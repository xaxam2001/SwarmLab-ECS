using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace SwarmLabECS.Core
{
    public class SwarmConfigAuthoring : MonoBehaviour
    {
        [System.Serializable]
        public class RuleSetup
        {
            public string targetSpeciesName;
    
            public float separationRadius;
            public float flockingRadius;
    
            public float separationWeight;
            public float alignmentWeight;
            public float cohesionWeight;
        }
        
        [System.Serializable]
        public class SpeciesSetup
        {
            public EntityAuthoring prefab;
            public float initialRandomVelocity;
            public int count;
            public float radius;
            public float3 spawnOffset;
            
            [Header("Rules Against Other Species")]
            public List<RuleSetup> rules;
        }

        public List<SpeciesSetup> speciesList;

        class Baker : Baker<SwarmConfigAuthoring>
        {
            public override void Bake(SwarmConfigAuthoring authoring)
            {
                Entity containerEntity = GetEntity(TransformUsageFlags.None);

                // Create a Dynamic Buffer (like a List<T>) to hold our species configs
                DynamicBuffer<SpeciesSpawnConfig> spawnBuffer = AddBuffer<SpeciesSpawnConfig>(containerEntity);

                DynamicBuffer<InteractionRule> interactionBuffer = AddBuffer<InteractionRule>(containerEntity);
                
                AddComponent(containerEntity, new SwarmGlobalSettings 
                { 
                    TotalSpeciesCount = authoring.speciesList.Count 
                });

                foreach (var species in authoring.speciesList)
                {
                    if (species.prefab == null)
                    {
                        Debug.LogWarning("Please assign a prefab to each species in the SwarmConfig component.");
                        continue;
                    }
                    
                    // We turn the GameObject Prefab into an Entity Prefab
                    Entity entityPrefab = GetEntity(species.prefab, TransformUsageFlags.Dynamic);

                    spawnBuffer.Add(new SpeciesSpawnConfig
                    {
                        PrefabEntity = entityPrefab,
                        Count = species.count,
                        InitialRandomVelocity = species.initialRandomVelocity,
                        SpawnRadius = species.radius,
                        SpawnOffset = species.spawnOffset
                    });
                    
                    foreach (var rule in species.rules)
                    {
                        interactionBuffer.Add(new InteractionRule
                        {
                            SeparationRadius = rule.separationRadius,
                            FlockingRadius = rule.flockingRadius,
                            SeparationWeight = rule.separationWeight,
                            AlignmentWeight = rule.alignmentWeight,
                            CohesionWeight = rule.cohesionWeight
                        });
                    }
                }
            }
        }
        
        private void OnValidate()
        {
            if (speciesList == null) return;

            int totalSpecies = speciesList.Count;

            for (int i = 0; i < totalSpecies; i++)
            {
                var species = speciesList[i];
                if (species == null) continue;

                if (species.rules == null) species.rules = new List<RuleSetup>();

                // 1. Force the list length to match the total number of species
                while (species.rules.Count < totalSpecies)
                {
                    species.rules.Add(new RuleSetup());
                }
                while (species.rules.Count > totalSpecies)
                {
                    species.rules.RemoveAt(species.rules.Count - 1);
                }

                // 2. Auto-name the rules so the user can't mess up the order
                for (int targetIndex = 0; targetIndex < totalSpecies; targetIndex++)
                {
                    string targetName = "Unknown";
                    if (speciesList[targetIndex] != null && speciesList[targetIndex].prefab != null)
                    {
                        targetName = speciesList[targetIndex].prefab.name;
                    }
                    
                    // Update the label. E.g., "vs Bird" or "vs Bee"
                    species.rules[targetIndex].targetSpeciesName = $"{targetName} (ID: {targetIndex})";
                }
            }
        }
    }
    
    public struct SpeciesSpawnConfig : IBufferElementData
    {
        public Entity PrefabEntity; // The BAKED version of the prefab
        public int Count;

        public float InitialRandomVelocity;

        public float SpawnRadius;
        public float3 SpawnOffset;
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
    }
    
}
