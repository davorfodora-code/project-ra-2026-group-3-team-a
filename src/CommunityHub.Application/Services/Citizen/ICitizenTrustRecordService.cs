using CommunityHub.Application.Dto.Citizen;

namespace CommunityHub.Application.Services.Citizen;

public interface ICitizenTrustRecordService
{
    CitizenTrustRecordDto? GetMyTrustRecord(long citizenId);
    List<DistrictMemberTrustRecordDto> GetDistrictTrustRecords(long citizenId);
    void RefreshTrustForCitizen(long citizenId, long districtId);
    void RefreshTrustForDistrict(long districtId);
}
