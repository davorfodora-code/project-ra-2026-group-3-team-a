using System;
using System.Linq;
using CommunityHub.Application.Services.DistrictCoordinator;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;

public partial class FinancesViewModel
{
    private void OpenCategoryDetail(BudgetCategoryCardViewModel card)
    {
        if (_selectedDistrict == null || _selectedPeriod == null) return;
        _detailCategoryId  = card.Id;
        DetailCategoryName = card.Name;
        LoadDetailData();
        IsDetailView = true;
    }

    private void GoBackToGrid()
    {
        IsDetailView        = false;
        ShowFullDescription = false;
        ActiveDialog        = null;
        LoadGridData();
    }

    internal void LoadDetailData()
    {
        if (_selectedDistrict == null || _selectedPeriod == null) return;
        try
        {
            _allDonations = _budgetService.GetDonations(
                _selectedDistrict.Id, _detailCategoryId, _selectedPeriod.Start, _selectedPeriod.End);
            _allExpenses  = _budgetService.GetExpenses(
                _selectedDistrict.Id, _detailCategoryId, _selectedPeriod.Start, _selectedPeriod.End);

            decimal don = _allDonations.Sum(d => d.Amount);
            decimal exp = _allExpenses.Sum(e => e.Amount);
            DetailTotalDonations = BudgetService.FormatRsd(don);
            DetailTotalExpenses  = BudgetService.FormatRsd(exp);
            DetailBalance        = BudgetService.FormatRsd(don - exp);

            _donPage = 1;
            _expPage = 1;
            SortAndPage();
        }
        catch (Exception ex)
        {
            ShowNotification($"Greška pri učitavanju detalja: {ex.Message}");
        }
    }

    private void SortAndPage()
    {
        _allDonations = _donSort == "Datum ↓"
            ? _allDonations.OrderByDescending(d => d.DonatedAt).ToList()
            : _allDonations.OrderBy(d => d.DonatedAt).ToList();

        _allExpenses = _expSort == "Datum ↓"
            ? _allExpenses.OrderByDescending(e => e.ExpenseDate).ToList()
            : _allExpenses.OrderBy(e => e.ExpenseDate).ToList();

        RefreshDonationsPage();
        RefreshExpensesPage();
    }

    internal void RefreshDonationsPage()
    {
        Donations.Clear();
        foreach (var d in _allDonations.Skip((_donPage - 1) * PageSize).Take(PageSize))
            Donations.Add(d);
        OnPropertyChanged(nameof(DonPageLabel));
    }

    internal void RefreshExpensesPage()
    {
        Expenses.Clear();
        foreach (var e in _allExpenses.Skip((_expPage - 1) * PageSize).Take(PageSize))
            Expenses.Add(e);
        OnPropertyChanged(nameof(ExpPageLabel));
    }
}
