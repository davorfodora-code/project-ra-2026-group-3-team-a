using System;
using System.Collections.Generic;
using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Services.DistrictCoordinator;

public interface IReviewRepository
{
    List<DistrictReview> GetByDistrictId(long districtId, DateTime since);
}
