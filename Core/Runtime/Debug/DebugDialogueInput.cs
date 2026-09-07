using UnityEngine;

namespace StoryEngine.Core.Diagnostics
{
    [DisallowMultipleComponent]
    public class DebugDialogueInput : MonoBehaviour
    {
        public DialogueRunner runner;
        public KeyCode advanceKey = KeyCode.Space;
        public bool mouseClickAdvances = true;

        private void Awake()
        {
            if (runner == null)
                runner = GetComponent<DialogueRunner>();
        }

        private void Update()
        {
            if (runner == null)
                return;

            if (runner.State == DialogueRunnerState.WaitingForAdvance)
            {
                bool advance = Input.GetKeyDown(advanceKey) ||
                               (mouseClickAdvances && Input.GetMouseButtonDown(0));

                if (advance)
                    runner.Advance();
            }
            else if (runner.State == DialogueRunnerState.WaitingForChoice)
            {
                int maxDigits = Mathf.Min(9, runner.ActiveChoices.Count);

                for (int i = 0; i < maxDigits; ++i)
                {
                    var key = (KeyCode)((int)KeyCode.Alpha1 + i);
                    if (Input.GetKeyDown(key))
                    {
                        runner.SelectChoice(i);
                        break;
                    }
                }
            }
        }
    }
}