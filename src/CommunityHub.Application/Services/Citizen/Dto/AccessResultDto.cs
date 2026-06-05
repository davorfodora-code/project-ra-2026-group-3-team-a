namespace CommunityHub.Application.Dto.Citizen;

public enum AccessResultType
{
    Success,
    Warning,
    Error
}

public class AccessResultDto
{
    public AccessResultType Type { get; set; }
    public string Message { get; set; } = string.Empty;
}
