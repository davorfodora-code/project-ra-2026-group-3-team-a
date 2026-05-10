using CommunityHub.Application.Dto.BuildingManager;
using CommunityHub.Application.Services.BuildingManager;

namespace CommunityHub.Ui.ViewModels.BuildingManager;

public class CommonRoomsViewModel
{
    private readonly CommonRoomService _service;

    public CommonRoomsViewModel()
    {
        _service = new CommonRoomService();
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
