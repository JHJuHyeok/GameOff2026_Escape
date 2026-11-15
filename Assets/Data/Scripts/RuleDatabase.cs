using System.Collections.Generic;
using UnityEngine;

namespace Ju.RuleSystem
{
    [CreateAssetMenu(menuName = "Database/Rule Database", fileName = "RuleDatabase")]
    public class RuleDatabase : ScriptableObject
    {
        public List<RuleDefinition> rules = new();
    }
}
