using System.Collections.ObjectModel;
using CommunityHub.Application;
using CommunityHub.Application.Services.Tenant.Dto;
using CommunityHub.Application.Services.Tenant;

namespace CommunityHub.Ui.ViewModels.Tenant;

public class MyBuildingRequestsViewModel : BaseViewModel
{
    private readonly BuildingAccessRequestService _service;
    private readonly long _tenantId;
    private List<TenantAccessRequestDto> _allRequests = new();

    public ObservableCollection<TenantAccessRequestDto> FilteredRequests { get; } = new();

    public MyBuildingRequestsViewModel(long tenantId)
    {
        _tenantId = tenantId;
        _service = new BuildingAccessRequestService(
            Injector.Get<IBuildingAccessRequestRepository>(),
            Injector.Get<IBuildingMembershipRepository>());
    }

    public void LoadRequests()
    {
        _allRequests = _service.GetAllByTenant(_tenantId);
    }

    public void ApplyFilters(string status, bool newestFirst)
    {
        IEnumerable<TenantAccessRequestDto> filtered = _allRequests;

        if (status != "Svi")
            filtered = filtered.Where(r => r.Status == status);

        var sorted = newestFirst
            ? filtered.OrderByDescending(r => r.CreatedAt)
            : filtered.OrderBy(r => r.CreatedAt);

        FilteredRequests.Clear();
        foreach (var r in sorted)
            FilteredRequests.Add(r);
    }

    public void Cancel(long requestId)
    {
        _service.Cancel(requestId);
    }
}
