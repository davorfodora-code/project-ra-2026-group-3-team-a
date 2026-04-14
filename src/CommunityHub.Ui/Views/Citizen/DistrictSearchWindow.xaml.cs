using System.Windows;
using System.Windows.Controls;
using CommunityHub.Application.Database.Repositories.Citizen;
using CommunityHub.Application.Domain;

namespace CommunityHub.Ui.Views.Citizen;

public partial class DistrictSearchWindow : Window
{
    private readonly long _userId;
    private readonly CitizenDistrictDbRepository _districtRepository;
    private List<District> _currentDistricts;

    public DistrictSearchWindow(long userId)
    {
        InitializeComponent();
        _userId = userId;
        _districtRepository = new CitizenDistrictDbRepository();
        _currentDistricts = new List<District>();

        LoadAllDistricts();
    }

    private void LoadAllDistricts()
    {
        _currentDistricts = _districtRepository.GetAll();
        DistrictsDataGrid.ItemsSource = _currentDistricts.Select(d => new DistrictRowViewModel(d)).ToList();
        ImagesBorder.Visibility = Visibility.Collapsed;
    }

    private void PerformSearch()
    {
        string? name = string.IsNullOrWhiteSpace(NameTextBox.Text) ? null : NameTextBox.Text.Trim();
        string? address = string.IsNullOrWhiteSpace(AddressTextBox.Text) ? null : AddressTextBox.Text.Trim();
        string? city = string.IsNullOrWhiteSpace(CityTextBox.Text) ? null : CityTextBox.Text.Trim();
        string? country = string.IsNullOrWhiteSpace(CountryTextBox.Text) ? null : CountryTextBox.Text.Trim();

        _currentDistricts = _districtRepository.Search(name, address, city, country);
        DistrictsDataGrid.ItemsSource = _currentDistricts.Select(d => new DistrictRowViewModel(d)).ToList();
        ImagesBorder.Visibility = Visibility.Collapsed;
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        CitizenHomeWindow homeWindow = new CitizenHomeWindow(_userId);
        homeWindow.Show();
        Close();
    }

    private void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        PerformSearch();
    }

    private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        PerformSearch();
    }

    private void DistrictsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DistrictsDataGrid.SelectedItem is not DistrictRowViewModel selected)
        {
            ImagesBorder.Visibility = Visibility.Collapsed;
            return;
        }

        ImagesBorder.Visibility = Visibility.Visible;

        if (selected.ImageUrls.Count > 0)
        {
            ImagesListBox.ItemsSource = selected.ImageUrls;
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
}

