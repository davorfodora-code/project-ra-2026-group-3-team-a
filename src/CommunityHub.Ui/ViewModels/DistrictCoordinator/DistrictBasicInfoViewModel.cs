using CommunityHub.Ui.ViewModels.Common;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;

public class DistrictBasicInfoViewModel : ViewModelBase
{
    private string _name = string.Empty;
    private string _description = string.Empty;
    private string _city = string.Empty;
    private string _country = string.Empty;

    private string _nameError = string.Empty;
    private string _descriptionError = string.Empty;
    private string _cityError = string.Empty;
    private string _countryError = string.Empty;

    public string Name
    {
        get => _name;
        set { if (SetProperty(ref _name, value)) ValidateName(); }
    }

    public string Description
    {
        get => _description;
        set { if (SetProperty(ref _description, value)) ValidateDescription(); }
    }

    public string City
    {
        get => _city;
        set { if (SetProperty(ref _city, value)) ValidateCity(); }
    }

    public string Country
    {
        get => _country;
        set { if (SetProperty(ref _country, value)) ValidateCountry(); }
    }

    public string NameError
    {
        get => _nameError;
        private set { if (SetProperty(ref _nameError, value)) OnPropertyChanged(nameof(HasNameError)); }
    }
    public bool HasNameError => !string.IsNullOrEmpty(_nameError);

    public string DescriptionError
    {
        get => _descriptionError;
        private set { if (SetProperty(ref _descriptionError, value)) OnPropertyChanged(nameof(HasDescriptionError)); }
    }
    public bool HasDescriptionError => !string.IsNullOrEmpty(_descriptionError);

    public string CityError
    {
        get => _cityError;
        private set { if (SetProperty(ref _cityError, value)) OnPropertyChanged(nameof(HasCityError)); }
    }
    public bool HasCityError => !string.IsNullOrEmpty(_cityError);

    public string CountryError
    {
        get => _countryError;
        private set { if (SetProperty(ref _countryError, value)) OnPropertyChanged(nameof(HasCountryError)); }
    }
    public bool HasCountryError => !string.IsNullOrEmpty(_countryError);

    public bool ValidateAll()
    {
        ValidateName();
        ValidateDescription();
        ValidateCity();
        ValidateCountry();
        return !HasNameError && !HasDescriptionError && !HasCityError && !HasCountryError;
    }

    private void ValidateName()
    {
        NameError = string.IsNullOrWhiteSpace(_name) ? "Naziv kvarta je obavezna stavka." : string.Empty;
    }

    private void ValidateDescription()
    {
        string text = _description.Trim();
        DescriptionError = string.IsNullOrWhiteSpace(text)
            ? "Opis kvarta je obavezna stavka."
            : text.Length < 30
                ? "Opis kvarta mora imati najmanje 30 karaktera."
                : string.Empty;
    }

    private void ValidateCity()
    {
        CityError = string.IsNullOrWhiteSpace(_city) ? "Grad je obavezna stavka." : string.Empty;
    }

    private void ValidateCountry()
    {
        CountryError = string.IsNullOrWhiteSpace(_country) ? "Država je obavezna stavka." : string.Empty;
    }
}
