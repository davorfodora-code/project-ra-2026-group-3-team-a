using System.Globalization;

namespace CommunityHub.Application.Dto.Citizen;

public class DonationCategoryDto
{
    public string Name { get; set; } = string.Empty;
}

public class DonationBudgetCategoryDto
{
    public string Category { get; set; } = string.Empty;
    public decimal DonatedAmount { get; set; }
    public decimal SpentAmount { get; set; }
    public decimal AvailableAmount => DonatedAmount - SpentAmount;

    public string DonatedAmountDisplay => FormatMoney(DonatedAmount);
    public string SpentAmountDisplay => FormatMoney(SpentAmount);
    public string AvailableAmountDisplay => FormatMoney(AvailableAmount);

    private static string FormatMoney(decimal amount) =>
        amount.ToString("N2", CultureInfo.GetCultureInfo("sr-RS"));
}

public class DonationExpenseHistoryDto
{
    public long Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime SpentAt { get; set; }

    public string AmountDisplay => Amount.ToString("N2", CultureInfo.GetCultureInfo("sr-RS"));
    public string SpentAtDisplay => SpentAt.ToString("dd.MM.yyyy. HH:mm");
}

public class DonationHistoryDto
{
    public long Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime DonatedAt { get; set; }

    public string AmountDisplay => Amount.ToString("N2", CultureInfo.GetCultureInfo("sr-RS"));
    public string DonatedAtDisplay => DonatedAt.ToString("dd.MM.yyyy. HH:mm");
}

public class DonationDashboardDto
{
    public string DistrictName { get; set; } = string.Empty;
    public decimal TotalBudget { get; set; }
    public List<DonationBudgetCategoryDto> CategoryBudgets { get; set; } = new();
    public List<DonationExpenseHistoryDto> ExpenseHistory { get; set; } = new();
    public List<DonationHistoryDto> MyDonations { get; set; } = new();

    public string TotalBudgetDisplay => TotalBudget.ToString("N2", CultureInfo.GetCultureInfo("sr-RS"));
}

public class CreateDonationDto
{
    public decimal Amount { get; set; }
    public string Category { get; set; } = string.Empty;
}

public class DonationActionResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public static DonationActionResultDto Ok(string message) =>
        new() { Success = true, Message = message };

    public static DonationActionResultDto Fail(string message) =>
        new() { Success = false, Message = message };
}
