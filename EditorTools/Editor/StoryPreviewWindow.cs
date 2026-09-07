using UnityEditor;
using UnityEngine;
using StoryEngine.Core;

namespace StoryEngine.EditorTools
{
    public class StoryPreviewWindow : EditorWindow
    {
        private DialogueGraphAsset _graph;
        private DialogueNodeAsset _startNode;

        internal static DialogueGraphAsset PendingGraph;
        internal static DialogueNodeAsset PendingNode;

        [MenuItem("StoryEngine/Story Preview")]
        public static void Open() => GetWindow<StoryPreviewWindow>("Story Preview");

        private void OnGUI()
        {
            _graph = (DialogueGraphAsset)EditorGUILayout.ObjectField("Graph", _graph, typeof(DialogueGraphAsset), false);

            if (_graph != null)
            {
                var nodes = _graph.nodes;
                var names = new string[nodes.Count];
                int current = 0;

                for (int i = 0; i < nodes.Count; ++i)
                {
                    names[i] = nodes[i] != null ? nodes[i].editorTitle : "(null)";
                    if (nodes[i] == _startNode) current = i;
                }

                int selected = EditorGUILayout.Popup("Start node", current, names);
                if (names.Length > 0) _startNode = nodes[selected];
            }

            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(_graph == null);
            if (GUILayout.Button("Play from Entry"))
                Launch(_graph, null);
            if (GUILayout.Button("Play from Selected Node"))
                Launch(_graph, _startNode);
            EditorGUI.EndDisabledGroup();
        }

        private static void Launch(DialogueGraphAsset graph, DialogueNodeAsset node)
        {
            PendingGraph = graph;
            PendingNode = node;
            EditorApplication.isPlaying = true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void HookPlayMode()
        {
            if (PendingGraph == null) return;

            var hookObject = new GameObject("[StoryPreviewHook]");
            var hook = hookObject.AddComponent<PreviewHook>();
            hook.Graph = PendingGraph;
            hook.Node = PendingNode;

            PendingGraph = null;
            PendingNode = null;
        }

        private class PreviewHook : MonoBehaviour
        {
            public DialogueGraphAsset Graph;
            public DialogueNodeAsset Node;

            private System.Collections.IEnumerator Start()
            {
                yield return null;

                var runner = FindObjectOfType<DialogueRunner>();
                if (runner == null) yield break;

                runner.StartGraph(Graph);
                if (Node != null) runner.GoToNode(Node);
            }
        }
    }
}