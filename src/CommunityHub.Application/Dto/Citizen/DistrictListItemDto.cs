namespace CommunityHub.Application.Dto.Citizen;

public class DistrictListItemDto
{
    public long DistrictId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string LocationDisplay { get; set; } = string.Empty;
    public string StreetsDisplay { get; set; } = string.Empty;
    public List<string> ImageUrls { get; set; } = new();
    public int ImageCount => ImageUrls.Count;
}
