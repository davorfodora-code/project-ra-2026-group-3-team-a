using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Dto.Citizen;

public class CitizenMeetingDto
{
    public long Id { get; set; }
    public long DistrictId { get; set; }
    public MeetingType MeetingType { get; set; }
    public TimeSpan MeetingTime { get; set; }
    public DateTime DateRangeStart { get; set; }
    public DateTime DateRangeEnd { get; set; }
    public MeetingStatus Status { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public DateTime? MyPreferredDate { get; set; }

    public string TitleDisplay => MeetingType switch
    {
        MeetingType.Dobrodoslica => "Sastanak dobrodošlice",
        MeetingType.Motivacija => "Motivacioni sastanak",
        _ => MeetingType.ToString()
    };

    public string TopicDisplay => MeetingType switch
    {
        MeetingType.Dobrodoslica => "Dobrodošlica novim građanima kvarta",
        MeetingType.Motivacija => "Veće uključivanje građana u kvartovske aktivnosti",
        _ => string.Empty
    };

    public string StatusDisplay => Status switch
    {
        MeetingStatus.UPripremi => "U pripremi",
        MeetingStatus.Zakazan => "Zakazan",
        MeetingStatus.Otkazan => "Otkazan",
        _ => Status.ToString()
    };

    public string MeetingTimeDisplay => DateTime.Today.Add(MeetingTime).ToString("HH:mm");

    public string DateRangeDisplay =>
        $"{DateRangeStart:dd.MM.yyyy.} — {DateRangeEnd:dd.MM.yyyy.}";

    public string ScheduledDateDisplay =>
        ScheduledDate.HasValue ? ScheduledDate.Value.ToString("dd.MM.yyyy.") : "-";

    public bool IsInPreparation => Status == MeetingStatus.UPripremi;

    public bool IsScheduled => Status == MeetingStatus.Zakazan;

    public bool CanVote =>
        IsInPreparation && DateTime.Now < DateRangeStart.AddDays(-1);

    public string MyVoteDisplay =>
        MyPreferredDate.HasValue ? MyPreferredDate.Value.ToString("dd.MM.yyyy.") : "Niste glasali";

    public List<DateTime> DateOptions
    {
        get
        {
            List<DateTime> dates = new();
            for (DateTime date = DateRangeStart.Date; date <= DateRangeEnd.Date; date = date.AddDays(1))
            {
                dates.Add(date);
            }

            return dates;
        }
    }
}

public class CastMeetingVoteDto
{
    public long MeetingId { get; set; }
    public DateTime PreferredDate { get; set; }
}

public class MeetingVoteResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
