using SwarmLabECS.Components;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;
using Random = Unity.Mathematics.Random;

namespace SwarmLabECS.Systems
{
    [BurstCompile]
    public partial struct SwarmSpawnSystem : ISystem
    {
        // caching the query even tho it doesn't impact performance that much here but just for good practice
        private EntityQuery _oldBoidsQuery;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SpeciesSpawnConfig>();
            state.RequireForUpdate<SpawnSwarmRequest>(); 
            
            // Create a query that finds every entity with a BoidTag
            _oldBoidsQuery = SystemAPI.QueryBuilder().WithAll<BoidTag>().Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {            
            // DESTROY HAS TO BE DONE BEFORE GETTING THE SPAWN BUFFER BC IT'S A STRUCTURAL CHANGE
            // Nuke all in one single, highly optimized call
            state.EntityManager.DestroyEntity(_oldBoidsQuery);
            
            DynamicBuffer<SpeciesSpawnConfig> spawnBuffer = SystemAPI.GetSingletonBuffer<SpeciesSpawnConfig>();
            
            // copy to a temp array to avoid structural changes
            using NativeArray<SpeciesSpawnConfig> safeArray = spawnBuffer.ToNativeArray(Allocator.Temp);
            
            // Ensure the seed is never 0 (Random throws an error if the seed is 0)
            uint baseSeed = (uint)(SystemAPI.Time.ElapsedTime * 1000.0) + 1u;

            int speciesID = 0;
            
            foreach (var species in safeArray)
            {
                if (species.Count <= 0) continue;
                
                // THE TRICK: We overwrite the component on the PREFAB itself.
                state.EntityManager.SetComponentData(species.PrefabEntity, new BoidSpawnSetup
                {
                    UseCubeSpawn = species.UseCubeSpawnZone,
                    Center = species.SpawnCenter,
                    Radius = species.SpawnRadius,
                    CubeSize = species.SpawnCubeSize,
                    InitialSpeed = species.InitialRandomVelocity
                });

                if (species.HasGravity)
                {
                    if (!state.EntityManager.HasComponent<EntityGravity>(species.PrefabEntity)) // extra safety check
                    {
                        state.EntityManager.AddComponent<EntityGravity>(species.PrefabEntity);
                    }
                    state.EntityManager.SetComponentData(species.PrefabEntity, new EntityGravity
                    {
                        Value = species.GravityValue,
                        RaycastStartHeight = species.RaycastStartHeight,
                        RaycastLength = species.RaycastLength,
                        HoverHeight = species.HoverHeight,
                        SpringStrength = species.SpringStrength,
                        Damping = species.Damping
                    });
                }
    
                // 3. Write the modified struct back to the prefab
                state.EntityManager.SetComponentData(species.PrefabEntity, new EntitySettings
                {
                    SpeciesId = speciesID,
                    MaxSpeed = species.MaxSpeed
                });
                
                // The 'using' keyword ensures .Dispose() is called automatically when the function ends (like a destructor).
                // this prevents memory leaks
                using var tempInstances = state.EntityManager.Instantiate(species.PrefabEntity, species.Count, Allocator.Temp);

                speciesID += 1;
            }
            
            // We schedule the job exactly ONCE, outside the loop.
            // It will find ALL new entities across all species and process them.
            var job = new InitializeBoidsJob
            {
                BaseSeed = baseSeed
            };

            job.ScheduleParallel();
            
            Entity requestEntity = SystemAPI.GetSingletonEntity<SpawnSwarmRequest>();
            state.EntityManager.DestroyEntity(requestEntity);
        }
    }
    
    [BurstCompile]
    internal partial struct InitializeBoidsJob : IJobEntity
    {
        public uint BaseSeed;

        private void Execute([EntityIndexInQuery] int index,
                            ref LocalTransform transform,
                            ref EntityVelocity entityVelocity,
                            in BoidSpawnSetup setup, // Read-only access to the spawn data
                            EnabledRefRW<NewEntity> newEntityTag)
        {
            var random = Random.CreateFromIndex(BaseSeed + (uint)index);

            float3 randomOffset;
            if (setup.UseCubeSpawn)
            {
                float3 halfSize = setup.CubeSize * 0.5f;
                randomOffset = random.NextFloat3(-halfSize, halfSize);
            }
            else
            {
                /* Here, the uniform randomness will cause the boids to cluster slightly more into the center of the
                 sphere. Not especially an issue, but for true uniform random the cube root of the random number can
                 be used */
                // CODE UPDATED TO TRUE UNIFORM DISTRIB IN A SPHERE
                float u = random.NextFloat(0f, 1f);
                float randomDistance = setup.Radius * math.pow(u, 1.0f / 3.0f); 
                randomOffset = random.NextFloat3Direction() * randomDistance;
            }
            // Write the data directly to the ECS memory chunks
            transform.Position = setup.Center + randomOffset;

            entityVelocity.Value = random.NextFloat3Direction() * setup.InitialSpeed;

            newEntityTag.ValueRW = false;
        }
    }
    
}