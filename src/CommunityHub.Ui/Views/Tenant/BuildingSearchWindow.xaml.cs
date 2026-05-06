using System.Windows;
using System.Windows.Controls;
using CommunityHub.Application.Dto.BuildingManager;
using CommunityHub.Ui.ViewModels.Tenant;

namespace CommunityHub.Ui.Views.Tenant;

public partial class BuildingSearchWindow : Window
{
    private readonly long _userId;
    private readonly BuildingSearchViewModel _viewModel;

    public BuildingSearchWindow(long userId)
    {
        InitializeComponent();
        _userId = userId;
        _viewModel = new BuildingSearchViewModel();
        BuildingsDataGrid.ItemsSource = _viewModel.LoadAll();
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        new TenantHomeWindow(_userId).Show();
        Close();
    }

    private void SearchButton_Click(object sender, RoutedEventArgs e) => PerformSearch();

    private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e) => PerformSearch();

    private void PerformSearch()
    {
        string? address = string.IsNullOrWhiteSpace(AddressTextBox.Text) ? null : AddressTextBox.Text.Trim();
        string? neighborhood = string.IsNullOrWhiteSpace(NeighborhoodTextBox.Text) ? null : NeighborhoodTextBox.Text.Trim();
        string? city = string.IsNullOrWhiteSpace(CityTextBox.Text) ? null : CityTextBox.Text.Trim();
        string? country = string.IsNullOrWhiteSpace(CountryTextBox.Text) ? null : CountryTextBox.Text.Trim();

        BuildingsDataGrid.ItemsSource = _viewModel.Search(address, neighborhood, city, country);
        ImagesBorder.Visibility = Visibility.Collapsed;
    }

    private void AccessRequestButton_Click(object sender, RoutedEventArgs e)
    {
        if (BuildingsDataGrid.SelectedItem is BuildingDto selected)
            new BuildingAccessRequestForm(_userId, selected.Id).ShowDialog();
    }

    private void BuildingsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        AccessRequestButton.IsEnabled = BuildingsDataGrid.SelectedItem != null;

        if (BuildingsDataGrid.SelectedItem is BuildingDto selected)
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
