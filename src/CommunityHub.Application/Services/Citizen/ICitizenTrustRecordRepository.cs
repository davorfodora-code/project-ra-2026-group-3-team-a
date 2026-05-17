using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Services.Citizen;

public interface ICitizenTrustRecordRepository
{
    CitizenTrustRecordEntry? GetByUserAndDistrict(long userId, long districtId);
    List<CitizenTrustRecordEntry> GetByDistrict(long districtId);
    void UpdateTrustLevel(long userId, long districtId, CitizenTrustLevel trustLevel);
}
