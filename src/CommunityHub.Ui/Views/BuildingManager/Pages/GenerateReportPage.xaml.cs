using System.Windows;
using System.Windows.Controls;
using CommunityHub.Ui.Controls;
using CommunityHub.Ui.ViewModels.BuildingManager;

namespace CommunityHub.Ui.Views.BuildingManager.Pages;

public partial class GenerateReportPage : Page
{
    private readonly BuildingManagerWindow _shell;
    private readonly GenerateReportViewModel _vm;

    public GenerateReportPage(BuildingManagerWindow shell)
    {
        InitializeComponent();
        _shell      = shell;
        _vm         = new GenerateReportViewModel(shell.UserId);
        DataContext = _vm;
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
    }

    private void Generate_Click(object sender, RoutedEventArgs e)
    {
        if (!_vm.TryGenerate()) return;
        _shell.ShowToast("Izveštaj je uspešno generisan.");
        if (NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
    }

    private void HelpButton_Click(object sender, RoutedEventArgs e)
    {
        HelpDialog.Show(this,
            "Izaberite zgradu i vremenski period. " +
            "Klikom na Generiši se kreira PDF izveštaj sa pregledom svih " +
            "skupština i prisustva u izabranom periodu.");
    }
}
