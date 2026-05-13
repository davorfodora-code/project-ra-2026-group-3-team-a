using CommunityHub.Application;
using CommunityHub.Application.Services.BuildingManager;

namespace CommunityHub.Ui.ViewModels.BuildingManager;

public class HomeViewModel : BaseViewModel
{
    private readonly BuildingAccessRequestService _accessRequestService;
    private readonly long _managerId;

    private int _pendingAccessRequestsCount;

    public HomeViewModel(long managerId)
    {
        _managerId = managerId;
        _accessRequestService = new BuildingAccessRequestService(Injector.Get<IBuildingAccessRequestRepository>());
        LoadCounters();
    }

    // Baner skupstine — primer vrednosti dok se ne implementiraju skupštine. 
    public string BannerBuildingName => "Zgrada u Dunavskoj 4";
    public string BannerDate => "20. 04. 2026. - 19:00";

    public int PendingAccessRequestsCount
    {
        get => _pendingAccessRequestsCount;
        private set => SetProperty(ref _pendingAccessRequestsCount, value);
    }

    // Placeholder vrednosti — kad odgovarajuci servisi budu implementirani, racunaj iz njih
    public int RentalRequestsCount => 0;
    public int UnresolvedProblemsCount => 0;
    public int ActiveAdsCount => 0;

    public void LoadCounters()
    {
        try
        {
            PendingAccessRequestsCount = _accessRequestService
                .GetByManagerId(_managerId)
                .Count(r => r.IsPending);
        }
        catch
        {
            PendingAccessRequestsCount = 0;
        }
    }
}
