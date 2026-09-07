using UnityEditor;
using UnityEngine;
using StoryEngine.Core;

namespace StoryEngine.EditorTools
{
    [CustomEditor(typeof(EndingDefinitionAsset))]
    public class EndingDefinitionAssetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var ending = (EndingDefinitionAsset)target;

            EditorGUILayout.Space();

            int count = SaveSystem.GetEndingCount(ending.id);
            EditorGUILayout.LabelField($"Reached by players: {count}");

            if (Application.isPlaying && GUILayout.Button("Test requirements in Play Mode"))
            {
                var runner = Object.FindObjectOfType<DialogueRunner>();
                if (runner == null)
                {
                    Debug.LogWarning("No DialogueRunner in scene.");
                }
                else
                {
                    bool met = ending.RequirementsMet(runner.BuildConditionContext());
                    Debug.Log($"Ending '{ending.id}' requirements met: {met}", ending);
                }
            }
        }
    }
}