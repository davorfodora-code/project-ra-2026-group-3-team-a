using System.Collections.ObjectModel;
using CommunityHub.Application.Dto.DistrictCoordinator;
using CommunityHub.Application.Services.DistrictCoordinator;
using CommunityHub.Ui.ViewModels.Common;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;

public class MembershipsViewModel : ViewModelBase
{
    private readonly DistrictAccessRequestService _service;
    private readonly long _coordinatorId;

    private DistrictDto? _selectedDistrict;

    public MembershipsViewModel(long coordinatorId)
    {
        _coordinatorId = coordinatorId;
        _service       = new DistrictAccessRequestService();

        Districts   = new ObservableCollection<DistrictDto>();
        Memberships = new ObservableCollection<DistrictMembershipDto>();
    }

    public ObservableCollection<DistrictDto>           Districts   { get; }
    public ObservableCollection<DistrictMembershipDto> Memberships { get; }

    public DistrictDto? SelectedDistrict
    {
        get => _selectedDistrict;
        set
        {
            if (SetProperty(ref _selectedDistrict, value) && value != null)
                LoadMemberships(value.Id);
        }
    }

    public void Initialize()
    {
        Districts.Clear();
        foreach (var d in _service.GetDistrictsByCoordinatorId(_coordinatorId))
            Districts.Add(d);

        if (Districts.Count > 0)
            SelectedDistrict = Districts[0];
    }

    private void LoadMemberships(long districtId)
    {
        Memberships.Clear();
        foreach (var m in _service.GetMembershipsByDistrictId(districtId))
            Memberships.Add(m);
    }
}
