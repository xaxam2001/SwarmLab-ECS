using Unity.Entities;
using UnityEngine;
using NaughtyAttributes;

namespace SwarmLabECS.Core
{

    public class SwarmManager : MonoBehaviour
    {
        public static SwarmManager Instance { get; private set; }

        [Header("General Settings")]
        [SerializeField] private bool drawSpawnZones = true;
        [SerializeField] private SwarmConfigAuthoring swarmConfig;
        

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
                if (!drawSpawnZones) continue;
                
                // Generate a consistent color based on the species name
                Color speciesColor = species.prefab == null ? 
                    Color.limeGreen:
                    Color.HSVToRGB(
                    (species.prefab.name.GetHashCode() * 0.13f) % 1f,
                    0.7f, 1f);
                
                Gizmos.color = speciesColor;

                if (species.useCubeSpawnZone)
                    Gizmos.DrawWireCube(species.spawnOffset, species.cubeSize);
                else
                    Gizmos.DrawWireSphere(species.spawnOffset, species.radius);

                // Draw a small solid sphere at the center of the zone
                Gizmos.color = new Color(speciesColor.r, speciesColor.g, speciesColor.b, 0.4f);
                Gizmos.DrawSphere(species.spawnOffset, 0.05f);

            }
        
            
            Gizmos.matrix = originalMatrix;
        }
    }
}
