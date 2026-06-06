using System.Windows;
using System.Windows.Controls;
using CommunityHub.Ui.Settings;

namespace CommunityHub.Ui.Views.BuildingManager.Pages;

public partial class SettingsPage : Page
{
    private readonly BuildingManagerWindow _shell;
    private bool _isLoading = true;

    public SettingsPage(BuildingManagerWindow shell)
    {
        InitializeComponent();
        _shell = shell;
        TooltipsToggle.IsChecked = AppSettings.TooltipsEnabled;
        _isLoading = false;
    }

    private void TooltipsToggle_Changed(object sender, RoutedEventArgs e)
    {
        if (_isLoading) return;
        AppSettings.TooltipsEnabled = TooltipsToggle.IsChecked == true;
        AppSettings.Save();
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
    }

    private void HelpButton_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            "Tooltipovi: kada je opcija ukljucena, pored polja u aplikaciji se prikazuju mala (i) dugmad sa pomoci.",
            "Pomoć", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
