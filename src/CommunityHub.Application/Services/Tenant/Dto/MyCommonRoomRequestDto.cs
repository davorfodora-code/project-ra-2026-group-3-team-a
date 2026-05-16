using System.Globalization;

namespace CommunityHub.Application.Services.Tenant.Dto;

public class MyCommonRoomRequestDto
{
    public long RequestId { get; set; }
    public long RoomId { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public bool IsMultiDay { get; set; }
    public DateTime RequestedStartDate { get; set; }
    public DateTime RequestedEndDate { get; set; }
    public DateTime? ProposedStartDate { get; set; }
    public DateTime? ProposedEndDate { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string Status { get; set; } = string.Empty;

    public bool IsPending => Status == "Pending";
    public bool IsProposedCounter => Status == "ProposedCounter";
    public bool CanCancel => Status is "Pending" or "ProposedCounter";

    public string StatusDisplay => Status switch
    {
        "Pending"         => "Na čekanju",
        "ProposedCounter" => "Na čekanju - promena termina",
        "Approved"        => "Odobreno",
        "Rejected"        => "Odbijeno",
        "Cancelled"       => "Otkazano",
        _                 => Status
    };

    public string RentalTypeDisplay => IsMultiDay ? "Višednevno iznajmljivanje" : "Jednodnevno iznajmljivanje";

    public string RequestedDateRangeDisplay => FormatRange(RequestedStartDate, RequestedEndDate);
    public string ProposedDateRangeDisplay =>
        ProposedStartDate.HasValue && ProposedEndDate.HasValue
            ? FormatRange(ProposedStartDate.Value, ProposedEndDate.Value)
            : string.Empty;

    public string ApprovedDateDisplay =>
        ApprovedDate.HasValue
            ? ApprovedDate.Value.ToString("ddd, MMM d, yyyy", CultureInfo.InvariantCulture)
            : string.Empty;

    private static string FormatRange(DateTime start, DateTime end)
    {
        if (start.Date == end.Date)
            return start.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);
        return $"{start.ToString("MMM d", CultureInfo.InvariantCulture)} – {end.ToString("MMM d, yyyy", CultureInfo.InvariantCulture)}";
    }
}
