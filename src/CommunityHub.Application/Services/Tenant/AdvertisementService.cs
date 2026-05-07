using CommunityHub.Application.Database.Repositories.Tenant;
using CommunityHub.Application.Domain;
using CommunityHub.Application.Dto.Tenant;

namespace CommunityHub.Application.Services.Tenant;

public class AdvertisementService
{
    private readonly AdvertisementDbRepository _repository;

    public AdvertisementService()
    {
        _repository = new AdvertisementDbRepository();
    }

    public List<TenantBuildingDto> GetBuildingsForTenant(long tenantId)
        => _repository.GetBuildingsByTenant(tenantId);

    public List<AdvertisementDto> GetActiveBoardAds(long tenantId, long buildingId)
    {
        long bulletinBoardId = _repository.EnsureBulletinBoard(buildingId);
        _repository.ArchiveExpiredAds(bulletinBoardId);

        return _repository.GetActiveByBulletinBoard(bulletinBoardId)
            .Select(a => MapToDto(a, tenantId))
            .ToList();
    }

    public (bool Success, List<AdvertisementDto> MatchingAds) CreateAdvertisement(CreateAdvertisementDto dto)
    {
        long bulletinBoardId = _repository.EnsureBulletinBoard(dto.BuildingId);
        _repository.ArchiveExpiredAds(bulletinBoardId);

        long adId = _repository.CreateAdvertisement(
            bulletinBoardId, dto.TenantId, dto.Description,
            dto.IsOffering, dto.StartDate, dto.EndDate, dto.Category);

        _repository.CreateSlotsForAd(adId, dto.StartDate, dto.EndDate);

        var matchingAds = _repository.GetMatchingAds(
                bulletinBoardId, dto.IsOffering, dto.StartDate, dto.EndDate, dto.Category, adId)
            .Select(a => MapToDto(a, dto.TenantId))
            .ToList();

        return (true, matchingAds);
    }

    public void ArchiveAd(long adId) => _repository.ArchiveAd(adId);

    private static AdvertisementDto MapToDto(Advertisement a, long currentTenantId) => new()
    {
        Id = a.Id,
        TenantName = $"{a.Tenant.Name} {a.Tenant.Surname}",
        IsOffering = a.IsOffering,
        StartDate = a.StartDate,
        EndDate = a.EndDate,
        Category = a.Category,
        Description = a.Description,
        IsOwn = a.Tenant.Id == currentTenantId
    };
}
