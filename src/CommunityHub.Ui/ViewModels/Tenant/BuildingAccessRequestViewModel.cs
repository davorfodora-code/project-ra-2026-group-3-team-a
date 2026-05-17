using CommunityHub.Application;
using CommunityHub.Application.Services.Tenant.Dto;
using CommunityHub.Application.Services.Tenant;

namespace CommunityHub.Ui.ViewModels.Tenant;

public class BuildingAccessRequestViewModel : BaseViewModel
{
    private readonly BuildingAccessRequestService _service;

    public BuildingAccessRequestViewModel()
    {
        _service = new BuildingAccessRequestService(
            Injector.Get<IBuildingAccessRequestRepository>(),
            Injector.Get<IBuildingMembershipRepository>());
    }

    public List<int> GetApartmentNumbers(long buildingId)
    {
        return _service.GetApartmentNumbers(buildingId);
    }

    public bool IsApartmentOccupied(long buildingId, int apartmentNumber)
    {
        return _service.IsApartmentOccupied(buildingId, apartmentNumber);
    }

    public bool HasPendingRequest(long tenantId, long buildingId)
    {
        return _service.HasPendingRequest(tenantId, buildingId);
    }

    public void Create(CreateBuildingAccessRequestDto dto)
    {
        _service.Create(dto);
    }
}
