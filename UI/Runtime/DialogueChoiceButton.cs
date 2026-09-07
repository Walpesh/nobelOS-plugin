using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StoryEngine.UI
{
    [DisallowMultipleComponent]
    public class DialogueChoiceButton : MonoBehaviour
    {
        public Button button;
        public TextMeshProUGUI labelText;

        public int Index { get; set; }

        public void SetLabel(string value)
        {
            if (labelText != null) labelText.text = value;
        }

        public void SetInteractable(bool value)
        {
            if (button != null) button.interactable = value;
        }
    }
}