using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Services.BuildingManager;

public interface IAssemblyRepository
{
    List<TenantAssembly> GetByManagerId(long managerId);
    TenantAssembly? GetById(long assemblyId);
    bool HasTimeConflict(long managerId, DateTime scheduledAt);
    TenantAssembly Create(TenantAssembly assembly);
    List<(int ApartmentNumber, bool IsConfirmed)> GetAttendanceSummary(long assemblyId, long buildingId);
}
