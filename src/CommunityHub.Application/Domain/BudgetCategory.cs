namespace CommunityHub.Application.Domain;

public class BudgetCategory
{
    public BudgetCategory(long id, string name)
    {
        Id   = id;
        Name = name;
    }
    public long   Id   { get; }
    public string Name { get; }
}
