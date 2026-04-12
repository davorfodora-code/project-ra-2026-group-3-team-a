namespace CommunityHub.Application.Domain;

public class DistrictStreet
{
    public long Id { get; private set; }
    public District? District { get; internal set; }
    public string StreetName { get; private set; }
    public int StartNumber { get; private set; }
    public int EndNumber { get; private set; }

    public DistrictStreet(long id, string streetName, int startNumber, int endNumber)
    {
        Id = id;
        StreetName = streetName;
        StartNumber = startNumber;
        EndNumber = endNumber;
        District = null;
    }

    public DistrictStreet(string streetName, int startNumber, int endNumber)
    {
        Id = 0;
        StreetName = streetName;
        StartNumber = startNumber;
        EndNumber = endNumber;
        District = null;
    }
}
