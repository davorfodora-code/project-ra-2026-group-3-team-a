using System.Windows;
using System.Windows.Controls;
using CommunityHub.Application.Dto.Citizen;
using CommunityHub.Ui.ViewModels.Citizen;

namespace CommunityHub.Ui.Views.Citizen;

public partial class DistrictSearchWindow : Window
{
    private readonly long _userId;
    private readonly DistrictSearchViewModel _viewModel;

    public DistrictSearchWindow(long userId)
    {
        InitializeComponent();
        _userId = userId;
        _viewModel = new DistrictSearchViewModel(userId);

        LoadAllDistricts();
    }

    private void LoadAllDistricts()
    {
        DistrictsDataGrid.ItemsSource = _viewModel.LoadAllDistricts();
        ImagesBorder.Visibility = Visibility.Collapsed;
    }

    private void PerformSearch()
    {
        DistrictSearchFilterDto filter = BuildSearchFilterFromInputs();
        DistrictsDataGrid.ItemsSource = _viewModel.Search(filter);
        ImagesBorder.Visibility = Visibility.Collapsed;
    }

    private DistrictSearchFilterDto BuildSearchFilterFromInputs()
    {
        return new DistrictSearchFilterDto
        {
            Name = string.IsNullOrWhiteSpace(NameTextBox.Text) ? null : NameTextBox.Text.Trim(),
            Address = string.IsNullOrWhiteSpace(AddressTextBox.Text) ? null : AddressTextBox.Text.Trim(),
            City = string.IsNullOrWhiteSpace(CityTextBox.Text) ? null : CityTextBox.Text.Trim(),
            Country = string.IsNullOrWhiteSpace(CountryTextBox.Text) ? null : CountryTextBox.Text.Trim()
        };
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
        UpdateDistrictPreview(DistrictsDataGrid.SelectedItem as DistrictRowViewModel);
    }

    private void UpdateDistrictPreview(DistrictRowViewModel? selected)
    {
        if (selected == null)
        {
            ImagesBorder.Visibility = Visibility.Collapsed;
            AccessDistrictButton.IsEnabled = false;
            return;
        }

        AccessDistrictButton.IsEnabled = true;
        ImagesBorder.Visibility = Visibility.Visible;

        if (selected.ImageUrls.Count > 0)
        {
            ImagesListBox.ItemsSource = selected.ImageUrls;
            ImagesListBox.Visibility = Visibility.Visible;
            NoImagesText.Visibility = Visibility.Collapsed;
            return;
        }

        ImagesListBox.ItemsSource = null;
        ImagesListBox.Visibility = Visibility.Collapsed;
        NoImagesText.Visibility = Visibility.Visible;
    }

    private void AccessDistrictButton_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetSelectedDistrict(out DistrictRowViewModel selectedDistrict))
        {
            return;
        }

        try
        {
            AccessResultDto result = _viewModel.RequestDistrictAccess(selectedDistrict.DistrictId);
            ShowResult(result);
        }
        catch (Exception ex)
        {
            ShowMessageBox($"Greška: {ex.Message}", "Greška", MessageBoxImage.Error);
        }
    }

    private bool TryGetSelectedDistrict(out DistrictRowViewModel selectedDistrict)
    {
        selectedDistrict = null!;

        if (DistrictsDataGrid.SelectedItem is not DistrictRowViewModel selectedVm)
        {
            ShowMessageBox("Morate selektovati kvart.", "Upozorenje", MessageBoxImage.Warning);
            return false;
        }

        selectedDistrict = selectedVm;
        return true;
    }

    private void ShowResult(AccessResultDto result)
    {
        MessageBoxImage image = result.Type switch
        {
            AccessResultType.Success => MessageBoxImage.Information,
            AccessResultType.Warning => MessageBoxImage.Warning,
            _ => MessageBoxImage.Error
        };

        string caption = result.Type switch
        {
            AccessResultType.Success => "Uspeh",
            AccessResultType.Warning => "Upozorenje",
            _ => "Greška"
        };

        ShowMessageBox(result.Message, caption, image);
    }

    private static void ShowMessageBox(string text, string caption, MessageBoxImage image)
    {
        MessageBox.Show(text, caption, MessageBoxButton.OK, image);
    }

    private void HomeNavButton_Click(object sender, RoutedEventArgs e)
    {
        new CitizenHomeWindow(_userId).Show();
        Close();
    }

    private void RequestsNavButton_Click(object sender, RoutedEventArgs e)
    {
        new MyDistrictRequestsWindow(_userId).Show();
        Close();
    }

}

