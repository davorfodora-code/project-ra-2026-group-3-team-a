using System.Collections.ObjectModel;
using CommunityHub.Application.Services.BuildingManager.Dto;

namespace CommunityHub.Ui.ViewModels.BuildingManager;

public class NewBuildingViewModel : BaseViewModel
{
    private readonly BuildingRegistrationViewModel _regVm;
    private readonly long _managerId;
    private string _addressInput = string.Empty;
    private string _neighborhood = string.Empty;
    private string _city = string.Empty;
    private string _country = "Srbija";
    private string _floorsText = string.Empty;
    private string _errorMessage = string.Empty;

    public NewBuildingViewModel(long managerId)
    {
        _managerId = managerId;
        _regVm = new BuildingRegistrationViewModel();
        Floors = new ObservableCollection<FloorInputViewModel>();
        ImagePaths = new ObservableCollection<string>();
    }

    public string AddressInput
    {
        get => _addressInput;
        set => SetProperty(ref _addressInput, value);
    }

    public string Neighborhood
    {
        get => _neighborhood;
        set => SetProperty(ref _neighborhood, value);
    }

    public string City
    {
        get => _city;
        set => SetProperty(ref _city, value);
    }

    public string Country
    {
        get => _country;
        set => SetProperty(ref _country, value);
    }

    public string FloorsText
    {
        get => _floorsText;
        set
        {
            if (SetProperty(ref _floorsText, value))
                RegenerateFloors();
        }
    }

    public ObservableCollection<FloorInputViewModel> Floors { get; }
    public ObservableCollection<string> ImagePaths { get; }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            if (SetProperty(ref _errorMessage, value))
                OnPropertyChanged(nameof(HasError));
        }
    }
    public bool HasError => !string.IsNullOrEmpty(_errorMessage);

    private void RegenerateFloors()
    {
        Floors.Clear();
        if (!int.TryParse(_floorsText, out int n) || n <= 0 || n > 50) return;
        for (int i = 1; i <= n; i++)
            Floors.Add(new FloorInputViewModel(i));
    }

    public void AddImage(string path)
    {
        if (!ImagePaths.Contains(path))
            ImagePaths.Add(path);
    }

    public void RemoveImage(string path) => ImagePaths.Remove(path);

    public bool TrySubmit()
    {
        ErrorMessage = string.Empty;
        string? err = Validate();
        if (err != null) { ErrorMessage = err; return false; }
        try { _regVm.RegisterBuilding(BuildDto()); return true; }
        catch (Exception ex) { ErrorMessage = $"Greska: {ex.Message}"; return false; }
    }

    private string? Validate()
    {
        if (string.IsNullOrWhiteSpace(_addressInput))
            return "Unesite adresu zgrade.";
        if (_addressInput.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).Length < 2)
            return "Adresa mora sadrzati ulicu i kucni broj (npr. Nemanjina 4).";
        if (string.IsNullOrWhiteSpace(_neighborhood)) return "Unesite naziv naselja.";
        if (string.IsNullOrWhiteSpace(_city))         return "Unesite grad.";
        if (string.IsNullOrWhiteSpace(_country))      return "Unesite drzavu.";
        if (!int.TryParse(_floorsText, out int f) || f <= 0)
            return "Broj spratova mora biti pozitivan broj.";
        if (ImagePaths.Count == 0) return "Dodajte bar jednu sliku zgrade.";
        foreach (FloorInputViewModel floor in Floors)
            if (floor.Apartments.Count == 0)
                return $"Sprat {floor.FloorNumber} mora imati bar jedan stan.";
        return null;
    }

    private CreateBuildingDto BuildDto()
    {
        string[] parts = _addressInput.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string streetNumber = parts[^1];
        string street = string.Join(" ", parts[..^1]);
        int.TryParse(_floorsText, out int floors);
        CreateBuildingDto dto = new CreateBuildingDto
        {
            Street = street, StreetNumber = streetNumber,
            Neighborhood = _neighborhood.Trim(), City = _city.Trim(), Country = _country.Trim(),
            NumberOfFloors = floors, ManagerId = _managerId,
            ImagePaths = new List<string>(ImagePaths)
        };
        foreach (FloorInputViewModel floor in Floors)
            dto.Floors.Add(new FloorInputDto { FloorNumber = floor.FloorNumber, ApartmentNumbers = floor.Apartments.ToList() });
        return dto;
    }
}
