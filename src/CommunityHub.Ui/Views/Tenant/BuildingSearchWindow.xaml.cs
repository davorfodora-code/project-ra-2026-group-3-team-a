using System.Windows;
using System.Windows.Controls;
using CommunityHub.Application.Database.Repositories.BuildingManager;
using CommunityHub.Application.Domain;

namespace CommunityHub.Ui.Views.Tenant;

public partial class BuildingSearchWindow : Window
{
    private readonly long _userId;
    private readonly BuildingDbRepository _buildingRepository;

    public BuildingSearchWindow(long userId)
    {
        InitializeComponent();
        _userId = userId;
        _buildingRepository = new BuildingDbRepository();
        LoadAllBuildings();
    }

    private void LoadAllBuildings()
    {
        var buildings = _buildingRepository.Search(null, null, null, null);
        BuildingsDataGrid.ItemsSource = buildings.Select(b => new BuildingViewModel(b)).ToList();
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        TenantHomeWindow homeWindow = new TenantHomeWindow(_userId);
        homeWindow.Show();
        this.Close();
    }

    private void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        PerformSearch();
    }

    private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        PerformSearch();
    }

    private void PerformSearch()
    {
        string? address = string.IsNullOrWhiteSpace(AddressTextBox.Text) ? null : AddressTextBox.Text.Trim();
        string? neighborhood = string.IsNullOrWhiteSpace(NeighborhoodTextBox.Text) ? null : NeighborhoodTextBox.Text.Trim();
        string? city = string.IsNullOrWhiteSpace(CityTextBox.Text) ? null : CityTextBox.Text.Trim();
        string? country = string.IsNullOrWhiteSpace(CountryTextBox.Text) ? null : CountryTextBox.Text.Trim();

        var buildings = _buildingRepository.Search(address, neighborhood, city, country);
        BuildingsDataGrid.ItemsSource = buildings.Select(b => new BuildingViewModel(b)).ToList();

        ImagesBorder.Visibility = Visibility.Collapsed;
    }

    private void BuildingsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (BuildingsDataGrid.SelectedItem is BuildingViewModel selected)
        {
            ImagesBorder.Visibility = Visibility.Visible;

            if (selected.ImagePaths.Count > 0)
            {
                ImagesListBox.ItemsSource = selected.ImagePaths;
                ImagesListBox.Visibility = Visibility.Visible;
                NoImagesText.Visibility = Visibility.Collapsed;
            }
            else
            {
                ImagesListBox.ItemsSource = null;
                ImagesListBox.Visibility = Visibility.Collapsed;
                NoImagesText.Visibility = Visibility.Visible;
            }
        }
        else
        {
            ImagesBorder.Visibility = Visibility.Collapsed;
        }
    }
}

public class BuildingViewModel
{
    public string AddressDisplay { get; }
    public string Neighborhood { get; }
    public string LocationDisplay { get; }
    public int NumberOfFloors { get; }
    public int TotalApartments { get; }
    public int ImageCount { get; }
    public List<string> ImagePaths { get; }

    public BuildingViewModel(Building building)
    {
        AddressDisplay = $"{building.Address.Street} {building.Address.StreetNumber}";
        Neighborhood = building.Neighborhood;
        LocationDisplay = $"{building.Location.City}, {building.Location.Country}";
        NumberOfFloors = building.NumberOfFloors;
        TotalApartments = building.TotalApartments;
        ImageCount = building.Images.Count;
        ImagePaths = building.Images.Select(img => img.ImagePath).ToList();
    }
}
