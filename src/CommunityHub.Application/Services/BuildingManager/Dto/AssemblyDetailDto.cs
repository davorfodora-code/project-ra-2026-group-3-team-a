namespace CommunityHub.Application.Services.BuildingManager.Dto;

public class AssemblyDetailDto
{
    public long Id { get; init; }
    public long BuildingId { get; init; }
    public string BuildingAddress { get; init; } = string.Empty;
    public DateTime ScheduledAt { get; init; }
    public string Status { get; init; } = string.Empty;
    public List<AssemblyTopicDto>   Topics      { get; init; } = new();
    public List<TopicSuggestionDto> Suggestions { get; init; } = new();
    public List<AttendanceItemDto>  Attendances { get; init; } = new();

    public string DateLine     => ScheduledAt.ToString("dd.MM.yyyy.");
    public string TimeLine     => ScheduledAt.ToString("HH:mm");

    public string StatusDisplay => Status switch
    {
        "Scheduled" => "Zakazana",
        "Confirmed" => "Potvrđena",
        "Cancelled" => "Otkazana",
        _           => Status
    };

    public bool   HasSuggestions      => Suggestions.Count > 0;
    // Teme mogu da se menjaju najkasnije 24h pred skupstinu
    public bool   CanEditTopics        => DateTime.Now < ScheduledAt.AddHours(-24);
    public string EditDeadlineDisplay  => ScheduledAt.AddHours(-24).ToString("dd.MM.yyyy. HH:mm");
}
