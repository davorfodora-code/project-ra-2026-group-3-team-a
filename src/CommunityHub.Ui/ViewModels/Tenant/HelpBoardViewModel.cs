using System.Collections.ObjectModel;
using CommunityHub.Application;
using CommunityHub.Application.Services.Tenant.Dto;
using CommunityHub.Application.Services.Tenant;

namespace CommunityHub.Ui.ViewModels.Tenant;

public class HelpBoardViewModel : BaseViewModel
{
    private readonly AdvertisementService _service;
    private readonly long _tenantId;
    private long? _selectedBuildingId;

    public List<TenantBuildingDto> Buildings { get; private set; } = [];
    public ObservableCollection<AdvertisementDto> Ads { get; } = new();

    public long? SelectedBuildingId
    {
        get => _selectedBuildingId;
        private set => SetProperty(ref _selectedBuildingId, value);
    }

    public HelpBoardViewModel(long tenantId)
    {
        _tenantId = tenantId;
        _service = new AdvertisementService(Injector.Get<IAdvertisementRepository>());
        Buildings = _service.GetBuildingsForTenant(tenantId);
        if (Buildings.Count > 0)
            SelectedBuildingId = Buildings[0].BuildingId;
    }

    public bool IsInBuilding() => Buildings.Count > 0;

    public void SelectBuilding(long buildingId) => SelectedBuildingId = buildingId;

    public void LoadAds()
    {
        Ads.Clear();
        if (SelectedBuildingId == null) return;
        foreach (var ad in _service.GetActiveBoardAds(_tenantId, SelectedBuildingId.Value))
            Ads.Add(ad);
    }

    public (bool Success, List<AdvertisementDto> MatchingAds) CreateAd(CreateAdvertisementDto dto)
        => _service.CreateAdvertisement(dto);

    public void ArchiveAd(long adId) => _service.ArchiveAd(adId);
}
