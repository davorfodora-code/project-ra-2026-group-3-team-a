using CommunityHub.Application.Domain;
using CommunityHub.Application.Services.Tenant.Dto;

namespace CommunityHub.Application.Services.Tenant;

public class AdvertisementService
{
    private readonly IAdvertisementRepository _repository;

    public AdvertisementService(IAdvertisementRepository repository)
    {
        _repository = repository;
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

    public (bool Success, long NewAdId, List<AdvertisementDto> MatchingAds) CreateAdvertisement(CreateAdvertisementDto dto)
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

        return (true, adId, matchingAds);
    }

    public void ArchiveAd(long adId) => _repository.ArchiveAd(adId);

    public List<SlotDto> GetAvailableSlots(long adId, DateTime rangeStart, DateTime rangeEnd)
        => _repository.GetAvailableSlots(adId, rangeStart, rangeEnd);

    public void ReserveSlot(long slotId, long requesterAdId, long tenantId)
        => _repository.ReserveSlot(slotId, requesterAdId, tenantId);

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
