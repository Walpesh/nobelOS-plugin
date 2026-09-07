using System.Collections.Generic;
using UnityEngine;

namespace StoryEngine.Core
{
    [CreateAssetMenu(fileName = "Ending", menuName = "StoryEngine/Ending Definition", order = 80)]
    public class EndingDefinitionAsset : ScriptableObject
    {
        public string id;
        public LocalizedText title;
        public LocalizedText description;

        [Tooltip("Условия, при которых финал считается 'истинным' (для секретных финалов и статистики).")]
        public List<ConditionAsset> requirements = new List<ConditionAsset>();

        public bool IsSecret;

        public bool RequirementsMet(in ConditionContext ctx)
            => ConditionEvaluator.AllPass(requirements, ctx);

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(id))
            {
                id = UnityEditor.AssetDatabase.AssetPathToGUID(UnityEditor.AssetDatabase.GetAssetPath(this));
                if (string.IsNullOrEmpty(id)) id = System.Guid.NewGuid().ToString("N");
            }
        }
#endif
    }
}