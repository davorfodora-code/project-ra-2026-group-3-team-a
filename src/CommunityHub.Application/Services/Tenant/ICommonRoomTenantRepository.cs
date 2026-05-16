using CommunityHub.Application.Services.Tenant.Dto;

namespace CommunityHub.Application.Services.Tenant;

public interface ICommonRoomTenantRepository
{
    List<CommonRoomDto> GetByBuildingId(long buildingId);
}
