namespace CommunityHub.Application.Services.Tenant.Dto;

public class TenantAccessRequestDto
{
    public long RequestId { get; set; }
    public string AddressDisplay { get; set; } = string.Empty;
    public string Neighborhood { get; set; } = string.Empty;
    public string LocationDisplay { get; set; } = string.Empty;
    public int ApartmentNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string CreatedAtFormatted => CreatedAt.ToString("dd.MM.yyyy. HH:mm");
    public bool CanCancel { get; set; }
}
