using System;
using System.Windows.Input;
using CommunityHub.Application.Services.DistrictCoordinator.Dto;
using CommunityHub.Ui.ViewModels.Common;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;

public class BudgetCategoryCardViewModel : ViewModelBase
{
    public BudgetCategoryCardViewModel(BudgetCategoryDto dto, Action<BudgetCategoryCardViewModel> onDetail)
    {
        Id             = dto.Id;
        Name           = dto.Name;
        BalanceDisplay = dto.BalanceDisplay;
        DonationsDisplay = dto.DonationsDisplay;
        ExpensesDisplay  = dto.ExpensesDisplay;
        OpenDetailCommand = new RelayCommand(_ => onDetail(this));
    }

    public long   Id               { get; }
    public string Name             { get; }
    public string BalanceDisplay   { get; }
    public string DonationsDisplay { get; }
    public string ExpensesDisplay  { get; }

    public ICommand OpenDetailCommand { get; }
}
