using UnityEngine;

namespace StoryEngine.Core.Diagnostics
{
    [DisallowMultipleComponent]
    public class DebugDialoguePresenter : MonoBehaviour, IDialoguePresenter
    {
        [SerializeField] private DialogueRunner runner;
        [SerializeField] private bool logLines = true;
        [SerializeField] private bool logChoices = true;
        [SerializeField] private bool logEndings = true;

        private void Awake()
        {
            if (runner == null)
                runner = GetComponent<DialogueRunner>();

            if (runner != null)
                runner.SetPresenter(this);
        }

        private void OnDisable()
        {
            if (runner != null)
                runner.SetPresenter(null);
        }

        public void Clear()
        {
        }

        public void OnLinePresented(LinePresentedArgs args)
        {
            if (!logLines)
                return;

            var speaker = string.IsNullOrEmpty(args.speakerName) ? "Narrator" : args.speakerName;
            Debug.Log($"[LINE] {speaker}: {args.text}");
        }

        public void OnChoicesPresented(ChoicesPresentedArgs args)
        {
            if (!logChoices)
                return;

            Debug.Log($"[CHOICE] {args.prompt}");

            for (int i = 0; i < args.choices.Count; ++i)
            {
                var choice = args.choices[i];
                Debug.Log($"  {i + 1}. {choice.label}");
            }
        }

        public void OnStoryFinished(StoryFinishedArgs args)
        {
            if (!logEndings)
                return;

            Debug.Log($"[END] id={args.endingId}, title={args.endingTitle}, description={args.endingDescription}");
        }
    }
}