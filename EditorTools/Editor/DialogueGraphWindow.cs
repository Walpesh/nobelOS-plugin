using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using StoryEngine.Core;

namespace StoryEngine.EditorTools
{
    public class DialogueGraphWindow : EditorWindow
    {
        private GraphView _graphView;
        private DialogueGraphAsset _graph;
        private ObjectField _graphField;
        private readonly Dictionary<DialogueNodeAsset, DialogueNodeView> _views = new Dictionary<DialogueNodeAsset, DialogueNodeView>();

        [MenuItem("StoryEngine/Dialogue Graph Editor")]
        public static void Open()
        {
            var window = GetWindow<DialogueGraphWindow>();
            window.titleContent = new GUIContent("Dialogue Graph");

            if (Selection.activeObject is DialogueGraphAsset selected)
                window.ShowGraph(selected);
        }

        private void OnEnable()
        {
            _graphView = new GraphView();
            _graphView.graphViewChanged += OnGraphChanged;
            _graphView.viewTransformChanged += _ => SavePositions();
            _graphView.nodeCreationRequest = context => ShowCreateNodeMenu(context);
            _graphView.Add(new GridBackground());

            var toolbar = new Toolbar();

            _graphField = new ObjectField("Graph") { objectType = typeof(DialogueGraphAsset) };
            _graphField.RegisterValueChangedCallback(evt => ShowGraph((DialogueGraphAsset)evt.newValue));
            toolbar.Add(_graphField);

            toolbar.Add(new ToolbarButton(() => ShowGraph(_graph)) { text = "Reload" });
            toolbar.Add(new ToolbarButton(Validate) { text = "Validate" });

            rootVisualElement.Add(toolbar);
            rootVisualElement.Add(_graphView);
        }

        public void ShowGraph(DialogueGraphAsset graph)
        {
            _graph = graph;
            _graphView.DeleteElements(_graphView.graphElements.ToList());
            _views.Clear();

            if (graph == null) return;

            _graphField.value = graph;

            for (int i = 0; i < graph.nodes.Count; ++i)
            {
                var node = graph.nodes[i];
                if (node != null) CreateView(node);
            }

            RebuildEdges();
        }

        private DialogueNodeView CreateView(DialogueNodeAsset asset)
        {
            var view = new DialogueNodeView(asset, this);
            _graphView.AddElement(view);
            _views[asset] = view;
            return view;
        }

        private void RebuildEdges()
        {
            foreach (var pair in _views)
                pair.Value.RefreshEdges(_views);
        }

        private GraphViewChange OnGraphChanged(GraphViewChange change)
        {
            if (change.edgesToCreate != null)
            {
                for (int i = 0; i < change.edgesToCreate.Count; ++i)
                    ApplyConnection(change.edgesToCreate[i]);
            }

            if (change.movedElements != null)
                SavePositions();

            return change;
        }

        private void ApplyConnection(Edge edge)
        {
            if (edge.output == null || edge.input == null) return;

            var sourceView = edge.output.node as DialogueNodeView;
            var targetView = edge.input.node as DialogueNodeView;
            if (sourceView == null || targetView == null || _graph == null) return;

            int choiceIndex = edge.output.userData is int index ? index : -1;

            if (sourceView.Asset is LineNodeAsset line && choiceIndex < 0)
            {
                line.nextNode = targetView.Asset;
                EditorUtility.SetDirty(line);
            }
            else if (sourceView.Asset is ChoiceNodeAsset choice && choiceIndex >= 0 && choiceIndex < choice.choices.Count)
            {
                choice.choices[choiceIndex].nextNode = targetView.Asset;
                EditorUtility.SetDirty(choice);
            }

            AssetDatabase.SaveAssets();
        }

        private void SavePositions()
        {
            if (_graph == null) return;

            foreach (var pair in _views)
            {
                pair.Key.editorPosition = pair.Value.GetPosition().position;
                EditorUtility.SetDirty(pair.Key);
            }
        }

        private void Validate()
        {
            if (_graph == null) return;

            var issues = AdvancedGraphValidator.Validate(_graph);
            if (issues.Count == 0) Debug.Log($"Graph '{_graph.name}' is valid.", _graph);
            else foreach (var issue in issues) Debug.LogWarning(issue, _graph);
        }

        private void ShowCreateNodeMenu(NodeCreationContext context)
        {
            if (_graph == null) return;

            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Create/Line"), false, () => CreateNode<LineNodeAsset>(context));
            menu.AddItem(new GUIContent("Create/Choice"), false, () => CreateNode<ChoiceNodeAsset>(context));
            menu.AddItem(new GUIContent("Create/End"), false, () => CreateNode<EndNodeAsset>(context));
            menu.ShowAsContext();
        }

