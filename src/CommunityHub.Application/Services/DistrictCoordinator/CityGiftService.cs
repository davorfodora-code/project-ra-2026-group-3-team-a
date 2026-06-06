using System;
using System.Collections.Generic;
using System.Linq;
using CommunityHub.Application.Services.DistrictCoordinator.Dto;

namespace CommunityHub.Application.Services.DistrictCoordinator;

public class CityGiftService
{
    private readonly ICityGiftRepository _repo;

    public CityGiftService(ICityGiftRepository repo) => _repo = repo;

    public List<CityGiftDto> GetAllGifts(long coordinatorId) => _repo.GetAllGifts(coordinatorId);

    public List<long> GetRegisteredDistrictIds(long giftId, long coordinatorId)
        => _repo.GetRegisteredDistrictIds(giftId, coordinatorId);

    public void Register(long giftId, long districtId, long coordinatorId)
        => _repo.Register(giftId, districtId, coordinatorId,
               DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified));

    public void AwardPendingGifts()
    {
        foreach (var (giftId, amount) in _repo.GetPendingGifts())
            AwardGift(giftId, amount);
    }

    private void AwardGift(long giftId, decimal amount)
    {
        var registrations = _repo.GetRegistrations(giftId);
        if (registrations.Count == 0)
        {
            _repo.MarkAwarded(giftId, null);
            return;
        }

        var categoryIds = _repo.GetCategoryIds();
        if (categoryIds.Count == 0)
        {
            _repo.MarkAwarded(giftId, null);
            return;
        }

        var winner = registrations
            .OrderBy(r => _repo.GetDistrictTotalBudget(r.DistrictId))
            .First();

        decimal perCategory = amount / categoryIds.Count;
        DateTime now = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);

        foreach (long catId in categoryIds)
            _repo.AddGiftDonation(winner.DistrictId, catId, winner.CoordinatorId, perCategory, now);

        _repo.MarkAwarded(giftId, winner.DistrictId);
    }
}
