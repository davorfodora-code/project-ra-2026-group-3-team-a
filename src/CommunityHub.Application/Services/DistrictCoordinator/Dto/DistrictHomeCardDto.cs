using System.Globalization;

namespace CommunityHub.Application.Services.DistrictCoordinator.Dto;

public class DistrictHomeCardDto
{
    public long DistrictId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public decimal Budget { get; set; }
    public int MembersCount { get; set; }
    public int StreetsCount { get; set; }
    public int PendingRequestsCount { get; set; }

    public string LocationDisplay => $"{City}, {Country}";

    public string BudgetDisplay => Budget.ToString("N0", CultureInfo.GetCultureInfo("sr-Latn-RS")) + " RSD";
}
