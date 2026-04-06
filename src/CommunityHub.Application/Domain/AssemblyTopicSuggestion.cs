namespace CommunityHub.Application.Domain;

public class AssemblyTopicSuggestion
{
    public long Id { get; private set; }
    public TenantAssembly Assembly { get; private set; }
    public User Tenant { get; private set; }
    public string Topic { get; private set; }

    public AssemblyTopicSuggestion(long id, TenantAssembly assembly, User tenant, string topic)
    {
        Id = id;
        Assembly = assembly;
        Tenant = tenant;
        Topic = topic;
    }

    public AssemblyTopicSuggestion(TenantAssembly assembly, User tenant, string topic)
    {
        Id = 0;
        Assembly = assembly;
        Tenant = tenant;
        Topic = topic;
    }
}
