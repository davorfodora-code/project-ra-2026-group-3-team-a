namespace CommunityHub.Ui.ViewModels.BuildingManager;

// VM za jedan unos teme u formi za novu skupstinu
public class TopicEntryViewModel : BaseViewModel
{
    public string Text { get; }
    public TopicEntryViewModel(string text) => Text = text;
}
