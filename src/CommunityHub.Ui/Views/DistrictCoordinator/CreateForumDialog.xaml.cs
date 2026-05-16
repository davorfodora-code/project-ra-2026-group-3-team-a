using System.Windows;
using CommunityHub.Ui.ViewModels.DistrictCoordinator;

namespace CommunityHub.Ui.Views.DistrictCoordinator;

public partial class CreateForumDialog : Window
{
    private readonly CreateForumDialogViewModel _vm;

    public CreateForumDialog(long coordinatorId)
    {
        InitializeComponent();
        _vm = new CreateForumDialogViewModel(coordinatorId);
        _vm.CloseRequested += confirmed => DialogResult = confirmed;
        DataContext = _vm;
    }
}
