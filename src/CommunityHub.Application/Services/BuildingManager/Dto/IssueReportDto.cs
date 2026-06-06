namespace CommunityHub.Application.Services.BuildingManager.Dto;

public class IssueReportDto
{
    public long Id { get; init; }
    public long BuildingId { get; init; }
    public string TenantFullName { get; init; } = string.Empty;
    public string BuildingAddress { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Priority { get; init; } = string.Empty;
    public DateTime ReportedAt { get; init; }
    public string Status { get; init; } = string.Empty;

    public string DateLine => ReportedAt.ToString("dd.MM.yyyy.");

    public string PriorityDisplay => Priority switch
    {
        "Urgent"    => "HITNO",
        "SolveSoon" => "MOŽE DA ČEKA",
        "CanWait"   => "U NAREDNOM PERIODU",
        _           => Priority
    };

    public string StatusDisplay => Status switch
    {
        "Unresolved"          => "nerešen",
        "PotentiallyResolved" => "potencijalno rešen",
        "Resolved"            => "rešen",
        _                     => Status
    };

    public bool CanMarkAsResolved => Status == "Unresolved";
}
