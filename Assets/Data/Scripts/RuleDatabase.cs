using System.Collections.Generic;
using UnityEngine;

namespace Ju.RuleSystem
{
    [CreateAssetMenu(fileName = "RuleDatabase", menuName = "RuleSystem/Rule Database")]
    public class RuleDatabase : ScriptableObject
    {
        public List<RuleDefinition> rules = new();
    }
}
