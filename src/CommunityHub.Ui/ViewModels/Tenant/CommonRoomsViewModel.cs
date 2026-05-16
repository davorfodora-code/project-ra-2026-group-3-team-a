using CommunityHub.Application;
using CommunityHub.Application.Services.Tenant;
using CommunityHub.Application.Services.Tenant.Dto;

namespace CommunityHub.Ui.ViewModels.Tenant;

public class CommonRoomsViewModel : BaseViewModel
{
    private readonly CommonRoomService _service;
    public long TenantId { get; }

    public List<TenantBuildingDto> Buildings { get; private set; } = [];

    private long? _selectedBuildingId;
    public long? SelectedBuildingId
    {
        get => _selectedBuildingId;
        private set => SetProperty(ref _selectedBuildingId, value);
    }

    private List<CommonRoomDto> _rooms = [];
    public List<CommonRoomDto> Rooms
    {
        get => _rooms;
        private set => SetProperty(ref _rooms, value);
    }

    private List<MyCommonRoomRequestDto> _requests = [];
    public List<MyCommonRoomRequestDto> Requests
    {
        get => _requests;
        private set => SetProperty(ref _requests, value);
    }

    public CommonRoomsViewModel(long tenantId)
    {
        TenantId = tenantId;
        _service = new CommonRoomService(
            Injector.Get<ICommonRoomTenantRepository>(),
            Injector.Get<ICommonRoomRequestTenantRepository>(),
            Injector.Get<IAdvertisementRepository>());

        Buildings = _service.GetMyBuildings(tenantId);
        if (Buildings.Count > 0)
            SelectedBuildingId = Buildings[0].BuildingId;
    }

    public bool IsInBuilding() => Buildings.Count > 0;

    public void SelectBuilding(long buildingId) => SelectedBuildingId = buildingId;

    public void LoadRooms()
    {
        if (SelectedBuildingId == null) { Rooms = []; return; }
        Rooms = _service.GetRooms(SelectedBuildingId.Value);
    }

    public void LoadRequests()
    {
        if (SelectedBuildingId == null) { Requests = []; return; }
        Requests = _service.GetMyRequests(TenantId, SelectedBuildingId.Value);
    }

    public void SubmitRequest(CreateCommonRoomRequestDto dto) => _service.SubmitRequest(dto);

    public void CancelRequest(long requestId) => _service.CancelRequest(requestId);

    public void AcceptProposedDate(long requestId) => _service.AcceptProposedDate(requestId);
}
