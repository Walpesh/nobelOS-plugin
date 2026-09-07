using UnityEditor;
using UnityEngine;
using StoryEngine.Core;

namespace StoryEngine.Core.Editor
{
    [CustomEditor(typeof(DialogueGraphAsset))]
    public class DialogueGraphAssetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var graph = (DialogueGraphAsset)target;

            EditorGUILayout.Space();

            if (GUILayout.Button("Validate Graph"))
            {
                var issues = graph.Validate();

                if (issues.Count == 0)
                {
                    Debug.Log($"Dialogue graph '{graph.name}' is valid.", graph);
                }
                else
                {
                    foreach (var issue in issues)
                        Debug.LogWarning(issue, graph);
                }
            }
        }
    }
}