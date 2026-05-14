using CommunityHub.Application.Domain;
using CommunityHub.Application.Services.BuildingManager.Dto;

namespace CommunityHub.Application.Services.BuildingManager;

public class BuildingAccessRequestService
{
    private readonly IBuildingAccessRequestRepository _repository;

    public BuildingAccessRequestService(IBuildingAccessRequestRepository repository)
    {
        _repository = repository;
    }

    public List<BuildingAccessRequestDto> GetByManagerId(long managerId)
    {
        return _repository.GetByManagerId(managerId)
            .Select(MapRequestToDto)
            .ToList();
    }

    public void Approve(long requestId)
    {
        _repository.Approve(requestId);
    }

    public void Reject(long requestId, string? reason)
    {
        _repository.Reject(requestId, reason);
    }

    public List<BuildingDto> GetBuildingsByManagerId(long managerId)
    {
        return _repository.GetBuildingsByManagerId(managerId)
            .Select(MapBuildingToDto)
            .ToList();
    }

    public List<BuildingMembershipDto> GetMembershipsByBuildingId(long buildingId)
    {
        return _repository.GetMembershipsByBuildingId(buildingId)
            .Select(MapMembershipToDto)
            .ToList();
    }

    private BuildingAccessRequestDto MapRequestToDto(BuildingAccessRequest r) => new BuildingAccessRequestDto
    {
        RequestId = r.Id,
        TenantFullName = $"{r.Tenant.Name} {r.Tenant.Surname}",
        BuildingAddress = $"{r.Building.Address.Street} {r.Building.Address.StreetNumber}",
        Neighborhood = r.Building.Neighborhood,
        Location = $"{r.Building.Location.City}, {r.Building.Location.Country}",
        ApartmentNumber = r.ApartmentNumber,
        CreatedAt = r.CreatedAt,
        Status = r.Status
    };

    private BuildingDto MapBuildingToDto(Building b) => new BuildingDto
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

    private BuildingMembershipDto MapMembershipToDto(BuildingMembership m) => new BuildingMembershipDto
    {
        TenantFullName = $"{m.Tenant.Name} {m.Tenant.Surname}",
        FloorNumber = m.FloorNumber,
        ApartmentNumber = m.ApartmentNumber,
        JoinedAt = m.JoinedAt
    };
}
