namespace CommunityHub.Application.Services.BuildingManager.Dto;

public class TopicSuggestionDto
{
    public long Id { get; init; }
    public string TenantFullName { get; init; } = string.Empty;
    public string Topic { get; init; } = string.Empty;
}
