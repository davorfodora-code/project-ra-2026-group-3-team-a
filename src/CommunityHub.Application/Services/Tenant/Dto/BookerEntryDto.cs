namespace CommunityHub.Application.Services.Tenant.Dto;

public class BookerEntryDto
{
    public string BookerName { get; set; } = string.Empty;
    public bool IsBookerOffering { get; set; }
    public string BookerAdDescription { get; set; } = string.Empty;
    public List<BookedSlotDto> Slots { get; set; } = [];

    public string BookerTypeDisplay => IsBookerOffering ? "Offering help" : "Seeking help";
}
