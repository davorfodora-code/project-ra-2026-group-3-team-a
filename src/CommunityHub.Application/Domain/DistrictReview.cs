using System;

namespace CommunityHub.Application.Domain;

public class DistrictReview
{
    public DistrictReview(long id, long districtId, int rating, string? comment, DateTime createdAt)
    {
        Id         = id;
        DistrictId = districtId;
        Rating     = rating;
        Comment    = comment;
        CreatedAt  = createdAt;
    }

    public long      Id         { get; }
    public long      DistrictId { get; }
    public int       Rating     { get; }
    public string?   Comment    { get; }
    public DateTime  CreatedAt  { get; }
}
