using System;
using System.Collections.Generic;
using CommunityHub.Application.Services.DistrictCoordinator.Dto;

namespace CommunityHub.Application.Services.DistrictCoordinator;

public interface ICityGiftRepository
{
    List<CityGiftDto>                           GetAllGifts(long coordinatorId);
    List<(long Id, decimal Amount)>             GetPendingGifts();
    List<(long DistrictId, long CoordinatorId)> GetRegistrations(long giftId);
    List<long>                                  GetRegisteredDistrictIds(long giftId, long coordinatorId);
    void                                        Register(long giftId, long districtId, long coordinatorId, DateTime registeredAt);
    decimal                                     GetDistrictTotalBudget(long districtId);
    List<long>                                  GetCategoryIds();
    void                                        AddGiftDonation(long districtId, long categoryId, long coordinatorId, decimal amount, DateTime donatedAt);
    void                                        MarkAwarded(long giftId, long? winnerDistrictId);
}
