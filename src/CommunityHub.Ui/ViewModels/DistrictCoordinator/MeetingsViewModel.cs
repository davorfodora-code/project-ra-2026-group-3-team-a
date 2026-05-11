using System;
using System.Collections.Generic;
using System.Linq;
using CommunityHub.Application.Dto.DistrictCoordinator;
using CommunityHub.Application.Services.DistrictCoordinator;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;

public class MeetingsViewModel
{
    private readonly DistrictMeetingService _service;
    private readonly long _coordinatorId;
    private List<MeetingDto> _allMeetings;

    public MeetingsViewModel(long coordinatorId)
    {
        _coordinatorId = coordinatorId;
        _service = new DistrictMeetingService();
        _allMeetings = new List<MeetingDto>();
    }

    public void LoadMeetings()
    {
        _allMeetings = _service.GetAllByCoordinatorId(_coordinatorId);
    }

    public List<MeetingDto> GetScheduled()
    {
        return _allMeetings
            .Where(m => m.IsScheduled)
            .OrderBy(m => m.ScheduledDate)
            .ToList();
    }

    public List<MeetingDto> GetInPreparationAndCancelled()
    {
        return _allMeetings
            .Where(m => !m.IsScheduled)
            .OrderByDescending(m => m.DateRangeStart)
            .ToList();
    }

    // Vraća sve sastanke za određeni kvart (za prikaz u StatisticsWindow)
    public List<MeetingDto> GetAllForDistrict(long districtId)
    {
        return _allMeetings
            .Where(m => m.DistrictId == districtId)
            .OrderByDescending(m => m.DateRangeStart)
            .ToList();
    }

    public MeetingDto ScheduleMeeting(CreateMeetingDto dto)
    {
        return _service.ScheduleMeeting(dto);
    }

    // Poziva se kada je 24h pred početni datum — vraća rezultat resolving-a
    public ResolveResult ResolveMeeting(long meetingId)
    {
        return _service.ResolveMeeting(meetingId);
    }

    // Koordinator bira datum u slučaju izjednačenja
    public void ConfirmMeetingDate(long meetingId, DateTime chosenDate)
    {
        _service.ConfirmMeetingDate(meetingId, chosenDate);
    }

    // Vraća sastanke koji su spremni za resolving (24h pred početni datum)
    public List<MeetingDto> GetReadyForResolve()
    {
        return _allMeetings
            .Where(m => m.IsReadyForResolve)
            .ToList();
    }
}
