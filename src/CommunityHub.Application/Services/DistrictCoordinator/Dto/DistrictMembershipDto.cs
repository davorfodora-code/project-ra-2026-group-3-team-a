using System;

namespace CommunityHub.Application.Services.DistrictCoordinator.Dto;

public class DistrictMembershipDto
{
    public long DistrictId { get; set; }
    public string CitizenFullName { get; set; } = string.Empty;
    public string CitizenAddress { get; set; } = string.Empty;
    public DateTime JoinDate { get; set; }
    public string TrustLevel { get; set; } = string.Empty;

    public string JoinDateFormatted => JoinDate.ToString("dd.MM.yyyy.");
}
