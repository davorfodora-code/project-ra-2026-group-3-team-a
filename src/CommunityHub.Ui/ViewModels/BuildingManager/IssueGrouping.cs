using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityHub.Application.Services.BuildingManager.Dto;

namespace CommunityHub.Ui.ViewModels.BuildingManager;

// Razvrstava prijave problema u tri prioritetne kolekcije (Urgent / SolveSoon / CanWait)
internal static class IssueGrouping
{
    public static void RefillByPriority(
        IEnumerable<IssueReportDto> issues,
        ObservableCollection<IssueReportItemViewModel> urgent,
        ObservableCollection<IssueReportItemViewModel> solveSoon,
        ObservableCollection<IssueReportItemViewModel> canWait)
    {
        urgent.Clear();
        solveSoon.Clear();
        canWait.Clear();
        foreach (IssueReportDto dto in issues)
        {
            IssueReportItemViewModel item = new IssueReportItemViewModel(dto);
            if (dto.Priority == "Urgent")         urgent.Add(item);
            else if (dto.Priority == "SolveSoon") solveSoon.Add(item);
            else                                  canWait.Add(item);
        }
    }
}
