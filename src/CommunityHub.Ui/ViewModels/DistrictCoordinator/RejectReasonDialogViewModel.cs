using System;
using System.Windows.Input;
using CommunityHub.Ui.ViewModels.Common;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;

public class RejectReasonDialogViewModel : ViewModelBase
{
    private string _reason = string.Empty;

    public RejectReasonDialogViewModel()
    {
        ConfirmCommand = new RelayCommand(_ => CloseRequested?.Invoke(true));
        CancelCommand  = new RelayCommand(_ => CloseRequested?.Invoke(false));
    }

    public string Reason
    {
        get => _reason;
        set => SetProperty(ref _reason, value);
    }

    public ICommand ConfirmCommand { get; }
    public ICommand CancelCommand  { get; }

    public event Action<bool>? CloseRequested;
}
