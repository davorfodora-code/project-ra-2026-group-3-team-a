using CommunityHub.Application.Services.Tenant.Dto;

namespace CommunityHub.Application.Services.Tenant;

public interface ITenantAssemblyRepository
{
    List<TenantAssemblyDto> GetByTenantId(long tenantId);
    int GetUpcomingCountByTenantId(long tenantId);
    List<TenantBuildingDto> GetBuildingsByTenantId(long tenantId);
    void RegisterAttendance(long assemblyId, long tenantId, int apartmentNumber);
    void CancelAttendance(long assemblyId, long tenantId);
    void ProposeTopic(long assemblyId, long tenantId, string topic);
    void UpdateStatus(long assemblyId, string status);
    int GetConfirmedCount(long assemblyId);
    int GetTotalApartments(long buildingId);
}
