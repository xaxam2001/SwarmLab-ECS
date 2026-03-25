using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace SwarmLabECS.Core
{
    
    [BurstCompile]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(BuildPhysicsWorld))]
    public partial struct MovementSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BoidTag>();
            state.RequireForUpdate<SwarmGlobalSettings>();
            state.RequireForUpdate<PhysicsWorldSingleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            PhysicsWorldSingleton physicsSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

            ComponentLookup<EntityGravity>
                gravityLookup = SystemAPI.GetComponentLookup<EntityGravity>(isReadOnly: true);
            
            // getting the Interaction Matrix between the different species
            NativeArray<InteractionRule> interactionMatrixFlatten = SystemAPI.GetSingletonBuffer<InteractionRule>()
                .ToNativeArray(Allocator.TempJob);
            
            // getting all the transforms and all the boids setting (for the specie of each boids)
            EntityQuery boidQuery = SystemAPI.QueryBuilder().WithAll<BoidTag, LocalTransform, EntitySettings, EntityVelocity>().Build();
            
            NativeArray<LocalTransform> allTransforms = boidQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob);
            NativeArray<EntitySettings> allSettings = boidQuery.ToComponentDataArray<EntitySettings>(Allocator.TempJob);
            
            // getting a snapshot of all the boids velocity
            NativeArray<EntityVelocity> allVelocities = boidQuery.ToComponentDataArray<EntityVelocity>(Allocator.TempJob);
            
            int totalSpeciesCount = SystemAPI.GetSingleton<SwarmGlobalSettings>().TotalSpeciesCount;
            
            float cellSize = 3f; 
            GridHash gridHash = new GridHash(cellSize);

            // make a spatial map for each boid
            int boidCount = allTransforms.Length;
            var spatialMap = new NativeParallelMultiHashMap<int, int>(boidCount, Allocator.TempJob);

            var hashJob = new HashPositionsJob()
            {
                Positions = allTransforms,
                GridHash = gridHash,
                SpatialMap = spatialMap.AsParallelWriter()
            };

            JobHandle hashHandle = hashJob.Schedule(boidCount, 64, state.Dependency);
            
            var movementJob = new MovementJob()
            {
                InteractionMatrixFlatten = interactionMatrixFlatten,
                AllTransforms = allTransforms,
                AllSettings = allSettings,
                AllVelocities = allVelocities,
                DeltaTime = SystemAPI.Time.DeltaTime,
                TotalSpeciesCount = totalSpeciesCount,
                CollisionWorld = physicsSingleton.CollisionWorld,
                GravityLookup = gravityLookup,
                
                SpatialMap = spatialMap,
                GridHash = gridHash
            };

            // schedule the movementJob but this makes it dependent on the hashJob
            state.Dependency = movementJob.ScheduleParallel(hashHandle);
            spatialMap.Dispose(state.Dependency);
        }
    }

    [BurstCompile]
    internal partial struct MovementJob : IJobEntity
    {
        [ReadOnly, DeallocateOnJobCompletion] public NativeArray<InteractionRule> InteractionMatrixFlatten;
        [ReadOnly, DeallocateOnJobCompletion] public NativeArray<LocalTransform> AllTransforms;
        [ReadOnly, DeallocateOnJobCompletion] public NativeArray<EntitySettings> AllSettings;
        [ReadOnly, DeallocateOnJobCompletion] public NativeArray<EntityVelocity> AllVelocities;
        [ReadOnly] public NativeParallelMultiHashMap<int, int> SpatialMap;
        [ReadOnly] public GridHash GridHash;
        [ReadOnly] public CollisionWorld CollisionWorld;
        [ReadOnly] public ComponentLookup<EntityGravity> GravityLookup;
        public float DeltaTime;
        public int TotalSpeciesCount;
        
        private void Execute( Entity entity,
            ref LocalTransform transform,
            ref EntityVelocity entityVelocity,
            in EntitySettings entitySettings)
        {
            float3 force = CalculateForce(transform, entityVelocity, entitySettings.SpeciesId, entitySettings.MaxSpeed);

            if (GravityLookup.HasComponent(entity))
            {
                EntityGravity gravity = GravityLookup[entity]; // Extract the data safely
            
                force.y = 0f; // Override Y so swarm doesn't pull them into the sky
                entityVelocity.Value += force * DeltaTime;

                // Setup the Raycast
                float3 rayStart = transform.Position + new float3(0, gravity.RaycastStartHeight, 0);
                float3 rayDirection = new float3(0, -gravity.RaycastLength, 0);
            
                RaycastInput rayInput = new RaycastInput
                {
                    Start = rayStart,
                    End = rayStart + rayDirection,
                    Filter = CollisionFilter.Default
                };

                // Fire the Raycast
                if (CollisionWorld.CastRay(rayInput, out RaycastHit hit))
                {
                    float currentHeightFromGround = transform.Position.y - hit.Position.y;
                    float displacement = currentHeightFromGround - gravity.HoverHeight;
                    float springForce = -gravity.SpringStrength * displacement;
                    float dampingForce = -gravity.Damping * entityVelocity.Value.y;

                    entityVelocity.Value.y += (springForce + dampingForce) * DeltaTime;
                }
                else
                {
                    entityVelocity.Value.y -= gravity.Value * DeltaTime;
                }
            }
            else
            {
                entityVelocity.Value += force * DeltaTime;
            }
            
            float velMagnitude = math.lengthsq(entityVelocity.Value);
            
            if (velMagnitude > entitySettings.MaxSpeed * entitySettings.MaxSpeed)
                entityVelocity.Value = math.normalize(entityVelocity.Value) * entitySettings.MaxSpeed;

            transform.Position += entityVelocity.Value * DeltaTime;

            if (velMagnitude > 0.1)
                transform.Rotation = quaternion.LookRotationSafe(entityVelocity.Value, new float3(0,1,0));
        }

        private float3 CalculateForce(LocalTransform myTransform, EntityVelocity myVelocity, int mySpeciesId, float maxSpeed)
        {
            
            float3 centerOfMass = float3.zero;
            float3 averageVelocity = float3.zero;
            float3 separationPush = float3.zero;
            
            int cohesionCount = 0;
            int alignmentCount = 0;
            int separationCount = 0;
            
            float cohesionWeightSum = 0f;
            float alignmentWeightSum = 0f;
            float separationWeightSum = 0f;
            
            int3 myGridPos = GridHash.GetGridPos(myTransform.Position);

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    for (int z = -1; z <= 1; z++)
                    {
                        // Calculate the exact coordinate of the cube we are checking right now
                        int3 neighborGridPos = new int3(myGridPos.x + x, myGridPos.y + y, myGridPos.z + z);
                        
                        // Get the hashkey for this specific cube
                        int cellHash = GridHash.Hash(neighborGridPos);
                        
                        // try to get the first value of the hash (this method also init the iterator
                        if (SpatialMap.TryGetFirstValue(cellHash, out int neighborIndex, out var iterator))
                        {
                            do
                            {

                                LocalTransform neighborTransform = AllTransforms[ neighborIndex];
                                EntityVelocity neighborVelocity = AllVelocities[neighborIndex];
                                int neighborSpecies = AllSettings[neighborIndex].SpeciesId;

                                float distSq = math.distancesq(myTransform.Position, neighborTransform.Position);

                                // if distance is very small then it's us
                                if (distSq < 0.001f) continue;

                                InteractionRule rule =
                                    InteractionMatrixFlatten[(mySpeciesId * TotalSpeciesCount) + neighborSpecies];

                                if (distSq < (rule.FlockingRadius * rule.FlockingRadius))
                                {
                                    // ========== COHESION ===========
                                    if (rule.CohesionWeight > 0)
                                    {
                                        centerOfMass += neighborTransform.Position;
                                        cohesionWeightSum += rule.CohesionWeight;
                                        cohesionCount++;
                                    }

                                    // ========= ALIGNMENT ============
                                    if (rule.AlignmentWeight > 0)
                                    {
                                        averageVelocity += neighborVelocity.Value;
                                        alignmentWeightSum += rule.AlignmentWeight;
                                        alignmentCount++;
                                    }
                                }

                                // ======== SEPARATION ===========
                                if (distSq < (rule.SeparationRadius * rule.SeparationRadius))
                                {
                                    if (rule.SeparationWeight > 0)
                                    {
                                        float3 diff = myTransform.Position - neighborTransform.Position;
                                        // The direction is weighted by how close they are (diff / distSq)
                                        separationPush += (diff / distSq);
                                        separationWeightSum += rule.SeparationWeight;
                                        separationCount++;
                                    }
                                }
                            } while (SpatialMap.TryGetNextValue(out neighborIndex, ref iterator));
                        }
                    }
                }
            }

            float3 totalForce = float3.zero;

            // ==== REYNOLDS STEERING FORMULA =====
            if (cohesionCount > 0)
            {
                centerOfMass /= cohesionCount; // Find the true center
                float3 desiredVelocity = math.normalize(centerOfMass - myTransform.Position) * maxSpeed;
                float3 steeringForce = desiredVelocity - myVelocity.Value;
        
                // Apply the average weight to the final steering force!
                totalForce += steeringForce * (cohesionWeightSum / cohesionCount);
            }
    
            if (alignmentCount > 0)
            {
                averageVelocity /= alignmentCount; // Find the true average direction
                float3 desiredVelocity = math.normalize(averageVelocity) * maxSpeed;
                float3 steeringForce = desiredVelocity - myVelocity.Value;
        
                totalForce += steeringForce * (alignmentWeightSum / alignmentCount);
            }
    
            if (separationCount > 0)
            {
                separationPush /= separationCount; // Find the average escape route
                float3 desiredVelocity = math.normalize(separationPush) * maxSpeed;
                float3 steeringForce = desiredVelocity - myVelocity.Value;
        
                totalForce += steeringForce * (separationWeightSum / separationCount);
            }
            
            return totalForce;
        }


    }
    
    [BurstCompile]
    public struct HashPositionsJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<LocalTransform> Positions;
        public GridHash GridHash;
        public NativeParallelMultiHashMap<int, int>.ParallelWriter SpatialMap;

        public void Execute(int index)
        {
            // Get the integer cell
            int3 gridPos = GridHash.GetGridPos(Positions[index].Position);
            int hash = GridHash.Hash(gridPos);

            // Add the array index with the corresponding hashkey
            SpatialMap.Add(hash, index);
        }
    }
}