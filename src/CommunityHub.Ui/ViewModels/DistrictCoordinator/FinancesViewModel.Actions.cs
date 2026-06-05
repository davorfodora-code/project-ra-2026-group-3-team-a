using System;
using CommunityHub.Application.Services.DistrictCoordinator.Dto;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;

public partial class FinancesViewModel
{
    private void OpenAddDonation()
    {
        if (_selectedDistrict == null) return;
        var vm = new AddDonationViewModel(_selectedDistrict.Id, _detailCategoryId, _coordinatorId);
        vm.Saved           += () => { CloseDialog(); LoadDetailData(); LoadGridData(); ShowNotification("Uspešno ste dodali donaciju."); };
        vm.CancelRequested += CloseDialog;
        ActiveDialog = vm;
    }

    private void OpenAddExpense()
    {
        if (_selectedDistrict == null) return;
        var vm = new AddEditExpenseViewModel(_selectedDistrict.Id, _detailCategoryId);
        vm.Saved           += _ => { CloseDialog(); LoadDetailData(); LoadGridData(); ShowNotification("Uspešno ste dodali trošak."); };
        vm.CancelRequested += CloseDialog;
        ActiveDialog = vm;
    }

    private void OpenEditExpense(ExpenseDto expense)
    {
        if (_selectedDistrict == null) return;
        var existing = _budgetService.GetExpenseById(expense.Id);
        if (existing == null) return;
        var vm = new AddEditExpenseViewModel(_selectedDistrict.Id, _detailCategoryId, existing);
        vm.Saved           += _ => { CloseDialog(); LoadDetailData(); LoadGridData(); ShowNotification("Uspešno ste sačuvali izmene."); };
        vm.CancelRequested += CloseDialog;
        ActiveDialog = vm;
    }

    private void DoDeleteExpense(ExpenseDto expense)
    {
        try
        {
            _budgetService.DeleteExpense(expense.Id);
            LoadDetailData();
            LoadGridData();
            ShowNotification("Trošak je obrisan.");
        }
        catch (Exception ex)
        {
            ShowNotification($"Greška: {ex.Message}");
        }
    }

    private void CloseDialog() => ActiveDialog = null;

    private void ShowFullDesc(string? description)
    {
        if (string.IsNullOrWhiteSpace(description)) return;
        FullDescriptionText = description;
        ShowFullDescription = true;
    }

    internal void ShowNotification(string message)
    {
        NotificationText = message;
        HasNotification  = true;
    }
}
