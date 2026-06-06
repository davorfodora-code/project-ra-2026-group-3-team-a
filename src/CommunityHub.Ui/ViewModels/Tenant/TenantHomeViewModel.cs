using CommunityHub.Application;
using CommunityHub.Application.Services.Tenant;

namespace CommunityHub.Ui.ViewModels.Tenant;

public class TenantHomeViewModel : BaseViewModel
{
    private readonly BuildingAccessRequestService _requestService;
    private readonly AdvertisementService _adService;
    private readonly TenantIssueService _issueService;
    private readonly long _tenantId;

    private int _pendingRequestsCount;
    private int _activeAdsCount;
    private int _unresolvedIssuesCount;

    public int PendingRequestsCount
    {
        get => _pendingRequestsCount;
        private set => SetProperty(ref _pendingRequestsCount, value);
    }

    public int ActiveAdsCount
    {
        get => _activeAdsCount;
        private set => SetProperty(ref _activeAdsCount, value);
    }

    public int UnresolvedIssuesCount
    {
        get => _unresolvedIssuesCount;
        private set => SetProperty(ref _unresolvedIssuesCount, value);
    }

    public TenantHomeViewModel(long tenantId)
    {
        _tenantId = tenantId;
        _requestService = new BuildingAccessRequestService(
            Injector.Get<IBuildingAccessRequestRepository>(),
            Injector.Get<IBuildingMembershipRepository>());
        _adService = new AdvertisementService(Injector.Get<IAdvertisementRepository>());
        _issueService = new TenantIssueService(
            Injector.Get<ITenantIssueRepository>(),
            Injector.Get<IAdvertisementRepository>());
    }

    public void LoadStats()
    {
        PendingRequestsCount = _requestService.GetAllByTenant(_tenantId)
            .Count(r => r.Status == "Pending");

        var buildings = _adService.GetBuildingsForTenant(_tenantId);
        ActiveAdsCount = buildings.Sum(b => _adService.GetActiveBoardAds(_tenantId, b.BuildingId).Count);

        UnresolvedIssuesCount = _issueService.GetByTenantId(_tenantId)
            .Count(i => i.Status == "Unresolved");
    }
}
