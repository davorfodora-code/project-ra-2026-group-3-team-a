using CommunityHub.Application.Dto.Citizen;

namespace CommunityHub.Application.Services.Citizen;

public interface ICitizenCoordinatorReviewRepository
{
    List<CoordinatorReviewDto> GetActiveReviewsByDistrict(long districtId, long citizenId);
    CoordinatorReviewDto? GetReviewById(long reviewId, long citizenId);
    long Create(long districtId, long coordinatorId, long authorId, int rating, string? justification, DateTime createdAt);
    long GetDistrictCoordinatorId(long districtId);
    bool HasReviewInMonth(long districtId, long authorId, int year, int month);
    int CountRemovedReviewsByAuthor(long authorId);
    bool ReviewBelongsToDistrict(long reviewId, long districtId);
    bool HasReport(long reviewId, long reporterId);
    void AddReport(long reviewId, long reporterId, DateTime reportedAt);
    int CountReports(long reviewId);
    int CountTrustedReports(long reviewId);
    void MarkRemoved(long reviewId, DateTime removedAt);
}