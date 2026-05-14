using System.Collections.Generic;

namespace CommunityHub.Application.Services.DistrictCoordinator.Dto;

public class StreetInputDto
{
    public string StreetName { get; set; } = string.Empty;
    public int StartNumber { get; set; }
    public int EndNumber { get; set; }
}

public class CreateDistrictDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public List<StreetInputDto> Streets { get; set; } = new();
    public List<string> ImagePaths { get; set; } = new();
    public long CoordinatorId { get; set; }
}
