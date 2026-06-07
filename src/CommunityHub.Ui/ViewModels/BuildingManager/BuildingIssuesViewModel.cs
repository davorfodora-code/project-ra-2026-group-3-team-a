using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityHub.Application;
using CommunityHub.Application.Services.BuildingManager;
using CommunityHub.Application.Services.BuildingManager.Dto;

namespace CommunityHub.Ui.ViewModels.BuildingManager;

// Prijave problema vezane za jednu zgradu — koristi se unutar BuildingDetailViewModel-a
public class BuildingIssuesViewModel : BaseViewModel
{
    private readonly IssueReportService _service;
    private readonly long _buildingId;

    public BuildingIssuesViewModel(long buildingId)
    {
        _buildingId     = buildingId;
        _service        = new IssueReportService(Injector.Get<IIssueReportRepository>());
        UrgentIssues    = new ObservableCollection<IssueReportItemViewModel>();
        SolveSoonIssues = new ObservableCollection<IssueReportItemViewModel>();
        CanWaitIssues   = new ObservableCollection<IssueReportItemViewModel>();
        Load();
    }

    public ObservableCollection<IssueReportItemViewModel> UrgentIssues    { get; }
    public ObservableCollection<IssueReportItemViewModel> SolveSoonIssues { get; }
    public ObservableCollection<IssueReportItemViewModel> CanWaitIssues   { get; }

    public bool HasUrgentIssues    => UrgentIssues.Count    > 0;
    public bool HasSolveSoonIssues => SolveSoonIssues.Count > 0;
    public bool HasCanWaitIssues   => CanWaitIssues.Count   > 0;
    public bool HasNoIssues        => !HasUrgentIssues && !HasSolveSoonIssues && !HasCanWaitIssues;

    public void Load()
    {
        List<IssueReportDto> list;
        try { list = _service.GetByBuildingId(_buildingId); }
        catch { list = new List<IssueReportDto>(); }

        IssueGrouping.RefillByPriority(list, UrgentIssues, SolveSoonIssues, CanWaitIssues);
        OnPropertyChanged(nameof(HasUrgentIssues));
        OnPropertyChanged(nameof(HasSolveSoonIssues));
        OnPropertyChanged(nameof(HasCanWaitIssues));
        OnPropertyChanged(nameof(HasNoIssues));
    }

    public void MarkAsResolved(long reportId)
    {
        try { _service.MarkAsResolved(reportId); }
        catch { /* ignorisi */ }
        Load();
    }
}
