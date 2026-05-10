using System.Collections.Generic;
using CommunityHub.Application.Database.Repositories.DistrictCoordinator;
using CommunityHub.Application.Dto.DistrictCoordinator;

namespace CommunityHub.Application.Services.DistrictCoordinator;

public class DistrictStatisticsService
{
    private readonly CitizenTrustStatisticDbRepository _repository;

    public DistrictStatisticsService()
    {
        _repository = new CitizenTrustStatisticDbRepository();
    }

    public List<CitizenTrustStatisticDto> GetStatisticsByCoordinatorId(long coordinatorId)
    {
        return _repository.GetByCoordinatorId(coordinatorId);
    }
}
