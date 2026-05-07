using CommunityHub.Application.Dto.Tenant;
using CommunityHub.Application.Services.Tenant;

namespace CommunityHub.Ui.ViewModels.Tenant;

public class HelpBoardViewModel
{
    private readonly AdvertisementService _service;
    private readonly long _tenantId;
    private List<AdvertisementDto> _activeAds = [];

    public List<TenantBuildingDto> Buildings { get; private set; } = [];
    public long? SelectedBuildingId { get; private set; }

    public HelpBoardViewModel(long tenantId)
    {
        _tenantId = tenantId;
        _service = new AdvertisementService();
        Buildings = _service.GetBuildingsForTenant(tenantId);
        if (Buildings.Count > 0)
            SelectedBuildingId = Buildings[0].BuildingId;
    }

    public bool IsInBuilding() => Buildings.Count > 0;

    public void SelectBuilding(long buildingId) => SelectedBuildingId = buildingId;

    public void LoadAds()
    {
        if (SelectedBuildingId == null) { _activeAds = []; return; }
        _activeAds = _service.GetActiveBoardAds(_tenantId, SelectedBuildingId.Value);
    }

    public List<AdvertisementDto> GetAds() => _activeAds;

    public (bool Success, List<AdvertisementDto> MatchingAds) CreateAd(CreateAdvertisementDto dto)
        => _service.CreateAdvertisement(dto);

    public void ArchiveAd(long adId) => _service.ArchiveAd(adId);
}
