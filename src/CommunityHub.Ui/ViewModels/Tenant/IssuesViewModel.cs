using System.Collections.ObjectModel;
using CommunityHub.Application;
using CommunityHub.Application.Services.Tenant;
using CommunityHub.Application.Services.Tenant.Dto;

namespace CommunityHub.Ui.ViewModels.Tenant;

public class IssuesViewModel : BaseViewModel
{
    private readonly TenantIssueService _service;
    public long TenantId { get; }

    public ObservableCollection<IssueReportDto> Issues { get; } = new();
    public List<TenantBuildingDto> Buildings { get; private set; } = [];
    private List<IssueReportDto> _all = [];
    private long _filterBuildingId;

    public int UnresolvedCount => _all.Count(i => i.Status == "Unresolved");
    public int PotentiallyResolvedCount => _all.Count(i => i.Status == "PotentiallyResolved");
    public int ResolvedCount => _all.Count(i => i.Status == "Resolved");

    public IssuesViewModel(long tenantId)
    {
        TenantId = tenantId;
        _service = new TenantIssueService(
            Injector.Get<ITenantIssueRepository>(),
            Injector.Get<IAdvertisementRepository>());
    }

    public void Load()
    {
        _all = _service.GetByTenantId(TenantId);
        Buildings = _service.GetMyBuildings(TenantId);
        ApplyFilter(_filterBuildingId);
        OnPropertyChanged(nameof(UnresolvedCount));
        OnPropertyChanged(nameof(PotentiallyResolvedCount));
        OnPropertyChanged(nameof(ResolvedCount));
    }

    public void ApplyFilter(long buildingId)
    {
        _filterBuildingId = buildingId;
        Issues.Clear();
        var filtered = buildingId == 0 ? _all : _all.Where(i => i.BuildingId == buildingId).ToList();
        foreach (var i in filtered) Issues.Add(i);
    }

    public void Report(CreateIssueReportDto dto) => _service.Report(dto);

    public void ConfirmResolved(long id) { _service.ConfirmResolved(id); Load(); }

    public void DenyResolved(long id) { _service.DenyResolved(id); Load(); }
}
