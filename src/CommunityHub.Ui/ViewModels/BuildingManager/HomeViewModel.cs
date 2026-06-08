using CommunityHub.Application;
using CommunityHub.Application.Services.BuildingManager;
using CommunityHub.Application.Services.BuildingManager.Dto;

namespace CommunityHub.Ui.ViewModels.BuildingManager;

public class HomeViewModel : BaseViewModel
{
    private readonly BuildingAccessRequestService _accessRequestService;
    private readonly CommonRoomRequestService     _rentalService;
    private readonly IssueReportService           _issueService;
    private readonly BulletinBoardStatsService    _bulletinService;
    private readonly AssemblyService              _assemblyService;
    private readonly long _managerId;

    private int _pendingAccessRequestsCount;
    private int _rentalRequestsCount;
    private int _unresolvedProblemsCount;
    private int _activeAdsCount;
    private string _bannerBuildingName = "Nema zakazanih skupština";
    private string _bannerDate         = string.Empty;
    private long?  _nextAssemblyId;

    public HomeViewModel(long managerId)
    {
        _managerId            = managerId;
        _accessRequestService = new BuildingAccessRequestService(Injector.Get<IBuildingAccessRequestRepository>());
        _rentalService        = new CommonRoomRequestService(Injector.Get<ICommonRoomRequestRepository>());
        _issueService         = new IssueReportService(Injector.Get<IIssueReportRepository>());
        _bulletinService      = new BulletinBoardStatsService();
        _assemblyService      = new AssemblyService(
            Injector.Get<IAssemblyRepository>(),
            Injector.Get<IBuildingRepository>());
        LoadCounters();
        LoadBanner();
    }

    public string BannerBuildingName
    {
        get => _bannerBuildingName;
        private set => SetProperty(ref _bannerBuildingName, value);
    }
    public string BannerDate
    {
        get => _bannerDate;
        private set => SetProperty(ref _bannerDate, value);
    }

    // ID prve naredne skupstine — null ako nema (sluzi za navigaciju sa banera)
    public long? NextAssemblyId => _nextAssemblyId;

    public int PendingAccessRequestsCount
    {
        get => _pendingAccessRequestsCount;
        private set => SetProperty(ref _pendingAccessRequestsCount, value);
    }
    public int RentalRequestsCount
    {
        get => _rentalRequestsCount;
        private set => SetProperty(ref _rentalRequestsCount, value);
    }
    public int UnresolvedProblemsCount
    {
        get => _unresolvedProblemsCount;
        private set => SetProperty(ref _unresolvedProblemsCount, value);
    }
    public int ActiveAdsCount
    {
        get => _activeAdsCount;
        private set => SetProperty(ref _activeAdsCount, value);
    }

    // Učitava brojače preko svih zgrada kojima upravnik upravlja
    public void LoadCounters()
    {
        PendingAccessRequestsCount = CountSafe(() =>
            _accessRequestService.GetByManagerId(_managerId).Count(r => r.IsPending));

        RentalRequestsCount = CountSafe(() =>
            _rentalService.GetByManagerId(_managerId).Count(r => r.Status == "Pending"));

        UnresolvedProblemsCount = CountSafe(() =>
            _issueService.GetByManagerId(_managerId).Count(i => i.Status == "Unresolved"));

        ActiveAdsCount = CountSafe(SumActiveAdsAcrossBuildings);
    }

    // Sabira aktivne oglase ('Aktivan') preko svih zgrada upravnika
    private int SumActiveAdsAcrossBuildings()
    {
        var buildings = _bulletinService.GetManagedBuildings(_managerId);
        int total = 0;
        foreach (var b in buildings)
            total += _bulletinService.GetAllAds(b.BuildingId).Count(a => a.Status == "Aktivan");
        return total;
    }

    // Banner — prva naredna zakazana ili potvrđena skupština; ako je nema, fallback poruka
    public void LoadBanner()
    {
        AssemblySummaryDto? next;
        try
        {
            next = _assemblyService.GetByManagerId(_managerId)
                .Where(a => a.ScheduledAt > DateTime.Now && a.Status != "Cancelled")
                .OrderBy(a => a.ScheduledAt)
                .FirstOrDefault();
        }
        catch { next = null; }

        if (next == null)
        {
            BannerBuildingName = "Nema zakazanih skupština";
            BannerDate         = string.Empty;
            _nextAssemblyId    = null;
            return;
        }
        BannerBuildingName = next.BuildingAddress;
        BannerDate         = next.ScheduledAt.ToString("dd. MM. yyyy. - HH:mm");
        _nextAssemblyId    = next.Id;
    }

    private static int CountSafe(Func<int> fetch)
    {
        try { return fetch(); }
        catch { return 0; }
    }
}
