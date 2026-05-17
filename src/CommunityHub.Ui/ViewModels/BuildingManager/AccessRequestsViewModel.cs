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
    private List<BuildingAccessRequestDto> _allRequests = new();
    private List<CommonRoomRequestDto> _allRentalRequests = new();
    private string _statusFilter = "Svi";
    private string _rentalStatusFilter = "Svi";
    private bool _newestFirst = true;
    private bool _rentalNewestFirst = true;
    private int _activeTabIndex;

    public AccessRequestsViewModel(long managerId)
    {
        _managerId = managerId;
        _accessService = new BuildingAccessRequestService(Injector.Get<IBuildingAccessRequestRepository>());
        _rentalService = new CommonRoomRequestService(Injector.Get<ICommonRoomRequestRepository>());
        FilteredRequests       = new ObservableCollection<AccessRequestItemViewModel>();
        FilteredRentalRequests = new ObservableCollection<RoomRentalRequestItemViewModel>();
        LoadAndApply();
    }

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

    public string StatusFilter
    {
        get => _statusFilter;
        set { if (SetProperty(ref _statusFilter, value)) ApplyFilters(); }
    }

    public void ToggleSort()
    {
        _newestFirst = !_newestFirst;
        ApplyFilters();
    }

    public ObservableCollection<AccessRequestItemViewModel> FilteredRequests { get; }
    public bool HasRequests   => FilteredRequests.Count > 0;
    public bool HasNoRequests => FilteredRequests.Count == 0;

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
        FilteredRequests.Clear();
        foreach (var dto in filtered)
            FilteredRequests.Add(new AccessRequestItemViewModel(dto));
        OnPropertyChanged(nameof(HasRequests));
        OnPropertyChanged(nameof(HasNoRequests));
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

    public string RentalStatusFilter
    {
        get => _rentalStatusFilter;
        set { if (SetProperty(ref _rentalStatusFilter, value)) ApplyRentalFilters(); }
    }

    public void ToggleRentalSort()
    {
        _rentalNewestFirst = !_rentalNewestFirst;
        ApplyRentalFilters();
    }

    public ObservableCollection<RoomRentalRequestItemViewModel> FilteredRentalRequests { get; }
    public bool HasRentalRequests   => FilteredRentalRequests.Count > 0;
    public bool HasNoRentalRequests => FilteredRentalRequests.Count == 0;

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
        FilteredRentalRequests.Clear();
        foreach (var dto in filtered)
            FilteredRentalRequests.Add(new RoomRentalRequestItemViewModel(dto));
        OnPropertyChanged(nameof(HasRentalRequests));
        OnPropertyChanged(nameof(HasNoRentalRequests));
    }

    public List<DateOnly> GetBookedDatesForRoom(long roomId) =>
        _rentalService.GetBookedDatesForRoom(roomId);

    public void ProposeDate(long requestId, DateTime proposedStart, DateTime proposedEnd)
    {
        _rentalService.ProposeDate(requestId, proposedStart, proposedEnd);
        LoadRentalAndApply();
    }

    public void ApproveRentalWithDate(long requestId, DateTime date, DateTime _)
    {
        _rentalService.ApproveWithDate(requestId, date);
        LoadRentalAndApply();
    }
}