        private void CreateNode<T>(NodeCreationContext context) where T : DialogueNodeAsset
        {
            var graphPath = AssetDatabase.GetAssetPath(_graph);
            var folder = string.IsNullOrEmpty(graphPath) ? "Assets" : System.IO.Path.GetDirectoryName(graphPath);

            var asset = CreateInstance<T>();
            asset.id = System.Guid.NewGuid().ToString("N");
            asset.editorPosition = context.screenMousePosition;

            var path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/Node_{typeof(T).Name}_{_graph.nodes.Count}.asset");
            AssetDatabase.CreateAsset(asset, path);

            _graph.nodes.Add(asset);
            if (_graph.entryNode == null) _graph.entryNode = asset;

            EditorUtility.SetDirty(_graph);
            AssetDatabase.SaveAssets();

            var view = CreateView(asset);
            view.SetPosition(new Rect(context.screenMousePosition, new Vector2(220, 120)));
        }

        public void FocusNodeAsset(DialogueNodeAsset asset)
        {
            Selection.activeObject = asset;
        }
    }

    internal class DialogueNodeView : Node
    {
        public DialogueNodeAsset Asset { get; }
        private readonly DialogueGraphWindow _window;
        private readonly List<Port> _outputs = new List<Port>();
        public Port Input { get; private set; }

        public DialogueNodeView(DialogueNodeAsset asset, DialogueGraphWindow window)
        {
            Asset = asset;
            _window = window;

            title = $"{asset.Kind}: {asset.editorTitle}";
            SetPosition(new Rect(asset.editorPosition, new Vector2(220, 120)));

            Input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            Input.portName = "in";
            inputContainer.Add(Input);

            BuildOutputs();

            var preview = new Label(GetPreviewText()) { style = { whiteSpace = WhiteSpace.Normal } };
            extensionContainer.Add(preview);

            this.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                evt.menu.AppendAction("Clear Next/All", _ => ClearConnections());
                evt.menu.AppendAction("Select Asset", _ => _window.FocusNodeAsset(Asset));
            }));

            RefreshExpandedState();
        }

        private string GetPreviewText()
        {
            if (Asset is LineNodeAsset line) return line.text.fallback;
            if (Asset is ChoiceNodeAsset choice) return choice.prompt.fallback;
            if (Asset is EndNodeAsset end) return end.endingId;
            return string.Empty;
        }

        private void BuildOutputs()
        {
            outputContainer.Clear();
            _outputs.Clear();

            if (Asset is LineNodeAsset)
            {
                var port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
                port.portName = "next";
                port.userData = -1;
                outputContainer.Add(port);
                _outputs.Add(port);
            }
            else if (Asset is ChoiceNodeAsset choice)
            {
                for (int i = 0; i < choice.choices.Count; ++i)
                {
                    var port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
                    port.portName = $"[{i}] {(choice.choices[i] != null ? choice.choices[i].label.fallback : "null")}";
                    port.userData = i;
                    outputContainer.Add(port);
                    _outputs.Add(port);
                }
            }
        }

        public void RefreshEdges(Dictionary<DialogueNodeAsset, DialogueNodeView> views)
        {
            for (int i = 0; i < _outputs.Count; ++i)
            {
                DialogueNodeAsset target = null;

                if (Asset is LineNodeAsset line && i == 0) target = line.nextNode;
                else if (Asset is ChoiceNodeAsset choice && i < choice.choices.Count && choice.choices[i] != null)
                    target = choice.choices[i].nextNode;

                if (target == null || !views.TryGetValue(target, out var targetView)) continue;

                var edge = _outputs[i].ConnectTo(targetView.Input);
                graphView.AddElement(edge);
            }
        }

        private void ClearConnections()
        {
            if (Asset is LineNodeAsset line)
            {
                line.nextNode = null;
                EditorUtility.SetDirty(line);
            }
            else if (Asset is ChoiceNodeAsset choice)
            {
                for (int i = 0; i < choice.choices.Count; ++i)
                {
                    if (choice.choices[i] != null) choice.choices[i].nextNode = null;
                }
                EditorUtility.SetDirty(choice);
            }

            AssetDatabase.SaveAssets();
            _window.ShowGraph(Asset is DialogueNodeAsset ? GetGraph() : null);
        }

        private DialogueGraphAsset GetGraph()
        {
            // Перезагрузка графа берётся из окна; здесь достаточно вернуть null-safe путь через окно.
            return null;
        }
    }
}