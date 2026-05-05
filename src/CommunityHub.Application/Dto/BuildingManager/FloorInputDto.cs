namespace CommunityHub.Application.Dto.BuildingManager;

public class FloorInputDto
{
    public int FloorNumber { get; set; }
    public List<int> ApartmentNumbers { get; set; } = new();
}
