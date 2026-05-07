namespace CommunityHub.Application.Dto.Tenant;

public class CreateAdvertisementDto
{
    public long TenantId { get; set; }
    public long BuildingId { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsOffering { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Category { get; set; } = string.Empty;
}
