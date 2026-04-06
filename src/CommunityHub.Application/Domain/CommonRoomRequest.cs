namespace CommunityHub.Application.Domain;

public class CommonRoomRequest
{
    public long Id { get; private set; }
    public User Tenant { get; private set; }
    public CommonRoom CommonRoom { get; private set; }
    public DateTime RequestedStartDate { get; private set; }
    public DateTime RequestedEndDate { get; private set; }
    public DateTime? ProposedStartDate { get; private set; }
    public DateTime? ProposedEndDate { get; private set; }
    public DateTime? ApprovedDate { get; private set; }
    public string Status { get; private set; }

    public CommonRoomRequest(long id, User tenant, CommonRoom commonRoom, DateTime requestedStartDate, DateTime requestedEndDate, DateTime? proposedStartDate, DateTime? proposedEndDate, DateTime? approvedDate, string status)
    {
        Id = id;
        Tenant = tenant;
        CommonRoom = commonRoom;
        RequestedStartDate = requestedStartDate;
        RequestedEndDate = requestedEndDate;
        ProposedStartDate = proposedStartDate;
        ProposedEndDate = proposedEndDate;
        ApprovedDate = approvedDate;
        Status = status;
    }

    public CommonRoomRequest(User tenant, CommonRoom commonRoom, DateTime requestedStartDate, DateTime requestedEndDate, DateTime? proposedStartDate, DateTime? proposedEndDate, DateTime? approvedDate, string status)
    {
        Id = 0;
        Tenant = tenant;
        CommonRoom = commonRoom;
        RequestedStartDate = requestedStartDate;
        RequestedEndDate = requestedEndDate;
        ProposedStartDate = proposedStartDate;
        ProposedEndDate = proposedEndDate;
        ApprovedDate = approvedDate;
        Status = status;
    }
}
