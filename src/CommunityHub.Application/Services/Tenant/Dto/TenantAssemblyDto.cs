using System.Globalization;

namespace CommunityHub.Application.Services.Tenant.Dto;

public class TenantAssemblyDto
{
    public long Id { get; init; }
    public long BuildingId { get; init; }
    public string BuildingAddress { get; init; } = "";
    public DateTime ScheduledAt { get; init; }
    public string Status { get; init; } = "";
    public List<string> Topics { get; } = [];
    public int ConfirmedCount { get; init; }
    public int TotalApartments { get; init; }
    public bool IsTenantConfirmed { get; init; }
    public int ApartmentNumber { get; init; }

    public DateTime Deadline => ScheduledAt.AddHours(-24);
    public bool IsDeadlinePassed => DateTime.Now >= Deadline;
    public bool IsFuture => ScheduledAt > DateTime.Now;
    public double AttendancePercent => TotalApartments == 0 ? 0 : (double)ConfirmedCount / TotalApartments * 100;
    public bool HasQuorum => ConfirmedCount * 2 > TotalApartments;

    public bool CanActOnAttendance => (Status == "Scheduled" || Status == "Confirmed") && !IsDeadlinePassed && IsFuture;
    public bool ShowConfirmButton => CanActOnAttendance && !IsTenantConfirmed;
    public bool ShowCancelButton => CanActOnAttendance && IsTenantConfirmed;
    public bool ShowProposeButton => (Status == "Scheduled" || Status == "Confirmed") && !IsDeadlinePassed && IsFuture;

    public string StatusDisplay => Status switch
    {
        "Scheduled" => "Zakazana",
        "Confirmed" => "Potvrđena",
        "Cancelled"  => "Otkazana",
        _ => Status
    };

    public string TenantAttendanceDisplay => IsTenantConfirmed ? "Potvrđeno" : "Nije potvrđeno";

    public string DateDisplay => ScheduledAt.ToString("dddd, dd MMM yyyy", CultureInfo.InvariantCulture);
    public string TimeDisplay => ScheduledAt.ToString("HH:mm");
    public string DeadlineDisplay => $"Rok: {Deadline:dd.MM. HH:mm}";
    public string AttendanceDisplay => $"Prisustvo: {ConfirmedCount}/{TotalApartments} stanova ({AttendancePercent:F0}%)";
    public string QuorumLabel => HasQuorum ? "50% dostignuto"
        : Status == "Cancelled" ? "50% nije dostignuto"
        : "Potrebno 50%";
}
