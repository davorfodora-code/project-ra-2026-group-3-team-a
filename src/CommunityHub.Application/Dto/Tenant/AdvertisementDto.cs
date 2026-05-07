namespace CommunityHub.Application.Dto.Tenant;

public class AdvertisementDto
{
    public long Id { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public bool IsOffering { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsOwn { get; set; }

    public string TypeDisplay => IsOffering ? "Nudi pomoć" : "Traži pomoć";
    public string DateRangeDisplay => $"{StartDate:dd.MM.yyyy.} – {EndDate:dd.MM.yyyy.}";
}
