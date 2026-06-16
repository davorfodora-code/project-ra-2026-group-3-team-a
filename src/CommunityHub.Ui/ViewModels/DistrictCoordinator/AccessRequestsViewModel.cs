using System;
using System.Collections.ObjectModel;
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
    private DistrictDto? _selectedDistrict;
    private bool _isRequestsSection = true;

    public AccessRequestsViewModel(long coordinatorId)
    {
        _coordinatorId = coordinatorId;
        _service = new DistrictAccessRequestService(
            Injector.Get<IDistrictAccessRequestRepository>(),
            Injector.Get<IDistrictRepository>());

        Districts      = new ObservableCollection<DistrictDto>();
        RequestList    = new RequestListViewModel();
        MembershipList = new MembershipListViewModel();
        Overlay        = new RequestOverlayViewModel();

        Overlay.ConfirmRequested        += OnOverlayConfirm;
        RequestList.ApproveRequested    += d => Overlay.ShowApprove(d);
        RequestList.RejectRequested     += d => Overlay.ShowReject(d);
        ShowRequestsSectionCommand    = new RelayCommand(_ => IsRequestsSection = true);
        ShowMembershipsSectionCommand = new RelayCommand(_ => IsRequestsSection = false);
        RefreshCommand         = new RelayCommand(_ => Initialize());
        CloseOverlayCommand    = new RelayCommand(_ => Overlay.Close());
        ApproveSelectedCommand = new RelayCommand(
            _ => Overlay.ShowApprove(RequestList.SelectedRequest!),
            _ => RequestList.SelectedRequest != null);
        RejectSelectedCommand  = new RelayCommand(
            _ => Overlay.ShowReject(RequestList.SelectedRequest!),
            _ => RequestList.SelectedRequest != null);
        RequestList.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(RequestList.SelectedRequest))
            {
                (ApproveSelectedCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (RejectSelectedCommand  as RelayCommand)?.RaiseCanExecuteChanged();
            }
        };
    }

    public RequestOverlayViewModel                Overlay        { get; }
    public ObservableCollection<DistrictDto>      Districts      { get; }
    public RequestListViewModel                   RequestList    { get; }
    public MembershipListViewModel                MembershipList { get; }

    public DistrictDto? SelectedDistrict
    {
        get => _selectedDistrict;
        set
        {
            if (SetProperty(ref _selectedDistrict, value) && value != null)
            {
                RequestList.Load(_service.GetByCoordinatorId(_coordinatorId), value);
                try   { MembershipList.Load(_service.GetMembershipsByDistrictId(value.Id)); }
                catch (Exception ex) { NotificationRequested?.Invoke($"Greška pri učitavanju članstava:\n{ex.Message}", false); }
            }
        }
    }

    public bool IsRequestsSection
    {
        get => _isRequestsSection;
        set { if (SetProperty(ref _isRequestsSection, value)) OnPropertyChanged(nameof(IsMembershipsSection)); }
    }
    public bool IsMembershipsSection => !_isRequestsSection;

    public ICommand ShowRequestsSectionCommand    { get; }
    public ICommand ShowMembershipsSectionCommand { get; }
    public ICommand RefreshCommand                { get; }
    public ICommand CloseOverlayCommand           { get; }
    public ICommand ApproveSelectedCommand        { get; }
    public ICommand RejectSelectedCommand         { get; }

    public event Action<string, bool>? NotificationRequested;

    public void Initialize()
    {
        try   { LoadDistricts(); }
        catch (Exception ex) { NotificationRequested?.Invoke($"Greška pri učitavanju:\n{ex.Message}", false); }
    }

    private void LoadDistricts()
    {
        Districts.Clear();
        foreach (DistrictDto d in _service.GetDistrictsByCoordinatorId(_coordinatorId)) Districts.Add(d);
        if (Districts.Count > 0) SelectedDistrict = Districts[0];
    }

    private void OnOverlayConfirm(DistrictAccessRequestDto request, bool isApprove, string? reason)
    {
        try
        {
            if (isApprove) { _service.Approve(request.RequestId); NotificationRequested?.Invoke("Zahtev je odobren. Članstvo je kreirano.", true); }
            else           { _service.Reject(request.RequestId, reason); NotificationRequested?.Invoke("Zahtev je odbijen.", true); }
        }
        catch (Exception ex) { NotificationRequested?.Invoke($"Greška: {ex.Message}", false); }
        finally
        {
            Overlay.Close();
            RequestList.Load(_service.GetByCoordinatorId(_coordinatorId), _selectedDistrict);
            if (isApprove && _selectedDistrict != null)
                MembershipList.Load(_service.GetMembershipsByDistrictId(_selectedDistrict.Id));
        }
    }
}
