using UnityEngine;

namespace StoryEngine.Core
{
    public sealed class LineNodeHandler : IDialogueNodeHandler
    {
        public bool CanHandle(DialogueNodeAsset node) => node is LineNodeAsset;

        public void Enter(DialogueRunner runner, DialogueNodeAsset node)
        {
            var line = (LineNodeAsset)node;
            var text = runner.Localize(line.text);
            var portrait = line.portraitOverride != null
                ? line.portraitOverride
                : (line.speaker != null ? line.speaker.defaultPortrait : null);

            runner.PresentLine(line, line.speaker, text, portrait, line.voiceClip, line.nextNode);
        }
    }

    public sealed class ChoiceNodeHandler : IDialogueNodeHandler
    {
        public bool CanHandle(DialogueNodeAsset node) => node is ChoiceNodeAsset;

        public void Enter(DialogueRunner runner, DialogueNodeAsset node)
        {
            var choiceNode = (ChoiceNodeAsset)node;

            if (choiceNode.choices == null || choiceNode.choices.Count == 0)
            {
                runner.FinishStory(null, "error.no_choices", "Error", "Choice node has no choices.");
                return;
            }

            var ctx = runner.BuildConditionContext();

            runner.PrepareChoices(choiceNode.choices.Count);

            for (int i = 0; i < choiceNode.choices.Count; ++i)
            {
                var option = choiceNode.choices[i];
                if (option == null) continue;

                if (!ConditionEvaluator.AllPass(option.showConditions, ctx))
                    continue;

                bool interactable = ConditionEvaluator.AllPass(option.enableConditions, ctx);

                if (!interactable && choiceNode.hideUnavailableChoices)
                    continue;

                runner.AddChoice(
                    runner.Localize(option.label),
                    option.isCorrect,
                    option.nextNode,
                    interactable,
                    option.id);
            }

            if (runner.ActiveChoices.Count == 0)
            {
                runner.FinishStory(null, "error.no_valid_choices", "Error", "Choice node has no visible choices.");
                return;
            }

            var portrait = choiceNode.promptPortraitOverride != null
                ? choiceNode.promptPortraitOverride
                : (choiceNode.promptSpeaker != null ? choiceNode.promptSpeaker.defaultPortrait : null);

            runner.PresentChoices(choiceNode, choiceNode.promptSpeaker, runner.Localize(choiceNode.prompt), portrait);
        }
    }

    public sealed class EndNodeHandler : IDialogueNodeHandler
    {
        public bool CanHandle(DialogueNodeAsset node) => node is EndNodeAsset;

        public void Enter(DialogueRunner runner, DialogueNodeAsset node)
        {
            var end = (EndNodeAsset)node;
            runner.FinishStory(
                end,
                end.endingId,
                runner.Localize(end.endingTitle),
                runner.Localize(end.endingDescription));
        }
    }
}