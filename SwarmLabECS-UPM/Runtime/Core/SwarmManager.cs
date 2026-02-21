using Unity.Entities;
using UnityEngine;
using NaughtyAttributes;

namespace SwarmLabECS.Core
{

    public class SwarmManager : MonoBehaviour
    {
        private enum SimulationMode { Volumetric, Planar }

        public static SwarmManager Instance { get; private set; }

        [Header("General Settings")]
        [SerializeField] private bool drawSpawnZones = true;
        [SerializeField] private SwarmConfigAuthoring swarmConfig;
        
        [Header("Simulation Mode")]
        [SerializeField] private SimulationMode simulationMode = SimulationMode.Volumetric;
        
        [Header("Simulation Boundaries")]
        // TODO: transform to 3D and 2D boundaries
        [SerializeField] private Transform planarBoundary;
        [SerializeField] private Vector2 planarSize = new Vector2(50f, 50f);

        public SwarmConfigAuthoring Config => swarmConfig;
        
        public void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning("There can be only one SwarmManager in the scene. Destroying duplicate...");
                Destroy(gameObject);
            }
            else Instance = this;
        }
        
        [Button("Generate Swarm")]
        public void GenerateSwarm()
        {
            // Get the ECS World
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            // Create a new empty entity
            Entity requestEntity = entityManager.CreateEntity();

            // Add the "Spawn Request" tag
            // The System will see this next frame and trigger the logic
            entityManager.AddComponent<SpawnSwarmRequest>(requestEntity);
        
            Debug.Log("Spawn Request Sent to ECS!");
        }
        
        private void OnDrawGizmos()
        {
            if (swarmConfig == null || swarmConfig.speciesList == null) return;

            // Draw everything in Local Space (relative to the Manager's rotation/position)
            Matrix4x4 originalMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;


            foreach (var species in swarmConfig.speciesList)
            {
                // ONLY DRAW SPHERES IN VOLUMETRIC MODE
                if (simulationMode != SimulationMode.Volumetric) continue;
                if (!drawSpawnZones) continue;
                
                
                // Generate a consistent color based on the species name
                Color speciesColor = species.prefab == null? 
                    Color.limeGreen:
                    Color.HSVToRGB(
                    (species.prefab.name.GetHashCode() * 0.13f) % 1f,
                    0.7f, 1f);
                
                Gizmos.color = speciesColor;

                Gizmos.DrawWireSphere(species.spawnCenter, species.radius);

                // Draw a small solid sphere at the center of the zone
                Gizmos.color = new Color(speciesColor.r, speciesColor.g, speciesColor.b, 0.4f);
                Gizmos.DrawSphere(species.spawnCenter, 0.05f);

            }
        
            
            Gizmos.matrix = originalMatrix;
            
            // Draw 2D Boundary if enabled
            if (simulationMode != SimulationMode.Planar || planarBoundary == null) return;
            
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.matrix = planarBoundary.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(planarSize.x, 0, planarSize.y));
            Gizmos.color = new Color(0, 1, 0, 0.1f);
            Gizmos.DrawCube(Vector3.zero, new Vector3(planarSize.x, 0, planarSize.y));
            Gizmos.matrix = originalMatrix;
        }

        [ContextMenu("Create Simulation Plane")]
        public void CreatePlane()
        {
            GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.name = "SimulationBoundary";
            plane.transform.position = Vector3.zero;
            plane.transform.localScale = new Vector3(planarSize.x / 10f, 1, planarSize.y / 10f); // Plane default size is 10x10
            
            // Assign
            planarBoundary = plane.transform;
            simulationMode = SimulationMode.Planar;
            
            // Optional: transparent material or just collider, but for now default is fine
            Debug.Log("Created Simulation Plane and enabled 2D mode.");
        }
    }
}
