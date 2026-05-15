namespace CommunityHub.Application.Services.BuildingManager.Dto;

// Statistika jedne kategorije oglasa za izabrani period.
public class CategoryStatsDto
{
    public string Category     { get; init; } = string.Empty;
    public int OfferingCount   { get; init; }
    public int SeekingCount    { get; init; }
}
