using CommunityHub.Application.Domain;
using CommunityHub.Application.Services.BuildingManager.Dto;

namespace CommunityHub.Application.Services.BuildingManager;

public class IssueReportService
{
    private readonly IIssueReportRepository _repository;

    public IssueReportService(IIssueReportRepository repository)
    {
        _repository = repository;
    }

    public List<IssueReportDto> GetByBuildingId(long buildingId)
    {
        return _repository.GetByBuildingId(buildingId)
            .Select(MapToDto)
            .ToList();
    }

    public List<IssueReportDto> GetByManagerId(long managerId)
    {
        return _repository.GetByManagerId(managerId)
            .Select(MapToDto)
            .ToList();
    }

    // Oznacava problem kao potencijalno resen — stanar ce potvrditi ili odbiti
    public void MarkAsResolved(long reportId)
    {
        _repository.MarkAsPotentiallyResolved(reportId);
    }

    private static IssueReportDto MapToDto(IssueReport r) => new IssueReportDto
    {
        Id             = r.Id,
        BuildingId     = r.Building.Id,
        TenantFullName = $"{r.Tenant.Name} {r.Tenant.Surname}",
        BuildingAddress = $"{r.Building.Address.Street} {r.Building.Address.StreetNumber}",
        Description    = r.Description,
        Priority       = r.Priority,
        ReportedAt     = r.ReportedAt,
        Status         = r.Status
    };
}
