using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;

namespace SwarmLabECS.Core
{
    [BurstCompile]
    public partial struct SwarmSpawnSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SpeciesSpawnConfig>();
            state.RequireForUpdate<SpawnSwarmRequest>(); 
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {            
            // DESTROY HAS TO BE DONE BEFORE GETTING THE SPAWN BUFFER BC IT'S A STRUCTURAL CHANGE
            // Create a query that finds every entity with a BoidTag
            EntityQuery oldBoidsQuery = SystemAPI.QueryBuilder().WithAll<BoidTag>().Build();
            
            // Nuke all in one single, highly optimized call
            state.EntityManager.DestroyEntity(oldBoidsQuery);
            
            DynamicBuffer<SpeciesSpawnConfig> spawnBuffer = SystemAPI.GetSingletonBuffer<SpeciesSpawnConfig>();
            
            // copy to a temp array to avoid structural changes
            using NativeArray<SpeciesSpawnConfig> safeArray = spawnBuffer.ToNativeArray(Allocator.Temp);
            
            // Ensure the seed is never 0 (Random throws an error if the seed is 0)
            uint baseSeed = (uint)(SystemAPI.Time.ElapsedTime * 1000.0) + 1u;

            foreach (var species in safeArray)
            {
                if (species.count <= 0) continue;
                
                // THE TRICK: We overwrite the component on the PREFAB itself.
                state.EntityManager.SetComponentData(species.prefabEntity, new BoidSpawnSetup
                {
                    center = species.spawnCenter,
                    radius = species.spawnRadius,
                    initialSpeed = species.initialRandomVelocity
                });
                
                // The 'using' keyword ensures .Dispose() is called automatically when the function ends (like a destructor).
                // this prevents memory leaks
                using var tempInstances = state.EntityManager.Instantiate(species.prefabEntity, species.count, Allocator.Temp);

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

            /* Here, the uniform randomness will cause the boids to cluster slightly more into the center of the
             sphere. Not especially an issue, but for true uniform random the cube root of the random number can
             be used */
            float randomDistance = random.NextFloat(0f, setup.radius);
            float3 randomOffset = random.NextFloat3Direction() * randomDistance;

            // Write the data directly to the ECS memory chunks
            transform.Position = setup.center + randomOffset;

            entityVelocity.value = random.NextFloat3Direction() * setup.initialSpeed;

            newEntityTag.ValueRW = false;
        }
    }
    
    public struct SpawnSwarmRequest : IComponentData
    {
    }
}