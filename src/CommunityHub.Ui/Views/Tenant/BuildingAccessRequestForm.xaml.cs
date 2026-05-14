using System.Windows;
using System.Windows.Controls;
using CommunityHub.Application.Services.BuildingManager.Dto;
using CommunityHub.Application.Services.Tenant.Dto;
using CommunityHub.Ui.ViewModels.Tenant;

namespace CommunityHub.Ui.Views.Tenant;

public partial class BuildingAccessRequestForm : Window
{
    private readonly long _userId;
    private readonly long _buildingId;
    private readonly BuildingAccessRequestViewModel _viewModel;
    private bool _isOccupied;

    public BuildingAccessRequestForm(long userId, long buildingId)
        : this(userId, buildingId, null) { }

    public BuildingAccessRequestForm(long userId, long buildingId, BuildingDto? buildingDto)
    {
        InitializeComponent();
        _userId = userId;
        _buildingId = buildingId;
        _viewModel = new BuildingAccessRequestViewModel();

        if (buildingDto != null)
        {
            BuildingAddressText.Text = buildingDto.AddressDisplay;
            BuildingLocationText.Text = $"{buildingDto.Neighborhood}, {buildingDto.LocationDisplay}";
        }
        else
        {
            BuildingAddressText.Text = $"Zgrada #{buildingId}";
            BuildingLocationText.Text = string.Empty;
        }
    }

    private void ApartmentNumberTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ErrorTextBlock.Visibility = Visibility.Collapsed;
        WarningBorder.Visibility = Visibility.Collapsed;
        _isOccupied = false;

        if (!int.TryParse(ApartmentNumberTextBox.Text.Trim(), out int num) || num <= 0)
            return;

        if (_viewModel.IsApartmentOccupied(_buildingId, num))
        {
            _isOccupied = true;
            WarningTextBlock.Text = $"Stan {num} već ima prijavljenog stanara. Proverite da niste pogrešili broj stana. Zahtev možete i dalje podneti.";
            WarningBorder.Visibility = Visibility.Visible;
            ConfirmButton.Content = "Ipak podnesi";
        }
        else
        {
            ConfirmButton.Content = "Podnesi";
        }
    }

    private void ConfirmButton_Click(object sender, RoutedEventArgs e)
    {
        ErrorTextBlock.Visibility = Visibility.Collapsed;

        if (!int.TryParse(ApartmentNumberTextBox.Text.Trim(), out int num) || num <= 0)
        {
            ErrorTextBlock.Text = "Unesite ispravan broj stana.";
            ErrorTextBlock.Visibility = Visibility.Visible;
            return;
        }

        if (_viewModel.HasPendingRequest(_userId, _buildingId))
        {
            ErrorTextBlock.Text = "Već imate aktivan zahtev za ovu zgradu.";
            ErrorTextBlock.Visibility = Visibility.Visible;
            return;
        }

        _viewModel.Create(new CreateBuildingAccessRequestDto
        {
            TenantId = _userId,
            BuildingId = _buildingId,
            ApartmentNumber = num
        });

        MessageBox.Show("Zahtev za pristup zgradi je uspešno kreiran.", "Uspeh",
            MessageBoxButton.OK, MessageBoxImage.Information);

        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
