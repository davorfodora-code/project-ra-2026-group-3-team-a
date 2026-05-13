namespace CommunityHub.Application.Domain;

public class DistrictEventRequiredItem
{
    public long Id { get; private set; }
    public string Name { get; private set; }
    public long? AssignedVolunteerId { get; private set; }

    public bool IsCovered => AssignedVolunteerId.HasValue;

    public DistrictEventRequiredItem(long id, string name, long? assignedVolunteerId = null)
    {
        Id = id;
        Name = name;
        AssignedVolunteerId = assignedVolunteerId;
    }

    public DistrictEventRequiredItem(string name)
    {
        Id = 0;
        Name = name;
        AssignedVolunteerId = null;
    }

    public void AssignToVolunteer(long volunteerId)
    {
        if (volunteerId == 0)
            throw new InvalidOperationException("Neispravan identifikator volontera.");
        if (IsCovered)
            throw new InvalidOperationException("Stavka je vec preuzeta od strane drugog volontera.");

        AssignedVolunteerId = volunteerId;
    }
}
