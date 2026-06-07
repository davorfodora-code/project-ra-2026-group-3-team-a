using CommunityHub.Application;
using CommunityHub.Application.Domain;
using CommunityHub.Application.Dto.Citizen;

namespace CommunityHub.Application.Services.Citizen;

public class CitizenTrustRecordService : ICitizenTrustRecordService
{
    private readonly ICitizenTrustRecordRepository _trustRepository;
    private readonly ICitizenDistrictMembershipRepository _membershipRepository;

    public CitizenTrustRecordService()
    {
        _trustRepository = Injector.Get<ICitizenTrustRecordRepository>();
        _membershipRepository = Injector.Get<ICitizenDistrictMembershipRepository>();
    }

    public CitizenTrustRecordService(
        ICitizenTrustRecordRepository trustRepository,
        ICitizenDistrictMembershipRepository membershipRepository)
    {
        _trustRepository = trustRepository;
        _membershipRepository = membershipRepository;
    }

    public CitizenTrustRecordDto? GetMyTrustRecord(long citizenId)
    {
        long? districtId = _membershipRepository.GetMembershipDistrictId(citizenId);
        if (districtId == null)
        {
            return null;
        }

        RefreshTrustForCitizen(citizenId, districtId.Value);
        CitizenTrustRecordEntry? entry = _trustRepository.GetByUserAndDistrict(citizenId, districtId.Value);
        if (entry == null)
        {
            return null;
        }

        return new CitizenTrustRecordDto
        {
            JoinDate = entry.JoinDate,
            OrganizedCompletedEventsCount = entry.OrganizedCompletedEventsCount,
            ConfirmedVolunteerEventsCount = entry.ConfirmedVolunteerEventsCount,
            TrustLevel = entry.TrustLevel.ToString()
        };
    }

    public List<DistrictMemberTrustRecordDto> GetDistrictTrustRecords(long citizenId)
    {
        long? districtId = _membershipRepository.GetMembershipDistrictId(citizenId);
        if (districtId == null)
        {
            return new List<DistrictMemberTrustRecordDto>();
        }

        RefreshTrustForDistrict(districtId.Value);

        return _trustRepository.GetByDistrict(districtId.Value)
            .OrderBy(e => e.CitizenFullName)
            .Select(MapToDistrictMemberDto)
            .ToList();
    }

    public void RefreshTrustForCitizen(long citizenId, long districtId)
    {
        CitizenTrustRecordEntry? entry = _trustRepository.GetByUserAndDistrict(citizenId, districtId);
        if (entry == null)
        {
            return;
        }

        int totalActivities = entry.TotalActivities;
        CitizenTrustLevel evaluated = CitizenTrustLevelEvaluator.Evaluate(entry.JoinDate, totalActivities, DateTime.Now);

        if (entry.TrustLevel != evaluated)
        {
            _trustRepository.UpdateTrustLevel(citizenId, districtId, evaluated);
        }
    }

    public void RefreshTrustForDistrict(long districtId)
    {
        List<CitizenTrustRecordEntry> entries = _trustRepository.GetByDistrict(districtId);
        foreach (CitizenTrustRecordEntry entry in entries)
        {
            int totalActivities = entry.TotalActivities;
            CitizenTrustLevel evaluated = CitizenTrustLevelEvaluator.Evaluate(entry.JoinDate, totalActivities, DateTime.Now);
            if (entry.TrustLevel != evaluated)
            {
                _trustRepository.UpdateTrustLevel(entry.CitizenId, districtId, evaluated);
            }
        }
    }

    private static DistrictMemberTrustRecordDto MapToDistrictMemberDto(CitizenTrustRecordEntry entry)
    {
        return new DistrictMemberTrustRecordDto
        {
            CitizenId = entry.CitizenId,
            CitizenFullName = entry.CitizenFullName,
            JoinDate = entry.JoinDate,
            OrganizedCompletedEventsCount = entry.OrganizedCompletedEventsCount,
            ConfirmedVolunteerEventsCount = entry.ConfirmedVolunteerEventsCount,
            TrustLevel = entry.TrustLevel.ToString()
        };
    }
}
