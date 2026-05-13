using System;
using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Dto.DistrictCoordinator;

public class CitizenTrustStatisticDto
{
    public long DistrictId { get; set; }
    public string DistrictName { get; set; } = string.Empty;
    public int NovCount { get; set; }
    public int NeaktivanCount { get; set; }
    public int AktivanCount { get; set; }
    public int IstaknutiCount { get; set; }
    public int OdPoverenjaCount { get; set; }

    public int TotalCount => NovCount + NeaktivanCount + AktivanCount + IstaknutiCount + OdPoverenjaCount;

    public MeetingType? Recommendation
    {
        get
        {
            if (TotalCount == 0) return null;

            int maxCount = Math.Max(NovCount, NeaktivanCount);
            if (maxCount == 0) return null;

            if (NovCount > NeaktivanCount) return MeetingType.Dobrodoslica;
            if (NeaktivanCount > NovCount) return MeetingType.Motivacija;

            return null;
        }
    }

    public string RecommendationDisplay => Recommendation switch
    {
        MeetingType.Dobrodoslica => "Preporučuje se sastanak dobrodošlice (najviše novih građana)",
        MeetingType.Motivacija   => "Preporučuje se motivacioni sastanak (najviše neaktivnih građana)",
        _                        => "Nema jasne preporuke za sastanak"
    };
}
