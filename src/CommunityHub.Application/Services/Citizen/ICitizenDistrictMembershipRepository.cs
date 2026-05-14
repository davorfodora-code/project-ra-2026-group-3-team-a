namespace CommunityHub.Application.Services.Citizen;

public interface ICitizenDistrictMembershipRepository
{
    bool HasMembership(long userId);
    long? GetMembershipDistrictId(long userId);
    long Create(long userId, long districtId, DateTime? joinDate = null);
}
