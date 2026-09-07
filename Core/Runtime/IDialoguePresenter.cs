namespace StoryEngine.Core
{
    public interface IDialoguePresenter
    {
        void Clear();
        void OnLinePresented(LinePresentedArgs args);
        void OnChoicesPresented(ChoicesPresentedArgs args);
        void OnStoryFinished(StoryFinishedArgs args);
    }
}