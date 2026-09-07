using System;
using System.Collections.Generic;
using UnityEngine;

namespace StoryEngine.Core
{
    [DisallowMultipleComponent]
    public class DialogueRunner : MonoBehaviour
    {
        [SerializeField] private StoryProjectAsset storyProject;
        [SerializeField] private bool startOnStart = true;
        [SerializeField] private bool logWarnings = true;
        [SerializeField] private bool useMetaProfile = true;

        private readonly List<IDialogueNodeHandler> _handlers = new List<IDialogueNodeHandler>(8);
        private readonly List<DialogueChoiceRuntime> _activeChoices = new List<DialogueChoiceRuntime>(8);
        private readonly HashSet<string> _visitedNodeIds = new HashSet<string>();

        private ILocalizationProvider _localizationProvider;
        private IDialoguePresenter _presenter;
        private DialogueNodeAsset _nextNode;

        public DialogueRunnerState State { get; private set; }
        public DialogueGraphAsset CurrentGraph { get; private set; }
        public DialogueNodeAsset CurrentNode { get; private set; }
        public SceneContext CurrentContext { get; private set; }
        public StoryProjectAsset StoryProject => storyProject;
        public IReadOnlyList<DialogueChoiceRuntime> ActiveChoices => _activeChoices;
        public StoryAttributeState Attributes { get; } = new StoryAttributeState();
        public StoryStatistics Statistics { get; private set; } = new StoryStatistics();

        public event Action<StoryProjectAsset> StoryStarted;
        public event Action<DialogueGraphAsset> GraphStarted;
        public event Action<NodeEnteredArgs> NodeEntered;
        public event Action<LinePresentedArgs> LinePresented;
        public event Action<ChoicesPresentedArgs> ChoicesPresented;
        public event Action<ChoiceSelectedArgs> ChoiceSelected;
        public event Action<StoryFinishedArgs> StoryFinished;
        public event Action<string, AttributeValue> AttributeChanged;
        public event Action<string> StoryEventRaised;

        private void Awake()
        {
            EnsureDefaultHandlers();
            Attributes.Changed += OnAttributeChanged;
        }

        private void Start()
        {
            if (startOnStart && State == DialogueRunnerState.Idle && storyProject != null)
                StartStory();
        }

        public void SetStoryProject(StoryProjectAsset project) => storyProject = project;
        public void SetLocalizationProvider(ILocalizationProvider provider) => _localizationProvider = provider;

        public void SetPresenter(IDialoguePresenter presenter)
        {
            if (_presenter == presenter) return;
            _presenter?.Clear();
            _presenter = presenter;
            _presenter?.Clear();
        }

        public void RegisterHandler(IDialogueNodeHandler handler, bool insertFirst = false)
        {
            if (handler == null) return;
            if (insertFirst) _handlers.Insert(0, handler);
            else _handlers.Add(handler);
        }

        public void StartStory()
        {
            if (storyProject == null)
            {
                LogError("Story Project is not assigned.");
                return;
            }

            Attributes.Initialize(storyProject.attributes);
            Statistics = new StoryStatistics();
            _visitedNodeIds.Clear();

            if (useMetaProfile && Application.isPlaying)
                SaveSystem.RecordPlaythrough();

            StoryStarted?.Invoke(storyProject);
            StartGraph(storyProject.startGraph);
        }

        public void StartGraph(DialogueGraphAsset graph)
        {
            if (graph == null)
            {
                LogError("Start Graph is not assigned.");
                return;
            }

            EnsureDefaultHandlers();

            CurrentGraph = graph;
            CurrentNode = null;
            _nextNode = null;
            _activeChoices.Clear();
            State = DialogueRunnerState.Running;

            _presenter?.Clear();
            GraphStarted?.Invoke(graph);

            EnterNode(graph.entryNode);
        }

        public void Advance()
        {
            if (State != DialogueRunnerState.WaitingForAdvance)
            {
                LogWarning("Advance called while runner is not waiting for advance.");
                return;
            }

            var next = _nextNode;
            _nextNode = null;
            State = DialogueRunnerState.Running;
            EnterNode(next);
        }

        public void SelectChoice(int choiceIndex)
        {
            if (State != DialogueRunnerState.WaitingForChoice)
            {
                LogWarning("SelectChoice called while runner is not waiting for choice.");
                return;
            }

            if (choiceIndex < 0 || choiceIndex >= _activeChoices.Count)
            {
                LogWarning($"Choice index {choiceIndex} is out of range.");
                return;
            }

            var choice = _activeChoices[choiceIndex];
            if (!choice.isInteractable)
            {
                LogWarning("Selected choice is not interactable.");
                return;
            }

            State = DialogueRunnerState.Running;

            if (!string.IsNullOrEmpty(choice.choiceId))
            {
                Statistics.MarkChoice(choice.choiceId);
                if (useMetaProfile && Application.isPlaying)
                    SaveSystem.RecordChoice(choice.choiceId);
            }

            ApplyChoiceEffects(choice);

            ChoiceSelected?.Invoke(new ChoiceSelectedArgs
            {
                graph = CurrentGraph,
                node = CurrentNode,
                choiceIndex = choiceIndex,
                choice = choice
            });

            EnterNode(choice.targetNode);
        }

