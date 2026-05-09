namespace CommunityHub.Application.Dto.BuildingManager;

public class BuildingAccessRequestDto
{
    public long RequestId { get; set; }
    public string TenantFullName { get; set; } = string.Empty;
    public string BuildingAddress { get; set; } = string.Empty;
    public string Neighborhood { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int ApartmentNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedAtFormatted => CreatedAt.ToString("dd.MM.yyyy. HH:mm");
    public string DateDisplay => CreatedAt.ToString("dd.MM.yyyy.");
    public string Status { get; set; } = string.Empty;

    public string StatusDisplay => Status switch
    {
        "Pending"  => "ČEKA ODOBRENJE",
        "Accepted" => "Prihvaćen",
        "Rejected" => "Odbijen",
        _          => Status
    };

    public bool IsPending => Status == "Pending";
}
