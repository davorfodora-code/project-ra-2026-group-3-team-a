using System.Windows;
using System.Windows.Controls;
using CommunityHub.Ui.ViewModels.Tenant;

namespace CommunityHub.Ui.Views.Tenant.Pages;

public partial class ReportsPage : Page
{
    private readonly TenantWindow _shell;
    private readonly HelpSchedulingReportViewModel _vm;

    public ReportsPage(TenantWindow shell)
    {
        InitializeComponent();
        _shell      = shell;
        _vm         = new HelpSchedulingReportViewModel(shell.UserId);
        DataContext = _vm;
    }

    private void Generate_Click(object sender, RoutedEventArgs e)
    {
        _vm.TryGenerate();
    }
}
