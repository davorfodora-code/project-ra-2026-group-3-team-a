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
        if (!TryGetSelectedDistrict(out District district))
        {
            return;
        }

        if (TryHandleExistingMembership(district))
        {
            return;
        }

        if (!TryGetResidenceStreetParts(out string streetName, out int streetNumber))
        {
            return;
        }

        try
        {
            ProcessDistrictAccess(district, streetName, streetNumber);
        }
        catch (Exception ex)
        {
            ShowMessageBox($"Greška: {ex.Message}", "Greška", MessageBoxImage.Error);
        }
    }

    private bool TryGetSelectedDistrict(out District district)
    {
        district = null!;

        if (DistrictsDataGrid.SelectedItem is not DistrictRowViewModel selectedVm)
        {
            ShowMessageBox("Morate selektovati kvart.", "Upozorenje", MessageBoxImage.Warning);
            return false;
        }

        District? resolved = FindDistrictById(selectedVm.DistrictId);
        if (resolved == null)
        {
            ShowMessageBox("Selektovani kvart nije pronađen.", "Greška", MessageBoxImage.Error);
            return false;
        }

        district = resolved;
        return true;
    }

    private bool TryHandleExistingMembership(District district)
    {
        if (!_membershipRepository.HasMembership(_userId))
        {
            return false;
        }

        long? existingDistrictId = _membershipRepository.GetMembershipDistrictId(_userId);
        if (existingDistrictId == district.Id)
        {
            ShowMessageBox("Već ste član ovog kvarta.", "Informacija", MessageBoxImage.Information);
            return true;
        }

        ShowMessageBox(
            "Već ste član nekog kvarta. Ne možete tražiti pristup drugom kvartu.",
            "Upozorenje",
            MessageBoxImage.Warning);
        return true;
    }

    private bool TryGetResidenceStreetParts(out string streetName, out int streetNumber)
    {
        streetName = string.Empty;
        streetNumber = 0;

        string? rawAddress = _userProfileRepository.GetAddressByUserId(_userId);
        if (string.IsNullOrWhiteSpace(rawAddress))
        {
            ShowMessageBox("Nije moguće pročitati adresu stanovanja iz profila.", "Greška", MessageBoxImage.Error);
            return false;
        }

        if (!TryParseResidenceStreetAndNumber(rawAddress, out streetName, out streetNumber))
        {
            ShowMessageBox(
                "Adresa stanovanja nije u očekivanom formatu (npr. 'Zemunska 10, Novi Sad').",
                "Greška",
                MessageBoxImage.Error);
            return false;
        }

        return true;
    }

    private void ProcessDistrictAccess(District district, string streetName, int streetNumber)
    {
        if (IsAddressInDistrict(streetName, streetNumber, district))
        {
            _membershipRepository.Create(_userId, district.Id, DateTime.Now);
            ShowMessageBox("Adresa se poklapa sa kvartom — automatski ste učlanjeni.", "Uspeh", MessageBoxImage.Information);
            return;
        }

        if (_accessRequestRepository.ExistsPending(_userId, district.Id))
        {
            ShowMessageBox("Već imate aktivan zahtev za ovaj kvart.", "Upozorenje", MessageBoxImage.Warning);
            return;
        }

        _accessRequestRepository.Create(_userId, district.Id);
        ShowMessageBox("Kreiran je zahtev za pristup kvartu (status: Pending).", "Uspeh", MessageBoxImage.Information);
    }

    private static void ShowMessageBox(string text, string caption, MessageBoxImage image)
    {
        MessageBox.Show(text, caption, MessageBoxButton.OK, image);
    }

    private District? FindDistrictById(long districtId)
    {
        return _currentDistricts.FirstOrDefault(d => d.Id == districtId);
    }

    private static bool TryParseResidenceStreetAndNumber(string rawAddress, out string streetName, out int streetNumber)
    {
        string trimmed = rawAddress.Trim();
        int commaIndex = trimmed.IndexOf(',');
        if (commaIndex > 0)
        {
            trimmed = trimmed[..commaIndex].Trim();
        }

        trimmed = trimmed.Trim().TrimEnd('.', ',', ';');

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
        string a = NormalizeStreetToken(districtStreetName);
        string b = NormalizeStreetToken(residenceStreetName);

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

    private static string NormalizeStreetToken(string value)
    {
        return value.Trim()
            .Replace("  ", " ", StringComparison.Ordinal)
            .TrimEnd('.', ',', ';');
    }
}

