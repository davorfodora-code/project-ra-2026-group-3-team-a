using CommunityHub.Application.Domain;
using System.Collections.Generic;

namespace CommunityHub.Application.Services.Citizen
{
    public interface ICitizenDistrictAccessRequestRepository
    {
        bool ExistsPending(long userId, long districtId);
        long Create(long userId, long districtId);
        List<DistrictAccessRequest> GetAllByUserId(long userId);
        void Delete(long requestId);
    }
}
