using CommunityHub.Application.Dto.Citizen;

namespace CommunityHub.Application.Services.Citizen;

public interface ICitizenCityFacilityService
{
    List<CityFacilityListItemDto> GetMyDistrictFacilities(long citizenId);
    CityFacilityActionResultDto VoteForFacility(long citizenId, long facilityId);
    CityFacilityStatisticsDto GetMyDistrictStatistics(long citizenId, int year, int? month);
    List<CityFacilityReservationHistoryDto> GetFacilityHistory(long citizenId, long facilityId);
}