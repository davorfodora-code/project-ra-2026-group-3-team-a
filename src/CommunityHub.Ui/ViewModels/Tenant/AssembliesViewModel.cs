using System.Collections.ObjectModel;
using CommunityHub.Application;
using CommunityHub.Application.Services.Tenant;
using CommunityHub.Application.Services.Tenant.Dto;

namespace CommunityHub.Ui.ViewModels.Tenant;

public class AssembliesViewModel : BaseViewModel
{
    private readonly TenantAssemblyService _service;
    private readonly long _tenantId;
    private List<TenantAssemblyDto> _all = [];
    private long _filterBuildingId;

    public List<TenantBuildingDto> Buildings { get; private set; } = [];
    public ObservableCollection<TenantAssemblyDto> Assemblies { get; } = new();

    public AssembliesViewModel(long tenantId)
    {
        _tenantId = tenantId;
        _service = new TenantAssemblyService(Injector.Get<ITenantAssemblyRepository>());
    }

    public void Load()
    {
        _all = _service.GetByTenantId(_tenantId);
        Buildings = _service.GetBuildings(_tenantId);
        ApplyFilter(_filterBuildingId);
    }

    public void ApplyFilter(long buildingId)
    {
        _filterBuildingId = buildingId;
        Assemblies.Clear();
        var filtered = buildingId == 0 ? _all : _all.Where(a => a.BuildingId == buildingId).ToList();
        foreach (var a in filtered) Assemblies.Add(a);
    }

    public void RegisterAttendance(TenantAssemblyDto dto)
    {
        _service.RegisterAttendance(dto.Id, _tenantId, dto.ApartmentNumber, dto.BuildingId);
        Load();
    }

    public void CancelAttendance(TenantAssemblyDto dto)
    {
        _service.CancelAttendance(dto.Id, _tenantId, dto.BuildingId);
        Load();
    }

    public void ProposeTopic(long assemblyId, string topic)
        => _service.ProposeTopic(assemblyId, _tenantId, topic);
}
