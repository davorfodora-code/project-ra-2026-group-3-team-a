using System.Globalization;

namespace CommunityHub.Application.Services.Tenant.Dto;

public class BookedSlotDto
{
    public long SlotId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public string DisplayText =>
        $"{StartTime.ToString("ddd, MMM d", CultureInfo.InvariantCulture)} — " +
        $"{StartTime.ToString("h:mm", CultureInfo.InvariantCulture)}–" +
        $"{EndTime.ToString("h:mm tt", CultureInfo.InvariantCulture)}";
}
