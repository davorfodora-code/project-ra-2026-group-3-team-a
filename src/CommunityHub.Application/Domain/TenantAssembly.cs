namespace CommunityHub.Application.Domain;

public class TenantAssembly
{
    public long Id { get; private set; }
    public Building Building { get; private set; }
    public DateTime ScheduledAt { get; private set; }
    public List<AssemblyTopic> Topics { get; private set; }
    public string Status { get; private set; }

    public TenantAssembly(long id, Building building, DateTime scheduledAt, string status)
    {
        Id = id;
        Building = building;
        ScheduledAt = scheduledAt;
        Topics = new List<AssemblyTopic>();
        Status = status;
    }

    public TenantAssembly(Building building, DateTime scheduledAt, string status)
    {
        Id = 0;
        Building = building;
        ScheduledAt = scheduledAt;
        Topics = new List<AssemblyTopic>();
        Status = status;
    }

    public void AddTopic(AssemblyTopic topic)
    {
        Topics.Add(topic);
        topic.Assembly = this;
    }

    // Teme mogu da se menjaju najkasnije 24h pred skupstinu
    public bool CanEditTopics() => DateTime.Now < ScheduledAt.AddHours(-24);
}
