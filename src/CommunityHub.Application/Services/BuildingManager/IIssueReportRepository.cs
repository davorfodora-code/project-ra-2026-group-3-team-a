using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Services.BuildingManager;

public interface IIssueReportRepository
{
    List<IssueReport> GetByBuildingId(long buildingId);
    List<IssueReport> GetByManagerId(long managerId);
    void MarkAsPotentiallyResolved(long reportId);
}
