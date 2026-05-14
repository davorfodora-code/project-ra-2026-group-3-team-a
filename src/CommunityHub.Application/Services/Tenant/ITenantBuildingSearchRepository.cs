using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Services.Tenant;

public interface ITenantBuildingSearchRepository
{
    List<Building> Search(string? address, string? neighborhood, string? city, string? country);
}
