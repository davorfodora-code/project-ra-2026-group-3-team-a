using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityHub.Application;
using CommunityHub.Application.Services.DistrictCoordinator;
using CommunityHub.Application.Services.DistrictCoordinator.Dto;
using CommunityHub.Ui.ViewModels.Common;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;


public class AccessRequestsViewModel : ViewModelBase
{
    private readonly DistrictAccessRequestService _service;
    private readonly long _coordinatorId;

    private List<DistrictAccessRequestDto> _allRequests = new();
    private List<DistrictMembershipDto>    _allMemberships = new();

    private string _requestFilter = "Svi";
    private string _trustFilter   = "Svi";
    private DistrictDto? _selectedDistrict;

    private bool _isOverlayVisible;
    private string _overlayTitle = string.Empty;
    private bool _isApproveAction;
    private DistrictAccessRequestDto? _pendingRequest;
    private string _rejectionReason = string.Empty;

    public AccessRequestsViewModel(long coordinatorId)
    {
        _coordinatorId = coordinatorId;
        _service       = new DistrictAccessRequestService(
            Injector.Get<IDistrictAccessRequestRepository>(),
            Injector.Get<IDistrictRepository>());

        Requests    = new ObservableCollection<DistrictAccessRequestDto>();
        Districts   = new ObservableCollection<DistrictDto>();
        Memberships = new ObservableCollection<DistrictMembershipDto>();

        SetRequestFilterCommand = new RelayCommand(p => RequestFilter = p?.ToString() ?? "Svi");
        SetTrustFilterCommand   = new RelayCommand(p => TrustFilter   = p?.ToString() ?? "Svi");

        ShowApproveOverlayCommand = new RelayCommand(p =>
        {
            if (p is DistrictAccessRequestDto dto) ShowApproveOverlay(dto);
        });
        ShowRejectOverlayCommand = new RelayCommand(p =>
        {
            if (p is DistrictAccessRequestDto dto) ShowRejectOverlay(dto);
        });

        ConfirmOverlayCommand = new RelayCommand(_ => ConfirmOverlay(), _ => PendingRequest != null);
        CancelOverlayCommand  = new RelayCommand(_ => CancelOverlay());
    }


    public ObservableCollection<DistrictAccessRequestDto> Requests { get; }
    public ObservableCollection<DistrictDto>              Districts { get; }
    public ObservableCollection<DistrictMembershipDto>    Memberships { get; }

    public string RequestFilter
    {
        get => _requestFilter;
        set
        {
            if (SetProperty(ref _requestFilter, value))
                ApplyRequestFilter();
        }
    }

    public string TrustFilter
    {
        get => _trustFilter;
        set
        {
            if (SetProperty(ref _trustFilter, value))
                ApplyTrustFilter();
        }
    }

    public DistrictDto? SelectedDistrict
    {
        get => _selectedDistrict;
        set
        {
            if (SetProperty(ref _selectedDistrict, value) && value != null)
            {
                TrustFilter = "Svi"; // reset
                LoadMemberships(value.Id);
            }
        }
    }

    public bool IsOverlayVisible
    {
        get => _isOverlayVisible;
        private set => SetProperty(ref _isOverlayVisible, value);
    }

    public string OverlayTitle
    {
        get => _overlayTitle;
        private set => SetProperty(ref _overlayTitle, value);
    }

    public bool IsApproveAction
    {
        get => _isApproveAction;
        private set
        {
            if (SetProperty(ref _isApproveAction, value))
            {
                OnPropertyChanged(nameof(IsRejectAction));
                OnPropertyChanged(nameof(OverlayConfirmText));
            }
        }
    }

    public bool IsRejectAction => !_isApproveAction;

    public string OverlayConfirmText => _isApproveAction ? "Odobri pristup" : "Odbij pristup";

    public DistrictAccessRequestDto? PendingRequest
    {
        get => _pendingRequest;
        private set => SetProperty(ref _pendingRequest, value);
    }

    public string RejectionReason
    {
        get => _rejectionReason;
        set => SetProperty(ref _rejectionReason, value);
    }

    public ICommand SetRequestFilterCommand    { get; }
    public ICommand SetTrustFilterCommand      { get; }
    public ICommand ShowApproveOverlayCommand  { get; }
    public ICommand ShowRejectOverlayCommand   { get; }
    public ICommand ConfirmOverlayCommand      { get; }
    public ICommand CancelOverlayCommand       { get; }


    public event Action<string, bool>? NotificationRequested; 

    public void Initialize()
    {
        LoadRequests();
        LoadDistricts();
    }

    private void LoadRequests()
    {
        _allRequests = _service.GetByCoordinatorId(_coordinatorId);
        ApplyRequestFilter();
    }

    private void LoadDistricts()
    {
        Districts.Clear();
        foreach (var d in _service.GetDistrictsByCoordinatorId(_coordinatorId))
            Districts.Add(d);

        if (Districts.Count > 0)
            SelectedDistrict = Districts[0];
    }

    private void LoadMemberships(long districtId)
    {
        _allMemberships = _service.GetMembershipsByDistrictId(districtId);
        ApplyTrustFilter();
    }

    private void ApplyRequestFilter()
    {
        IEnumerable<DistrictAccessRequestDto> filtered = _allRequests;
        if (_requestFilter != "Svi")
            filtered = filtered.Where(r => r.Status == _requestFilter);

        Requests.Clear();
        foreach (var r in filtered.OrderByDescending(r => r.CreatedAt))
            Requests.Add(r);
    }

    private void ApplyTrustFilter()
    {
        IEnumerable<DistrictMembershipDto> filtered = _allMemberships;
        if (_trustFilter != "Svi")
            filtered = filtered.Where(m => m.TrustLevel == _trustFilter);

        Memberships.Clear();
        foreach (var m in filtered)
            Memberships.Add(m);
    }

    private void ShowApproveOverlay(DistrictAccessRequestDto dto)
    {
        PendingRequest   = dto;
        IsApproveAction  = true;
        OverlayTitle     = "Odobravanje pristupa";
        RejectionReason  = string.Empty;
        IsOverlayVisible = true;
    }

    private void ShowRejectOverlay(DistrictAccessRequestDto dto)
    {
        PendingRequest   = dto;
        IsApproveAction  = false;
        OverlayTitle     = "Odbijanje zahteva";
        RejectionReason  = string.Empty;
        IsOverlayVisible = true;
    }

    private void CancelOverlay()
    {
        IsOverlayVisible = false;
        PendingRequest   = null;
    }

    private void ConfirmOverlay()
    {
        if (PendingRequest == null) return;

        try
        {
            if (IsApproveAction)
            {
                _service.Approve(PendingRequest.RequestId);
                NotificationRequested?.Invoke("Zahtev je odobren. Članstvo je kreirano.", true);
            }
            else
            {
                string? reason = string.IsNullOrWhiteSpace(RejectionReason) ? null : RejectionReason.Trim();
                _service.Reject(PendingRequest.RequestId, reason);
                NotificationRequested?.Invoke("Zahtev je odbijen.", true);
            }
        }
        catch (Exception ex)
        {
            NotificationRequested?.Invoke($"Greška: {ex.Message}", false);
        }
        finally
        {
            bool wasApprove = IsApproveAction;
            IsOverlayVisible = false;
            PendingRequest   = null;

            LoadRequests();
            if (wasApprove && SelectedDistrict != null)
                LoadMemberships(SelectedDistrict.Id);
        }
    }
}
