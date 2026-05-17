using CommunityHub.Application.Domain;
using CommunityHub.Application.Services.Citizen;

namespace CommunityHub.Application.Database.Repositories.Citizen;

public class CitizenDistrictEventDbRepository : ICitizenDistrictEventRepository
{
    private readonly CitizenDistrictEventWriteDbRepository _writeRepository = new();
    private readonly CitizenDistrictEventReadDbRepository _readRepository = new();

    public long Create(DistrictEvent districtEvent) => _writeRepository.Create(districtEvent);

    public List<DistrictEvent> GetAllByDistrictId(long districtId) => _readRepository.GetAllByDistrictId(districtId);

    public DistrictEvent? GetById(long eventId) => _readRepository.GetById(eventId);

    public void UpdateStatus(long eventId, DistrictEventStatus status) => _writeRepository.UpdateStatus(eventId, status);
}
