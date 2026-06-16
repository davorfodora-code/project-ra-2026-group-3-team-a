using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityHub.Application.Services.DistrictCoordinator.Dto;
using CommunityHub.Ui.ViewModels.Common;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;

public class RequestListViewModel : ViewModelBase
{
    private List<DistrictAccessRequestDto> _allRequests = new();
    private DistrictDto? _district;
    private string _requestFilter = "Svi";

    public RequestListViewModel()
    {
        Requests = new ObservableCollection<DistrictAccessRequestDto>();
        SetRequestFilterCommand   = new RelayCommand(p => RequestFilter = p?.ToString() ?? "Svi");
        ShowApproveOverlayCommand = new RelayCommand(p => { if (p is DistrictAccessRequestDto d) ApproveRequested?.Invoke(d); });
        ShowRejectOverlayCommand  = new RelayCommand(p => { if (p is DistrictAccessRequestDto d) RejectRequested?.Invoke(d); });
    }

    public ObservableCollection<DistrictAccessRequestDto> Requests { get; }

    private DistrictAccessRequestDto? _selectedRequest;
    public DistrictAccessRequestDto? SelectedRequest
    {
        get => _selectedRequest;
        set => SetProperty(ref _selectedRequest, value);
    }

    public string RequestFilter
    {
        get => _requestFilter;
        set { if (SetProperty(ref _requestFilter, value)) ApplyFilter(); }
    }

    public ICommand SetRequestFilterCommand   { get; }
    public ICommand ShowApproveOverlayCommand { get; }
    public ICommand ShowRejectOverlayCommand  { get; }

    public event Action<DistrictAccessRequestDto>? ApproveRequested;
    public event Action<DistrictAccessRequestDto>? RejectRequested;

    public void Load(IEnumerable<DistrictAccessRequestDto> requests, DistrictDto? district)
    {
        _allRequests    = requests.ToList();
        _district       = district;
        _requestFilter  = "Svi";
        OnPropertyChanged(nameof(RequestFilter));
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        IEnumerable<DistrictAccessRequestDto> filtered = _allRequests;
        if (_district != null)        filtered = filtered.Where(r => r.DistrictName == _district.Name);
        if (_requestFilter != "Svi")  filtered = filtered.Where(r => r.Status == _requestFilter);
        Requests.Clear();
        foreach (DistrictAccessRequestDto r in filtered.OrderByDescending(r => r.CreatedAt)) Requests.Add(r);
    }
}
