using System;
using UnityEngine;

namespace StoryEngine.Core
{
    public enum AttributeType
    {
        Number = 0,
        Bool = 1,
        String = 2
    }

    [Serializable]
    public struct AttributeValue
    {
        public float number;
        public bool boolValue;
        public string stringValue;

        public static AttributeValue AsNumber(float v) => new AttributeValue { number = v };
        public static AttributeValue AsBool(bool v) => new AttributeValue { boolValue = v };
        public static AttributeValue AsString(string v) => new AttributeValue { stringValue = v };
    }

    [CreateAssetMenu(fileName = "Attribute", menuName = "StoryEngine/Attribute Definition", order = 40)]
    public class AttributeDefinitionAsset : ScriptableObject
    {
        public string id;
        public string displayName;
        public AttributeType type = AttributeType.Number;

        [Header("Defaults")]
        public float defaultNumber;
        public bool defaultBool;
        public string defaultString;

        [Header("Number clamp")]
        public bool clampNumber;
        public float minNumber;
        public float maxNumber = 999999f;

        public bool hiddenFromUI;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(id))
            {
                id = UnityEditor.AssetDatabase.AssetPathToGUID(UnityEditor.AssetDatabase.GetAssetPath(this));
                if (string.IsNullOrEmpty(id)) id = System.Guid.NewGuid().ToString("N");
            }

            if (string.IsNullOrEmpty(displayName)) displayName = name;
        }
#endif
    }
}