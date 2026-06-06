namespace CommunityHub.Application.Services.Tenant.Dto;

public class IssueReportDto
{
    public long Id { get; init; }
    public long BuildingId { get; init; }
    public string BuildingAddress { get; init; } = "";
    public string Description { get; init; } = "";
    public string Priority { get; init; } = "";
    public DateTime ReportedAt { get; init; }
    public string Status { get; init; } = "";

    public string PriorityDisplay => Priority switch
    {
        "Urgent"    => "Hitno",
        "SolveSoon" => "Treba rešiti uskoro",
        "CanWait"   => "Može da čeka",
        _           => Priority
    };

    public string StatusDisplay => Status switch
    {
        "Unresolved"          => "Nerešen",
        "PotentiallyResolved" => "Potencijalno rešen",
        "Resolved"            => "Rešen",
        _                     => Status
    };

    public string PriorityIcon => Priority switch
    {
        "Urgent"    => "CircleExclamation",
        "SolveSoon" => "TriangleExclamation",
        "CanWait"   => "Clock",
        _           => "Circle"
    };

    public bool NeedsConfirmation => Status == "PotentiallyResolved";
}
