using System;

namespace CommunityHub.Application.Domain;

public class DistrictMeeting
{
    public long Id { get; private set; }
    public District District { get; private set; }
    public MeetingType MeetingType { get; private set; }
    public TimeSpan MeetingTime { get; private set; }
    public DateTime DateRangeStart { get; private set; }
    public DateTime DateRangeEnd { get; private set; }
    public MeetingStatus Status { get; private set; }
    public DateTime? ScheduledDate { get; private set; }
    public int VoteCount { get; private set; }

    public DistrictMeeting(long id, District district, MeetingType meetingType,
        TimeSpan meetingTime, DateTime dateRangeStart, DateTime dateRangeEnd,
        MeetingStatus status, DateTime? scheduledDate, int voteCount = 0)
    {
        Id = id;
        District = district;
        MeetingType = meetingType;
        MeetingTime = meetingTime;
        DateRangeStart = dateRangeStart;
        DateRangeEnd = dateRangeEnd;
        Status = status;
        ScheduledDate = scheduledDate;
        VoteCount = voteCount;
    }

    public DistrictMeeting(District district, MeetingType meetingType,
        TimeSpan meetingTime, DateTime dateRangeStart, DateTime dateRangeEnd)
    {
        Id = 0;
        District = district;
        MeetingType = meetingType;
        MeetingTime = meetingTime;
        DateRangeStart = dateRangeStart;
        DateRangeEnd = dateRangeEnd;
        Status = MeetingStatus.UPripremi;
        ScheduledDate = null;
    }

    public void Schedule(DateTime date)
    {
        Status = MeetingStatus.Zakazan;
        ScheduledDate = date;
    }

    public void Cancel()
    {
        Status = MeetingStatus.Otkazan;
        ScheduledDate = null;
    }
}
