using CommunityHub.Application;
using CommunityHub.Application.Dto.BuildingManager;
using CommunityHub.Application.Services.BuildingManager;

namespace CommunityHub.Ui.ViewModels.BuildingManager;

public class BuildingRegistrationViewModel
{
    private readonly BuildingService _service;

    public BuildingRegistrationViewModel()
    {
        _service = new BuildingService(Injector.Get<IBuildingRepository>());
    }

    public BuildingDto RegisterBuilding(CreateBuildingDto dto)
    {
        return _service.Create(dto);
    }
}
