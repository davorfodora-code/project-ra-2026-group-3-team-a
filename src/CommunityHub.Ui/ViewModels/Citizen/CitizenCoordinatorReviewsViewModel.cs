using CommunityHub.Application;
using CommunityHub.Application.Dto.Citizen;
using CommunityHub.Application.Services.Citizen;

namespace CommunityHub.Ui.ViewModels.Citizen;

public class CitizenCoordinatorReviewsViewModel
{
    private readonly long _citizenId;
    private readonly ICitizenCoordinatorReviewService _service;

    public CitizenCoordinatorReviewsViewModel(long citizenId)
    {
        _citizenId = citizenId;
        _service = Injector.Get<ICitizenCoordinatorReviewService>();
    }

    public List<CoordinatorReviewDto> LoadReviews()
    {
        return _service.GetMyDistrictCoordinatorReviews(_citizenId);
    }

    public CoordinatorReviewActionResultDto CreateReview(int rating, string justification)
    {
        return _service.CreateReview(_citizenId, new CreateCoordinatorReviewDto
        {
            Rating = rating,
            Justification = justification
        });
    }

    public CoordinatorReviewActionResultDto ReportReview(long reviewId)
    {
        return _service.ReportReview(_citizenId, reviewId);
    }
}