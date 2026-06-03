using System.Windows;
using System.Windows.Controls;
using CommunityHub.Ui.ViewModels.BuildingManager;

namespace CommunityHub.Ui.Views.BuildingManager.Pages;

public partial class AssemblyDetailPage : Page
{
    private readonly BuildingManagerWindow   _shell;
    private readonly AssemblyDetailViewModel _vm;

    public AssemblyDetailPage(BuildingManagerWindow shell, long assemblyId)
    {
        InitializeComponent();
        _shell      = shell;
        _vm         = new AssemblyDetailViewModel(assemblyId);
        DataContext = _vm;
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
    }

    private void HelpButton_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            "Pregled detalja skupštine.\n" +
            "Prisustvo po stanovima prikazuje ko je potvrdio dolazak.",
            "Pomoć", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
