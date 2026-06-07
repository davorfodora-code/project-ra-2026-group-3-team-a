using System.Windows;
using System.Windows.Controls;
using CommunityHub.Ui.Controls;
using CommunityHub.Ui.ViewModels.BuildingManager;

namespace CommunityHub.Ui.Views.BuildingManager.Pages;

public partial class AssembliesPage : Page
{
    private readonly BuildingManagerWindow _shell;
    private readonly AssembliesViewModel   _vm;

    public AssembliesPage(BuildingManagerWindow shell)
    {
        InitializeComponent();
        _shell = shell;
        _vm    = new AssembliesViewModel(shell.UserId);
        DataContext = _vm;
    }

    private void FabAdd_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new NewAssemblyPage(_shell, _vm.Load));
    }

    private void AssemblyCard_Click(object sender, RoutedEventArgs e)
    {
        AssemblyItemViewModel item = (AssemblyItemViewModel)((Button)sender).Tag;
        NavigationService?.Navigate(new AssemblyDetailPage(_shell, item.Id));
    }

    private void HelpButton_Click(object sender, RoutedEventArgs e)
    {
        HelpDialog.Show(this,
            "Pregled svih zakazanih skupština stanara.\n\n" +
            "Kliknite na skupštinu za detalje i upravljanje temama.\n\n" +
            "Dugme + otvara formu za zakazivanje nove skupštine.");
    }
}
