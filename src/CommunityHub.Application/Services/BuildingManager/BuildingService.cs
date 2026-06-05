using CommunityHub.Application.Domain;
using CommunityHub.Application.Services.BuildingManager.Dto;

namespace CommunityHub.Application.Services.BuildingManager;

public class BuildingService
{
    private readonly IBuildingRepository _repository;

    public BuildingService(IBuildingRepository repository)
    {
        _repository = repository;
    }

    public BuildingDto Create(CreateBuildingDto dto)
    {
        Address address = new Address(dto.Street, dto.StreetNumber);
        Location location = new Location(dto.City, dto.Country);
        Building building = new Building(address, dto.Neighborhood, location, dto.NumberOfFloors, dto.ManagerId);

        foreach (FloorInputDto floorDto in dto.Floors)
        {
            Floor floor = new Floor(floorDto.FloorNumber);
            foreach (int apartmentNumber in floorDto.ApartmentNumbers)
                floor.AddApartment(new Apartment(apartmentNumber));
            building.AddFloor(floor);
        }

        foreach (string imagePath in dto.ImagePaths)
            building.AddImage(new BuildingImage(imagePath));

        Building created = _repository.Create(building);
        return MapToDto(created);
    }

    // Vraća punu zgradu sa spratovima, stanovima i slikama
    public BuildingDto? GetById(long buildingId)
    {
        Building? building = _repository.GetById(buildingId);
        if (building == null) return null;

        BuildingDto dto = MapToDto(building);

        foreach (Floor floor in building.Floors)
        {
            dto.Floors.Add(new FloorSummaryDto
            {
                FloorNumber = floor.FloorNumber,
                ApartmentNumbers = floor.Apartments
                    .Select(a => a.ApartmentNumber)
                    .ToList()
            });
        }

        return dto;
    }

    public List<BuildingDto> Search(string? address, string? neighborhood, string? city, string? country)
    {
        return _repository.Search(address, neighborhood, city, country)
            .Select(MapToDto)
            .ToList();
    }

    public List<BuildingDto> GetByManagerId(long managerId)
    {
        return _repository.GetByManagerId(managerId)
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
