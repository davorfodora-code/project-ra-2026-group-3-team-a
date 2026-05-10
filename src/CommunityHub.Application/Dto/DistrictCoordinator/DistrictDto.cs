namespace CommunityHub.Application.Dto.DistrictCoordinator;

public class DistrictDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public decimal Budget { get; set; }

    public string DisplayText => $"{Name} — {City}, {Country}";
}
