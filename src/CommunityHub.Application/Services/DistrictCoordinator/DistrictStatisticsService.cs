using System.Collections.Generic;
using CommunityHub.Application.Services.DistrictCoordinator.Dto;

namespace CommunityHub.Application.Services.DistrictCoordinator;

public class DistrictStatisticsService
{
    private readonly ICitizenTrustStatisticRepository _repository;

    public DistrictStatisticsService(ICitizenTrustStatisticRepository repository)
    {
        _repository = repository;
    }

    public List<CitizenTrustStatisticDto> GetStatisticsByCoordinatorId(long coordinatorId)
    {
        return _repository.GetByCoordinatorId(coordinatorId);
    }
}
