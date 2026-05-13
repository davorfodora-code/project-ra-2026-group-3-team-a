using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Services.BuildingManager;

public interface IBuildingRepository
{
    Building Create(Building building);
    Building? GetById(long buildingId);
    List<Building> Search(string? address, string? neighborhood, string? city, string? country);
}
