using System.Collections.Generic;
using System.Linq;
using CommunityHub.Application.Dto.DistrictCoordinator;
using CommunityHub.Application.Services.DistrictCoordinator;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;

public class AccessRequestsViewModel
{
    private readonly DistrictAccessRequestService _service;
    private readonly long _coordinatorId;
    private List<DistrictAccessRequestDto> _allRequests;

    public AccessRequestsViewModel(long coordinatorId)
    {
        _coordinatorId = coordinatorId;
        _service = new DistrictAccessRequestService();
        _allRequests = new List<DistrictAccessRequestDto>();
    }

    public void LoadRequests()
    {
        _allRequests = _service.GetByCoordinatorId(_coordinatorId);
    }

    public List<DistrictAccessRequestDto> GetFiltered(string status, bool newestFirst)
    {
        IEnumerable<DistrictAccessRequestDto> filtered = _allRequests;

        if (status != "Svi")
            filtered = filtered.Where(r => r.Status == status);

        return newestFirst
            ? filtered.OrderByDescending(r => r.CreatedAt).ToList()
            : filtered.OrderBy(r => r.CreatedAt).ToList();
    }

    public void Approve(long requestId)
    {
        _service.Approve(requestId);
    }

    public void Reject(long requestId, string? reason)
    {
        _service.Reject(requestId, reason);
    }
}
