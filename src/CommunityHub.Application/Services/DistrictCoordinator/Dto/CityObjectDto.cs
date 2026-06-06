namespace CommunityHub.Application.Services.DistrictCoordinator.Dto;

public class CityObjectDto
{
    public long   Id        { get; set; }
    public string Name      { get; set; } = string.Empty;
    public string Type      { get; set; } = string.Empty;
    public string City      { get; set; } = string.Empty;
    public int    Capacity  { get; set; }
    public int    VoteCount { get; set; }

    public string LocationCapacityDisplay => $"{City} — kapacitet {Capacity}";
}
