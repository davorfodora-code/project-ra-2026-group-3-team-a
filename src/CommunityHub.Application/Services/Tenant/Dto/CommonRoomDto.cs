namespace CommunityHub.Application.Services.Tenant.Dto;

public class CommonRoomDto
{
    public long RoomId { get; set; }
    public long BuildingId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int FloorNumber { get; set; }
    public bool IsMultiDay { get; set; }

    public string FloorDisplay => $"Sprat {FloorNumber}";
    public string RentalTypeDisplay => IsMultiDay ? "Višednevno iznajmljivanje" : "Jednodnevno iznajmljivanje";
}
