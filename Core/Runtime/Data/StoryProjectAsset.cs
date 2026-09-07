using System.Collections.Generic;
using UnityEngine;

namespace StoryEngine.Core
{
    [CreateAssetMenu(fileName = "StoryProject", menuName = "StoryEngine/Story Project", order = 0)]
    public class StoryProjectAsset : ScriptableObject
    {
        public string id;
        public DialogueGraphAsset startGraph;
        public List<DialogueGraphAsset> graphs = new List<DialogueGraphAsset>();
        public List<AttributeDefinitionAsset> attributes = new List<AttributeDefinitionAsset>();
        public List<EndingDefinitionAsset> endings = new List<EndingDefinitionAsset>();

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(id))
            {
                id = UnityEditor.AssetDatabase.AssetPathToGUID(UnityEditor.AssetDatabase.GetAssetPath(this));
                if (string.IsNullOrEmpty(id)) id = System.Guid.NewGuid().ToString("N");
            }

            if (startGraph != null && !graphs.Contains(startGraph))
                graphs.Add(startGraph);
        }
#endif
    }
}