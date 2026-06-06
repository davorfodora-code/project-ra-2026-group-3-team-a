using System.Globalization;

namespace CommunityHub.Application.Services.DistrictCoordinator.Dto;

public class BudgetCategoryDto
{
    public long    Id             { get; set; }
    public string  Name           { get; set; } = string.Empty;
    public decimal TotalDonations { get; set; }
    public decimal TotalExpenses  { get; set; }
    public decimal Balance        => TotalDonations - TotalExpenses;

    public string BalanceDisplay       => FormatRsd(Balance);
    public string DonationsDisplay     => FormatRsd(TotalDonations);
    public string ExpensesDisplay      => FormatRsd(TotalExpenses);

    private static string FormatRsd(decimal v) =>
        v.ToString("N0", CultureInfo.InvariantCulture).Replace(",", ".") + " RSD";
}
