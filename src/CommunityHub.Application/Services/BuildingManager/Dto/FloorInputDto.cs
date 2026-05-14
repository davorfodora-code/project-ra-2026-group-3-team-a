namespace CommunityHub.Application.Services.BuildingManager.Dto;

public class FloorInputDto
{
    public int FloorNumber { get; set; }
    public List<int> ApartmentNumbers { get; set; } = new();
}
