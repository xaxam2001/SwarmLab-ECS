using UnityEngine;

namespace SwarmLabECS.Core 
{

    [CreateAssetMenu(fileName = "Species Definition", menuName = "SwarmLab/Species Definition")]
    public class SpeciesDefinition : ScriptableObject
    {
        public string speciesName;
        
        [Header("Behavior Settings")]
        public float maxSpeed;
    }
}