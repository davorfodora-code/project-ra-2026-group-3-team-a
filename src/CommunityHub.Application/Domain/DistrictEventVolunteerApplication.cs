namespace CommunityHub.Application.Domain;

public class DistrictEventVolunteerApplication
{
    public long Id { get; private set; }
    public User Citizen { get; private set; }
    public DateTime AppliedAt { get; private set; }
    public List<long> SelectedItemIds { get; private set; }

    public DistrictEventVolunteerApplication(long id, User citizen, DateTime appliedAt, IEnumerable<long>? selectedItemIds = null)
    {
        Id = id;
        Citizen = citizen;
        AppliedAt = appliedAt;
        SelectedItemIds = selectedItemIds?.Distinct().ToList() ?? new List<long>();
    }

    public DistrictEventVolunteerApplication(User citizen, DateTime appliedAt, IEnumerable<long>? selectedItemIds = null)
    {
        Id = 0;
        Citizen = citizen;
        AppliedAt = appliedAt;
        SelectedItemIds = selectedItemIds?.Distinct().ToList() ?? new List<long>();
    }

    public void AddSelectedItems(IEnumerable<long> itemIds)
    {
        foreach (long itemId in itemIds.Distinct())
        {
            if (!SelectedItemIds.Contains(itemId))
            {
                SelectedItemIds.Add(itemId);
            }
        }
    }
}
