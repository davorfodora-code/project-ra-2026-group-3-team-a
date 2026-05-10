using System.Collections.Generic;
using System.Linq;
using CommunityHub.Application.Database.Repositories.DistrictCoordinator;
using CommunityHub.Application.Domain;
using CommunityHub.Application.Dto.DistrictCoordinator;

namespace CommunityHub.Application.Services.DistrictCoordinator;

public class DistrictService
{
    private readonly DistrictDbRepository _repository;

    public DistrictService()
    {
        _repository = new DistrictDbRepository();
    }

    public DistrictDto Create(CreateDistrictDto dto)
    {
        Location location = new Location(dto.City, dto.Country);
        User coordinator = new User(dto.CoordinatorId, "", "", "", "", System.DateTime.MinValue, "DistrictCoordinator");

        District district = new District(dto.Name, dto.Description, location, 0, coordinator);

        foreach (StreetInputDto streetDto in dto.Streets)
            district.AddStreet(new DistrictStreet(streetDto.StreetName, streetDto.StartNumber, streetDto.EndNumber));

        foreach (string imagePath in dto.ImagePaths)
            district.AddImage(new Image(imagePath));

        District created = _repository.Create(district);
        return MapToDto(created);
    }

    public List<DistrictDto> GetByCoordinatorId(long coordinatorId)
    {
        return _repository.GetByCoordinatorId(coordinatorId)
            .Select(MapToDto)
            .ToList();
    }

    private static DistrictDto MapToDto(District d) => new DistrictDto
    {
        Id          = d.Id,
        Name        = d.Name,
        Description = d.Description,
        City        = d.Location.City,
        Country     = d.Location.Country,
        Budget      = d.Budget
    };
}
