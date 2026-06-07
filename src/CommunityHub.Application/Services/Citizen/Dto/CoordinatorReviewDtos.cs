namespace CommunityHub.Application.Dto.Citizen;

public class CoordinatorReviewDto
{
    public long Id { get; set; }
    public long DistrictId { get; set; }
    public long CoordinatorId { get; set; }
    public long AuthorId { get; set; }
    public string AuthorFullName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Justification { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int ReportCount { get; set; }
    public int TrustedReportCount { get; set; }
    public bool IsMine { get; set; }
    public bool IsReportedByMe { get; set; }

    public string RatingDisplay => $"{Rating}/5";
    public string CreatedAtDisplay => CreatedAt.ToString("dd.MM.yyyy. HH:mm");
    public string ReportDisplay => $"{ReportCount} ukupno / {TrustedReportCount} poverljivih";
    public string MyStatusDisplay => IsMine ? "Moja recenzija" : string.Empty;
    public string ReportedStatusDisplay => IsReportedByMe ? "Već prijavljeno" : "Može se prijaviti";
}

public class CreateCoordinatorReviewDto
{
    public int Rating { get; set; }
    public string Justification { get; set; } = string.Empty;
}

public class CoordinatorReviewActionResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public static CoordinatorReviewActionResultDto Ok(string message) =>
        new() { Success = true, Message = message };

    public static CoordinatorReviewActionResultDto Fail(string message) =>
        new() { Success = false, Message = message };
}