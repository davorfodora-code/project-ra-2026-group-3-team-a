using System.Windows;
using System.Windows.Controls;
using CommunityHub.Ui.ViewModels.BuildingManager;

namespace CommunityHub.Ui.Views.BuildingManager.Pages;

public partial class BulletinBoardPage : Page
{
    private readonly BuildingManagerWindow _shell;
    private readonly BulletinBoardPageViewModel _vm;

    public BulletinBoardPage(BuildingManagerWindow shell)
    {
        InitializeComponent();
        _shell = shell;
        _vm = new BulletinBoardPageViewModel(shell.UserId);
        DataContext = _vm;
    }

    private void Tab_Click(object sender, RoutedEventArgs e)
    {
        int index = int.Parse((string)((Button)sender).Tag);
        _vm.ActiveTabIndex = index;
    }

    private void StatusFilter_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (_vm == null) return;
        if (StatusFilterCombo.SelectedItem is ComboBoxItem item && item.Tag is string tag)
            _vm.StatusFilter = tag;
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
    }

    private void HelpButton_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            "Oglasi: pregled svih aktivnih i arhiviranih oglasa za odabranu zgradu.\n\n" +
            "Statistika: broj oglasa po tipu i kategoriji za izabrani period,\n" +
            "trenutno stanje oglasne table i stanar koji je najviše pomagao.",
            "Pomoć", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
