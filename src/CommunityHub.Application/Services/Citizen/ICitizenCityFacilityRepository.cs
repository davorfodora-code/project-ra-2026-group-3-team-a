using CommunityHub.Application.Dto.Citizen;

namespace CommunityHub.Application.Services.Citizen;

public interface ICitizenCityFacilityRepository
{
    List<CityFacilityListItemDto> GetFacilitiesByDistrict(long districtId, long citizenId);
    CityFacilityListItemDto? GetFacilityById(long facilityId, long citizenId);
    void Vote(long facilityId, long citizenId);
    bool HasVote(long facilityId, long citizenId);
    bool FacilityBelongsToDistrict(long facilityId, long districtId);
    CityFacilityStatisticsDto GetStatistics(long districtId, int year, int? month);
    List<CityFacilityReservationHistoryDto> GetReservationHistory(long districtId, long facilityId);
}