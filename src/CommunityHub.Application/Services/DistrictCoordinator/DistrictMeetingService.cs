using System;
using System.Collections.Generic;
using System.Linq;
using CommunityHub.Application.Database.Repositories.DistrictCoordinator;
using CommunityHub.Application.Domain;
using CommunityHub.Application.Dto.DistrictCoordinator;

namespace CommunityHub.Application.Services.DistrictCoordinator;

public class ResolveResult
{
    public bool HasVotes { get; set; }
    public bool IsTie { get; set; }
    public DateTime? WinnerDate { get; set; }
    public List<DateTime> TiedDates { get; set; } = new();
}

public class DistrictMeetingService
{
    private readonly DistrictMeetingDbRepository _repository;
    private readonly DistrictDbRepository _districtRepository;

    public DistrictMeetingService()
    {
        _repository = new DistrictMeetingDbRepository();
        _districtRepository = new DistrictDbRepository();
    }

    public MeetingDto ScheduleMeeting(CreateMeetingDto dto)
    {
        District district = new District(dto.DistrictId, "");

        DistrictMeeting meeting = new DistrictMeeting(
            district, dto.MeetingType, dto.MeetingTime,
            dto.DateRangeStart, dto.DateRangeEnd);

        DistrictMeeting created = _repository.Create(meeting);
        return MapToDto(created);
    }

    public List<MeetingDto> GetAllByCoordinatorId(long coordinatorId)
    {
        return _repository.GetByCoordinatorId(coordinatorId)
            .Select(MapToDto)
            .ToList();
    }

    // Pokušava da razreši sastanak (poziva se 24h pre početnog datuma opsega).
    // Vraća ResolveResult koji opisuje šta se desilo:
    //   - Nema glasova       → sastanak se automatski otkazuje
    //   - Ima jasnog pobednika → sastanak se zakazuje na taj datum
    //   - Izjednačen broj   → koordinator mora ručno odabrati datum
    public ResolveResult ResolveMeeting(long meetingId)
    {
        Dictionary<DateTime, int> votes = _repository.GetVoteResults(meetingId);

        if (votes.Count == 0)
        {
            _repository.UpdateStatus(meetingId, MeetingStatus.Otkazan, null);
            return new ResolveResult { HasVotes = false };
        }

        int maxVotes = votes.Values.Max();
        List<DateTime> topDates = votes
            .Where(kv => kv.Value == maxVotes)
            .Select(kv => kv.Key)
            .ToList();

        if (topDates.Count == 1)
        {
            DateTime winner = topDates[0];
            _repository.UpdateStatus(meetingId, MeetingStatus.Zakazan, winner);
            return new ResolveResult { HasVotes = true, IsTie = false, WinnerDate = winner };
        }

        // Izjednačenje — koordinator bira
        return new ResolveResult { HasVotes = true, IsTie = true, TiedDates = topDates };
    }

    // Koordinator ručno bira datum (slučaj izjednačenja)
    public void ConfirmMeetingDate(long meetingId, DateTime chosenDate)
    {
        _repository.UpdateStatus(meetingId, MeetingStatus.Zakazan, chosenDate);
    }

    private static MeetingDto MapToDto(DistrictMeeting m) => new MeetingDto
    {
        Id             = m.Id,
        DistrictId     = m.District.Id,
        DistrictName   = m.District.Name,
        MeetingType    = m.MeetingType,
        MeetingTime    = m.MeetingTime,
        DateRangeStart = m.DateRangeStart,
        DateRangeEnd   = m.DateRangeEnd,
        Status         = m.Status,
        ScheduledDate  = m.ScheduledDate,
        VoteCount      = m.VoteCount
    };
}
