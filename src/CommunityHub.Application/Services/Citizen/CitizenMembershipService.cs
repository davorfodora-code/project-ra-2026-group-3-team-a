using CommunityHub.Application.Database.Repositories.Citizen;

namespace CommunityHub.Application.Services.Citizen;

public class CitizenMembershipService
{
    private readonly CitizenDistrictMembershipDbRepository _membershipRepository;

    public CitizenMembershipService()
    {
        _membershipRepository = new CitizenDistrictMembershipDbRepository();
    }

    public bool HasMembership(long userId)
    {
        return _membershipRepository.HasMembership(userId);
    }

    public long? GetMembershipDistrictId(long userId)
    {
        return _membershipRepository.GetMembershipDistrictId(userId);
    }

    public long CreateMembership(long userId, long districtId)
    {
        return _membershipRepository.Create(userId, districtId, DateTime.Now);
    }
}
