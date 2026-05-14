using System;
using System.Windows.Input;
using CommunityHub.Application;
using CommunityHub.Application.Domain;
using CommunityHub.Application.Services.DistrictCoordinator;
using CommunityHub.Application.Services.DistrictCoordinator.Dto;
using CommunityHub.Ui.ViewModels.Common;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;


public class ScheduleMeetingDialogViewModel : ViewModelBase
{
    private readonly DistrictMeetingService _service;
    private readonly long _districtId;

    private MeetingType _selectedType;
    private string _meetingTimeText = "20:00";
    private DateTime? _dateRangeStart;
    private DateTime? _dateRangeEnd;
    private string _errorText = string.Empty;

    public ScheduleMeetingDialogViewModel(long districtId, MeetingType? preselectedType)
    {
        _districtId      = districtId;
        _service         = new DistrictMeetingService(
            Injector.Get<IDistrictMeetingRepository>());
        _selectedType    = preselectedType ?? MeetingType.Dobrodoslica;
        _dateRangeStart  = DateTime.Today.AddDays(1);
        _dateRangeEnd    = DateTime.Today.AddDays(5);

        SelectDobrodoslicaCommand = new RelayCommand(_ => SelectedType = MeetingType.Dobrodoslica);
        SelectMotivacijaCommand   = new RelayCommand(_ => SelectedType = MeetingType.Motivacija);
        ConfirmCommand            = new RelayCommand(_ => OnConfirm());
        CancelCommand             = new RelayCommand(_ => CloseRequested?.Invoke(false));
    }

    public MeetingType SelectedType
    {
        get => _selectedType;
        set
        {
            if (SetProperty(ref _selectedType, value))
            {
                OnPropertyChanged(nameof(IsDobrodoslicaSelected));
                OnPropertyChanged(nameof(IsMotivacijaSelected));
            }
        }
    }

    public bool IsDobrodoslicaSelected => _selectedType == MeetingType.Dobrodoslica;
    public bool IsMotivacijaSelected   => _selectedType == MeetingType.Motivacija;

    public string MeetingTimeText
    {
        get => _meetingTimeText;
        set => SetProperty(ref _meetingTimeText, value);
    }

    public DateTime? DateRangeStart
    {
        get => _dateRangeStart;
        set => SetProperty(ref _dateRangeStart, value);
    }

    public DateTime? DateRangeEnd
    {
        get => _dateRangeEnd;
        set => SetProperty(ref _dateRangeEnd, value);
    }

    public string ErrorText
    {
        get => _errorText;
        private set
        {
            if (SetProperty(ref _errorText, value))
                OnPropertyChanged(nameof(HasError));
        }
    }
    public bool HasError => !string.IsNullOrEmpty(_errorText);

    public ICommand SelectDobrodoslicaCommand { get; }
    public ICommand SelectMotivacijaCommand   { get; }
    public ICommand ConfirmCommand            { get; }
    public ICommand CancelCommand             { get; }

    public event Action<bool>? CloseRequested;

    private void OnConfirm()
    {
        ErrorText = string.Empty;

        if (!TimeSpan.TryParseExact(_meetingTimeText.Trim(), @"hh\:mm", null, out TimeSpan meetingTime))
        {
            ErrorText = "Vreme mora biti u formatu HH:mm (npr. 20:00).";
            return;
        }
        if (_dateRangeStart == null)
        {
            ErrorText = "Odaberite početni datum opsega.";
            return;
        }
        if (_dateRangeEnd == null)
        {
            ErrorText = "Odaberite krajnji datum opsega.";
            return;
        }
        if (_dateRangeStart.Value <= DateTime.Today)
        {
            ErrorText = "Početni datum mora biti u budućnosti.";
            return;
        }
        if (_dateRangeEnd.Value < _dateRangeStart.Value)
        {
            ErrorText = "Krajnji datum mora biti posle početnog datuma.";
            return;
        }

        CreateMeetingDto dto = new CreateMeetingDto
        {
            DistrictId     = _districtId,
            MeetingType    = _selectedType,
            MeetingTime    = meetingTime,
            DateRangeStart = _dateRangeStart.Value,
            DateRangeEnd   = _dateRangeEnd.Value
        };

        try
        {
            _service.ScheduleMeeting(dto);
            CloseRequested?.Invoke(true);
        }
        catch (Exception ex)
        {
            ErrorText = $"Greška: {ex.Message}";
        }
    }
}
