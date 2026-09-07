using System;
using System.Collections.Generic;
using UnityEngine;

namespace StoryEngine.Core
{
    [Serializable]
    public class DialogueChoice
    {
        public string id;
        public LocalizedText label;
        public DialogueNodeAsset nextNode;
        public bool isCorrect;
        public string editorComment;

        [Tooltip("Если НЕ выполнены — выбор скрыт полностью.")]
        public List<ConditionAsset> showConditions = new List<ConditionAsset>();

        [Tooltip("Если НЕ выполнены — выбор виден, но недоступен (серый).")]
        public List<ConditionAsset> enableConditions = new List<ConditionAsset>();

        [Tooltip("Применяются при выборе этого варианта.")]
        public List<EffectAsset> effects = new List<EffectAsset>();
    }

    [CreateAssetMenu(fileName = "ChoiceNode", menuName = "StoryEngine/Nodes/Choice", order = 21)]
    public class ChoiceNodeAsset : DialogueNodeAsset
    {
        public override DialogueNodeKind Kind => DialogueNodeKind.Choice;

        public LocalizedText prompt;
        public SpeakerAsset promptSpeaker;
        public Sprite promptPortraitOverride;

        public List<DialogueChoice> choices = new List<DialogueChoice>();
        public bool hideUnavailableChoices;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            if (string.IsNullOrEmpty(editorTitle))
            {
                var preview = prompt.fallback ?? string.Empty;
                editorTitle = preview.Length > 42 ? preview.Substring(0, 42) + "..." : preview;
                if (string.IsNullOrEmpty(editorTitle)) editorTitle = name;
            }

            for (int i = 0; i < choices.Count; ++i)
            {
                var choice = choices[i];
                if (choice != null && string.IsNullOrEmpty(choice.id))
                    choice.id = System.Guid.NewGuid().ToString("N");
            }
        }
#endif
    }
}