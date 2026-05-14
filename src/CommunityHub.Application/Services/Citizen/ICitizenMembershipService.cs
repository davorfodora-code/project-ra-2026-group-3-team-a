using System.Collections.Generic;

namespace CommunityHub.Application.Services.Citizen
{
    public interface ICitizenMembershipService
    {
        bool HasMembership(long userId);
        long? GetMembershipDistrictId(long userId);
        long CreateMembership(long userId, long districtId);

    }
}
