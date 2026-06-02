using System.Windows;
using System.Windows.Controls;
using CommunityHub.Application.Dto.Citizen;
using CommunityHub.Application.Services.Citizen;
using CommunityHub.Ui.ViewModels.Citizen;

namespace CommunityHub.Ui.Views.Citizen;

public partial class DistrictSearchWindow : Page
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
        CitizenShellNavigation.Navigate(this, new CitizenHomeWindow(_userId));
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
            bool wasMember = new CitizenMembershipService().HasMembership(_userId);
            AccessResultDto result = _viewModel.RequestDistrictAccess(selectedDistrict.DistrictId);
            ShowResult(result, wasMember);
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

    private void ShowResult(AccessResultDto result, bool wasMember)
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

        MessageBoxResult dialogResult = MessageBox.Show(
            result.Message,
            caption,
            MessageBoxButton.OK,
            image);

        if (dialogResult != MessageBoxResult.OK ||
            result.Type != AccessResultType.Success ||
            wasMember)
        {
            return;
        }

        bool isMemberNow = new CitizenMembershipService().HasMembership(_userId);
        if (isMemberNow)
        {
            CitizenShellNavigation.RefreshMembershipState(this);
        }
    }

    private static void ShowMessageBox(string text, string caption, MessageBoxImage image)
    {
        MessageBox.Show(text, caption, MessageBoxButton.OK, image);
    }

    private void HomeNavButton_Click(object sender, RoutedEventArgs e)
    {
        CitizenShellNavigation.Navigate(this, new CitizenHomeWindow(_userId));
    }

    private void RequestsNavButton_Click(object sender, RoutedEventArgs e)
    {
        CitizenShellNavigation.Navigate(this, new MyDistrictRequestsWindow(_userId));
    }

}

