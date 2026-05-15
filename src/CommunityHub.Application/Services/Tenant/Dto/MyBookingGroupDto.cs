using System.Globalization;

namespace CommunityHub.Application.Services.Tenant.Dto;

public class MyBookingGroupDto
{
    public long OwnerAdId { get; set; }
    public long RequesterAdId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public bool IsOwnerOffering { get; set; }
    public string OwnerDescription { get; set; } = string.Empty;
    public DateTime OwnerStartDate { get; set; }
    public DateTime OwnerEndDate { get; set; }
    public string OwnerCategory { get; set; } = string.Empty;
    public bool IsRequesterOffering { get; set; }
    public List<BookedSlotDto> Slots { get; set; } = [];

    public string OwnerTypeDisplay => IsOwnerOffering ? "Offering help" : "Seeking help";

    public string OwnerDateRangeDisplay =>
        $"{OwnerStartDate.ToString("MMM d", CultureInfo.InvariantCulture)} – " +
        $"{OwnerEndDate.ToString("MMM d, yyyy", CultureInfo.InvariantCulture)}";

    public string RequesterAdLabel =>
        $"{(IsRequesterOffering ? "Offering help" : "Seeking help")} - {OwnerCategory}";
}
