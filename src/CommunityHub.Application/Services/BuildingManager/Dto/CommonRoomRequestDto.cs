namespace CommunityHub.Application.Services.BuildingManager.Dto;

public class CommonRoomRequestDto
{
    public long RequestId { get; init; }
    public string TenantFullName { get; init; } = string.Empty;
    public long RoomId { get; init; }
    public string RoomName { get; init; } = string.Empty;
    public string BuildingAddress { get; init; } = string.Empty;
    public DateTime RequestedStartDate { get; init; }
    public DateTime RequestedEndDate { get; init; }
    public bool IsMultiDay { get; init; }
    public DateTime? ProposedStartDate { get; init; }
    public DateTime? ProposedEndDate { get; init; }
    public string Status { get; init; } = string.Empty;

    // Opseg koji je stanar tražio (npr. "15.04. - 17.04.2026.")
    public string RequestedRangeLine =>
        RequestedStartDate.ToString("dd.MM.") + " - " + RequestedEndDate.ToString("dd.MM.yyyy") + ".";

    // Tip iznajmljivanja
    public string RentalTypeLine => IsMultiDay ? "Na više dana" : "Na 1 dan";

    // Status za prikaz u UI-u
    public string StatusLine => Status switch
    {
        "Pending"         => "Na čekanju",
        "ProposedCounter" => "Na čekanju - promena termina",
        "Approved"        => "Odobreno",
        "Rejected"        => "Odbijeno",
        _                 => Status
    };
}