        public void StopStory()
        {
            State = DialogueRunnerState.Finished;
            CurrentGraph = null;
            CurrentNode = null;
            _nextNode = null;
            _activeChoices.Clear();
            _presenter?.Clear();
        }

        public string Localize(LocalizedText text) => text.Get(_localizationProvider);

        public bool HasVisitedNodeId(string nodeId) => _visitedNodeIds.Contains(nodeId);

        public void CopyActiveChoicesTo(List<DialogueChoiceRuntime> buffer)
        {
            if (buffer == null) return;
            buffer.Clear();
            for (int i = 0; i < _activeChoices.Count; ++i)
                buffer.Add(_activeChoices[i]);
        }

        public void GoToNode(DialogueNodeAsset node) => EnterNode(node);

        public void RaiseStoryEvent(string eventName)
        {
            if (string.IsNullOrEmpty(eventName)) return;
            StoryEventRaised?.Invoke(eventName);
        }

        public ConditionContext BuildConditionContext()
        {
            return new ConditionContext
            {
                runner = this,
                attributes = Attributes,
                sceneContext = CurrentContext
            };
        }

        // ------------------------------------------------------------------
        // Save / Load
        // ------------------------------------------------------------------

        public SaveData CaptureSaveData()
        {
            var data = new SaveData
            {
                version = SaveData.CurrentVersion,
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                graphId = CurrentGraph != null ? CurrentGraph.id : string.Empty,
                nodeId = CurrentNode != null ? CurrentNode.id : string.Empty
            };

            Attributes.FillSave(data.attributes);

            foreach (var nodeId in _visitedNodeIds)
                data.visitedNodes.Add(nodeId);

            foreach (var pair in Statistics.ChoiceCounts)
            {
                for (int i = 0; i < pair.Value; ++i)
                    data.madeChoices.Add(pair.Key);
            }

            return data;
        }

        public bool ApplySaveData(SaveData data)
        {
            if (data == null || storyProject == null) return false;

            DialogueGraphAsset graph = null;
            for (int i = 0; i < storyProject.graphs.Count; ++i)
            {
                if (storyProject.graphs[i] != null && storyProject.graphs[i].id == data.graphId)
                {
                    graph = storyProject.graphs[i];
                    break;
                }
            }

            if (graph == null) return false;

            var node = graph.GetNodeById(data.nodeId);
            if (node == null) return false;

            Attributes.Initialize(storyProject.attributes);
            Attributes.Restore(data.attributes);

            Statistics = new StoryStatistics();
            Statistics.RestoreVisited(data.visitedNodes);
            Statistics.RestoreChoices(data.madeChoices);

            _visitedNodeIds.Clear();
            for (int i = 0; i < data.visitedNodes.Count; ++i)
                _visitedNodeIds.Add(data.visitedNodes[i]);

            CurrentGraph = graph;
            CurrentNode = null;
            _nextNode = null;
            _activeChoices.Clear();
            State = DialogueRunnerState.Running;

            _presenter?.Clear();
            GraphStarted?.Invoke(graph);

            EnterNode(node);
            return true;
        }

        // ------------------------------------------------------------------
        // Handler API
        // ------------------------------------------------------------------

        public void PrepareChoices(int capacity)
        {
            _activeChoices.Clear();
            if (capacity > 0 && _activeChoices.Capacity < capacity)
                _activeChoices.Capacity = capacity;
        }

        public void AddChoice(string label, bool isCorrect, DialogueNodeAsset targetNode, bool isInteractable, string choiceId)
        {
            _activeChoices.Add(new DialogueChoiceRuntime
            {
                index = _activeChoices.Count,
                choiceId = choiceId,
                label = label,
                isCorrect = isCorrect,
                isInteractable = isInteractable,
                targetNode = targetNode
            });
        }

        public void PresentLine(DialogueNodeAsset node, SpeakerAsset speaker, string text, Sprite portrait, AudioClip voiceClip, DialogueNodeAsset nextNode)
        {
            _nextNode = nextNode;
            State = DialogueRunnerState.WaitingForAdvance;

            var args = new LinePresentedArgs
            {
                graph = CurrentGraph,
                node = node,
                speaker = speaker,
                speakerName = GetSpeakerName(speaker),
                text = text,
                portrait = portrait,
                voiceClip = voiceClip,
                context = CurrentContext,
                isNarration = speaker == null || speaker.isNarrator
            };

            LinePresented?.Invoke(args);
            _presenter?.OnLinePresented(args);
        }

