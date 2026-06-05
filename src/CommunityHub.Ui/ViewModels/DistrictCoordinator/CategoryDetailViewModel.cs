using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityHub.Application;
using CommunityHub.Application.Services.DistrictCoordinator;
using CommunityHub.Application.Services.DistrictCoordinator.Dto;
using CommunityHub.Ui.ViewModels.Common;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;

public class CategoryDetailViewModel : ViewModelBase
{
    private readonly BudgetService _service;
    private readonly long _districtId;
    private readonly DateTime _start;
    private readonly DateTime _end;

    public CategoryDetailViewModel(long categoryId, string categoryName, long districtId, DateTime start, DateTime end)
    {
        CategoryId   = categoryId;
        CategoryName = categoryName;
        _service     = new BudgetService(Injector.Get<IBudgetRepository>());
        _districtId  = districtId;
        _start       = start;
        _end         = end;

        Donations = new ObservableCollection<DonationDto>();
        Expenses  = new ObservableCollection<ExpenseDto>();

        AddExpenseCommand    = new RelayCommand(_ => AddExpenseRequested?.Invoke(categoryId, districtId));
        ViewExpenseCommand   = new RelayCommand(p => { if (p is ExpenseDto e) ViewExpenseRequested?.Invoke(e); });
        BackCommand          = new RelayCommand(_ => BackRequested?.Invoke());
        CloseCommand         = new RelayCommand(_ => CloseRequested?.Invoke());
    }

    public long   CategoryId   { get; }
    public string CategoryName { get; }

    public ObservableCollection<DonationDto> Donations { get; }
    public ObservableCollection<ExpenseDto>  Expenses  { get; }

    public ICommand AddExpenseCommand  { get; }
    public ICommand ViewExpenseCommand { get; }
    public ICommand BackCommand        { get; }
    public ICommand CloseCommand       { get; }

    public event Action<long, long>?   AddExpenseRequested;
    public event Action<ExpenseDto>?   ViewExpenseRequested;
    public event Action?               BackRequested;
    public event Action?               CloseRequested;

    public void Load()
    {
        try
        {
            var donations = _service.GetDonations(_districtId, CategoryId, _start, _end);
            Donations.Clear();
            foreach (var d in donations) Donations.Add(d);

            var expenses = _service.GetExpenses(_districtId, CategoryId, _start, _end);
            Expenses.Clear();
            foreach (var e in expenses) Expenses.Add(e);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CategoryDetail] Load error: {ex.Message}");
        }
    }
}
