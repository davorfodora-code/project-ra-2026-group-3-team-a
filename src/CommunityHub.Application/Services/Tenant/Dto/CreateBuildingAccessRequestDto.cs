namespace CommunityHub.Application.Services.Tenant.Dto;

public class CreateBuildingAccessRequestDto
{
    public long TenantId { get; set; }
    public long BuildingId { get; set; }
    public int ApartmentNumber { get; set; }
}
