namespace CommunityHub.Ui.ViewModels.Citizen;

public class CitizenHomeViewModel
{
    public long UserId { get; }

    public CitizenHomeViewModel(long userId)
    {
        UserId = userId;
    }
}
