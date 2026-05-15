using CommunityHub.Application.Services.BuildingManager.Dto;

namespace CommunityHub.Application.Services.BuildingManager;

public class BulletinBoardStatsService
{
    private readonly IBulletinBoardStatsRepository _repo;

    public BulletinBoardStatsService()
    {
        _repo = Injector.Get<IBulletinBoardStatsRepository>();
    }

    public List<(long BuildingId, string Address)> GetManagedBuildings(long managerId)
        => _repo.GetManagedBuildings(managerId);

    public List<BulletinBoardAdDto> GetAllAds(long buildingId)
        => _repo.GetAllAds(buildingId);

    public BulletinBoardStatsDto GetStats(long buildingId, int year, int? month)
        => _repo.GetStats(buildingId, year, month);
}
