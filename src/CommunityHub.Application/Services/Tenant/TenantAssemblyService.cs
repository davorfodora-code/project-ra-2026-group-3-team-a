using CommunityHub.Application.Services.Tenant.Dto;

namespace CommunityHub.Application.Services.Tenant;

public class TenantAssemblyService
{
    private readonly ITenantAssemblyRepository _repo;

    public TenantAssemblyService(ITenantAssemblyRepository repo) => _repo = repo;

    public List<TenantAssemblyDto> GetByTenantId(long tenantId)
    {
        var assemblies = _repo.GetByTenantId(tenantId);
        var toCancel = assemblies
            .Where(a => a.Status == "Scheduled" && a.IsDeadlinePassed && !a.HasQuorum)
            .ToList();
        foreach (var a in toCancel)
            _repo.UpdateStatus(a.Id, "Cancelled");
        return toCancel.Count > 0 ? _repo.GetByTenantId(tenantId) : assemblies;
    }

    public int GetUpcomingCountByTenantId(long tenantId)
        => _repo.GetUpcomingCountByTenantId(tenantId);

    public List<TenantBuildingDto> GetBuildings(long tenantId)
        => _repo.GetBuildingsByTenantId(tenantId);

    public void RegisterAttendance(long assemblyId, long tenantId, int apartmentNumber, long buildingId)
    {
        _repo.RegisterAttendance(assemblyId, tenantId, apartmentNumber);
        UpdateQuorumStatus(assemblyId, buildingId);
    }

    public void CancelAttendance(long assemblyId, long tenantId, long buildingId)
    {
        _repo.CancelAttendance(assemblyId, tenantId);
        UpdateQuorumStatus(assemblyId, buildingId);
    }

    public void ProposeTopic(long assemblyId, long tenantId, string topic)
        => _repo.ProposeTopic(assemblyId, tenantId, topic);

    private void UpdateQuorumStatus(long assemblyId, long buildingId)
    {
        int confirmed = _repo.GetConfirmedCount(assemblyId);
        int total = _repo.GetTotalApartments(buildingId);
        _repo.UpdateStatus(assemblyId, confirmed * 2 > total ? "Confirmed" : "Scheduled");
    }
}
