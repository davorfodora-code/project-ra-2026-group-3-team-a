using System.Collections.Generic;
using CommunityHub.Application.Dto.DistrictCoordinator;
using CommunityHub.Application.Services.DistrictCoordinator;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;

public class MembershipsViewModel
{
    private readonly DistrictAccessRequestService _service;
    private readonly long _coordinatorId;
    private List<DistrictDto> _districts;
    private List<DistrictMembershipDto> _memberships;

    public MembershipsViewModel(long coordinatorId)
    {
        _coordinatorId = coordinatorId;
        _service = new DistrictAccessRequestService();
        _districts = new List<DistrictDto>();
        _memberships = new List<DistrictMembershipDto>();
    }

    public void LoadDistricts()
    {
        _districts = _service.GetDistrictsByCoordinatorId(_coordinatorId);
    }

    public List<DistrictDto> GetDistricts() => _districts;

    public void LoadMemberships(long districtId)
    {
        _memberships = _service.GetMembershipsByDistrictId(districtId);
    }

    public List<DistrictMembershipDto> GetMemberships() => _memberships;
}
