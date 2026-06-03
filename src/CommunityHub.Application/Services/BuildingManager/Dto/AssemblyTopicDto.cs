namespace CommunityHub.Application.Services.BuildingManager.Dto;

public class AssemblyTopicDto
{
    public long Id { get; init; }
    public string Text { get; init; } = string.Empty;
    public bool IsAdditional { get; init; }
}
