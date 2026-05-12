using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityHub.Ui.ViewModels.Common;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;

public class ResolveTieDialogViewModel : ViewModelBase
{
    private DateTime? _selectedDate;

    public ResolveTieDialogViewModel(IEnumerable<DateTime> tiedDates)
    {
        TiedDates = new ObservableCollection<DateTime>(tiedDates.OrderBy(d => d));

        ConfirmCommand = new RelayCommand(_ => OnConfirm());
        CancelCommand  = new RelayCommand(_ => CloseRequested?.Invoke(false));
    }

    public ObservableCollection<DateTime> TiedDates { get; }

    public DateTime? SelectedDate
    {
        get => _selectedDate;
        set => SetProperty(ref _selectedDate, value);
    }

    public ICommand ConfirmCommand { get; }
    public ICommand CancelCommand  { get; }

    public event Action<bool>? CloseRequested;
    public event Action<string>? NotificationRequested;

    private void OnConfirm()
    {
        if (_selectedDate == null)
        {
            NotificationRequested?.Invoke("Morate odabrati datum.");
            return;
        }
        CloseRequested?.Invoke(true);
    }
}
