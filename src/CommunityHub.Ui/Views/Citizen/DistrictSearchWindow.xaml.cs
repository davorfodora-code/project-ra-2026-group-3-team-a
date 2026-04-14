using System.Windows;
using System.Windows.Controls;
using CommunityHub.Application.Database.Repositories.Citizen;
using CommunityHub.Application.Domain;

namespace CommunityHub.Ui.Views.Citizen;

public partial class DistrictSearchWindow : Window
{
    private readonly long _userId;
    private readonly CitizenDistrictDbRepository _districtRepository;
    private readonly CitizenUserProfileDbRepository _userProfileRepository;
    private readonly CitizenDistrictMembershipDbRepository _membershipRepository;
    private readonly CitizenDistrictAccessRequestDbRepository _accessRequestRepository;
    private List<District> _currentDistricts;

    public DistrictSearchWindow(long userId)
    {
        InitializeComponent();
        _userId = userId;
        _districtRepository = new CitizenDistrictDbRepository();
        _userProfileRepository = new CitizenUserProfileDbRepository();
        _membershipRepository = new CitizenDistrictMembershipDbRepository();
        _accessRequestRepository = new CitizenDistrictAccessRequestDbRepository();
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
        }
        else
        {
            ImagesListBox.ItemsSource = null;
            ImagesListBox.Visibility = Visibility.Collapsed;
            NoImagesText.Visibility = Visibility.Visible;
        }
    }

    private void AccessDistrictButton_Click(object sender, RoutedEventArgs e)
    {
        if (DistrictsDataGrid.SelectedItem is not DistrictRowViewModel selectedVm)
        {
            MessageBox.Show("Morate selektovati kvart.", "Upozorenje",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        District? district = FindDistrictById(selectedVm.DistrictId);
        if (district == null)
        {
            MessageBox.Show("Selektovani kvart nije pronadjen.", "Greska",
                MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (_membershipRepository.HasMembership(_userId))
        {
            long? existingDistrictId = _membershipRepository.GetMembershipDistrictId(_userId);
            if (existingDistrictId == district.Id)
            {
                MessageBox.Show("Vec ste clan ovog kvarta.", "Informacija",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MessageBox.Show("Vec ste clan nekog kvarta. Ne mozete traziti pristup drugom kvartu.", "Upozorenje",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (HasAnyPendingRequest())
        {
            MessageBox.Show(
                "Vec imate zahtev u statusu 'Pending'. Obrisite ga pre nego sto podnesete novi zahtev.",
                "Upozorenje",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        string? rawAddress = _userProfileRepository.GetAddressByUserId(_userId);
        if (string.IsNullOrWhiteSpace(rawAddress))
        {
            MessageBox.Show("Nije moguce procitati adresu stanovanja iz profila.", "Greska",
                MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (!TryParseResidenceStreetAndNumber(rawAddress, out string streetName, out int streetNumber))
        {
            MessageBox.Show(
                "Adresa stanovanja nije u ocekivanom formatu (npr. 'Zemunska 10, Novi Sad').",
                "Greska",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        try
        {
            if (IsAddressInDistrict(streetName, streetNumber, district))
            {
                _membershipRepository.Create(_userId, district.Id, DateTime.Now);
                MessageBox.Show("Adresa se poklapa sa kvartom — automatski ste uclanjeni.", "Uspeh",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (_accessRequestRepository.ExistsPending(_userId, district.Id))
            {
                MessageBox.Show("Vec imate aktivan zahtev za ovaj kvart.", "Upozorenje",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _accessRequestRepository.Create(_userId, district.Id);
            MessageBox.Show("Kreiran je zahtev za pristup kvartu (status: Pending).", "Uspeh",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Greska: {ex.Message}", "Greska",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private District? FindDistrictById(long districtId)
    {
        return _currentDistricts.FirstOrDefault(d => d.Id == districtId);
    }

    private bool HasAnyPendingRequest()
    {
        List<DistrictAccessRequest> requests = _accessRequestRepository.GetAllByUserId(_userId);
        return requests.Any(r => r.Status == DistrictAccessRequestStatus.Pending);
    }

    private static bool TryParseResidenceStreetAndNumber(string rawAddress, out string streetName, out int streetNumber)
    {
        string trimmed = rawAddress.Trim();
        int commaIndex = trimmed.IndexOf(',');
        if (commaIndex > 0)
        {
            trimmed = trimmed[..commaIndex].Trim();
        }

        int lastSpace = trimmed.LastIndexOf(' ');
        if (lastSpace <= 0)
        {
            streetName = string.Empty;
            streetNumber = 0;
            return false;
        }

        string lastToken = trimmed[(lastSpace + 1)..].Trim();
        if (!int.TryParse(lastToken, out streetNumber) || streetNumber <= 0)
        {
            streetName = string.Empty;
            streetNumber = 0;
            return false;
        }

        streetName = trimmed[..lastSpace].Trim();
        return !string.IsNullOrWhiteSpace(streetName);
    }

    private static bool IsAddressInDistrict(string residenceStreetName, int residenceStreetNumber, District district)
    {
        string normalizedResidenceStreet = residenceStreetName.Trim();

        foreach (DistrictStreet street in district.Streets)
        {
            if (!IsSameStreetName(street.StreetName, normalizedResidenceStreet))
            {
                continue;
            }

            if (residenceStreetNumber >= street.StartNumber && residenceStreetNumber <= street.EndNumber)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsSameStreetName(string districtStreetName, string residenceStreetName)
    {
        string a = districtStreetName.Trim();
        string b = residenceStreetName.Trim();

        if (string.Equals(a, b, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Tolerancija za male varijacije u formatu (npr. "Bulevar ..." vs "...")
        if (a.Contains(b, StringComparison.OrdinalIgnoreCase) || b.Contains(a, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }
}

