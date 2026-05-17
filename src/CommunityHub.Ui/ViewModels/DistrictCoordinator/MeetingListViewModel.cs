using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityHub.Application.Services.DistrictCoordinator.Dto;
using CommunityHub.Ui.ViewModels.Common;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;

public class MeetingListViewModel : ViewModelBase
{
    private List<MeetingDto> _allMeetings = new();
    private string _activeFilter = "all";

    public MeetingListViewModel()
    {
        FilteredMeetings = new ObservableCollection<MeetingDto>();
        SetFilterCommand = new RelayCommand(p => ActiveFilter = p?.ToString() ?? "all");
    }

    public ObservableCollection<MeetingDto> FilteredMeetings { get; }

    public string ActiveFilter
    {
        get => _activeFilter;
        set { if (SetProperty(ref _activeFilter, value)) ApplyFilter(); }
    }

    public ICommand SetFilterCommand { get; }

    public void Load(IEnumerable<MeetingDto> meetings)
    {
        _allMeetings  = meetings.ToList();
        _activeFilter = "all";
        OnPropertyChanged(nameof(ActiveFilter));
        ApplyFilter();
    }

    public void Reset()
    {
        _allMeetings.Clear();
        FilteredMeetings.Clear();
    }

    private void ApplyFilter()
    {
        IEnumerable<MeetingDto> filtered = _activeFilter switch
        {
            "scheduled"  => _allMeetings.Where(m => m.IsScheduled),
            "inprogress" => _allMeetings.Where(m => m.IsInPreparation),
            _            => _allMeetings
        };

        FilteredMeetings.Clear();
        foreach (MeetingDto m in filtered) FilteredMeetings.Add(m);
    }
}
