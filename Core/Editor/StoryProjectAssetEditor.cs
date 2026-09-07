using UnityEditor;
using UnityEngine;
using StoryEngine.Core;

namespace StoryEngine.Core.Editor
{
    [CustomEditor(typeof(StoryProjectAsset))]
    public class StoryProjectAssetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var project = (StoryProjectAsset)target;

            EditorGUILayout.Space();

            if (GUILayout.Button("Validate Project"))
            {
                int issueCount = 0;

                if (project.startGraph == null)
                {
                    Debug.LogWarning("Start Graph is not assigned.", project);
                    issueCount++;
                }

                if (project.graphs == null || project.graphs.Count == 0)
                {
                    Debug.LogWarning("Graphs list is empty.", project);
                    issueCount++;
                }
                else
                {
                    for (int i = 0; i < project.graphs.Count; ++i)
                    {
                        var graph = project.graphs[i];

                        if (graph == null)
                        {
                            Debug.LogWarning($"Graphes[{i}] is null.", project);
                            issueCount++;
                            continue;
                        }

                        var issues = graph.Validate();
                        if (issues.Count == 0)
                        {
                            Debug.Log($"Graph '{graph.name}' is valid.", graph);
                        }
                        else
                        {
                            issueCount += issues.Count;
                            foreach (var issue in issues)
                                Debug.LogWarning(issue, graph);
                        }
                    }
                }

                if (issueCount == 0)
                    Debug.Log("Story project is valid.", project);
            }
        }
    }
}