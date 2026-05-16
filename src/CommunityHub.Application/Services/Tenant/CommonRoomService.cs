using CommunityHub.Application.Services.Tenant.Dto;

namespace CommunityHub.Application.Services.Tenant;

public class CommonRoomService
{
    private readonly ICommonRoomTenantRepository _roomRepo;
    private readonly ICommonRoomRequestTenantRepository _requestRepo;
    private readonly IAdvertisementRepository _membershipRepo;

    public CommonRoomService(
        ICommonRoomTenantRepository roomRepo,
        ICommonRoomRequestTenantRepository requestRepo,
        IAdvertisementRepository membershipRepo)
    {
        _roomRepo = roomRepo;
        _requestRepo = requestRepo;
        _membershipRepo = membershipRepo;
    }

    public List<TenantBuildingDto> GetMyBuildings(long tenantId)
        => _membershipRepo.GetBuildingsByTenant(tenantId);

    public List<CommonRoomDto> GetRooms(long buildingId)
        => _roomRepo.GetByBuildingId(buildingId);

    public List<MyCommonRoomRequestDto> GetMyRequests(long tenantId, long buildingId)
        => _requestRepo.GetByTenantAndBuilding(tenantId, buildingId);

    public void SubmitRequest(CreateCommonRoomRequestDto dto)
        => _requestRepo.Create(dto);

    public void CancelRequest(long requestId)
        => _requestRepo.Cancel(requestId);

    public void AcceptProposedDate(long requestId)
        => _requestRepo.AcceptProposedDate(requestId);
}
