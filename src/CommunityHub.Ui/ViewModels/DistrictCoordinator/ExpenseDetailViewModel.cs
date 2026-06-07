using System;
using System.Windows.Input;
using CommunityHub.Application.Services.DistrictCoordinator.Dto;
using CommunityHub.Ui.ViewModels.Common;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;

public class ExpenseDetailViewModel : ViewModelBase
{
    public ExpenseDetailViewModel(ExpenseDto expense)
    {
        Expense       = expense;
        BackCommand   = new RelayCommand(_ => BackRequested?.Invoke());
        EditCommand   = new RelayCommand(_ => EditRequested?.Invoke(expense.Id));
        CloseCommand  = new RelayCommand(_ => CloseRequested?.Invoke());
    }

    public ExpenseDto Expense { get; }

    public ICommand BackCommand  { get; }
    public ICommand EditCommand  { get; }
    public ICommand CloseCommand { get; }

    public event Action?       BackRequested;
    public event Action<long>? EditRequested;
    public event Action?       CloseRequested;
}
