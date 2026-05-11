using CommunityHub.Application.Dto.DistrictCoordinator;
using CommunityHub.Application.Services.DistrictCoordinator;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;

public class DistrictRegistrationViewModel
{
    private readonly DistrictService _service;

    public DistrictRegistrationViewModel()
    {
        _service = new DistrictService();
    }

    public DistrictDto RegisterDistrict(CreateDistrictDto dto)
    {
        return _service.Create(dto);
    }
}
