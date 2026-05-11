using System.Windows;
using CommunityHub.Ui.ViewModels.DistrictCoordinator;

namespace CommunityHub.Ui.Views.DistrictCoordinator;


public partial class CoordinatorHomeWindow : Window
{
    private readonly CoordinatorHomeViewModel _vm;

    public CoordinatorHomeWindow(long userId)
    {
        InitializeComponent();
        _vm = new CoordinatorHomeViewModel(userId);

        _vm.NavigateToRegisterDistrict += () => new DistrictRegWindow(_vm.CoordinatorId).Show();
        _vm.NavigateToAccessRequests   += () => new AccessRequestsWindow(_vm.CoordinatorId).Show();
        _vm.NavigateToStatistics       += () => new StatisticsWindow(_vm.CoordinatorId).Show();
        _vm.LogoutRequested            += OnLogout;

        DataContext = _vm;
    }

    private void OnLogout()
    {
        new LogInForm().Show();
        Close();
    }
}
