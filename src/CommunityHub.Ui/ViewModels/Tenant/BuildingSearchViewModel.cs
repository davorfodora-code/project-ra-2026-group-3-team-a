using CommunityHub.Application.Dto.BuildingManager;
using CommunityHub.Application.Services.Tenant;

namespace CommunityHub.Ui.ViewModels.Tenant;

public class BuildingSearchViewModel
{
    private readonly BuildingSearchService _service;

    public BuildingSearchViewModel()
    {
        _service = new BuildingSearchService();
    }

    public List<BuildingDto> LoadAll()
    {
        return _service.Search(null, null, null, null);
    }

    public List<BuildingDto> Search(string? address, string? neighborhood, string? city, string? country)
    {
        return _service.Search(address, neighborhood, city, country);
    }
}
