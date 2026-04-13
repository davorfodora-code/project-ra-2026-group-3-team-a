using System.Windows;
using System.Windows.Controls;
using CommunityHub.Application.Database.Repositories.Tenant;

namespace CommunityHub.Ui.Views.Tenant;

public partial class BuildingAccessRequestForm : Window
{
    private readonly long _userId;
    private readonly long _buildingId;
    private readonly BuildingMembershipDbRepository _membershipRepository;
    private readonly BuildingAccessRequestDbRepository _requestRepository;

    public BuildingAccessRequestForm(long userId, long buildingId)
    {
        InitializeComponent();
        _userId = userId;
        _buildingId = buildingId;
        _membershipRepository = new BuildingMembershipDbRepository();
        _requestRepository = new BuildingAccessRequestDbRepository();
    }

    private void ApartmentNumberTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        WarningTextBlock.Visibility = Visibility.Collapsed;
        ErrorTextBlock.Visibility = Visibility.Collapsed;

        if (!int.TryParse(ApartmentNumberTextBox.Text.Trim(), out int apartmentNumber))
            return;

        if (apartmentNumber <= 0)
            return;

        bool occupied = _membershipRepository.IsApartmentOccupied(_buildingId, apartmentNumber);
        if (occupied)
        {
            WarningTextBlock.Visibility = Visibility.Visible;
        }
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

        if (_requestRepository.ExistsPending(_userId, _buildingId))
        {
            ErrorTextBlock.Text = "Već imate aktivan zahtev za ovu zgradu.";
            ErrorTextBlock.Visibility = Visibility.Visible;
            return;
        }

        _requestRepository.Create(_userId, _buildingId, apartmentNumber);

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
