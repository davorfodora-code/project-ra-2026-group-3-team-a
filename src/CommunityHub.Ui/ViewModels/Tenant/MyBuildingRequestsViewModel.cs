using CommunityHub.Application.Dto.Tenant;
using CommunityHub.Application.Services.Tenant;

namespace CommunityHub.Ui.ViewModels.Tenant;

public class MyBuildingRequestsViewModel
{
    private readonly BuildingAccessRequestService _service;
    private readonly long _tenantId;
    private List<TenantAccessRequestDto> _allRequests = new();

    public MyBuildingRequestsViewModel(long tenantId)
    {
        _tenantId = tenantId;
        _service = new BuildingAccessRequestService();
    }

    public void LoadRequests()
    {
        _allRequests = _service.GetAllByTenant(_tenantId);
    }

    public List<TenantAccessRequestDto> GetFiltered(string status, bool newestFirst)
    {
        IEnumerable<TenantAccessRequestDto> filtered = _allRequests;

        if (status != "Svi")
            filtered = filtered.Where(r => r.Status == status);

        return newestFirst
            ? filtered.OrderByDescending(r => r.CreatedAt).ToList()
            : filtered.OrderBy(r => r.CreatedAt).ToList();
    }

    public void Cancel(long requestId)
    {
        _service.Cancel(requestId);
    }
}
