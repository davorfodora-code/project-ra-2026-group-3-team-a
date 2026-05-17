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

    private List<DistrictAccessRequestDto> _allRequests    = new();
    private List<DistrictMembershipDto>    _allMemberships = new();

    private string     _requestFilter    = "Svi";
    private string     _trustFilter      = "Svi";
    private DistrictDto? _selectedDistrict;
    private bool       _isRequestsSection = true;

    public AccessRequestsViewModel(long coordinatorId)
    {
        _coordinatorId = coordinatorId;
        _service       = new DistrictAccessRequestService(
            Injector.Get<IDistrictAccessRequestRepository>(),
            Injector.Get<IDistrictRepository>());

        Requests    = new ObservableCollection<DistrictAccessRequestDto>();
        Districts   = new ObservableCollection<DistrictDto>();
        Memberships = new ObservableCollection<DistrictMembershipDto>();

        Overlay = new RequestOverlayViewModel();
        Overlay.ConfirmRequested += OnOverlayConfirm;

        SetRequestFilterCommand       = new RelayCommand(p => RequestFilter      = p?.ToString() ?? "Svi");
        SetTrustFilterCommand         = new RelayCommand(p => TrustFilter        = p?.ToString() ?? "Svi");
        ShowRequestsSectionCommand    = new RelayCommand(_ => IsRequestsSection  = true);
        ShowMembershipsSectionCommand = new RelayCommand(_ => IsRequestsSection  = false);

        ShowApproveOverlayCommand = new RelayCommand(p => { if (p is DistrictAccessRequestDto d) Overlay.ShowApprove(d); });
        ShowRejectOverlayCommand  = new RelayCommand(p => { if (p is DistrictAccessRequestDto d) Overlay.ShowReject(d); });
    }

    public RequestOverlayViewModel                         Overlay     { get; }
    public ObservableCollection<DistrictAccessRequestDto>  Requests    { get; }
    public ObservableCollection<DistrictDto>               Districts   { get; }
    public ObservableCollection<DistrictMembershipDto>     Memberships { get; }

    public string RequestFilter
    {
        get => _requestFilter;
        set { if (SetProperty(ref _requestFilter, value)) ApplyRequestFilter(); }
    }

    public string TrustFilter
    {
        get => _trustFilter;
        set { if (SetProperty(ref _trustFilter, value)) ApplyTrustFilter(); }
    }

    public DistrictDto? SelectedDistrict
    {
        get => _selectedDistrict;
        set
        {
            if (SetProperty(ref _selectedDistrict, value) && value != null)
            {
                RequestFilter = "Svi";
                TrustFilter   = "Svi";
                ApplyRequestFilter();
                try   { LoadMemberships(value.Id); }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Zahtevi] LoadMemberships error: {ex}");
                    NotificationRequested?.Invoke($"Greška pri učitavanju članstava:\n{ex.Message}", false);
                }
            }
        }
    }

    public bool IsRequestsSection
    {
        get => _isRequestsSection;
        set { if (SetProperty(ref _isRequestsSection, value)) OnPropertyChanged(nameof(IsMembershipsSection)); }
    }

    public bool IsMembershipsSection => !_isRequestsSection;

    public ICommand SetRequestFilterCommand       { get; }
    public ICommand SetTrustFilterCommand         { get; }
    public ICommand ShowRequestsSectionCommand    { get; }
    public ICommand ShowMembershipsSectionCommand { get; }
    public ICommand ShowApproveOverlayCommand     { get; }
    public ICommand ShowRejectOverlayCommand      { get; }

    public event Action<string, bool>? NotificationRequested;

    public void Initialize()
    {
        try   { LoadDistricts(); LoadRequests(); }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Zahtevi] Initialize error: {ex}");
            NotificationRequested?.Invoke($"Greška pri učitavanju:\n{ex.Message}", false);
        }
    }

    private void LoadRequests()
    {
        _allRequests = _service.GetByCoordinatorId(_coordinatorId);
        ApplyRequestFilter();
    }

    private void LoadDistricts()
    {
        Districts.Clear();
        foreach (DistrictDto d in _service.GetDistrictsByCoordinatorId(_coordinatorId)) Districts.Add(d);
        if (Districts.Count > 0) SelectedDistrict = Districts[0];
    }

    private void LoadMemberships(long districtId)
        { _allMemberships = _service.GetMembershipsByDistrictId(districtId); ApplyTrustFilter(); }

    private void ApplyRequestFilter()
    {
        IEnumerable<DistrictAccessRequestDto> filtered = _allRequests;
        if (_selectedDistrict != null)
            filtered = filtered.Where(r => r.DistrictName == _selectedDistrict.Name);
        if (_requestFilter != "Svi")
            filtered = filtered.Where(r => r.Status == _requestFilter);

        Requests.Clear();
        foreach (DistrictAccessRequestDto r in filtered.OrderByDescending(r => r.CreatedAt))
            Requests.Add(r);
    }

    private void ApplyTrustFilter()
    {
        IEnumerable<DistrictMembershipDto> filtered = _allMemberships;
        if (_trustFilter != "Svi")
            filtered = filtered.Where(m => m.TrustLevel == _trustFilter);

        Memberships.Clear();
        foreach (DistrictMembershipDto m in filtered) Memberships.Add(m);
    }

    private void OnOverlayConfirm(DistrictAccessRequestDto request, bool isApprove, string? reason)
    {
        bool wasApprove = isApprove;
        try
        {
            if (isApprove)
            {
                _service.Approve(request.RequestId);
                NotificationRequested?.Invoke("Zahtev je odobren. Članstvo je kreirano.", true);
            }
            else
            {
                _service.Reject(request.RequestId, reason);
                NotificationRequested?.Invoke("Zahtev je odbijen.", true);
            }
        }
        catch (Exception ex)
        {
            NotificationRequested?.Invoke($"Greška: {ex.Message}", false);
        }
        finally
        {
            Overlay.Close();
            LoadRequests();
            if (wasApprove && SelectedDistrict != null) LoadMemberships(SelectedDistrict.Id);
        }
    }
}
