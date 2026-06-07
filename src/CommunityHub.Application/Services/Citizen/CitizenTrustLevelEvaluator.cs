using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Services.Citizen;

internal static class CitizenTrustLevelEvaluator
{
    private const int ActiveThreshold = 5;
    private const int HighlightedThreshold = 10;
    private const int TrustedThreshold = 16;

    public static CitizenTrustLevel Evaluate(DateTime joinDate, int totalActivities, DateTime referenceDate)
    {
        DateTime effectiveJoinDate = joinDate > referenceDate ? referenceDate : joinDate;
        if (IsNewMember(effectiveJoinDate, referenceDate))
        {
            return CitizenTrustLevel.Nov;
        }

        return EvaluateByActivityCount(totalActivities);
    }

    private static bool IsNewMember(DateTime joinDate, DateTime referenceDate) =>
        joinDate > referenceDate.AddYears(-1);

    private static CitizenTrustLevel EvaluateByActivityCount(int totalActivities) =>
        totalActivities switch
        {
            < ActiveThreshold => CitizenTrustLevel.Neaktivan,
            < HighlightedThreshold => CitizenTrustLevel.Aktivan,
            < TrustedThreshold => CitizenTrustLevel.Istaknuti,
            _ => CitizenTrustLevel.OdPoverenja
        };
}
