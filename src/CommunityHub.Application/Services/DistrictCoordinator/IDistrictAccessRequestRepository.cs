using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Services.DistrictCoordinator;

public interface IDistrictAccessRequestRepository
{
    long Create(long userId, long districtId);
    bool ExistsPending(long userId, long districtId);
    List<DistrictAccessRequest> GetByCoordinatorId(long coordinatorId);
    void Approve(long requestId);
    void Reject(long requestId, string? reason);
    List<DistrictMembership> GetMembershipsByDistrictId(long districtId);
    List<District> GetDistrictsByCoordinatorId(long coordinatorId);
}
