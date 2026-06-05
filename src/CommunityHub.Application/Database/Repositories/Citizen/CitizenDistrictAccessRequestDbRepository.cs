using CommunityHub.Application.Domain;
using CommunityHub.Application.Services.Citizen;

namespace CommunityHub.Application.Database.Repositories.Citizen;

public class CitizenDistrictAccessRequestDbRepository : ICitizenDistrictAccessRequestRepository
{
    private readonly CitizenDistrictAccessRequestQueryDbRepository _queryRepository = new();
    private readonly CitizenDistrictAccessRequestCommandDbRepository _commandRepository = new();

    public bool ExistsPending(long userId, long districtId) => _queryRepository.ExistsPending(userId, districtId);

    public long Create(long userId, long districtId) => _commandRepository.Create(userId, districtId);

    public List<DistrictAccessRequest> GetAllByUserId(long userId) => _queryRepository.GetAllByUserId(userId);

    public void Delete(long requestId) => _commandRepository.Delete(requestId);
}

