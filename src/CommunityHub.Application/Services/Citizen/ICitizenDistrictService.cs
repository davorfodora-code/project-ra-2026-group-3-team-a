using CommunityHub.Application.Dto.Citizen;
using System.Collections.Generic;

namespace CommunityHub.Application.Services.Citizen
{
    public interface ICitizenDistrictService
    {
        List<DistrictListItemDto> GetAll();
        List<DistrictListItemDto> Search(DistrictSearchFilterDto filter);
        DistrictListItemDto? GetById(long districtId);
    }
}
