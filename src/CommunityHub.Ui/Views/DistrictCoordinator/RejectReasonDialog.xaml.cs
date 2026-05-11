using System.Windows;
using CommunityHub.Ui.ViewModels.DistrictCoordinator;

namespace CommunityHub.Ui.Views.DistrictCoordinator;

public partial class RejectReasonDialog : Window
{
    private readonly RejectReasonDialogViewModel _vm;

    public string? Reason => string.IsNullOrWhiteSpace(_vm.Reason) ? null : _vm.Reason.Trim();

    public RejectReasonDialog()
    {
        InitializeComponent();
        _vm = new RejectReasonDialogViewModel();
        _vm.CloseRequested += confirmed =>
        {
            DialogResult = confirmed;
        };
        DataContext = _vm;
    }
}
