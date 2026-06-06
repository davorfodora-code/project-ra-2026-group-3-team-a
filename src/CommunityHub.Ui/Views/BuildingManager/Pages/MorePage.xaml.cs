using System.Windows;
using System.Windows.Controls;
using CommunityHub.Ui.Views;

namespace CommunityHub.Ui.Views.BuildingManager.Pages;

public partial class MorePage : Page
{
    private readonly BuildingManagerWindow _shell;

    public MorePage(BuildingManagerWindow shell)
    {
        InitializeComponent();
        _shell = shell;
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Stranica podešavanja dolazi uskoro.",
            "Podešavanja", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Wizard_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Pokretni vodič dolazi uskoro.",
            "Pokretni vodič", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Logout_Click(object sender, RoutedEventArgs e)
    {
        MessageBoxResult result = MessageBox.Show(
            "Da li ste sigurni da želite da se odjavite?",
            "Odjava", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        LogInForm loginForm = new LogInForm();
        loginForm.Show();
        _shell.Close();
    }

    private void HelpButton_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            "Dodatne opcije: podešavanja aplikacije, pokretni vodič i odjava.",
            "Pomoć", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
