using System.Collections.ObjectModel;
using CommunityHub.Application;
using CommunityHub.Application.Services.BuildingManager;
using CommunityHub.Application.Services.BuildingManager.Dto;

namespace CommunityHub.Ui.ViewModels.BuildingManager;

public class AccessRequestsViewModel : BaseViewModel
{
    private readonly BuildingAccessRequestService _service;
    private readonly long _managerId;

    private List<BuildingAccessRequestDto> _allRequests = new();
    private ObservableCollection<AccessRequestItemViewModel> _filteredRequests = new();
    private string _statusFilter = "Svi";
    private bool _newestFirst = true;
    private int _activeTabIndex;

    public AccessRequestsViewModel(long managerId)
    {
        _managerId = managerId;
        _service = new BuildingAccessRequestService(Injector.Get<IBuildingAccessRequestRepository>());
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
            }
        }
    }

    public bool IsAccessTabActive => _activeTabIndex == 0;
    public bool IsRentalTabActive => _activeTabIndex == 1;

    // --- Filter i sort ---

    public string StatusFilter
    {
        get => _statusFilter;
        set
        {
            if (SetProperty(ref _statusFilter, value))
                ApplyFilters();
        }
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

    // --- Lista zahteva ---

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
        try { _allRequests = _service.GetByManagerId(_managerId); }
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

    // --- Akcije ---

    public void Approve(long requestId)
    {
        _service.Approve(requestId);
        LoadAndApply();
    }

    public void Reject(long requestId, string? reason)
    {
        _service.Reject(requestId, reason);
        LoadAndApply();
    }
}
