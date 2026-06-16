using System.Collections.Generic;

namespace CommunityHub.Application.Services.DistrictCoordinator.Dto;

public class DistrictDetailDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public decimal Budget { get; set; }
    public List<string> Streets { get; set; } = new();
    public List<string> ImagePaths { get; set; } = new();
}
