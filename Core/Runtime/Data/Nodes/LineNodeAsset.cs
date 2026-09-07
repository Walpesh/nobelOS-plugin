using UnityEngine;

namespace StoryEngine.Core
{
    [CreateAssetMenu(fileName = "LineNode", menuName = "StoryEngine/Nodes/Line", order = 20)]
    public class LineNodeAsset : DialogueNodeAsset
    {
        public override DialogueNodeKind Kind => DialogueNodeKind.Line;

        public SpeakerAsset speaker;
        public LocalizedText text;
        public Sprite portraitOverride;
        public AudioClip voiceClip;
        public DialogueNodeAsset nextNode;

        [Header("Auto Advance (optional, UI can ignore)")]
        public bool autoAdvance;
        public float autoAdvanceDelay = 1f;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            if (string.IsNullOrEmpty(editorTitle))
            {
                var preview = text.fallback ?? string.Empty;
                editorTitle = preview.Length > 42 ? preview.Substring(0, 42) + "..." : preview;
                if (string.IsNullOrEmpty(editorTitle))
                    editorTitle = name;
            }
        }
#endif
    }
}