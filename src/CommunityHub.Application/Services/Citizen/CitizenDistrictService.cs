using CommunityHub.Application.Database.Repositories.Citizen;
using CommunityHub.Application.Domain;
using CommunityHub.Application.Dto.Citizen;

namespace CommunityHub.Application.Services.Citizen;

public class CitizenDistrictService
{
    private readonly CitizenDistrictDbRepository _districtRepository;

    public CitizenDistrictService()
    {
        _districtRepository = new CitizenDistrictDbRepository();
    }

    public List<DistrictListItemDto> GetAll()
    {
        return _districtRepository.GetAll()
            .Select(MapToDto)
            .ToList();
    }

    public List<DistrictListItemDto> Search(DistrictSearchFilterDto filter)
    {
        DistrictSearchFilter repositoryFilter = new DistrictSearchFilter(
            filter.Name,
            filter.Address,
            filter.City,
            filter.Country);

        return _districtRepository.Search(repositoryFilter)
            .Select(MapToDto)
            .ToList();
    }

    private static DistrictListItemDto MapToDto(District district)
    {
        return new DistrictListItemDto
        {
            DistrictId = district.Id,
            Name = district.Name,
            Description = district.Description,
            LocationDisplay = $"{district.Location.City}, {district.Location.Country}",
            StreetsDisplay = district.Streets.Count == 0
                ? "-"
                : string.Join(", ", district.Streets.Select(s => $"{s.StreetName}: {s.StartNumber}-{s.EndNumber}")),
            ImageUrls = district.Images.Select(i => i.Url).ToList()
        };
    }
}
