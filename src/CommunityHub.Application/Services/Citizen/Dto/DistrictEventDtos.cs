using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Dto.Citizen;

public class CreateDistrictEventDto
{
    public long DistrictId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan Duration { get; set; }
    public int MinVolunteers { get; set; }
    public List<string> RequiredItems { get; set; } = new();
}

public class DistrictEventListItemDto
{
    public long EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan Duration { get; set; }
    public int MinVolunteers { get; set; }
    public int CurrentVolunteers { get; set; }
    public DistrictEventStatus Status { get; set; }
    public bool IsOrganizer { get; set; }
    public List<EventRequiredItemDto> RequiredItems { get; set; } = new();

    public string EventDateDisplay => EventDate.ToString("dd.MM.yyyy.");
    public string StartTimeDisplay => DateTime.Today.Add(StartTime).ToString("HH:mm");
    public string DurationDisplay => $"{(int)Duration.TotalMinutes} min";
    public string StatusDisplay => Status switch
    {
        DistrictEventStatus.UPripremi => "U pripremi",
        DistrictEventStatus.Zakazan => "Zakazan",
        DistrictEventStatus.Otkazan => "Otkazan",
        DistrictEventStatus.Zavrsen => "Zavrsen",
        _ => Status.ToString()
    };
}

public class DistrictEventDetailsDto
{
    public long EventId { get; set; }
    public long DistrictId { get; set; }
    public string DistrictName { get; set; } = string.Empty;
    public long OrganizerId { get; set; }
    public string OrganizerName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan Duration { get; set; }
    public int MinVolunteers { get; set; }
    public int CurrentVolunteers { get; set; }
    public DistrictEventStatus Status { get; set; }
    public bool IsCitizenAlreadyApplied { get; set; }
    public List<EventRequiredItemDto> RequiredItems { get; set; } = new();
}

public class EventRequiredItemDto
{
    public long ItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public long? AssignedVolunteerId { get; set; }
    public string? AssignedVolunteerName { get; set; }
    public bool IsCovered => AssignedVolunteerId.HasValue;
}

public class VolunteerSignupDto
{
    public long EventId { get; set; }
    public List<long> SelectedItemIds { get; set; } = new();
}

public class VolunteerAttendanceDto
{
    public long VolunteerId { get; set; }
    public string VolunteerName { get; set; } = string.Empty;
    public bool Attended { get; set; }
}

public class EventActionResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
