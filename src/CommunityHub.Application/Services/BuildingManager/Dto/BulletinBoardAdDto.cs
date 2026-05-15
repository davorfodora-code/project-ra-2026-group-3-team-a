namespace CommunityHub.Application.Services.BuildingManager.Dto;

// DTO za jednu karticu oglasa na oglasnoj tabli (pregled za upravnika).
// Upravnik vidi sve oglase (aktivne i arhivirane), ali bez termina.
public class BulletinBoardAdDto
{
    public long Id              { get; init; }
    public string TenantFullName  { get; init; } = string.Empty;
    public string BuildingAddress { get; init; } = string.Empty;
    public bool IsOffering        { get; init; }
    public string Category        { get; init; } = string.Empty;
    public DateTime StartDate     { get; init; }
    public DateTime EndDate       { get; init; }
    public string Status          { get; init; } = string.Empty;

    public string TypeLine   => IsOffering ? "NUDI POMOĆ" : "TRAŽI POMOĆ";
    public string DateLine   => $"Datum: {StartDate:dd.MM.} - {EndDate:dd.MM.yyyy}.";
    public string StatusLine => $"Status: {Status.ToUpper()}";
}
