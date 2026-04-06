namespace CommunityHub.Application.Domain;

public class BulletinBoard
{
    public long Id { get; private set; }
    public Building Building { get; private set; }

    public BulletinBoard(long id, Building building)
    {
        Id = id;
        Building = building;
    }

    public BulletinBoard(Building building)
    {
        Id = 0;
        Building = building;
    }
}
