using CommunityHub.Application.Dto.Citizen;
using CommunityHub.Application.Domain;

namespace CommunityHub.Ui.Views.Citizen;

public class DistrictRowViewModel
{
    public long DistrictId { get; }
    public string Name { get; }
    public string Description { get; }
    public string LocationDisplay { get; }
    public string StreetsDisplay { get; }
    public int ImageCount { get; }
    public List<string> ImageUrls { get; }

    public DistrictRowViewModel(DistrictListItemDto district)
    {
        DistrictId = district.DistrictId;
        Name = district.Name;
        Description = district.Description;
        LocationDisplay = district.LocationDisplay;
        StreetsDisplay = district.StreetsDisplay;
        ImageUrls = district.ImageUrls;
        ImageCount = district.ImageCount;
    }

    public DistrictRowViewModel(District district)
    {
        DistrictId = district.Id;
        Name = district.Name;
        Description = district.Description;
        LocationDisplay = $"{district.Location.City}, {district.Location.Country}";

        StreetsDisplay = district.Streets.Count == 0
            ? "-"
            : string.Join(", ", district.Streets.Select(s => $"{s.StreetName}: {s.StartNumber}-{s.EndNumber}"));

        ImageUrls = district.Images.Select(i => i.Url).ToList();
        ImageCount = ImageUrls.Count;
    }
}
