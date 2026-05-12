using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Dto.Citizen;

public class DistrictAccessRequestDto
{
    public long RequestId { get; set; }
    public string DistrictName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DistrictAccessRequestStatus Status { get; set; }
    public string CreatedAtFormatted => CreatedAt.ToString("dd.MM.yyyy. HH:mm");
    public bool CanDelete { get; set; }
}
