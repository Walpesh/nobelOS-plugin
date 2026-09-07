using System;
using System.Collections.Generic;
using UnityEngine;

namespace StoryEngine.Core
{
    [CreateAssetMenu(fileName = "Background", menuName = "StoryEngine/Background", order = 70)]
    public class BackgroundAsset : ScriptableObject
    {
        public string id;
        public Sprite sprite;
        public float fadeDuration = 0.4f;

        [Header("Overlay")]
        public Color overlayColor = Color.black;
        [Range(0f, 1f)] public float overlayAlpha;

        [Header("Parallax")]
        [Range(0f, 60f)] public float parallaxStrength;

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

    [CreateAssetMenu(fileName = "BackgroundCatalog", menuName = "StoryEngine/Background Catalog", order = 71)]
    public class BackgroundCatalogAsset : ScriptableObject
    {
        [SerializeField] private List<BackgroundAsset> backgrounds = new List<BackgroundAsset>();

        public BackgroundAsset Get(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            for (int i = 0; i < backgrounds.Count; ++i)
            {
                var background = backgrounds[i];
                if (background != null && background.id == id)
                    return background;
            }

            return null;
        }

        public bool Contains(string id) => Get(id) != null;
    }
}