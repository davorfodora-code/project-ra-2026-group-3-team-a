namespace CommunityHub.Application.Domain;

public class Advertisement
{
    public long Id { get; private set; }
    public BulletinBoard BulletinBoard { get; private set; }
    public User Tenant { get; private set; }
    public string Description { get; private set; }
    public bool IsOffering { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public string Category { get; private set; }
    public string Status { get; private set; }

    public Advertisement(long id, BulletinBoard bulletinBoard, User tenant, string description, bool isOffering, DateTime startDate, DateTime endDate, string category, string status)
    {
        Id = id;
        BulletinBoard = bulletinBoard;
        Tenant = tenant;
        Description = description;
        IsOffering = isOffering;
        StartDate = startDate;
        EndDate = endDate;
        Category = category;
        Status = status;
    }

    public Advertisement(BulletinBoard bulletinBoard, User tenant, string description, bool isOffering, DateTime startDate, DateTime endDate, string category, string status)
    {
        Id = 0;
        BulletinBoard = bulletinBoard;
        Tenant = tenant;
        Description = description;
        IsOffering = isOffering;
        StartDate = startDate;
        EndDate = endDate;
        Category = category;
        Status = status;
    }

    public void Archive() => Status = "Arhiviran";
    public bool IsExpired() => EndDate.Date < DateTime.Today;
    public bool CanArchive(long tenantId) => Tenant.Id == tenantId && Status == "Aktivan";
}
