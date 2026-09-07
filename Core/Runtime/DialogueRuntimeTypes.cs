using System.Collections.Generic;
using UnityEngine;

namespace StoryEngine.Core
{
    public enum DialogueRunnerState
    {
        Idle,
        Running,
        WaitingForAdvance,
        WaitingForChoice,
        Finished
    }

    [System.Serializable]
    public struct SceneContext
    {
        public string backgroundId;
        public string locationId;
        public string timeOfDay;
        public string mood;

        public static SceneContext FromNode(DialogueNodeAsset node)
        {
            if (node == null) return default;

            return new SceneContext
            {
                backgroundId = node.backgroundId,
                locationId = node.locationId,
                timeOfDay = node.timeOfDay,
                mood = node.mood
            };
        }
    }

    public struct DialogueChoiceRuntime
    {
        public int index;
        public string choiceId;
        public string label;
        public bool isCorrect;
        public bool isInteractable;
        public DialogueNodeAsset targetNode;
    }

    public struct NodeEnteredArgs
    {
        public DialogueGraphAsset graph;
        public DialogueNodeAsset node;
        public SceneContext context;
    }

    public struct LinePresentedArgs
    {
        public DialogueGraphAsset graph;
        public DialogueNodeAsset node;
        public SpeakerAsset speaker;
        public string speakerName;
        public string text;
        public Sprite portrait;
        public AudioClip voiceClip;
        public SceneContext context;
        public bool isNarration;
    }

    public struct ChoicesPresentedArgs
    {
        public DialogueGraphAsset graph;
        public DialogueNodeAsset node;
        public SpeakerAsset speaker;
        public string speakerName;
        public Sprite portrait;
        public string prompt;
        public IReadOnlyList<DialogueChoiceRuntime> choices;
        public SceneContext context;
    }

    public struct ChoiceSelectedArgs
    {
        public DialogueGraphAsset graph;
        public DialogueNodeAsset node;
        public int choiceIndex;
        public DialogueChoiceRuntime choice;
    }

    public struct StoryFinishedArgs
    {
        public DialogueGraphAsset graph;
        public DialogueNodeAsset endNode;
        public string endingId;
        public string endingTitle;
        public string endingDescription;
    }
}