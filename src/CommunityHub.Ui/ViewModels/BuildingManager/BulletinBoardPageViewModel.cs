using System.Collections.ObjectModel;
using CommunityHub.Application.Services.BuildingManager;
using CommunityHub.Application.Services.BuildingManager.Dto;

namespace CommunityHub.Ui.ViewModels.BuildingManager;

public class BulletinBoardPageViewModel : BaseViewModel
{
    private readonly BulletinBoardStatsService _service;
    private List<BulletinBoardAdDto> _allAds = new();

    public BulletinBoardPageViewModel(long managerId)
    {
        _service = new BulletinBoardStatsService();
        var buildings = _service.GetManagedBuildings(managerId);
        Buildings   = buildings.Select(b => new BuildingOption(b.BuildingId, b.Address)).ToList();
        Periods     = PeriodOption.GenerateList();
        FilteredAds = new ObservableCollection<BulletinBoardAdDto>();
        Stats       = new BulletinBoardStatsViewModel(_service);
        if (Buildings.Count == 0) return;
        _selectedBuildingIndex = 0;
        _selectedPeriodIndex   = Periods.FindIndex(p =>
            p.Month == DateTime.Today.Month && p.Year == DateTime.Today.Year);
        if (_selectedPeriodIndex < 0) _selectedPeriodIndex = 0;
        LoadAds();
        ReloadStats();
    }

    public List<BuildingOption>             Buildings { get; }
    public List<PeriodOption>               Periods   { get; }
    public ObservableCollection<BulletinBoardAdDto> FilteredAds { get; }
    public BulletinBoardStatsViewModel      Stats     { get; }

    private int _selectedBuildingIndex;
    public int SelectedBuildingIndex
    {
        get => _selectedBuildingIndex;
        set
        {
            if (!SetProperty(ref _selectedBuildingIndex, value)) return;
            LoadAds();
            ReloadStats();
        }
    }

    private int _selectedPeriodIndex;
    public int SelectedPeriodIndex
    {
        get => _selectedPeriodIndex;
        set { if (SetProperty(ref _selectedPeriodIndex, value)) ReloadStats(); }
    }

    private int _activeTabIndex;
    public int ActiveTabIndex
    {
        get => _activeTabIndex;
        set
        {
            if (!SetProperty(ref _activeTabIndex, value)) return;
            OnPropertyChanged(nameof(IsAdsTabActive));
            OnPropertyChanged(nameof(IsStatsTabActive));
        }
    }
    public bool IsAdsTabActive   => _activeTabIndex == 0;
    public bool IsStatsTabActive => _activeTabIndex == 1;

    public bool HasAds   => FilteredAds.Count > 0;
    public bool HasNoAds => FilteredAds.Count == 0;

    private string _statusFilter = "Svi";
    public string StatusFilter
    {
        get => _statusFilter;
        set { if (SetProperty(ref _statusFilter, value)) ApplyFilter(); }
    }

    private long CurrentBuildingId => Buildings[_selectedBuildingIndex].BuildingId;

    private void LoadAds()
    {
        _allAds = _service.GetAllAds(CurrentBuildingId);
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        FilteredAds.Clear();
        foreach (BulletinBoardAdDto ad in _allAds)
        {
            if (_statusFilter == "Svi" || ad.Status == _statusFilter)
                FilteredAds.Add(ad);
        }
        OnPropertyChanged(nameof(HasAds));
        OnPropertyChanged(nameof(HasNoAds));
    }

    private void ReloadStats()
    {
        if (Buildings.Count == 0 || Periods.Count == 0) return;
        PeriodOption period = Periods[_selectedPeriodIndex];
        Stats.Reload(CurrentBuildingId, period.Year, period.Month);
    }
}
