using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace SwarmLabECS.Utils
{
    public struct GridHash
    {
        private readonly float _cellSize;

        public GridHash(float cellSize)
        {
            _cellSize = cellSize;
        }

        // Convert world position to a 3D grid coordinate
        public int3 GetGridPos(float3 position)
        {
            return new int3(math.floor(position / _cellSize));
        }

        // convert 3D grid coordinate into a single Integer hash using large prime numbers
        public int Hash(int3 gridPos)
        {
            // Prime numbers help distribute the hashes evenly and avoid collisions
            const int p1 = 73856093;
            const int p2 = 19349663;
            const int p3 = 83492791;

            return (gridPos.x * p1) ^ (gridPos.y * p2) ^ (gridPos.z * p3);
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