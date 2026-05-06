using System.Windows;
using System.Windows.Controls;
using CommunityHub.Application.Dto.Tenant;
using CommunityHub.Ui.ViewModels.Tenant;

namespace CommunityHub.Ui.Views.Tenant;

public partial class BuildingAccessRequestForm : Window
{
    private readonly long _userId;
    private readonly long _buildingId;
    private readonly BuildingAccessRequestViewModel _viewModel;

    public BuildingAccessRequestForm(long userId, long buildingId)
    {
        InitializeComponent();
        _userId = userId;
        _buildingId = buildingId;
        _viewModel = new BuildingAccessRequestViewModel();
    }

    private void ApartmentNumberTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        WarningTextBlock.Visibility = Visibility.Collapsed;
        ErrorTextBlock.Visibility = Visibility.Collapsed;

        if (!int.TryParse(ApartmentNumberTextBox.Text.Trim(), out int apartmentNumber) || apartmentNumber <= 0)
            return;

        if (_viewModel.IsApartmentOccupied(_buildingId, apartmentNumber))
            WarningTextBlock.Visibility = Visibility.Visible;
    }

    private void ConfirmButton_Click(object sender, RoutedEventArgs e)
    {
        ErrorTextBlock.Visibility = Visibility.Collapsed;

        if (!int.TryParse(ApartmentNumberTextBox.Text.Trim(), out int apartmentNumber) || apartmentNumber <= 0)
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
            ApartmentNumber = apartmentNumber
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
