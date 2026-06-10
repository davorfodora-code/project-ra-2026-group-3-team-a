using System;
using System.Collections.ObjectModel;
using CommunityHub.Application;
using CommunityHub.Application.Services.BuildingManager;
using CommunityHub.Application.Services.BuildingManager.Dto;

namespace CommunityHub.Ui.ViewModels.BuildingManager;

// VM za formu generisanja izveštaja o skupštinama u određenom periodu.
// Trenutno samo validira polja i vraca uspeh — generisanje PDF-a nije implementirano.
public class GenerateReportViewModel : BaseViewModel
{
    private readonly BuildingAccessRequestService _buildingService;
    private readonly long _managerId;

    private BuildingDto? _selectedBuilding;
    private DateTime?    _startDate;
    private DateTime?    _endDate;
    private string       _errorMessage = string.Empty;

    public GenerateReportViewModel(long managerId)
    {
        _managerId       = managerId;
        _buildingService = new BuildingAccessRequestService(
            Injector.Get<IBuildingAccessRequestRepository>());

        Buildings = new ObservableCollection<BuildingDto>(
            _buildingService.GetBuildingsByManagerId(managerId));

        // Default period — od početka tekuće godine do danas
        _startDate = new DateTime(DateTime.Today.Year, 1, 1);
        _endDate   = DateTime.Today;
    }

    public ObservableCollection<BuildingDto> Buildings { get; }

    public BuildingDto? SelectedBuilding
    {
        get => _selectedBuilding;
        set => SetProperty(ref _selectedBuilding, value);
    }

    public DateTime? StartDate
    {
        get => _startDate;
        set => SetProperty(ref _startDate, value);
    }

    public DateTime? EndDate
    {
        get => _endDate;
        set => SetProperty(ref _endDate, value);
    }

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

    public bool TryGenerate()
    {
        ErrorMessage = string.Empty;
        string? err = Validate();
        if (err != null) { ErrorMessage = err; return false; }
        // Generisanje PDF-a nije implementirano — samo vrati uspeh
        return true;
    }

    private string? Validate()
    {
        if (_selectedBuilding == null) return "Odaberite zgradu.";
        if (_startDate == null)        return "Odaberite početni datum.";
        if (_endDate == null)          return "Odaberite krajnji datum.";
        if (_endDate < _startDate)     return "Krajnji datum mora biti posle početnog.";
        return null;
    }
}
