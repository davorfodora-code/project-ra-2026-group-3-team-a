using System;

namespace CommunityHub.Application.Domain;

public class DistrictAccessRequest
{
    public long Id { get; private set; }
    public User User { get; private set; }
    public District District { get; private set; }
    public DateTime CreationDate { get; private set; }
    public DistrictAccessRequestStatus Status { get; private set; }
    public string? RejectionReason { get; private set; }

    public DistrictAccessRequest(long id, User user, District district, DateTime creationDate, DistrictAccessRequestStatus status, string? rejectionReason)
    {
        Id = id;
        User = user;
        District = district;
        CreationDate = creationDate;
        Status = status;
        RejectionReason = rejectionReason;
    }

    public DistrictAccessRequest(User user, District district)
    {
        Id = 0;
        User = user;
        District = district;
        CreationDate = DateTime.Now;
        Status = DistrictAccessRequestStatus.Pending;
        RejectionReason = null;
    }

    public void Approve()
    {
        Status = DistrictAccessRequestStatus.Approved;
    }

    public void Reject(string? reason = null)
    {
        Status = DistrictAccessRequestStatus.Rejected;
        RejectionReason = reason;
    }

    public bool CanBeCancelledByCitizen()
    {
        return Status == DistrictAccessRequestStatus.Pending;
    }
}
