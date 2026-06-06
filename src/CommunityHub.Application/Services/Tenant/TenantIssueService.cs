using CommunityHub.Application.Services.Tenant.Dto;

namespace CommunityHub.Application.Services.Tenant;

public class TenantIssueService
{
    private readonly ITenantIssueRepository _repo;
    private readonly IAdvertisementRepository _buildingRepo;

    public TenantIssueService(ITenantIssueRepository repo, IAdvertisementRepository buildingRepo)
    {
        _repo = repo;
        _buildingRepo = buildingRepo;
    }

    public List<IssueReportDto> GetByTenantId(long tenantId) => _repo.GetByTenantId(tenantId);

    public List<TenantBuildingDto> GetMyBuildings(long tenantId)
        => _buildingRepo.GetBuildingsByTenant(tenantId);

    public void Report(CreateIssueReportDto dto) => _repo.Create(dto);

    public void ConfirmResolved(long issueId) => _repo.ConfirmResolved(issueId);

    public void DenyResolved(long issueId) => _repo.DenyResolved(issueId);
}
