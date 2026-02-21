using System.Collections.Generic;
using UnityEngine;

namespace SwarmLabECS.Core
{
    // The base "Contract" for per-species settings
    [System.Serializable]
    public class SpeciesParams
    {
        [HideInInspector] public SpeciesDefinition species; // The "Key"
        // (Add your custom fields in the child classes)
    }

    [System.Serializable]
    public abstract class SteeringRule
    {
        // The Editor calls this to keep the lists in sync
        public abstract void SyncSpeciesList(List<SpeciesDefinition> allSpecies);

        // public abstract Vector3 CalculateForce(Entity entity, List<Entity> neighbors);
        
        public virtual void OnValidate() { }
        
        public virtual void DrawGizmos() { }
    }
}