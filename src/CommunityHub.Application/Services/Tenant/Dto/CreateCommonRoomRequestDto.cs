namespace CommunityHub.Application.Services.Tenant.Dto;

public class CreateCommonRoomRequestDto
{
    public long TenantId { get; set; }
    public long RoomId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
