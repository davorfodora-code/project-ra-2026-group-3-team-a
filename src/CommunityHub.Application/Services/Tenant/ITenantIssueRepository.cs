using CommunityHub.Application.Services.Tenant.Dto;

namespace CommunityHub.Application.Services.Tenant;

public interface ITenantIssueRepository
{
    List<IssueReportDto> GetByTenantId(long tenantId);
    void Create(CreateIssueReportDto dto);
    void ConfirmResolved(long issueId);
    void DenyResolved(long issueId);
}
