using System.Collections.ObjectModel;
using CommunityHub.Application;
using CommunityHub.Application.Services.BuildingManager;
using CommunityHub.Application.Services.BuildingManager.Dto;

namespace CommunityHub.Ui.ViewModels.BuildingManager;

public class AccessRequestsViewModel : BaseViewModel
{
    private readonly BuildingAccessRequestService _accessService;
    private readonly CommonRoomRequestService _rentalService;
    private readonly long _managerId;

    // --- Access requests stanje ---
    private List<BuildingAccessRequestDto> _allRequests = new();
    private ObservableCollection<AccessRequestItemViewModel> _filteredRequests = new();
    private string _statusFilter = "Svi";
    private bool _newestFirst = true;

    // --- Rental requests stanje ---
    private List<CommonRoomRequestDto> _allRentalRequests = new();
    private ObservableCollection<RoomRentalRequestItemViewModel> _filteredRentalRequests = new();
    private string _rentalStatusFilter = "Svi";
    private bool _rentalNewestFirst = true;

    private int _activeTabIndex;

    public AccessRequestsViewModel(long managerId)
    {
        _managerId = managerId;
        _accessService = new BuildingAccessRequestService(Injector.Get<IBuildingAccessRequestRepository>());
        _rentalService = new CommonRoomRequestService(Injector.Get<ICommonRoomRequestRepository>());
        LoadAndApply();
    }

    // --- Tabovi ---

    public int ActiveTabIndex
    {
        get => _activeTabIndex;
        set
        {
            if (SetProperty(ref _activeTabIndex, value))
            {
                OnPropertyChanged(nameof(IsAccessTabActive));
                OnPropertyChanged(nameof(IsRentalTabActive));
                if (IsAccessTabActive) LoadAndApply();
                if (IsRentalTabActive) LoadRentalAndApply();
            }
        }
    }

    public bool IsAccessTabActive => _activeTabIndex == 0;
    public bool IsRentalTabActive => _activeTabIndex == 1;

    // ================================================================
    // --- Access requests: filter, sort, lista, akcije ---
    // ================================================================

    public string StatusFilter
    {
        get => _statusFilter;
        set { if (SetProperty(ref _statusFilter, value)) ApplyFilters(); }
    }

    public bool NewestFirst
    {
        get => _newestFirst;
        private set => SetProperty(ref _newestFirst, value);
    }

    public void ToggleSort()
    {
        NewestFirst = !NewestFirst;
        ApplyFilters();
    }

    public ObservableCollection<AccessRequestItemViewModel> FilteredRequests
    {
        get => _filteredRequests;
        private set
        {
            SetProperty(ref _filteredRequests, value);
            OnPropertyChanged(nameof(HasRequests));
            OnPropertyChanged(nameof(HasNoRequests));
        }
    }

    public bool HasRequests => _filteredRequests.Count > 0;
    public bool HasNoRequests => _filteredRequests.Count == 0;

    public void LoadAndApply()
    {
        try { _allRequests = _accessService.GetByManagerId(_managerId); }
        catch { _allRequests = new List<BuildingAccessRequestDto>(); }
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        IEnumerable<BuildingAccessRequestDto> filtered = _allRequests;

        if (_statusFilter != "Svi")
            filtered = filtered.Where(r => r.Status == _statusFilter);

        filtered = _newestFirst
            ? filtered.OrderByDescending(r => r.CreatedAt)
            : filtered.OrderBy(r => r.CreatedAt);

        FilteredRequests = new ObservableCollection<AccessRequestItemViewModel>(
            filtered.Select(dto => new AccessRequestItemViewModel(dto)));
    }

    public void Approve(long requestId)
    {
        _accessService.Approve(requestId);
        LoadAndApply();
    }

    public void Reject(long requestId, string? reason)
    {
        _accessService.Reject(requestId, reason);
        LoadAndApply();
    }

    // ================================================================
    // --- Rental requests: filter, sort, lista, akcije ---
    // ================================================================

    public string RentalStatusFilter
    {
        get => _rentalStatusFilter;
        set { if (SetProperty(ref _rentalStatusFilter, value)) ApplyRentalFilters(); }
    }

    public bool RentalNewestFirst
    {
        get => _rentalNewestFirst;
        private set => SetProperty(ref _rentalNewestFirst, value);
    }

    public void ToggleRentalSort()
    {
        RentalNewestFirst = !RentalNewestFirst;
        ApplyRentalFilters();
    }

    public ObservableCollection<RoomRentalRequestItemViewModel> FilteredRentalRequests
    {
        get => _filteredRentalRequests;
        private set
        {
            SetProperty(ref _filteredRentalRequests, value);
            OnPropertyChanged(nameof(HasRentalRequests));
            OnPropertyChanged(nameof(HasNoRentalRequests));
        }
    }

    public bool HasRentalRequests => _filteredRentalRequests.Count > 0;
    public bool HasNoRentalRequests => _filteredRentalRequests.Count == 0;

    public void LoadRentalAndApply()
    {
        try { _allRentalRequests = _rentalService.GetByManagerId(_managerId); }
        catch { _allRentalRequests = new List<CommonRoomRequestDto>(); }
        ApplyRentalFilters();
    }

    private void ApplyRentalFilters()
    {
        IEnumerable<CommonRoomRequestDto> filtered = _allRentalRequests;

        if (_rentalStatusFilter != "Svi")
            filtered = filtered.Where(r => r.Status == _rentalStatusFilter);

        filtered = _rentalNewestFirst
            ? filtered.OrderByDescending(r => r.RequestedStartDate)
            : filtered.OrderBy(r => r.RequestedStartDate);

        FilteredRentalRequests = new ObservableCollection<RoomRentalRequestItemViewModel>(
            filtered.Select(dto => new RoomRentalRequestItemViewModel(dto)));
    }

    // Vraća zauzete datume za prostoriju — koristi RoomCalendarPage za prikaz
    public List<DateOnly> GetBookedDatesForRoom(long roomId)
    {
        return _rentalService.GetBookedDatesForRoom(roomId);
    }

    // Predlaže konkretan termin stanaru (kalendar → Potvrdi)
    public void ProposeDate(long requestId, DateTime proposedStart, DateTime proposedEnd)
    {
        _rentalService.ProposeDate(requestId, proposedStart, proposedEnd);
        LoadRentalAndApply();
    }
}
