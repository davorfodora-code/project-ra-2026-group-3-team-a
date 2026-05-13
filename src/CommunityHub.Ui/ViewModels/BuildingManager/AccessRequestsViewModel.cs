using CommunityHub.Application;
using CommunityHub.Application.Dto.BuildingManager;
using CommunityHub.Application.Services.BuildingManager;

namespace CommunityHub.Ui.ViewModels.BuildingManager;

public class AccessRequestsViewModel
{
    private readonly BuildingAccessRequestService _service;
    private readonly long _managerId;
    private List<BuildingAccessRequestDto> _allRequests;

    public AccessRequestsViewModel(long managerId)
    {
        _managerId = managerId;
        _service = new BuildingAccessRequestService(Injector.Get<IBuildingAccessRequestRepository>());
        _allRequests = new List<BuildingAccessRequestDto>();
    }

    public void LoadRequests()
    {
        _allRequests = _service.GetByManagerId(_managerId);
    }

    public List<BuildingAccessRequestDto> GetFiltered(string status, bool newestFirst)
    {
        IEnumerable<BuildingAccessRequestDto> filtered = _allRequests;

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
