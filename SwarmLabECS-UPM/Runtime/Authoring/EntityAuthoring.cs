using SwarmLabECS.Components;
using Unity.Entities;
using UnityEngine;

namespace SwarmLabECS.Authoring
{
    public class EntityAuthoring : MonoBehaviour
    {
        private class EntityBaker : Baker<EntityAuthoring>
        {
            public override void Bake(EntityAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent(entity, new BoidTag());
                AddComponent(entity, new NewEntity()); // adding the new tag (will be disabled when spawned by the spawning system
                AddComponent(entity, new BoidSpawnSetup()); // associated spawning data
                AddComponent(entity, new EntityVelocity());
                AddComponent(entity, new EntitySettings());
            }
        }   
    }


}