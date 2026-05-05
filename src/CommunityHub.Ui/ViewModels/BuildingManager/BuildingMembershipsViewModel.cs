using CommunityHub.Application.Dto.BuildingManager;
using CommunityHub.Application.Services.BuildingManager;

namespace CommunityHub.Ui.ViewModels.BuildingManager;

public class BuildingMembershipsViewModel
{
    private readonly BuildingAccessRequestService _service;
    private readonly long _managerId;

    public BuildingMembershipsViewModel(long managerId)
    {
        _managerId = managerId;
        _service = new BuildingAccessRequestService();
    }

    public List<BuildingDto> GetBuildingsByManagerId()
    {
        return _service.GetBuildingsByManagerId(_managerId);
    }

    public List<BuildingMembershipDto> GetMembershipsByBuildingId(long buildingId)
    {
        return _service.GetMembershipsByBuildingId(buildingId);
    }
}
