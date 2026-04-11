using System.Windows;
using System.Windows.Controls;
using CommunityHub.Application.Database.Repositories.BuildingManager;
using CommunityHub.Application.Domain;

namespace CommunityHub.Ui.Views.BuildingManager;

// Pomoćna klasa za prikaz zgrada u ComboBox-u
public class BuildingComboItem
{
    public long BuildingId { get; set; }
    public string DisplayText { get; set; }

    public BuildingComboItem(Building building)
    {
        BuildingId = building.Id;
        DisplayText = $"{building.Address.Street} {building.Address.StreetNumber}, " +
                      $"{building.Neighborhood} — {building.Location.City}";
    }
}

// Pomoćna klasa za prikaz članstava u DataGrid-u
public class MembershipRow
{
    public string TenantFullName { get; set; }
    public int FloorNumber { get; set; }
    public int ApartmentNumber { get; set; }
    public DateTime JoinedAt { get; set; }
    public string JoinedAtFormatted => JoinedAt.ToString("dd.MM.yyyy.");

    public MembershipRow(BuildingMembership membership)
    {
        TenantFullName = $"{membership.Tenant.Name} {membership.Tenant.Surname}";
        FloorNumber = membership.FloorNumber;
        ApartmentNumber = membership.ApartmentNumber;
        JoinedAt = membership.JoinedAt;
    }
}

public partial class BuildingMembershipsWindow : Window
{
    private readonly long _managerId;
    private readonly BuildingAccessRequestDbRepository _repository;

    public BuildingMembershipsWindow(long managerId)
    {
        InitializeComponent();
        _managerId = managerId;
        _repository = new BuildingAccessRequestDbRepository();

        LoadBuildings();
    }

    // Učitava listu zgrada upravnika u ComboBox
    private void LoadBuildings()
    {
        List<Building> buildings = _repository.GetBuildingsByManagerId(_managerId);
        List<BuildingComboItem> items = buildings.Select(b => new BuildingComboItem(b)).ToList();

        BuildingComboBox.ItemsSource = items;

        if (items.Count > 0)
        {
            BuildingComboBox.SelectedIndex = 0;
        }
    }

    // Kad se odabere zgrada, učitaj njena članstva
    private void BuildingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (BuildingComboBox.SelectedItem is BuildingComboItem selected)
        {
            List<BuildingMembership> memberships = _repository.GetMembershipsByBuildingId(selected.BuildingId);
            MembershipsDataGrid.ItemsSource = memberships.Select(m => new MembershipRow(m)).ToList();
        }
    }
}
