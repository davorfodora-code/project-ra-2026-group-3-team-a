using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityHub.Application;
using CommunityHub.Application.Services.DistrictCoordinator;
using CommunityHub.Application.Services.DistrictCoordinator.Dto;
using CommunityHub.Ui.ViewModels.Common;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;

public class ReservationDialogViewModel : ViewModelBase
{
    private readonly CityObjectService _service;
    private readonly long _cityObjectId;
    private readonly long _districtId;
    private int _step = 1;
    private string _durationHoursText = string.Empty;
    private DateTime? _rangeStart;
    private DateTime? _rangeEnd;
    private CityObjectSlotDto? _selectedSlot;
    private string _validationMessage = string.Empty;

    public ReservationDialogViewModel(long cityObjectId, string cityObjectName, long districtId)
    {
        _cityObjectId  = cityObjectId;
        _districtId    = districtId;
        CityObjectName = cityObjectName;
        _service       = new CityObjectService(Injector.Get<ICityObjectRepository>());

        AvailableSlots = new ObservableCollection<CityObjectSlotDto>();

        FindSlotsCommand = new RelayCommand(_ => DoFindSlots());
        ReserveCommand   = new RelayCommand(_ => DoReserve(), _ => _selectedSlot != null);
        BackCommand      = new RelayCommand(_ => GoToStep1());
        CancelCommand    = new RelayCommand(_ => CancelRequested?.Invoke());
    }

    public string CityObjectName { get; }
    public DateTime MinDate => DateTime.Today;

    public string DurationHoursText
    {
        get => _durationHoursText;
        set => SetProperty(ref _durationHoursText, value);
    }

    public DateTime? RangeStart
    {
        get => _rangeStart;
        set => SetProperty(ref _rangeStart, value);
    }

    public DateTime? RangeEnd
    {
        get => _rangeEnd;
        set => SetProperty(ref _rangeEnd, value);
    }

    public CityObjectSlotDto? SelectedSlot
    {
        get => _selectedSlot;
        set
        {
            if (SetProperty(ref _selectedSlot, value))
                (ReserveCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }

    public string ValidationMessage
    {
        get => _validationMessage;
        private set
        {
            if (SetProperty(ref _validationMessage, value))
                OnPropertyChanged(nameof(HasValidation));
        }
    }

    public bool HasValidation => !string.IsNullOrEmpty(_validationMessage);

    public bool IsStep1 => _step == 1;
    public bool IsStep2 => _step == 2;

    public ObservableCollection<CityObjectSlotDto> AvailableSlots { get; }

    public ICommand FindSlotsCommand { get; }
    public ICommand ReserveCommand   { get; }
    public ICommand BackCommand      { get; }
    public ICommand CancelCommand    { get; }

    public event Action? CancelRequested;
    public event Action? ReservationCompleted;

    private void DoFindSlots()
    {
        ValidationMessage = string.Empty;

        if (!int.TryParse(_durationHoursText, out int hours) || hours <= 0)
        {
            ValidationMessage = "Unesite ispravno trajanje posete u satima (ceo broj veći od 0).";
            return;
        }
        if (_rangeStart == null || _rangeEnd == null)
        {
            ValidationMessage = "Izaberite opseg datuma.";
            return;
        }
        if (_rangeEnd < _rangeStart)
        {
            ValidationMessage = "Krajnji datum mora biti posle početnog.";
            return;
        }

        try
        {
            var slots = _service.FindAvailableSlots(_cityObjectId, hours, _rangeStart.Value, _rangeEnd.Value);
            AvailableSlots.Clear();
            foreach (var slot in slots)
                AvailableSlots.Add(slot);

            SelectedSlot = null;
            GoToStep2();
        }
        catch (Exception ex)
        {
            ValidationMessage = $"Greška pri pretrazi termina: {ex.Message}";
        }
    }

    private void DoReserve()
    {
        if (_selectedSlot == null) return;
        try
        {
            _service.Reserve(_cityObjectId, _districtId, _selectedSlot.StartTime, _selectedSlot.EndTime);
            ReservationCompleted?.Invoke();
        }
        catch (Exception ex)
        {
            ValidationMessage = $"Greška pri rezervaciji: {ex.Message}";
        }
    }

    private void GoToStep2()
    {
        _step = 2;
        OnPropertyChanged(nameof(IsStep1));
        OnPropertyChanged(nameof(IsStep2));
    }

    private void GoToStep1()
    {
        _step = 1;
        ValidationMessage = string.Empty;
        OnPropertyChanged(nameof(IsStep1));
        OnPropertyChanged(nameof(IsStep2));
    }
}
