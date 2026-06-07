using System.Windows;
using System.Windows.Controls;
using CommunityHub.Application.Services.BuildingManager.Dto;
using CommunityHub.Ui.Controls;
using CommunityHub.Ui.ViewModels.BuildingManager;

namespace CommunityHub.Ui.Views.BuildingManager.Pages;

public partial class BuildingsPage : Page
{
    private readonly BuildingManagerWindow _shell;
    private readonly BuildingsViewModel _vm;

    public BuildingsPage(BuildingManagerWindow shell)
    {
        InitializeComponent();
        _shell = shell;
        _vm = new BuildingsViewModel(shell.UserId);
        DataContext = _vm;
        _vm.Load();
    }

    private void BuildingCard_Click(object sender, RoutedEventArgs e)
    {
        BuildingDto building = (BuildingDto)((Button)sender).Tag;
        _shell.MainFrame.Navigate(new BuildingDetailPage(_shell, building));
    }

    private void AddBuilding_Click(object sender, RoutedEventArgs e)
    {
        _shell.MainFrame.Navigate(new NewBuildingPage(_shell, _vm.Load));
    }

    private void HelpButton_Click(object sender, RoutedEventArgs e)
    {
        HelpDialog.Show(this,
            "Lista zgrada kojima upravljate.\n\n" +
            "Kliknite zgradu za detalje ili pritisnite + za registraciju nove.");
    }
}
