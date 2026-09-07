using UnityEngine;

namespace StoryEngine.Core
{
    [CreateAssetMenu(fileName = "EndNode", menuName = "StoryEngine/Nodes/End", order = 22)]
    public class EndNodeAsset : DialogueNodeAsset
    {
        public override DialogueNodeKind Kind => DialogueNodeKind.End;
        public override bool IsTerminal => true;

        public string endingId;
        public LocalizedText endingTitle;
        public LocalizedText endingDescription;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            if (string.IsNullOrEmpty(editorTitle))
                editorTitle = string.IsNullOrEmpty(endingId) ? name : endingId;
        }
#endif
    }
}