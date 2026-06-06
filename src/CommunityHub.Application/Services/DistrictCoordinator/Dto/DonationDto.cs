using System;
using System.Globalization;

namespace CommunityHub.Application.Services.DistrictCoordinator.Dto;

public class DonationDto
{
    public decimal  Amount    { get; set; }
    public DateTime DonatedAt { get; set; }

    public string AmountDisplay => Amount.ToString("N0", CultureInfo.InvariantCulture)
                                         .Replace(",", ".") + " RSD";
    public string DateDisplay   => DonatedAt.ToString("d.M.yyyy");
}
