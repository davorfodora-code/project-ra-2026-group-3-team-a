using System.Collections.ObjectModel;
using CommunityHub.Application;
using CommunityHub.Application.Services.BuildingManager;
using CommunityHub.Application.Services.BuildingManager.Dto;

namespace CommunityHub.Ui.ViewModels.BuildingManager;

// Opcija za filter dropdown — null BuildingId znaci "sve zgrade"
public record BuildingFilterItem(long? BuildingId, string Label);

public class IssueReportsViewModel : BaseViewModel
{
    private readonly IssueReportService           _issueService;
    private readonly BuildingAccessRequestService _buildingService;
    private readonly long _managerId;

    private List<IssueReportDto>  _allIssues = new();
    private BuildingFilterItem?   _selectedFilter;

    public IssueReportsViewModel(long managerId)
    {
        _managerId       = managerId;
        _issueService    = new IssueReportService(Injector.Get<IIssueReportRepository>());
        _buildingService = new BuildingAccessRequestService(Injector.Get<IBuildingAccessRequestRepository>());

        UrgentIssues    = new ObservableCollection<IssueReportItemViewModel>();
        SolveSoonIssues = new ObservableCollection<IssueReportItemViewModel>();
        CanWaitIssues   = new ObservableCollection<IssueReportItemViewModel>();
        BuildingFilters = new ObservableCollection<BuildingFilterItem>();

        LoadFilters();
        Load();
    }

    public ObservableCollection<BuildingFilterItem>        BuildingFilters { get; }
    public ObservableCollection<IssueReportItemViewModel>  UrgentIssues    { get; }
    public ObservableCollection<IssueReportItemViewModel>  SolveSoonIssues { get; }
    public ObservableCollection<IssueReportItemViewModel>  CanWaitIssues   { get; }

    public bool HasUrgentIssues    => UrgentIssues.Count > 0;
    public bool HasSolveSoonIssues => SolveSoonIssues.Count > 0;
    public bool HasCanWaitIssues   => CanWaitIssues.Count > 0;
    public bool HasNoIssues        => UrgentIssues.Count == 0 && SolveSoonIssues.Count == 0 && CanWaitIssues.Count == 0;

    public BuildingFilterItem? SelectedFilter
    {
        get => _selectedFilter;
        set
        {
            if (SetProperty(ref _selectedFilter, value))
                ApplyFilter();
        }
    }

    public void Load()
    {
        try { _allIssues = _issueService.GetByManagerId(_managerId); }
        catch { _allIssues = new List<IssueReportDto>(); }
        ApplyFilter();
    }

    public void MarkIssueAsResolved(long reportId)
    {
        try { _issueService.MarkAsResolved(reportId); }
        catch { /* ignorisi */ }
        Load();
    }

    private void LoadFilters()
    {
        BuildingFilters.Clear();
        BuildingFilters.Add(new BuildingFilterItem(null, "Sve zgrade"));
        foreach (var b in _buildingService.GetBuildingsByManagerId(_managerId))
            BuildingFilters.Add(new BuildingFilterItem(b.Id, b.AddressDisplay));
        _selectedFilter = BuildingFilters[0];
    }

    private void ApplyFilter()
    {
        IEnumerable<IssueReportDto> filtered = _allIssues;
        if (_selectedFilter?.BuildingId != null)
            filtered = filtered.Where(i => i.BuildingId == _selectedFilter.BuildingId);

        IssueGrouping.RefillByPriority(filtered, UrgentIssues, SolveSoonIssues, CanWaitIssues);
        OnPropertyChanged(nameof(HasUrgentIssues));
        OnPropertyChanged(nameof(HasSolveSoonIssues));
        OnPropertyChanged(nameof(HasCanWaitIssues));
        OnPropertyChanged(nameof(HasNoIssues));
    }
}
