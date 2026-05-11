using System.Collections.Generic;
using CommunityHub.Application.Dto.DistrictCoordinator;
using CommunityHub.Application.Services.DistrictCoordinator;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;

public class StatisticsViewModel
{
    private readonly DistrictStatisticsService _service;
    private readonly long _coordinatorId;
    private List<CitizenTrustStatisticDto> _statistics;

    public StatisticsViewModel(long coordinatorId)
    {
        _coordinatorId = coordinatorId;
        _service = new DistrictStatisticsService();
        _statistics = new List<CitizenTrustStatisticDto>();
    }

    public void LoadStatistics()
    {
        _statistics = _service.GetStatisticsByCoordinatorId(_coordinatorId);
    }

    public List<CitizenTrustStatisticDto> GetStatistics() => _statistics;
}
