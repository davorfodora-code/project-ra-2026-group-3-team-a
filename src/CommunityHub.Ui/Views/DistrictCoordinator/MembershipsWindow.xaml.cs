using System.Windows;
using CommunityHub.Ui.ViewModels.DistrictCoordinator;

namespace CommunityHub.Ui.Views.DistrictCoordinator;

public partial class MembershipsWindow : Window
{
    private readonly MembershipsViewModel _vm;

    public MembershipsWindow(long coordinatorId)
    {
        InitializeComponent();
        _vm = new MembershipsViewModel(coordinatorId);
        DataContext = _vm;
        Loaded += (_, _) => _vm.Initialize();
    }
}
