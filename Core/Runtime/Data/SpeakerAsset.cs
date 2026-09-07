using UnityEngine;

namespace StoryEngine.Core
{
    [CreateAssetMenu(fileName = "Speaker", menuName = "StoryEngine/Speaker", order = 10)]
    public class SpeakerAsset : ScriptableObject
    {
        public string id;
        public string displayName;
        public bool isNarrator;
        public Color nameColor = Color.white;
        public Sprite defaultPortrait;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(id))
            {
                id = UnityEditor.AssetDatabase.AssetPathToGUID(UnityEditor.AssetDatabase.GetAssetPath(this));
                if (string.IsNullOrEmpty(id))
                    id = System.Guid.NewGuid().ToString("N");
            }

            if (string.IsNullOrEmpty(displayName))
                displayName = name;
        }
#endif
    }
}