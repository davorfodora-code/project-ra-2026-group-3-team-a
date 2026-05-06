using CommunityHub.Application.Database.Repositories.BuildingManager;
using CommunityHub.Application.Domain;
using CommunityHub.Application.Dto.BuildingManager;

namespace CommunityHub.Application.Services.Tenant;

public class BuildingSearchService
{
    private readonly BuildingDbRepository _repository;

    public BuildingSearchService()
    {
        _repository = new BuildingDbRepository();
    }

    public List<BuildingDto> Search(string? address, string? neighborhood, string? city, string? country)
    {
        return _repository.Search(address, neighborhood, city, country)
            .Select(MapToDto)
            .ToList();
    }

    private BuildingDto MapToDto(Building b) => new BuildingDto
    {
        Id = b.Id,
        Street = b.Address.Street,
        StreetNumber = b.Address.StreetNumber,
        Neighborhood = b.Neighborhood,
        City = b.Location.City,
        Country = b.Location.Country,
        NumberOfFloors = b.NumberOfFloors,
        TotalApartments = b.TotalApartments,
        ImagePaths = b.Images.Select(i => i.ImagePath).ToList()
    };
}
