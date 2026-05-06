using CommunityHub.Application.Dto.Tenant;
using CommunityHub.Application.Services.Tenant;

namespace CommunityHub.Ui.ViewModels.Tenant;

public class BuildingAccessRequestViewModel
{
    private readonly BuildingAccessRequestService _service;

    public BuildingAccessRequestViewModel()
    {
        _service = new BuildingAccessRequestService();
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
