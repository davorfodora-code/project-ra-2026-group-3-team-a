using CommunityHub.Application.Dto.Citizen;

namespace CommunityHub.Application.Services.Citizen;

public interface ICitizenCoordinatorReviewService
{
    List<CoordinatorReviewDto> GetMyDistrictCoordinatorReviews(long citizenId);
    CoordinatorReviewActionResultDto CreateReview(long citizenId, CreateCoordinatorReviewDto dto);
    CoordinatorReviewActionResultDto ReportReview(long citizenId, long reviewId);
}