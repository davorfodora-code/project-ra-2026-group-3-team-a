namespace CommunityHub.Application.Dto.Citizen;

public class CitizenTrustRecordDto
{
    public DateTime JoinDate { get; set; }
    public int OrganizedCompletedEventsCount { get; set; }
    public int ConfirmedVolunteerEventsCount { get; set; }
    public string TrustLevel { get; set; } = string.Empty;
}

public class DistrictMemberTrustRecordDto
{
    public long CitizenId { get; set; }
    public string CitizenFullName { get; set; } = string.Empty;
    public DateTime JoinDate { get; set; }
    public int OrganizedCompletedEventsCount { get; set; }
    public int ConfirmedVolunteerEventsCount { get; set; }
    public string TrustLevel { get; set; } = string.Empty;
}
