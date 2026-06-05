using System.Collections.ObjectModel;
using CommunityHub.Application.Services.BuildingManager.Dto;

namespace CommunityHub.Ui.ViewModels.BuildingManager;

public class NewCommonRoomViewModel : BaseViewModel
{
    private readonly CommonRoomsViewModel _commonRoomsVm;
    private readonly BuildingDto _building;

    private string _name = string.Empty;
    private string _description = string.Empty;
    private int _selectedFloor;
    private bool _isMultiDay;
    private string _errorMessage = string.Empty;

    public NewCommonRoomViewModel(BuildingDto building)
    {
        _building = building;
        _commonRoomsVm = new CommonRoomsViewModel();

        AvailableFloors = new ObservableCollection<int>();
        for (int i = 0; i <= building.NumberOfFloors; i++)
            AvailableFloors.Add(i);
    }

    // --- Read-only kontekst forme ---

    public string BuildingLabel => $"Zgrada: {_building.AddressDisplay}";
    public string FloorHint => $"(dostupni spratovi: 0-{_building.NumberOfFloors})";
    public ObservableCollection<int> AvailableFloors { get; }

    // --- TwoWay polja forme ---

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public int SelectedFloor
    {
        get => _selectedFloor;
        set => SetProperty(ref _selectedFloor, value);
    }

    public bool IsMultiDay
    {
        get => _isMultiDay;
        set => SetProperty(ref _isMultiDay, value);
    }

    // --- Greska ---

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

    public bool TrySubmit()
    {
        ErrorMessage = string.Empty;
        string? err = Validate();
        if (err != null) { ErrorMessage = err; return false; }
        try
        {
            _commonRoomsVm.Add(new CreateCommonRoomDto
            {
                BuildingId  = _building.Id,
                Name        = Name.Trim(),
                Description = Description.Trim(),
                FloorNumber = SelectedFloor,
                IsMultiDay  = IsMultiDay
            });
            return true;
        }
        catch (Exception ex) { ErrorMessage = $"Greska: {ex.Message}"; return false; }
    }

    private string? Validate()
    {
        if (string.IsNullOrWhiteSpace(Name.Trim()))        return "Unesite naziv prostorije.";
        if (string.IsNullOrWhiteSpace(Description.Trim())) return "Unesite opis prostorije.";
        return null;
    }
}
