using System;
using System.Collections.Generic;
using CommunityHub.Application.Domain;
using CommunityHub.Application.Services.DistrictCoordinator.Dto;

namespace CommunityHub.Application.Services.DistrictCoordinator;

public interface IBudgetRepository
{
    List<BudgetCategory>    GetCategories();
    List<BudgetCategoryDto> GetCategoryBudgets(long districtId, DateTime start, DateTime end);
    List<DonationDto>       GetDonations(long districtId, long categoryId, DateTime start, DateTime end);
    List<ExpenseDto>        GetExpenses(long districtId, long categoryId, DateTime start, DateTime end);
    ExpenseDto?             GetExpenseById(long expenseId);
    long                    AddExpense(long districtId, long categoryId, decimal amount, string description, DateTime expenseDate);
    void                    UpdateExpense(long expenseId, decimal amount, string description, DateTime expenseDate);
    void                    DeleteExpense(long expenseId);
    void                    AddDonation(long districtId, long categoryId, long citizenId, decimal amount, DateTime donatedAt);
}
