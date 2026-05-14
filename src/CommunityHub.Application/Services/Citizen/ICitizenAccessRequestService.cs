using CommunityHub.Application.Dto.Citizen;
using System.Collections.Generic;

namespace CommunityHub.Application.Services.Citizen
{
    public interface ICitizenAccessRequestService
    {
        AccessResultDto RequestDistrictAccess(long userId, long districtId);
        List<DistrictAccessRequestDto> GetAllByUserId(long userId);
        AccessResultDto DeleteRequest(long userId, long requestId);
    }
}
