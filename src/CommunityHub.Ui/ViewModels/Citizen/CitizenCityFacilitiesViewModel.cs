using CommunityHub.Application;
using CommunityHub.Application.Dto.Citizen;
using CommunityHub.Application.Services.Citizen;

namespace CommunityHub.Ui.ViewModels.Citizen;

public class CitizenCityFacilitiesViewModel
{
    private readonly long _citizenId;
    private readonly ICitizenCityFacilityService _service;

    public CitizenCityFacilitiesViewModel(long citizenId)
    {
        _citizenId = citizenId;
        _service = Injector.Get<ICitizenCityFacilityService>();
    }

    public List<CityFacilityListItemDto> LoadFacilities()
    {
        return _service.GetMyDistrictFacilities(_citizenId);
    }

    public CityFacilityActionResultDto Vote(long facilityId)
    {
        return _service.VoteForFacility(_citizenId, facilityId);
    }

    public CityFacilityStatisticsDto LoadStatistics(int year, int? month)
    {
        return _service.GetMyDistrictStatistics(_citizenId, year, month);
    }

    public List<CityFacilityReservationHistoryDto> LoadHistory(long facilityId)
    {
        return _service.GetFacilityHistory(_citizenId, facilityId);
    }
}