namespace CommunityHub.Application.Domain;

public class AssemblyTopic
{
    public long Id { get; private set; }
    public TenantAssembly? Assembly { get; internal set; }
    public string Text { get; private set; }
    public bool IsAdditional { get; private set; }

    public AssemblyTopic(long id, string text, bool isAdditional)
    {
        Id = id;
        Assembly = null;
        Text = text;
        IsAdditional = isAdditional;
    }

    public AssemblyTopic(string text, bool isAdditional)
    {
        Id = 0;
        Assembly = null;
        Text = text;
        IsAdditional = isAdditional;
    }
}
