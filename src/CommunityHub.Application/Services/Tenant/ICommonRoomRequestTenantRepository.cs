using CommunityHub.Application.Services.Tenant.Dto;

namespace CommunityHub.Application.Services.Tenant;

public interface ICommonRoomRequestTenantRepository
{
    List<MyCommonRoomRequestDto> GetByTenantAndBuilding(long tenantId, long buildingId);
    void Create(CreateCommonRoomRequestDto dto);
    void Cancel(long requestId);
    void AcceptProposedDate(long requestId);
}
