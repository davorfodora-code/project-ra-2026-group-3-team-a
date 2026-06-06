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
        AssembliesCount.Text = _vm.UpcomingAssembliesCount.ToString();
        PendingRequestsCount.Text = _vm.PendingRequestsCount.ToString();
        HelpPostsCount.Text = _vm.ActiveAdsCount.ToString();
        IssuesCount.Text = _vm.UnresolvedIssuesCount.ToString();
    }

    private void NavAssemblies_Click(object sender, RoutedEventArgs e)
    {
        _shell.SetActiveNav(6);
        _shell.MainFrame.Navigate(new AssembliesPage(_shell));
    }

    private void NavHelpBoard_Click(object sender, RoutedEventArgs e)
    {
        _shell.SetActiveNav(2);
        _shell.MainFrame.Navigate(new HelpBoardPage(_shell));
    }

    private void NavIssues_Click(object sender, RoutedEventArgs e)
    {
        _shell.SetActiveNav(5);
        _shell.MainFrame.Navigate(new IssuesPage(_shell));
    }

    private void NavBuildings_Click(object sender, RoutedEventArgs e)
    {
        _shell.SetActiveNav(1);
        _shell.MainFrame.Navigate(new BuildingSearchPage(_shell));
    }
}
