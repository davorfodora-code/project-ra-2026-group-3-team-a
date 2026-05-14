using CommunityHub.Application;

namespace CommunityHub.Application.Services.Citizen;

public class CitizenMembershipService : ICitizenMembershipService
{
    private readonly ICitizenDistrictMembershipRepository _membershipRepository;

    public CitizenMembershipService()
    {
        _membershipRepository = Injector.Get<ICitizenDistrictMembershipRepository>();
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
