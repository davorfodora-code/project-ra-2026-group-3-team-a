using System;
using System.Collections.Generic;
using System.Text;

namespace CommunityHub.Application.Domain;

public class DistrictAccessRequest
{
    public long Id { get; private set; }
    public User User { get; private set; }
    public District District { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DistrictAccessRequestStatus Status { get; private set; }
    public string? RejectionReason { get; private set; }

    public DistrictAccessRequest(long id, User user, District district, DateTime createdAt, DistrictAccessRequestStatus status, string? rejectionReason)
    {
        Id = id;
        User = user;
        District = district;
        CreatedAt = createdAt;
        Status = status;
        RejectionReason = rejectionReason;
    }

    public DistrictAccessRequest(User user, District district)
    {
        Id = 0;
        User = user;
        District = district;
        CreatedAt = DateTime.UtcNow;
        Status = DistrictAccessRequestStatus.Pending;
        RejectionReason = null;
    }
}
