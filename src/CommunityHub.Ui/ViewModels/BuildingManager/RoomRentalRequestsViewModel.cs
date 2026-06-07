using System.Collections.ObjectModel;
using CommunityHub.Application;
using CommunityHub.Application.Services.BuildingManager;
using CommunityHub.Application.Services.BuildingManager.Dto;

namespace CommunityHub.Ui.ViewModels.BuildingManager;

// Zahtevi za iznajmljivanje zajednickih prostorija — koristi se kao pod-VM
// unutar AccessRequestsViewModel-a (tab "Iznajmljivanje").
public class RoomRentalRequestsViewModel : BaseViewModel
{
    private readonly CommonRoomRequestService _service;
    private readonly long _managerId;
    private List<CommonRoomRequestDto> _all = new();
    private string _statusFilter = "Svi";
    private bool   _newestFirst  = true;

    public RoomRentalRequestsViewModel(long managerId)
    {
        _managerId       = managerId;
        _service         = new CommonRoomRequestService(Injector.Get<ICommonRoomRequestRepository>());
        FilteredRequests = new ObservableCollection<RoomRentalRequestItemViewModel>();
    }

    public ObservableCollection<RoomRentalRequestItemViewModel> FilteredRequests { get; }
    public bool HasRequests   => FilteredRequests.Count > 0;
    public bool HasNoRequests => FilteredRequests.Count == 0;

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

    public void LoadAndApply()
    {
        try { _all = _service.GetByManagerId(_managerId); }
        catch { _all = new List<CommonRoomRequestDto>(); }
        ApplyFilters();
    }

    public List<DateOnly> GetBookedDatesForRoom(long roomId) =>
        _service.GetBookedDatesForRoom(roomId);

    public void ProposeDate(long requestId, DateTime proposedStart, DateTime proposedEnd)
    {
        _service.ProposeDate(requestId, proposedStart, proposedEnd);
        LoadAndApply();
    }

    public void ApproveWithDate(long requestId, DateTime date, DateTime _)
    {
        _service.ApproveWithDate(requestId, date);
        LoadAndApply();
    }

    private void ApplyFilters()
    {
        IEnumerable<CommonRoomRequestDto> filtered = _all;
        if (_statusFilter != "Svi")
            filtered = filtered.Where(r => r.Status == _statusFilter);
        filtered = _newestFirst
            ? filtered.OrderByDescending(r => r.RequestedStartDate)
            : filtered.OrderBy(r => r.RequestedStartDate);
        FilteredRequests.Clear();
        foreach (var dto in filtered)
            FilteredRequests.Add(new RoomRentalRequestItemViewModel(dto));
        OnPropertyChanged(nameof(HasRequests));
        OnPropertyChanged(nameof(HasNoRequests));
    }
}
