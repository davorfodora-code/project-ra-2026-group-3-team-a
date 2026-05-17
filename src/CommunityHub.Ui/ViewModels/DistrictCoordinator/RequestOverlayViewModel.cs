using System;
using System.Windows.Input;
using CommunityHub.Application.Services.DistrictCoordinator.Dto;
using CommunityHub.Ui.ViewModels.Common;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;

public class RequestOverlayViewModel : ViewModelBase
{
    private bool _isVisible;
    private string _title = string.Empty;
    private bool _isApproveAction;
    private DistrictAccessRequestDto? _pendingRequest;
    private string _rejectionReason = string.Empty;

    public RequestOverlayViewModel()
    {
        ConfirmCommand = new RelayCommand(_ => OnConfirm(), _ => PendingRequest != null);
        CancelCommand  = new RelayCommand(_ => Close());
    }

    public bool IsVisible
    {
        get => _isVisible;
        private set => SetProperty(ref _isVisible, value);
    }

    public string Title
    {
        get => _title;
        private set => SetProperty(ref _title, value);
    }

    public bool IsApproveAction
    {
        get => _isApproveAction;
        private set
        {
            if (SetProperty(ref _isApproveAction, value))
            {
                OnPropertyChanged(nameof(IsRejectAction));
                OnPropertyChanged(nameof(ConfirmText));
            }
        }
    }

    public bool IsRejectAction => !_isApproveAction;
    public string ConfirmText  => _isApproveAction ? "Odobri pristup" : "Odbij pristup";

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

    public ICommand ConfirmCommand { get; }
    public ICommand CancelCommand  { get; }

    public event Action<DistrictAccessRequestDto, bool, string?>? ConfirmRequested;

    public void ShowApprove(DistrictAccessRequestDto dto)
    {
        PendingRequest  = dto;
        IsApproveAction = true;
        Title           = "Odobravanje pristupa";
        RejectionReason = string.Empty;
        IsVisible       = true;
    }

    public void ShowReject(DistrictAccessRequestDto dto)
    {
        PendingRequest  = dto;
        IsApproveAction = false;
        Title           = "Odbijanje zahteva";
        RejectionReason = string.Empty;
        IsVisible       = true;
    }

    public void Close()
    {
        IsVisible      = false;
        PendingRequest = null;
    }

    private void OnConfirm()
    {
        if (PendingRequest == null) return;
        string? reason = string.IsNullOrWhiteSpace(RejectionReason) ? null : RejectionReason.Trim();
        ConfirmRequested?.Invoke(PendingRequest, IsApproveAction, reason);
    }
}
