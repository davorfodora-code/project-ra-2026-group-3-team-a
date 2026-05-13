using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Services.BuildingManager;

public interface ICommonRoomRepository
{
    List<CommonRoom> GetByBuildingId(long buildingId);
    CommonRoom Add(CommonRoom room);
}
