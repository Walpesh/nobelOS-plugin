using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using StoryEngine.Core;

namespace StoryEngine.UI
{
    [DisallowMultipleComponent]
    public class DialogueUIController : MonoBehaviour, IDialoguePresenter
    {
        [Header("Runner")]
        [SerializeField] private DialogueRunner runner;

        [Header("View")]
        [SerializeField] private CanvasGroup dialogueRoot;
        [SerializeField] private TextMeshProUGUI speakerNameText;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private Image portraitImage;
        [SerializeField] private GameObject continueIndicator;
        [SerializeField] private Transform choicesContainer;
        [SerializeField] private DialogueChoiceButton choiceButtonPrefab;

        [Header("Ending screen")]
        [SerializeField] private GameObject endingPanel;
        [SerializeField] private TextMeshProUGUI endingTitleText;
        [SerializeField] private TextMeshProUGUI endingDescriptionText;
        [SerializeField] private TextMeshProUGUI endingStatsText;
        [SerializeField] private bool showMetaStats = true;

        [Header("Behaviour")]
        [SerializeField] private bool typewriterEnabled = true;
        [SerializeField] private float charactersPerSecond = 60f;
        [SerializeField] private KeyCode advanceKey = KeyCode.Space;
        [SerializeField] private bool clickAdvances = true;
        [SerializeField] private int choiceButtonsPoolSize = 8;

        [Header("Hooks")]
        public UnityEvent onStoryFinished;

        private readonly List<DialogueChoiceButton> _choiceButtons = new List<DialogueChoiceButton>(8);

        private bool _typewriterRunning;
        private float _charProgress;
        private int _totalChars;
        private int _lastVisibleChars = -1;

        private void Awake()
        {
            if (runner == null) runner = GetComponent<DialogueRunner>();

            EnsureChoiceButtons(choiceButtonsPoolSize);

            if (runner != null) runner.SetPresenter(this);

            Clear();
        }

        private void OnEnable()
        {
            if (runner != null) runner.SetPresenter(this);
        }

        private void OnDestroy()
        {
            if (runner != null) runner.SetPresenter(null);
        }

        private void Update()
        {
            if (runner == null) return;

            if (_typewriterRunning)
            {
                _charProgress += Time.unscaledDeltaTime * charactersPerSecond;

                int visible = Mathf.Min(_totalChars, Mathf.FloorToInt(_charProgress));
                if (visible != _lastVisibleChars)
                {
                    _lastVisibleChars = visible;
                    dialogueText.maxVisibleCharacters = visible;
                }

                if (visible >= _totalChars)
                {
                    _typewriterRunning = false;
                    continueIndicator.SetActive(true);
                }
            }

            if (runner.State == DialogueRunnerState.WaitingForAdvance)
            {
                bool advanceInput = Input.GetKeyDown(advanceKey) ||
                                    (clickAdvances && Input.GetMouseButtonDown(0));

                if (advanceInput)
                {
                    if (_typewriterRunning)
                    {
                        _typewriterRunning = false;
                        _lastVisibleChars = _totalChars;
                        dialogueText.maxVisibleCharacters = _totalChars;
                        continueIndicator.SetActive(true);
                    }
                    else
                    {
                        runner.Advance();
                    }
                }
            }
        }

        // ------------------------------------------------------------------
        // IDialoguePresenter
        // ------------------------------------------------------------------

        public void Clear()
        {
            _typewriterRunning = false;
            _charProgress = 0f;
            _totalChars = 0;
            _lastVisibleChars = -1;

            dialogueRoot.gameObject.SetActive(false);
            endingPanel.SetActive(false);
            continueIndicator.SetActive(false);
            HideChoices();
        }

        public void OnLinePresented(LinePresentedArgs args)
        {
            dialogueRoot.gameObject.SetActive(true);
            endingPanel.SetActive(false);
            HideChoices();

            ApplySpeaker(args.speaker, args.speakerName);
            ApplyPortrait(args.portrait);

            dialogueText.text = args.text;
            StartTypewriter(args.text);
        }