        public void PresentChoices(DialogueNodeAsset node, SpeakerAsset speaker, string prompt, Sprite portrait)
        {
            State = DialogueRunnerState.WaitingForChoice;

            var args = new ChoicesPresentedArgs
            {
                graph = CurrentGraph,
                node = node,
                speaker = speaker,
                speakerName = GetSpeakerName(speaker),
                portrait = portrait,
                prompt = prompt,
                choices = _activeChoices,
                context = CurrentContext
            };

            ChoicesPresented?.Invoke(args);
            _presenter?.OnChoicesPresented(args);
        }

        public void FinishStory(EndNodeAsset endNode, string endingId, string endingTitle, string endingDescription)
        {
            State = DialogueRunnerState.Finished;
            CurrentNode = endNode;
            _activeChoices.Clear();
            _nextNode = null;

            Statistics.LastEndingId = endingId;

            if (useMetaProfile && Application.isPlaying && !string.IsNullOrEmpty(endingId) && !endingId.StartsWith("error."))
                SaveSystem.UnlockEnding(endingId);

            var args = new StoryFinishedArgs
            {
                graph = CurrentGraph,
                endNode = endNode,
                endingId = endingId ?? string.Empty,
                endingTitle = endingTitle ?? string.Empty,
                endingDescription = endingDescription ?? string.Empty
            };

            StoryFinished?.Invoke(args);
            _presenter?.OnStoryFinished(args);
        }

        // ------------------------------------------------------------------
        // Internals
        // ------------------------------------------------------------------

        private void ApplyChoiceEffects(DialogueChoiceRuntime choice)
        {
            if (CurrentNode is not ChoiceNodeAsset choiceNode) return;
            if (choiceNode.choices == null) return;

            DialogueChoice source = null;
            for (int i = 0; i < choiceNode.choices.Count; ++i)
            {
                var option = choiceNode.choices[i];
                if (option != null && option.id == choice.choiceId)
                {
                    source = option;
                    break;
                }
            }

            if (source == null || source.effects == null || source.effects.Count == 0) return;

            var ctx = new EffectContext
            {
                runner = this,
                attributes = Attributes,
                sceneContext = CurrentContext
            };

            for (int i = 0; i < source.effects.Count; ++i)
            {
                var effect = source.effects[i];
                if (effect != null)
                    effect.Apply(ctx);
            }
        }

        private void OnAttributeChanged(string id, AttributeValue value)
        {
            AttributeChanged?.Invoke(id, value);
        }

        private void EnterNode(DialogueNodeAsset node)
        {
            _activeChoices.Clear();
            _nextNode = null;

            if (node == null)
            {
                State = DialogueRunnerState.Finished;

                var errorArgs = new StoryFinishedArgs
                {
                    graph = CurrentGraph,
                    endNode = null,
                    endingId = "error.missing_node",
                    endingTitle = "Missing node",
                    endingDescription = "Dialogue runner reached a null node."
                };

                LogError("Dialogue node is null. Story stopped.");
                StoryFinished?.Invoke(errorArgs);
                _presenter?.OnStoryFinished(errorArgs);
                return;
            }

            CurrentNode = node;
            CurrentContext = SceneContext.FromNode(node);

            if (!string.IsNullOrEmpty(node.id))
            {
                _visitedNodeIds.Add(node.id);
                Statistics.MarkNodeVisited(node.id);
            }

            NodeEntered?.Invoke(new NodeEnteredArgs
            {
                graph = CurrentGraph,
                node = node,
                context = CurrentContext
            });

            for (int i = 0; i < _handlers.Count; ++i)
            {
                var handler = _handlers[i];
                if (handler != null && handler.CanHandle(node))
                {
                    handler.Enter(this, node);
                    return;
                }
            }

            State = DialogueRunnerState.Finished;
            LogError($"No dialogue handler found for node '{node.name}' kind '{node.Kind}'.");
        }

        private void EnsureDefaultHandlers()
        {
            if (_handlers.Count > 0) return;
            _handlers.Add(new LineNodeHandler());
            _handlers.Add(new ChoiceNodeHandler());
            _handlers.Add(new EndNodeHandler());
        }

        private string GetSpeakerName(SpeakerAsset speaker)
        {
            if (speaker == null) return string.Empty;
            return speaker.displayName ?? speaker.name;
        }

        private void LogWarning(string message)
        {
            if (logWarnings) Debug.LogWarning($"[DialogueRunner] {message}", this);
        }

        private void LogError(string message)
        {
            Debug.LogError($"[DialogueRunner] {message}", this);
        }
    }
}