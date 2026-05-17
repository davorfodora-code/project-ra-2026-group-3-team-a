using CommunityHub.Application;
using CommunityHub.Application.Dto.Citizen;
using CommunityHub.Application.Services.Citizen;

namespace CommunityHub.Ui.ViewModels.Citizen;

public class CitizenMeetingCardViewModel
{
    public long MeetingId { get; init; }
    public string TitleDisplay { get; init; } = string.Empty;
    public string StatusDisplay { get; init; } = string.Empty;
    public string TimeLine { get; init; } = string.Empty;
    public string TopicDisplay { get; init; } = string.Empty;
    public string MyVoteDisplay { get; set; } = string.Empty;
    public bool CanVote { get; init; }
    public bool ShowVotingSection { get; init; }
    public bool ShowVotingClosedNote { get; init; }
    public string VoteGroupName { get; init; } = string.Empty;
    public List<DateTime> DateOptions { get; init; } = new();
    public DateTime? SelectedDate { get; set; }
}

public class CitizenMeetingsViewModel
{
    private readonly long _citizenId;
    private readonly ICitizenMeetingService _meetingService;

    public CitizenMeetingsViewModel(long citizenId)
    {
        _citizenId = citizenId;
        _meetingService = Injector.Get<ICitizenMeetingService>();
    }

    public List<CitizenMeetingCardViewModel> LoadMeetingCards()
    {
        return _meetingService.GetMeetings(_citizenId)
            .Select(MapToCard)
            .ToList();
    }

    public MeetingVoteResultDto SaveVote(long meetingId, DateTime? selectedDate)
    {
        if (!selectedDate.HasValue)
        {
            return new MeetingVoteResultDto
            {
                Success = false,
                Message = "Izaberite datum pre čuvanja glasa."
            };
        }

        return _meetingService.CastOrUpdateVote(_citizenId, new CastMeetingVoteDto
        {
            MeetingId = meetingId,
            PreferredDate = selectedDate.Value
        });
    }

    private static CitizenMeetingCardViewModel MapToCard(CitizenMeetingDto meeting)
    {
        string timeLine = meeting.IsScheduled
            ? $"Vreme: {meeting.MeetingTimeDisplay}  |  Datum: {meeting.ScheduledDateDisplay}"
            : $"Vreme: {meeting.MeetingTimeDisplay}  |  Opseg: {meeting.DateRangeDisplay}";

        return new CitizenMeetingCardViewModel
        {
            MeetingId = meeting.Id,
            TitleDisplay = meeting.TitleDisplay,
            StatusDisplay = meeting.StatusDisplay,
            TimeLine = timeLine,
            TopicDisplay = meeting.TopicDisplay,
            MyVoteDisplay = meeting.MyVoteDisplay,
            CanVote = meeting.CanVote,
            ShowVotingSection = meeting.IsInPreparation,
            ShowVotingClosedNote = meeting.IsInPreparation && !meeting.CanVote,
            VoteGroupName = $"MeetingVote_{meeting.Id}",
            DateOptions = meeting.DateOptions,
            SelectedDate = meeting.MyPreferredDate
        };
    }
}
