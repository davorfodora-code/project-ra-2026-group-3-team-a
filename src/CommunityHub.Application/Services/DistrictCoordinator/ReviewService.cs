using System;
using System.Collections.Generic;
using System.Linq;
using CommunityHub.Application.Services.DistrictCoordinator.Dto;

namespace CommunityHub.Application.Services.DistrictCoordinator;

public class ReviewService
{
    private readonly IReviewRepository _repo;

    public ReviewService(IReviewRepository repo) => _repo = repo;

    public List<ReviewDto> GetReviewsForLastYear(long districtId)
    {
        var since = DateTime.Now.AddYears(-1);
        return _repo.GetByDistrictId(districtId, since)
            .Select(r => new ReviewDto
            {
                Rating    = r.Rating,
                Comment   = r.Comment,
                CreatedAt = r.CreatedAt
            })
            .OrderByDescending(r => r.CreatedAt)
            .ToList();
    }
}
