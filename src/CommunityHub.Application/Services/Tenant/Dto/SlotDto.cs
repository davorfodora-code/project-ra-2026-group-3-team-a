namespace CommunityHub.Application.Services.Tenant.Dto;

public class SlotDto
{
    public long Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public string DisplayText => $"{StartTime:dd.MM.yyyy.}  {StartTime:HH:mm} – {EndTime:HH:mm}";
}
