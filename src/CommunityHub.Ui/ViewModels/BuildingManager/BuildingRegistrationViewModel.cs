using CommunityHub.Application.Dto.BuildingManager;
using CommunityHub.Application.Services.BuildingManager;

namespace CommunityHub.Ui.ViewModels.BuildingManager;

public class BuildingRegistrationViewModel
{
    private readonly BuildingService _service;

    public BuildingRegistrationViewModel()
    {
        _service = new BuildingService();
    }

    public BuildingDto RegisterBuilding(CreateBuildingDto dto)
    {
        return _service.Create(dto);
    }
}
