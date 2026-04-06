namespace CommunityHub.Application.Domain;

public class BuildingMembership
{
    public long Id { get; private set; }
    public Building Building { get; private set; }
    public User Tenant { get; private set; }
    public int FloorNumber { get; private set; }
    public int ApartmentNumber { get; private set; }
    public DateTime JoinedAt { get; private set; }

    public BuildingMembership(long id, Building building, User tenant, int floorNumber, int apartmentNumber, DateTime joinedAt)
    {
        Id = id;
        Building = building;
        Tenant = tenant;
        FloorNumber = floorNumber;
        ApartmentNumber = apartmentNumber;
        JoinedAt = joinedAt;
    }

    public BuildingMembership(Building building, User tenant, int floorNumber, int apartmentNumber, DateTime joinedAt)
    {
        Id = 0;
        Building = building;
        Tenant = tenant;
        FloorNumber = floorNumber;
        ApartmentNumber = apartmentNumber;
        JoinedAt = joinedAt;
    }
}
