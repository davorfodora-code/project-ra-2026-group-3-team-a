namespace CommunityHub.Application.Services.Tenant.Dto;

public class TenantMembershipDto
{
    public long BuildingId { get; set; }
    public string Street { get; set; } = string.Empty;
    public string StreetNumber { get; set; } = string.Empty;
    public string Neighborhood { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public int FloorNumber { get; set; }
    public int ApartmentNumber { get; set; }
    public DateTime JoinedAt { get; set; }

    public string AddressDisplay => $"{Street} {StreetNumber}, {Neighborhood}, {City}";
    public string ApartmentInfo => $"Sprat {FloorNumber}, Stan {ApartmentNumber}";
    public string JoinedAtDisplay => $"od {JoinedAt:dd.MM.yyyy.}";
}
