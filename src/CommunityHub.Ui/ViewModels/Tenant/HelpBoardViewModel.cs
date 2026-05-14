using System.Collections.ObjectModel;
using CommunityHub.Application;
using CommunityHub.Application.Services.Tenant;
using CommunityHub.Application.Services.Tenant.Dto;

namespace CommunityHub.Ui.ViewModels.Tenant;

public class HelpBoardViewModel : BaseViewModel
{
    private readonly AdvertisementService _service;
    public long TenantId { get; }
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
        TenantId = tenantId;
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
        foreach (var ad in _service.GetActiveBoardAds(TenantId, SelectedBuildingId.Value))
            Ads.Add(ad);
    }

    public (bool Success, long NewAdId, List<AdvertisementDto> MatchingAds) CreateAd(CreateAdvertisementDto dto)
        => _service.CreateAdvertisement(dto);

    public void ArchiveAd(long adId) => _service.ArchiveAd(adId);

    public List<SlotDto> GetAvailableSlots(long adId) => _service.GetAvailableSlots(adId);

    public void ReserveSlot(long slotId, long requesterAdId)
        => _service.ReserveSlot(slotId, requesterAdId, TenantId);
}
