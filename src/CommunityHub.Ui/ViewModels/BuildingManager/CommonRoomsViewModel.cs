using CommunityHub.Application;
using CommunityHub.Application.Services.BuildingManager.Dto;
using CommunityHub.Application.Services.BuildingManager;

namespace CommunityHub.Ui.ViewModels.BuildingManager;

public class CommonRoomsViewModel
{
    private readonly CommonRoomService _service;

    public CommonRoomsViewModel()
    {
        _service = new CommonRoomService(
            Injector.Get<ICommonRoomRepository>(),
            Injector.Get<IBuildingRepository>());
    }

    public List<CommonRoomDto> GetByBuildingId(long buildingId)
    {
        return _service.GetByBuildingId(buildingId);
    }

    public void Add(CreateCommonRoomDto dto)
    {
        _service.Add(dto);
    }
}
