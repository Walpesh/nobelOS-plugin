namespace StoryEngine.Core
{
    public interface IDialogueNodeHandler
    {
        bool CanHandle(DialogueNodeAsset node);
        void Enter(DialogueRunner runner, DialogueNodeAsset node);
    }
}