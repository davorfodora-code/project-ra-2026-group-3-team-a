namespace CommunityHub.Application.Dto.BuildingManager;

public class FloorSummaryDto
{
    public int FloorNumber { get; set; }
    public List<int> ApartmentNumbers { get; set; } = new();

    // npr. "1. sprat: 101, 102, 103"
    public string Display =>
        $"{FloorNumber}. sprat: {string.Join(", ", ApartmentNumbers)}";
}
