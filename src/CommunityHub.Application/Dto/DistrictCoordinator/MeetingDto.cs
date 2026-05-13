using System;
using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Dto.DistrictCoordinator;

public class MeetingDto
{
    public long Id { get; set; }
    public long DistrictId { get; set; }
    public string DistrictName { get; set; } = string.Empty;
    public MeetingType MeetingType { get; set; }
    public TimeSpan MeetingTime { get; set; }
    public DateTime DateRangeStart { get; set; }
    public DateTime DateRangeEnd { get; set; }
    public MeetingStatus Status { get; set; }
    public DateTime? ScheduledDate { get; set; }

    public string MeetingTypeDisplay => MeetingType switch
    {
        MeetingType.Dobrodoslica => "Dobrodošlica",
        MeetingType.Motivacija   => "Motivacija",
        _                        => MeetingType.ToString()
    };

    public string StatusDisplay => Status switch
    {
        MeetingStatus.UPripremi => "U pripremi",
        MeetingStatus.Zakazan   => "Zakazan",
        MeetingStatus.Otkazan   => "Otkazan",
        _                       => Status.ToString()
    };

    public string MeetingTimeDisplay => DateTime.Today.Add(MeetingTime).ToString("HH:mm");

    public string DateRangeDisplay =>
        $"{DateRangeStart:dd.MM.yyyy.} — {DateRangeEnd:dd.MM.yyyy.}";

    public int VoteCount { get; set; }

    public string ScheduledDateDisplay =>
        ScheduledDate.HasValue ? ScheduledDate.Value.ToString("dd.MM.yyyy.") : "-";

    public string DateTimeDisplay => IsScheduled
        ? $"{ScheduledDate!.Value:dd.MM.yyyy.} {MeetingTimeDisplay}"
        : $"{DateRangeDisplay} {MeetingTimeDisplay}";

    public bool IsScheduled => Status == MeetingStatus.Zakazan;
    public bool IsInPreparation => Status == MeetingStatus.UPripremi;

    public bool IsReadyForResolve =>
        Status == MeetingStatus.UPripremi &&
        DateTime.Now >= DateRangeStart.AddDays(-1) &&
        DateTime.Now < DateRangeStart;
}

public class CreateMeetingDto
{
    public long DistrictId { get; set; }
    public MeetingType MeetingType { get; set; }
    public TimeSpan MeetingTime { get; set; }
    public DateTime DateRangeStart { get; set; }
    public DateTime DateRangeEnd { get; set; }
}
