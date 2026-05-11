using System;
using System.Windows.Input;
using CommunityHub.Ui.ViewModels.Common;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;


public class CoordinatorHomeViewModel : ViewModelBase
{
    public long CoordinatorId { get; }

    public CoordinatorHomeViewModel(long coordinatorId)
    {
        CoordinatorId = coordinatorId;

        RegisterDistrictCommand = new RelayCommand(_ => NavigateToRegisterDistrict?.Invoke());
        AccessRequestsCommand   = new RelayCommand(_ => NavigateToAccessRequests?.Invoke());
        StatisticsCommand       = new RelayCommand(_ => NavigateToStatistics?.Invoke());
        LogoutCommand           = new RelayCommand(_ => LogoutRequested?.Invoke());
    }

    public ICommand RegisterDistrictCommand { get; }
    public ICommand AccessRequestsCommand   { get; }
    public ICommand StatisticsCommand       { get; }
    public ICommand LogoutCommand           { get; }


    public event Action? NavigateToRegisterDistrict;
    public event Action? NavigateToAccessRequests;
    public event Action? NavigateToStatistics;
    public event Action? LogoutRequested;
}
