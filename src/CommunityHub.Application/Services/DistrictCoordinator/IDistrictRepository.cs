using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Services.DistrictCoordinator;

public interface IDistrictRepository
{
    District Create(District district);
    List<District> GetAll();
    List<District> GetByCoordinatorId(long coordinatorId);
    List<District> Search(string? name, string? address, string? city, string? country);
}
