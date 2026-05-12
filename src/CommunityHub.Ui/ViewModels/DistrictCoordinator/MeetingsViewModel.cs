using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityHub.Application.Domain;
using CommunityHub.Application.Dto.DistrictCoordinator;
using CommunityHub.Application.Services.DistrictCoordinator;
using CommunityHub.Ui.ViewModels.Common;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;

public class MeetingsViewModel : ViewModelBase
{
    private readonly DistrictMeetingService _service;
    private readonly long _coordinatorId;

    private List<MeetingDto> _allMeetings = new();

    public MeetingsViewModel(long coordinatorId)
    {
        _coordinatorId = coordinatorId;
        _service       = new DistrictMeetingService();

        ScheduledMeetings     = new ObservableCollection<MeetingDto>();
        InPreparationMeetings = new ObservableCollection<MeetingDto>();

        ScheduleNewCommand  = new RelayCommand(_ => RequestNewMeetingDialog());
        ResolveVotesCommand = new RelayCommand(_ => RunResolveCycle());
    }

    public long CoordinatorId => _coordinatorId;

    public ObservableCollection<MeetingDto> ScheduledMeetings     { get; }
    public ObservableCollection<MeetingDto> InPreparationMeetings { get; }

    public ICommand ScheduleNewCommand  { get; }
    public ICommand ResolveVotesCommand { get; }

    public event Action<long, MeetingType?, Action<bool>>? ScheduleMeetingDialogRequested;
    public event Action<MeetingDto, List<DateTime>, Action<DateTime?>>? ResolveTieDialogRequested;
    public event Action<string, string>? NotificationRequested; // (poruka, naslov)

    public void Initialize()
    {
        LoadMeetings();
        RunResolveCycle();
    }

    public void LoadMeetings()
    {
        _allMeetings = _service.GetAllByCoordinatorId(_coordinatorId);
        RefreshBindings();
    }

    private void RefreshBindings()
    {
        ScheduledMeetings.Clear();
        foreach (var m in _allMeetings.Where(m => m.IsScheduled).OrderBy(m => m.ScheduledDate))
            ScheduledMeetings.Add(m);

        InPreparationMeetings.Clear();
        foreach (var m in _allMeetings.Where(m => !m.IsScheduled).OrderByDescending(m => m.DateRangeStart))
            InPreparationMeetings.Add(m);
    }

    public List<MeetingDto> GetAllForDistrict(long districtId)
    {
        return _allMeetings
            .Where(m => m.DistrictId == districtId)
            .OrderByDescending(m => m.DateRangeStart)
            .ToList();
    }

    private void RequestNewMeetingDialog()
    {
        ScheduleMeetingDialogRequested?.Invoke(0, null, success =>
        {
            if (!success) return;
            NotificationRequested?.Invoke("Sastanak je uspešno zakazan!", "Uspeh");
            LoadMeetings();
        });
    }

    public void RunResolveCycle()
    {
        if (_allMeetings.Count == 0) LoadMeetings();

        List<MeetingDto> ready = _allMeetings.Where(m => m.IsReadyForResolve).ToList();
        if (ready.Count == 0) return;

        foreach (MeetingDto meeting in ready)
        {
            ResolveResult result = _service.ResolveMeeting(meeting.Id);

            if (!result.HasVotes)
            {
                NotificationRequested?.Invoke(
                    $"Sastanak '{meeting.MeetingTypeDisplay}' za kvart '{meeting.DistrictName}' " +
                    $"(opseg: {meeting.DateRangeDisplay}) je automatski otkazan jer nema glasova.",
                    "Sastanak otkazan");
            }
            else if (result.IsTie)
            {
                NotificationRequested?.Invoke(
                    $"Glasanje za sastanak '{meeting.MeetingTypeDisplay}' (kvart: '{meeting.DistrictName}') " +
                    $"je izjednačeno. Molimo odaberite jedan od predloženih datuma.",
                    "Izjednačen broj glasova");

                MeetingDto m = meeting;
                ResolveTieDialogRequested?.Invoke(m, result.TiedDates, chosen =>
                {
                    if (chosen.HasValue) _service.ConfirmMeetingDate(m.Id, chosen.Value);
                });
            }
            else
            {
                NotificationRequested?.Invoke(
                    $"Sastanak '{meeting.MeetingTypeDisplay}' za kvart '{meeting.DistrictName}' " +
                    $"je zakazan za {result.WinnerDate:dd.MM.yyyy.} u {meeting.MeetingTimeDisplay}.",
                    "Sastanak zakazan");
            }
        }

        LoadMeetings();
    }
}
