using System.Windows;
using System.Windows.Controls;
using CommunityHub.Application.Dto.BuildingManager;
using CommunityHub.Application.Services.BuildingManager;

namespace CommunityHub.Ui.Views.BuildingManager.Pages;

public partial class BuildingsPage : Page
{
    private readonly BuildingManagerWindow _shell;
    private readonly BuildingService _service;

    public BuildingsPage(BuildingManagerWindow shell)
    {
        InitializeComponent();
        _shell = shell;
        _service = new BuildingService();
        LoadBuildings();
    }

    private void LoadBuildings()
    {
        List<BuildingDto> buildings = _service.Search(null, null, null, null);

        if (buildings.Count == 0)
        {
            EmptyText.Visibility = Visibility.Visible;
            return;
        }

        BuildingsItemsControl.ItemsSource = buildings;
    }

    private void BuildingCard_Click(object sender, RoutedEventArgs e)
    {
        BuildingDto building = (BuildingDto)((Button)sender).Tag;
        _shell.MainFrame.Navigate(new BuildingDetailPage(_shell, building));
    }

    private void AddBuilding_Click(object sender, RoutedEventArgs e)
    {
        _shell.MainFrame.Navigate(new NewBuildingPage(_shell));
    }

    private void HelpButton_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            "Lista zgrada kojima upravljate. Kliknite zgradu za detalje ili pritisnite + za registraciju nove.",
            "Pomoć", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
