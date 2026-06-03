using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Services.Citizen;

public class CitizenTrustRecordEntry
{
    public long CitizenId { get; set; }
    public long DistrictId { get; set; }
    public string CitizenFullName { get; set; } = string.Empty;
    public DateTime JoinDate { get; set; }
    public int OrganizedCompletedEventsCount { get; set; }
    public int ConfirmedVolunteerEventsCount { get; set; }
    public CitizenTrustLevel TrustLevel { get; set; }

    public int TotalActivities => OrganizedCompletedEventsCount + ConfirmedVolunteerEventsCount;
}
