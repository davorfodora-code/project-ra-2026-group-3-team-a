using CommunityHub.Application.Dto.Citizen;

namespace CommunityHub.Application.Services.Citizen;

public class CitizenCoordinatorReviewService : ICitizenCoordinatorReviewService
{
    private const int GeneralReportRemovalThreshold = 10;
    private const int TrustedReportRemovalThreshold = 5;
    private const int RemovedReviewsBlockThreshold = 3;

    private readonly ICitizenCoordinatorReviewRepository _reviewRepository;
    private readonly ICitizenDistrictMembershipRepository _membershipRepository;

    public CitizenCoordinatorReviewService(
        ICitizenCoordinatorReviewRepository reviewRepository,
        ICitizenDistrictMembershipRepository membershipRepository)
    {
        _reviewRepository = reviewRepository;
        _membershipRepository = membershipRepository;
    }

    public List<CoordinatorReviewDto> GetMyDistrictCoordinatorReviews(long citizenId)
    {
        long? districtId = _membershipRepository.GetMembershipDistrictId(citizenId);
        if (!districtId.HasValue)
        {
            return new List<CoordinatorReviewDto>();
        }

        return _reviewRepository.GetActiveReviewsByDistrict(districtId.Value, citizenId);
    }

    public CoordinatorReviewActionResultDto CreateReview(long citizenId, CreateCoordinatorReviewDto dto)
    {
        long? districtId = _membershipRepository.GetMembershipDistrictId(citizenId);
        if (!districtId.HasValue)
        {
            return CoordinatorReviewActionResultDto.Fail("Niste član nijednog kvarta.");
        }

        if (dto.Rating < 1 || dto.Rating > 5)
        {
            return CoordinatorReviewActionResultDto.Fail("Ocena mora biti između 1 i 5.");
        }

        if (dto.Rating < 3 && string.IsNullOrWhiteSpace(dto.Justification))
        {
            return CoordinatorReviewActionResultDto.Fail("Za ocene niže od 3 morate uneti obrazloženje.");
        }

        int removedReviewsCount = _reviewRepository.CountRemovedReviewsByAuthor(citizenId);
        if (removedReviewsCount > RemovedReviewsBlockThreshold)
        {
            return CoordinatorReviewActionResultDto.Fail("Ne možete više ostavljati recenzije jer vam je uklonjeno više od 3 recenzije.");
        }

        DateTime now = DateTime.Now;
        if (_reviewRepository.HasReviewInMonth(districtId.Value, citizenId, now.Year, now.Month))
        {
            return CoordinatorReviewActionResultDto.Fail("Koordinatora možete recenzirati najviše jednom mesečno.");
        }

        long coordinatorId = _reviewRepository.GetDistrictCoordinatorId(districtId.Value);

        _reviewRepository.Create(
            districtId.Value,
            coordinatorId,
            citizenId,
            dto.Rating,
            string.IsNullOrWhiteSpace(dto.Justification) ? null : dto.Justification.Trim(),
            now);

        return CoordinatorReviewActionResultDto.Ok("Recenzija je uspešno sačuvana.");
    }

    public CoordinatorReviewActionResultDto ReportReview(long citizenId, long reviewId)
    {
        long? districtId = _membershipRepository.GetMembershipDistrictId(citizenId);
        if (!districtId.HasValue)
        {
            return CoordinatorReviewActionResultDto.Fail("Niste član nijednog kvarta.");
        }

        CoordinatorReviewDto? review = _reviewRepository.GetReviewById(reviewId, citizenId);
        if (review == null)
        {
            return CoordinatorReviewActionResultDto.Fail("Recenzija nije pronađena.");
        }

        if (!_reviewRepository.ReviewBelongsToDistrict(reviewId, districtId.Value))
        {
            return CoordinatorReviewActionResultDto.Fail("Recenzija ne pripada vašem kvartu.");
        }

        if (review.AuthorId == citizenId)
        {
            return CoordinatorReviewActionResultDto.Fail("Ne možete prijaviti sopstvenu recenziju.");
        }

        if (_reviewRepository.HasReport(reviewId, citizenId))
        {
            return CoordinatorReviewActionResultDto.Fail("Već ste prijavili ovu recenziju.");
        }

        _reviewRepository.AddReport(reviewId, citizenId, DateTime.Now);

        int reportCount = _reviewRepository.CountReports(reviewId);
        int trustedReportCount = _reviewRepository.CountTrustedReports(reviewId);

        if (reportCount > GeneralReportRemovalThreshold || trustedReportCount >= TrustedReportRemovalThreshold)
        {
            _reviewRepository.MarkRemoved(reviewId, DateTime.Now);
            return CoordinatorReviewActionResultDto.Ok("Recenzija je prijavljena i automatski uklonjena.");
        }

        return CoordinatorReviewActionResultDto.Ok("Recenzija je uspešno prijavljena.");
    }
}