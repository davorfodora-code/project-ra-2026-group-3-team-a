using System.Collections.Generic;
using System.Linq;
using CommunityHub.Application.Services.BuildingManager;
using CommunityHub.Application.Services.BuildingManager.Dto;

namespace CommunityHub.Ui.ViewModels.BuildingManager;

// Statistika oglasne table za izabranu zgradu i period — koristi se kao pod-VM
// unutar BulletinBoardPageViewModel-a (tab "Statistika").
public class BulletinBoardStatsViewModel : BaseViewModel
{
    private readonly BulletinBoardStatsService _service;
    private BulletinBoardStatsDto? _stats;

    public BulletinBoardStatsViewModel(BulletinBoardStatsService service)
    {
        _service = service;
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

    public void Reload(long buildingId, int year, int? month)
    {
        _stats = _service.GetStats(buildingId, year, month);
        NotifyAllChanged();
    }

    private void NotifyAllChanged()
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
