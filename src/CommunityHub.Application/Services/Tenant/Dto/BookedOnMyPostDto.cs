using System.Globalization;

namespace CommunityHub.Application.Services.Tenant.Dto;

public class BookedOnMyPostDto
{
    public long MyAdId { get; set; }
    public bool IsMyAdOffering { get; set; }
    public DateTime MyStartDate { get; set; }
    public DateTime MyEndDate { get; set; }
    public string MyCategory { get; set; } = string.Empty;
    public string MyStatus { get; set; } = string.Empty;
    public List<BookerEntryDto> Bookers { get; set; } = [];

    public bool IsActive => MyStatus == "Aktivan";
    public string StatusDisplay => IsActive ? "Active" : "Archived";
    public string MyAdTypeDisplay => IsMyAdOffering ? "Offering help" : "Seeking help";
    public string MyAdHeader => $"Your post: {MyAdTypeDisplay} - {MyCategory}";

    public string MyDateRangeDisplay =>
        $"{MyStartDate.ToString("MMM d", CultureInfo.InvariantCulture)} – " +
        $"{MyEndDate.ToString("MMM d, yyyy", CultureInfo.InvariantCulture)}";
}
