namespace CommunityHub.Application.Services.BuildingManager.Dto;

public class BuildingMembershipDto
{
    public string TenantFullName { get; set; } = string.Empty;
    public int FloorNumber { get; set; }
    public int ApartmentNumber { get; set; }
    public DateTime JoinedAt { get; set; }
    public string JoinedAtFormatted => JoinedAt.ToString("dd.MM.yyyy.");

    public string FloorApartmentDisplay => $"Sprat {FloorNumber}, stan {ApartmentNumber}";
    public string JoinedDisplay => $"Datum uclanjenja: {JoinedAtFormatted}";
}
