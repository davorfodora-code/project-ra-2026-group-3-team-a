namespace CommunityHub.Application.Services.BuildingManager.Dto;

public class CreateAssemblyDto
{
    public long BuildingId { get; set; }
    public DateTime ScheduledAt { get; set; }
    public List<string> Topics { get; set; } = new();
}
