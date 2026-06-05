using System;
using System.Globalization;

namespace CommunityHub.Application.Services.DistrictCoordinator.Dto;

public class ExpenseDto
{
    public long     Id          { get; set; }
    public decimal  Amount      { get; set; }
    public string   Description { get; set; } = string.Empty;
    public DateTime ExpenseDate { get; set; }

    public string AmountDisplay => Amount.ToString("N0", CultureInfo.InvariantCulture)
                                         .Replace(",", ".") + " RSD";
    public string DateDisplay   => ExpenseDate.ToString("d.M.yyyy");
}
