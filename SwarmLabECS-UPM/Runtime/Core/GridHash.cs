using Unity.Mathematics;

namespace SwarmLabECS.Core
{
    public struct GridHash
    {
        public readonly float CellSize;

        public GridHash(float cellSize)
        {
            CellSize = cellSize;
        }

        // Convert world position to a 3D grid coordinate
        public int3 GetGridPos(float3 position)
        {
            return new int3(math.floor(position / CellSize));
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
}