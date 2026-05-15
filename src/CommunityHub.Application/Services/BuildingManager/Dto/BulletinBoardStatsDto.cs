namespace CommunityHub.Application.Services.BuildingManager.Dto;

// Sve statistike oglasne table za izabrani period + trenutno stanje + top pomagač.
public class BulletinBoardStatsDto
{
    // Broj oglasa u izabranom periodu
    public int PeriodOfferingCount { get; init; }
    public int PeriodSeekingCount  { get; init; }

    // Breakdown po kategoriji za izabrani period
    public List<CategoryStatsDto> CategoryStats { get; init; } = new();

    // Trenutno stanje (bez filtera perioda)
    public int CurrentActiveCount   { get; init; }
    public int CurrentArchivedCount { get; init; }

    // Aktivni oglasi po kategoriji (trenutno stanje)
    public List<(string Category, int Count)> ActiveByCategory { get; init; } = new();

    // Top pomagač — stanar sa najviše zauzetih termina u oglasima gde je nudio pomoć
    public string? TopHelperName  { get; init; }
    public int     TopHelperSlots { get; init; }
}
