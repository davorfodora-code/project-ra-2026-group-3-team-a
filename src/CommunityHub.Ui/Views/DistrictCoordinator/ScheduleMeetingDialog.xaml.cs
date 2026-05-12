using System.Windows;
using CommunityHub.Application.Domain;
using CommunityHub.Ui.ViewModels.DistrictCoordinator;

namespace CommunityHub.Ui.Views.DistrictCoordinator;

public partial class ScheduleMeetingDialog : Window
{
    private readonly ScheduleMeetingDialogViewModel _vm;

    public ScheduleMeetingDialog(long districtId, MeetingType? preselectedType)
    {
        InitializeComponent();
        _vm = new ScheduleMeetingDialogViewModel(districtId, preselectedType);
        _vm.CloseRequested += confirmed => DialogResult = confirmed;
        DataContext = _vm;
    }
}
