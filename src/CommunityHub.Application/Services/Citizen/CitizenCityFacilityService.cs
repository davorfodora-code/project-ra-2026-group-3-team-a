using CommunityHub.Application;
using CommunityHub.Application.Dto.Citizen;

namespace CommunityHub.Application.Services.Citizen;

public class CitizenCityFacilityService : ICitizenCityFacilityService
{
    private readonly ICitizenCityFacilityRepository _facilityRepository;
    private readonly ICitizenDistrictMembershipRepository _membershipRepository;

    public CitizenCityFacilityService()
    {
        _facilityRepository = Injector.Get<ICitizenCityFacilityRepository>();
        _membershipRepository = Injector.Get<ICitizenDistrictMembershipRepository>();
    }

    public CitizenCityFacilityService(
        ICitizenCityFacilityRepository facilityRepository,
        ICitizenDistrictMembershipRepository membershipRepository)
    {
        _facilityRepository = facilityRepository;
        _membershipRepository = membershipRepository;
    }

    public List<CityFacilityListItemDto> GetMyDistrictFacilities(long citizenId)
    {
        long? districtId = _membershipRepository.GetMembershipDistrictId(citizenId);
        if (!districtId.HasValue)
        {
            return new List<CityFacilityListItemDto>();
        }

        return _facilityRepository.GetFacilitiesByDistrict(districtId.Value, citizenId);
    }

    public CityFacilityActionResultDto VoteForFacility(long citizenId, long facilityId)
    {
        long? districtId = _membershipRepository.GetMembershipDistrictId(citizenId);
        if (!districtId.HasValue)
        {
            return CityFacilityActionResultDto.Fail("Niste clan nijednog kvarta.");
        }

        if (!_facilityRepository.FacilityBelongsToDistrict(facilityId, districtId.Value))
        {
            return CityFacilityActionResultDto.Fail("Objekat ne pripada vasem kvartu.");
        }

        if (_facilityRepository.HasVote(facilityId, citizenId))
        {
            return CityFacilityActionResultDto.Fail("Vec ste glasali za ovaj objekat.");
        }

        _facilityRepository.Vote(facilityId, citizenId);
        return CityFacilityActionResultDto.Ok("Vas glas je uspesno zabelezen.");
    }

    public CityFacilityStatisticsDto GetMyDistrictStatistics(long citizenId, int year, int? month)
    {
        long? districtId = _membershipRepository.GetMembershipDistrictId(citizenId);
        if (!districtId.HasValue)
        {
            return new CityFacilityStatisticsDto();
        }

        return _facilityRepository.GetStatistics(districtId.Value, year, month);
    }

    public List<CityFacilityReservationHistoryDto> GetFacilityHistory(long citizenId, long facilityId)
    {
        long? districtId = _membershipRepository.GetMembershipDistrictId(citizenId);
        if (!districtId.HasValue)
        {
            return new List<CityFacilityReservationHistoryDto>();
        }

        if (!_facilityRepository.FacilityBelongsToDistrict(facilityId, districtId.Value))
        {
            return new List<CityFacilityReservationHistoryDto>();
        }

        return _facilityRepository.GetReservationHistory(districtId.Value, facilityId);
    }
}