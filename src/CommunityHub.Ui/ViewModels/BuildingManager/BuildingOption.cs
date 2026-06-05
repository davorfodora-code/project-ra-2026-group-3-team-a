namespace CommunityHub.Ui.ViewModels.BuildingManager;

public class BuildingOption
{
    public long   BuildingId { get; init; }
    public string Address    { get; init; } = string.Empty;

    public BuildingOption(long buildingId, string address)
    {
        BuildingId = buildingId;
        Address    = address;
    }
}
