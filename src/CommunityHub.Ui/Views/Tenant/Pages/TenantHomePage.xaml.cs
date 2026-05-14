using System.Windows;
using System.Windows.Controls;
using CommunityHub.Application.Database.Repositories;
using CommunityHub.Ui.ViewModels.Tenant;

namespace CommunityHub.Ui.Views.Tenant.Pages;

public partial class TenantHomePage : Page
{
    private readonly TenantWindow _shell;
    private readonly TenantHomeViewModel _vm;

    public TenantHomePage(TenantWindow shell)
    {
        InitializeComponent();
        _shell = shell;
        _vm = new TenantHomeViewModel(shell.UserId);

        var user = new UserDbRepository().GetById(shell.UserId);
        WelcomeText.Text = $"Welcome, {user?.Name ?? "Korisnik"}";

        _vm.LoadStats();
        PendingRequestsCount.Text = _vm.PendingRequestsCount.ToString();
        HelpPostsCount.Text = _vm.ActiveAdsCount.ToString();
    }

    private void NavHelpBoard_Click(object sender, RoutedEventArgs e)
    {
        _shell.SetActiveNav(2);
        _shell.MainFrame.Navigate(new HelpBoardPage(_shell));
    }

    private void NavBuildings_Click(object sender, RoutedEventArgs e)
    {
        _shell.SetActiveNav(1);
        _shell.MainFrame.Navigate(new BuildingSearchPage(_shell));
    }
}
