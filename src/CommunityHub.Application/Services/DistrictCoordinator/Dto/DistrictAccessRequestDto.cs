using System;

namespace CommunityHub.Application.Services.DistrictCoordinator.Dto;

public class DistrictAccessRequestDto
{
    public long RequestId { get; set; }
    public string CitizenFullName { get; set; } = string.Empty;
    public string CitizenAddress { get; set; } = string.Empty;
    public string DistrictName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }

    public string CreatedAtFormatted => CreatedAt.ToString("dd.MM.yyyy. HH:mm");

    public string StatusDisplay => Status switch
    {
        "Pending"  => "Čeka odobrenje",
        "Approved" => "Odobren",
        "Rejected" => "Odbijen",
        _          => Status
    };

    public bool IsPending => Status == "Pending";
}
