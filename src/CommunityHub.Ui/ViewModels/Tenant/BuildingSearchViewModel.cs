using System.Collections.ObjectModel;
using CommunityHub.Application;
using CommunityHub.Application.Services.BuildingManager.Dto;
using CommunityHub.Application.Services.Tenant;

namespace CommunityHub.Ui.ViewModels.Tenant;

public class BuildingSearchViewModel : BaseViewModel
{
    private readonly BuildingSearchService _service;

    public ObservableCollection<BuildingDto> Buildings { get; } = new();

    public BuildingSearchViewModel()
    {
        _service = new BuildingSearchService(Injector.Get<ITenantBuildingSearchRepository>());
    }

    public void LoadAll() => ApplySearch(null, null, null, null);

    public void Search(string? address, string? neighborhood, string? city, string? country)
        => ApplySearch(address, neighborhood, city, country);

    private void ApplySearch(string? address, string? neighborhood, string? city, string? country)
    {
        var results = _service.Search(address, neighborhood, city, country);
        Buildings.Clear();
        foreach (var b in results)
            Buildings.Add(b);
    }
}
