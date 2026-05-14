using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Services.Citizen;

public interface ICitizenDistrictEventRepository
{
    long Create(DistrictEvent districtEvent);
    List<DistrictEvent> GetAllByDistrictId(long districtId);
    DistrictEvent? GetById(long eventId);
    void UpdateStatus(long eventId, DistrictEventStatus status);
}
