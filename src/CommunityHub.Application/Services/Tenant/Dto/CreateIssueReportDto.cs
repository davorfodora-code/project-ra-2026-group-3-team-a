namespace CommunityHub.Application.Services.Tenant.Dto;

public class CreateIssueReportDto
{
    public long TenantId { get; set; }
    public long BuildingId { get; set; }
    public string Description { get; set; } = "";
    public string Priority { get; set; } = "";
}
