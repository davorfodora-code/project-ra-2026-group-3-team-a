using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityHub.Application.Services.DistrictCoordinator.Dto;
using CommunityHub.Ui.ViewModels.Common;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;

public class MembershipListViewModel : ViewModelBase
{
    private List<DistrictMembershipDto> _allMemberships = new();
    private string _trustFilter = "Svi";

    public MembershipListViewModel()
    {
        Memberships           = new ObservableCollection<DistrictMembershipDto>();
        SetTrustFilterCommand = new RelayCommand(p => TrustFilter = p?.ToString() ?? "Svi");
    }

    public ObservableCollection<DistrictMembershipDto> Memberships { get; }

    public string TrustFilter
    {
        get => _trustFilter;
        set { if (SetProperty(ref _trustFilter, value)) ApplyFilter(); }
    }

    public ICommand SetTrustFilterCommand { get; }

    public void Load(IEnumerable<DistrictMembershipDto> memberships)
    {
        _allMemberships = memberships.ToList();
        _trustFilter    = "Svi";
        OnPropertyChanged(nameof(TrustFilter));
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        IEnumerable<DistrictMembershipDto> filtered = _trustFilter == "Svi"
            ? _allMemberships
            : _allMemberships.Where(m => m.TrustLevel == _trustFilter);
        Memberships.Clear();
        foreach (DistrictMembershipDto m in filtered) Memberships.Add(m);
    }
}
