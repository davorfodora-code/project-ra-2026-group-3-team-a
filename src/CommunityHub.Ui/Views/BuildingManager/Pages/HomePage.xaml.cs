using System.Windows;
using System.Windows.Controls;

namespace CommunityHub.Ui.Views.BuildingManager.Pages;

public partial class HomePage : Page
{
    private readonly BuildingManagerWindow _shell;

    public HomePage(BuildingManagerWindow shell)
    {
        InitializeComponent();
        _shell = shell;
    }

    private void HelpButton_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            "Početna stranica prikazuje pregled nadolazeće skupštine i statistiku zahteva.",
            "Pomoć", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    // Klik na baner skupštine — otvara tab Skupštine
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

    // Problemi i oglasi — zasad vodi na Više (funkcionalnost dolazi u kasnijoj iteraciji)
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
