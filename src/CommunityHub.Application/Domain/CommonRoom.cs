namespace CommunityHub.Application.Domain;

public class CommonRoom
{
    public long Id { get; private set; }
    public Building Building { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public int FloorNumber { get; private set; }
    public bool IsMultiDay { get; private set; }

    public CommonRoom(long id, Building building, string name, string description, int floorNumber, bool isMultiDay)
    {
        Id = id;
        Building = building;
        Name = name;
        Description = description;
        FloorNumber = floorNumber;
        IsMultiDay = isMultiDay;
    }

    public CommonRoom(Building building, string name, string description, int floorNumber, bool isMultiDay)
    {
        Id = 0;
        Building = building;
        Name = name;
        Description = description;
        FloorNumber = floorNumber;
        IsMultiDay = isMultiDay;
    }
}
