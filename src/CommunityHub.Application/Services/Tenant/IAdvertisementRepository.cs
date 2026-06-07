using CommunityHub.Application.Domain;
using CommunityHub.Application.Services.Tenant.Dto;

namespace CommunityHub.Application.Services.Tenant;

public interface IAdvertisementRepository
{
    List<TenantBuildingDto> GetBuildingsByTenant(long tenantId);
    long EnsureBulletinBoard(long buildingId);
    long CreateAdvertisement(long bulletinBoardId, long tenantId, string description,
        bool isOffering, DateTime startDate, DateTime endDate, string category);
    void CreateSlotsForAd(long adId, DateTime startDate, DateTime endDate);
    List<Advertisement> GetActiveByBulletinBoard(long bulletinBoardId);
    List<Advertisement> GetMatchingAds(long bulletinBoardId, bool isOffering,
        DateTime startDate, DateTime endDate, string category, long excludeAdId);
    void ArchiveAd(long adId);
    void ArchiveExpiredAds(long bulletinBoardId);
    List<SlotDto> GetAvailableSlots(long adId, DateTime rangeStart, DateTime rangeEnd);
    void ReserveSlot(long slotId, long requesterAdId, long tenantId);
    void CreateMatchingAdNotifications(long newAdId, long bulletinBoardId, bool newAdIsOffering,
        string category, DateTime startDate, DateTime endDate);
}
