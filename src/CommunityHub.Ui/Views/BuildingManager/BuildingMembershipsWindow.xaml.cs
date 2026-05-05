using System.Windows;
using System.Windows.Controls;
using CommunityHub.Application.Dto.BuildingManager;
using CommunityHub.Ui.ViewModels.BuildingManager;

namespace CommunityHub.Ui.Views.BuildingManager;

public partial class BuildingMembershipsWindow : Window
{
    private readonly BuildingMembershipsViewModel _viewModel;

    public BuildingMembershipsWindow(long managerId)
    {
        InitializeComponent();
        _viewModel = new BuildingMembershipsViewModel(managerId);
        LoadBuildings();
    }

    // Učitava listu zgrada upravnika u ComboBox
    private void LoadBuildings()
    {
        List<BuildingDto> buildings = _viewModel.GetBuildingsByManagerId();
        BuildingComboBox.ItemsSource = buildings;

        if (buildings.Count > 0)
            BuildingComboBox.SelectedIndex = 0;
    }

    // Kad se odabere zgrada, učitaj njena članstva
    private void BuildingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (BuildingComboBox.SelectedItem is BuildingDto selected)
            MembershipsDataGrid.ItemsSource = _viewModel.GetMembershipsByBuildingId(selected.Id);
    }
}
