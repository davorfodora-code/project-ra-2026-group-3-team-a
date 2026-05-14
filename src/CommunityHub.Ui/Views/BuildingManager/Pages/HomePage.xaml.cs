using System.Windows;
using System.Windows.Controls;
using CommunityHub.Ui.ViewModels.BuildingManager;

namespace CommunityHub.Ui.Views.BuildingManager.Pages;

public partial class HomePage : Page
{
    private readonly BuildingManagerWindow _shell;
    private readonly HomeViewModel _vm;

    public HomePage(BuildingManagerWindow shell)
    {
        InitializeComponent();
        _shell = shell;
        _vm = new HomeViewModel(shell.UserId);
        DataContext = _vm;
    }

    private void HelpButton_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            "Početna stranica prikazuje pregled nadolazeće skupštine i statistiku zahteva.",
            "Pomoć", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void SkupstineLink_Click(object sender, RoutedEventArgs e)
    {
        _shell.SetActiveNav(3);
        _shell.MainFrame.Navigate(new AssembliesPage(_shell));
    }

    private void ZahteviPristup_Click(object sender, RoutedEventArgs e)
    {
        _shell.SetActiveNav(2);
        _shell.MainFrame.Navigate(new RequestsPage(_shell));
    }

    private void ZahteviIznajmljivanje_Click(object sender, RoutedEventArgs e)
    {
        _shell.SetActiveNav(2);
        _shell.MainFrame.Navigate(new RequestsPage(_shell));
    }

    private void NeresenoProblemi_Click(object sender, RoutedEventArgs e)
    {
        _shell.SetActiveNav(4);
        _shell.MainFrame.Navigate(new MorePage(_shell));
    }

    private void AktivniOglasi_Click(object sender, RoutedEventArgs e)
    {
        _shell.SetActiveNav(4);
        _shell.MainFrame.Navigate(new MorePage(_shell));
    }
}
