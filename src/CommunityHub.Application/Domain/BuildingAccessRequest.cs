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

    // Proverava da li zahtev može biti obrađen (samo Pending zahtevi)
    public bool CanBeProcessed() => Status == "Pending";

    // Odobrava zahtev — prelazi u status Accepted
    public void Approve()
    {
        if (!CanBeProcessed())
            throw new InvalidOperationException("Zahtev može biti obrađen samo ako je u statusu 'Pending'.");
        Status = "Accepted";
    }

    // Odbija zahtev sa opcionim obrazloženjem — prelazi u status Rejected
    public void Reject(string? reason)
    {
        if (!CanBeProcessed())
            throw new InvalidOperationException("Zahtev može biti obrađen samo ako je u statusu 'Pending'.");
        Status = "Rejected";
        RejectionReason = reason;
    }
}