        public void OnChoicesPresented(ChoicesPresentedArgs args)
        {
            dialogueRoot.gameObject.SetActive(true);
            endingPanel.SetActive(false);
            continueIndicator.SetActive(false);
            _typewriterRunning = false;

            ApplySpeaker(args.speaker, args.speakerName);
            ApplyPortrait(args.portrait);

            dialogueText.text = args.prompt;
            _totalChars = args.prompt != null ? args.prompt.Length : 0;
            _lastVisibleChars = _totalChars;
            dialogueText.maxVisibleCharacters = _totalChars;

            ShowChoices(args.choices);
        }

        public void OnStoryFinished(StoryFinishedArgs args)
        {
            _typewriterRunning = false;
            HideChoices();
            continueIndicator.SetActive(false);
            dialogueRoot.gameObject.SetActive(false);

            endingPanel.SetActive(true);

            if (endingTitleText != null) endingTitleText.text = args.endingTitle;
            if (endingDescriptionText != null) endingDescriptionText.text = args.endingDescription;

            if (endingStatsText != null)
                endingStatsText.text = showMetaStats ? BuildStatsText(args) : string.Empty;

            onStoryFinished?.Invoke();
        }

        // ------------------------------------------------------------------
        // Internals
        // ------------------------------------------------------------------

        private string BuildStatsText(StoryFinishedArgs args)
        {
            var profile = SaveSystem.LoadProfile();

            int unlocked = profile.unlockedEndings.Count;
            int totalEndings = runner != null && runner.StoryProject != null ? runner.StoryProject.endings.Count : 0;
            int visited = runner != null ? runner.Statistics.VisitedNodes.Count : 0;

            return $"Прохождений: {profile.playthroughs}\n" +
                   $"Узлов посещено: {visited}\n" +
                   $"Финалов открыто: {unlocked}" + (totalEndings > 0 ? $" / {totalEndings}" : "");
        }

        private void StartTypewriter(string text)
        {
            _totalChars = text != null ? text.Length : 0;
            _charProgress = 0f;
            _lastVisibleChars = -1;

            if (typewriterEnabled && _totalChars > 0)
            {
                _typewriterRunning = true;
                dialogueText.maxVisibleCharacters = 0;
                continueIndicator.SetActive(false);
            }
            else
            {
                _typewriterRunning = false;
                dialogueText.maxVisibleCharacters = _totalChars;
                continueIndicator.SetActive(true);
            }
        }

        private void ApplySpeaker(SpeakerAsset speaker, string speakerName)
        {
            if (speakerNameText == null) return;

            bool hasName = !string.IsNullOrEmpty(speakerName);
            speakerNameText.gameObject.SetActive(hasName);
            if (!hasName) return;

            speakerNameText.text = speakerName;
            if (speaker != null) speakerNameText.color = speaker.nameColor;
        }

        private void ApplyPortrait(Sprite portrait)
        {
            if (portraitImage == null) return;

            if (portrait != null)
            {
                portraitImage.sprite = portrait;
                portraitImage.gameObject.SetActive(true);
            }
            else
            {
                portraitImage.gameObject.SetActive(false);
            }
        }

        private void EnsureChoiceButtons(int count)
        {
            if (choiceButtonPrefab == null || choicesContainer == null) return;

            while (_choiceButtons.Count < count)
            {
                var instance = Instantiate(choiceButtonPrefab, choicesContainer);
                instance.gameObject.SetActive(false);

                var captured = instance;
                instance.button.onClick.AddListener(() => OnChoiceClicked(captured));

                _choiceButtons.Add(instance);
            }
        }

        private void ShowChoices(IReadOnlyList<DialogueChoiceRuntime> choices)
        {
            if (choiceButtonPrefab == null || choicesContainer == null) return;

            EnsureChoiceButtons(choices.Count);

            for (int i = 0; i < _choiceButtons.Count; ++i)
            {
                var button = _choiceButtons[i];

                if (i < choices.Count)
                {
                    button.Index = i;
                    button.SetLabel(choices[i].label);
                    button.SetInteractable(choices[i].isInteractable);
                    button.gameObject.SetActive(true);
                }
                else
                {
                    button.gameObject.SetActive(false);
                }
            }
        }

        private void HideChoices()
        {
            for (int i = 0; i < _choiceButtons.Count; ++i)
                _choiceButtons[i].gameObject.SetActive(false);
        }

        private void OnChoiceClicked(DialogueChoiceButton button)
        {
            if (runner == null) return;
            if (runner.State != DialogueRunnerState.WaitingForChoice) return;
            runner.SelectChoice(button.Index);
        }
    }
}