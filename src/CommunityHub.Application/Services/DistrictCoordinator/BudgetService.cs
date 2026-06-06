using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CommunityHub.Application.Domain;
using CommunityHub.Application.Services.DistrictCoordinator.Dto;

namespace CommunityHub.Application.Services.DistrictCoordinator;

public class BudgetService
{
    private readonly IBudgetRepository _repo;

    public BudgetService(IBudgetRepository repo) => _repo = repo;

    public List<BudgetCategory>    GetCategories()                                                      => _repo.GetCategories();
    public List<BudgetCategoryDto> GetCategoryBudgets(long districtId, DateTime start, DateTime end)    => _repo.GetCategoryBudgets(districtId, start, end);
    public List<DonationDto>       GetDonations(long districtId, long categoryId, DateTime start, DateTime end) => _repo.GetDonations(districtId, categoryId, start, end);
    public List<ExpenseDto>        GetExpenses(long districtId, long categoryId, DateTime start, DateTime end)  => _repo.GetExpenses(districtId, categoryId, start, end);
    public ExpenseDto?             GetExpenseById(long expenseId)                                       => _repo.GetExpenseById(expenseId);

    public (decimal TotalDonations, decimal TotalExpenses, decimal Saldo) GetSummary(
        long districtId, DateTime start, DateTime end)
    {
        var cats    = _repo.GetCategoryBudgets(districtId, start, end);
        decimal don = cats.Sum(c => c.TotalDonations);
        decimal exp = cats.Sum(c => c.TotalExpenses);
        return (don, exp, don - exp);
    }

    public long AddExpense(long districtId, long categoryId, decimal amount, string description, DateTime expenseDate)
        => _repo.AddExpense(districtId, categoryId, amount, description, expenseDate);

    public void UpdateExpense(long expenseId, decimal amount, string description, DateTime expenseDate)
        => _repo.UpdateExpense(expenseId, amount, description, expenseDate);

    public void DeleteExpense(long expenseId)
        => _repo.DeleteExpense(expenseId);

    public void AddDonation(long districtId, long categoryId, long citizenId, decimal amount, DateTime donatedAt)
        => _repo.AddDonation(districtId, categoryId, citizenId, amount, donatedAt);

    public static string FormatRsd(decimal v) =>
        v.ToString("N0", CultureInfo.InvariantCulture).Replace(",", ".") + " RSD";
}
