namespace CommunityHub.Application.Services.BuildingManager.Dto;

public class CreateBuildingDto
{
    public string Street { get; set; } = string.Empty;
    public string StreetNumber { get; set; } = string.Empty;
    public string Neighborhood { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public int NumberOfFloors { get; set; }
    public long ManagerId { get; set; }
    public List<FloorInputDto> Floors { get; set; } = new();
    public List<string> ImagePaths { get; set; } = new();
}
