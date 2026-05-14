namespace CommunityHub.Application.Services.BuildingManager.Dto;

public class CreateCommonRoomDto
{
    public long BuildingId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int FloorNumber { get; set; }
    public bool IsMultiDay { get; set; }
}
