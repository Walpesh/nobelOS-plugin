using UnityEngine;

namespace StoryEngine.Core
{
    public enum DialogueNodeKind
    {
        Generic = 0,
        Line = 1,
        Choice = 2,
        End = 3
    }

    public abstract class DialogueNodeAsset : ScriptableObject
    {
        public string id;
        public string editorTitle;

        [TextArea(2, 5)]
        public string editorComment;

        [Header("Scene Context")]
        public string backgroundId;
        public string locationId;
        public string timeOfDay;
        public string mood;

        [HideInInspector] public Vector2 editorPosition;

        public abstract DialogueNodeKind Kind { get; }
        public virtual bool IsTerminal => false;

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (string.IsNullOrEmpty(id))
            {
                id = UnityEditor.AssetDatabase.AssetPathToGUID(UnityEditor.AssetDatabase.GetAssetPath(this));
                if (string.IsNullOrEmpty(id)) id = System.Guid.NewGuid().ToString("N");
            }

            if (string.IsNullOrEmpty(editorTitle))
                editorTitle = name;
        }
#endif
    }
}