namespace CommunityHub.Application.Domain;

public class CityObject
{
    public CityObject(long id, string name, string type, Location location, int capacity)
    {
        Id       = id;
        Name     = name;
        Type     = type;
        Location = location;
        Capacity = capacity;
    }

    public long     Id       { get; }
    public string   Name     { get; }
    public string   Type     { get; }
    public Location Location { get; }
    public int      Capacity { get; }
}
