namespace CommunityHub.Application.Domain;

public class BuildingAccessRequest
{
    public long Id { get; private set; }
    public User Tenant { get; private set; }
    public Building Building { get; private set; }
    public int ApartmentNumber { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string Status { get; private set; }
    public string? RejectionReason { get; private set; }

    public BuildingAccessRequest(long id, User tenant, Building building, int apartmentNumber, DateTime createdAt, string status, string? rejectionReason)
    {
        Id = id;
        Tenant = tenant;
        Building = building;
        ApartmentNumber = apartmentNumber;
        CreatedAt = createdAt;
        Status = status;
        RejectionReason = rejectionReason;
    }

    public BuildingAccessRequest(User tenant, Building building, int apartmentNumber, DateTime createdAt, string status, string? rejectionReason)
    {
        Id = 0;
        Tenant = tenant;
        Building = building;
        ApartmentNumber = apartmentNumber;
        CreatedAt = createdAt;
        Status = status;
        RejectionReason = rejectionReason;
    }
}
