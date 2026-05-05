namespace CommunityHub.Application.Dto.BuildingManager;

public class BuildingMembershipDto
{
    public string TenantFullName { get; set; } = string.Empty;
    public int FloorNumber { get; set; }
    public int ApartmentNumber { get; set; }
    public DateTime JoinedAt { get; set; }
    public string JoinedAtFormatted => JoinedAt.ToString("dd.MM.yyyy.");
}
