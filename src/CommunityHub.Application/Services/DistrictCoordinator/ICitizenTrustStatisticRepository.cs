using System.Collections.Generic;
using CommunityHub.Application.Services.DistrictCoordinator.Dto;

namespace CommunityHub.Application.Services.DistrictCoordinator;

public interface ICitizenTrustStatisticRepository
{
    List<CitizenTrustStatisticDto> GetByCoordinatorId(long coordinatorId);
}
