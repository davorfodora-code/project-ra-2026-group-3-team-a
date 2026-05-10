namespace CommunityHub.Application.Dto.BuildingManager;

public class CommonRoomDto
{
    public long Id { get; set; }
    public long BuildingId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int FloorNumber { get; set; }
    public bool IsMultiDay { get; set; }

    public string FloorDisplay => FloorNumber == 0 ? "Prizemlje" : $"{FloorNumber}. sprat";
}
