using System.Collections.ObjectModel;
using CommunityHub.Application.Services.BuildingManager;
using CommunityHub.Application.Services.BuildingManager.Dto;

namespace CommunityHub.Ui.ViewModels.BuildingManager;

public class BulletinBoardPageViewModel : BaseViewModel
{
    private readonly BulletinBoardStatsService _service;
    private List<BulletinBoardAdDto> _allAds = new();
    private BulletinBoardStatsDto?   _stats;

    public BulletinBoardPageViewModel(long managerId)
    {
        _service = new BulletinBoardStatsService();
        var buildings = _service.GetManagedBuildings(managerId);
        Buildings   = buildings.Select(b => new BuildingOption(b.BuildingId, b.Address)).ToList();
        Periods     = PeriodOption.GenerateList();
        FilteredAds = new ObservableCollection<BulletinBoardAdDto>();
        if (Buildings.Count == 0) return;
        _selectedBuildingIndex = 0;
        _selectedPeriodIndex   = Periods.FindIndex(p =>
            p.Month == DateTime.Today.Month && p.Year == DateTime.Today.Year);
        if (_selectedPeriodIndex < 0) _selectedPeriodIndex = 0;
        LoadAds();
        LoadStats();
    }

    public List<BuildingOption> Buildings { get; }

    private int _selectedBuildingIndex;
    public int SelectedBuildingIndex
    {
        get => _selectedBuildingIndex;
        set
        {
            if (SetProperty(ref _selectedBuildingIndex, value))
            {
                LoadAds();
                LoadStats();
            }
        }
    }

    private int _activeTabIndex;
    public int ActiveTabIndex
    {
        get => _activeTabIndex;
        set
        {
            if (SetProperty(ref _activeTabIndex, value))
            {
                OnPropertyChanged(nameof(IsAdsTabActive));
                OnPropertyChanged(nameof(IsStatsTabActive));
            }
        }
    }
    public bool IsAdsTabActive   => _activeTabIndex == 0;
    public bool IsStatsTabActive => _activeTabIndex == 1;

    public ObservableCollection<BulletinBoardAdDto> FilteredAds { get; }
    public bool HasAds   => FilteredAds.Count > 0;
    public bool HasNoAds => FilteredAds.Count == 0;

    private string _statusFilter = "Svi";
    public string StatusFilter
    {
        get => _statusFilter;
        set { if (SetProperty(ref _statusFilter, value)) ApplyFilter(); }
    }

    public List<PeriodOption> Periods { get; }

    private int _selectedPeriodIndex;
    public int SelectedPeriodIndex
    {
        get => _selectedPeriodIndex;
        set { if (SetProperty(ref _selectedPeriodIndex, value)) LoadStats(); }
    }

    public int PeriodOfferingCount => _stats?.PeriodOfferingCount ?? 0;
    public int PeriodSeekingCount  => _stats?.PeriodSeekingCount  ?? 0;
    public List<CategoryStatsDto> CategoryStats    => _stats?.CategoryStats ?? new();
    public bool                   HasCategoryStats => CategoryStats.Count > 0;
    public int  CurrentActiveCount   => _stats?.CurrentActiveCount   ?? 0;
    public int  CurrentArchivedCount => _stats?.CurrentArchivedCount ?? 0;
    public IEnumerable<string> ActiveByCategoryLines =>
        _stats?.ActiveByCategory.Select(x => $"{x.Category}: {x.Count}")
        ?? Enumerable.Empty<string>();
    public bool HasActiveByCategory => _stats?.ActiveByCategory.Count > 0;
    public string TopHelperText => _stats?.TopHelperName is string name
        ? $"{name} ({_stats.TopHelperSlots} termina)"
        : "Nema podataka";

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

    private void LoadStats()
    {
        if (Buildings.Count == 0 || Periods.Count == 0) return;
        PeriodOption period = Periods[_selectedPeriodIndex];
        _stats = _service.GetStats(CurrentBuildingId, period.Year, period.Month);
        NotifyStatsChanged();
    }

    private void NotifyStatsChanged()
    {
        OnPropertyChanged(nameof(PeriodOfferingCount));
        OnPropertyChanged(nameof(PeriodSeekingCount));
        OnPropertyChanged(nameof(CategoryStats));
        OnPropertyChanged(nameof(HasCategoryStats));
        OnPropertyChanged(nameof(CurrentActiveCount));
        OnPropertyChanged(nameof(CurrentArchivedCount));
        OnPropertyChanged(nameof(ActiveByCategoryLines));
        OnPropertyChanged(nameof(HasActiveByCategory));
        OnPropertyChanged(nameof(TopHelperText));
    }
}
