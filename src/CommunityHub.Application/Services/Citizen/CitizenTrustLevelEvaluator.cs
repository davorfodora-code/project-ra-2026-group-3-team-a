using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Services.Citizen;

internal static class CitizenTrustLevelEvaluator
{
    public static CitizenTrustLevel Evaluate(DateTime joinDate, int totalActivities, DateTime referenceDate)
    {
        if (joinDate > referenceDate)
        {
            joinDate = referenceDate;
        }

        if (joinDate > referenceDate.AddYears(-1))
        {
            return CitizenTrustLevel.Nov;
        }

        if (totalActivities < 5)
        {
            return CitizenTrustLevel.Neaktivan;
        }

        if (totalActivities <= 9)
        {
            return CitizenTrustLevel.Aktivan;
        }

        if (totalActivities <= 15)
        {
            return CitizenTrustLevel.Istaknuti;
        }

        return CitizenTrustLevel.OdPoverenja;
    }
}
