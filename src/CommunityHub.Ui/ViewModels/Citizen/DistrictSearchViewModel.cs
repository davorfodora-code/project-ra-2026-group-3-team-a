using CommunityHub.Application;
using CommunityHub.Application.Dto.Citizen;
using CommunityHub.Application.Services.Citizen;
using CommunityHub.Ui.Views.Citizen;

namespace CommunityHub.Ui.ViewModels.Citizen;

public class DistrictSearchViewModel
{
    private readonly long _userId;
    private readonly ICitizenDistrictService _districtService;
    private readonly ICitizenAccessRequestService _accessService;
    private List<DistrictListItemDto> _currentDistricts = new();

    public DistrictSearchViewModel(long userId)
    {
        _userId = userId;
        _districtService = Injector.Get<ICitizenDistrictService>();
        _accessService = Injector.Get<ICitizenAccessRequestService>();
    }

    public List<DistrictRowViewModel> LoadAllDistricts()
    {
        _currentDistricts = _districtService.GetAll();
        return _currentDistricts.Select(d => new DistrictRowViewModel(d)).ToList();
    }

    public List<DistrictRowViewModel> Search(DistrictSearchFilterDto filter)
    {
        _currentDistricts = _districtService.Search(filter);
        return _currentDistricts.Select(d => new DistrictRowViewModel(d)).ToList();
    }

    public DistrictRowViewModel? FindSelectedDistrict(long districtId)
    {
        DistrictListItemDto? district = _currentDistricts.FirstOrDefault(d => d.DistrictId == districtId);
        return district == null ? null : new DistrictRowViewModel(district);
    }

    public AccessResultDto RequestDistrictAccess(long districtId)
    {
        return _accessService.RequestDistrictAccess(_userId, districtId);
    }
}
