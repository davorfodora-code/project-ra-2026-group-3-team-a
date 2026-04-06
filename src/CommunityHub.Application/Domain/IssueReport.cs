namespace CommunityHub.Application.Domain;

public class IssueReport
{
    public long Id { get; private set; }
    public User Tenant { get; private set; }
    public Building Building { get; private set; }
    public string Description { get; private set; }
    public string Priority { get; private set; }
    public DateTime ReportedAt { get; private set; }
    public string Status { get; private set; }

    public IssueReport(long id, User tenant, Building building, string description, string priority, DateTime reportedAt, string status)
    {
        Id = id;
        Tenant = tenant;
        Building = building;
        Description = description;
        Priority = priority;
        ReportedAt = reportedAt;
        Status = status;
    }

    public IssueReport(User tenant, Building building, string description, string priority, DateTime reportedAt, string status)
    {
        Id = 0;
        Tenant = tenant;
        Building = building;
        Description = description;
        Priority = priority;
        ReportedAt = reportedAt;
        Status = status;
    }
}
