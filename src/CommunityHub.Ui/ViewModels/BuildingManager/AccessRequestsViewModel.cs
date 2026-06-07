using System.Collections.ObjectModel;
using CommunityHub.Application;
using CommunityHub.Application.Services.BuildingManager;
using CommunityHub.Application.Services.BuildingManager.Dto;

namespace CommunityHub.Ui.ViewModels.BuildingManager;

public class AccessRequestsViewModel : BaseViewModel
{
    private readonly BuildingAccessRequestService _accessService;
    private readonly long _managerId;
    private List<BuildingAccessRequestDto> _allRequests = new();
    private string _statusFilter = "Svi";
    private bool   _newestFirst  = true;
    private int    _activeTabIndex;

    public AccessRequestsViewModel(long managerId)
    {
        _managerId       = managerId;
        _accessService   = new BuildingAccessRequestService(Injector.Get<IBuildingAccessRequestRepository>());
        FilteredRequests = new ObservableCollection<AccessRequestItemViewModel>();
        Rental           = new RoomRentalRequestsViewModel(managerId);
        LoadAndApply();
    }

    public RoomRentalRequestsViewModel Rental { get; }

    public int ActiveTabIndex
    {
        get => _activeTabIndex;
        set
        {
            if (!SetProperty(ref _activeTabIndex, value)) return;
            OnPropertyChanged(nameof(IsAccessTabActive));
            OnPropertyChanged(nameof(IsRentalTabActive));
            if (IsAccessTabActive) LoadAndApply();
            if (IsRentalTabActive) Rental.LoadAndApply();
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
}
