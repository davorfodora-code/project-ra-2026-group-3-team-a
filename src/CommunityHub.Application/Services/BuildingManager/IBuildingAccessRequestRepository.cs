using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Services.BuildingManager;

public interface IBuildingAccessRequestRepository
{
    List<BuildingAccessRequest> GetByManagerId(long managerId);
    void Approve(long requestId);
    void Reject(long requestId, string? reason);
    List<BuildingMembership> GetMembershipsByBuildingId(long buildingId);
    List<Building> GetBuildingsByManagerId(long managerId);
}
