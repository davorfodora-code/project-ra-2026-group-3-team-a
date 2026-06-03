using System.Collections.ObjectModel;
using CommunityHub.Application;
using CommunityHub.Application.Services.BuildingManager;
using CommunityHub.Application.Services.BuildingManager.Dto;

namespace CommunityHub.Ui.ViewModels.BuildingManager;

public class AssembliesViewModel : BaseViewModel
{
    private readonly AssemblyService _service;
    private readonly long _managerId;

    private AssemblySummaryDto? _nextAssembly;

    public AssembliesViewModel(long managerId)
    {
        _managerId = managerId;
        _service   = new AssemblyService(
            Injector.Get<IAssemblyRepository>(),
            Injector.Get<IBuildingRepository>());
        Assemblies = new ObservableCollection<AssemblyItemViewModel>();
        Load();
    }

    public ObservableCollection<AssemblyItemViewModel> Assemblies { get; }

    // Prva naredna skupstina (za hero karticu na vrhu)
    public AssemblySummaryDto? NextAssembly
    {
        get => _nextAssembly;
        private set
        {
            if (SetProperty(ref _nextAssembly, value))
            {
                OnPropertyChanged(nameof(HasNextAssembly));
                OnPropertyChanged(nameof(NextBuildingLine));
                OnPropertyChanged(nameof(NextDateTimeLine));
            }
        }
    }

    public bool   HasNextAssembly => _nextAssembly != null;
    public string NextBuildingLine  => _nextAssembly?.BuildingAddress ?? string.Empty;
    public string NextDateTimeLine  => _nextAssembly?.DateTimeLine    ?? string.Empty;

    public bool HasAssemblies   => Assemblies.Count > 0;
    public bool HasNoAssemblies => Assemblies.Count == 0;

    public void Load()
    {
        try
        {
            List<AssemblySummaryDto> list = _service.GetByManagerId(_managerId);
            NextAssembly = list
                .Where(a => a.ScheduledAt > DateTime.Now && a.Status != "Cancelled")
                .OrderBy(a => a.ScheduledAt)
                .FirstOrDefault();

            Assemblies.Clear();
            foreach (AssemblySummaryDto dto in list)
                Assemblies.Add(new AssemblyItemViewModel(dto));
        }
        catch
        {
            Assemblies.Clear();
            NextAssembly = null;
        }
        OnPropertyChanged(nameof(HasAssemblies));
        OnPropertyChanged(nameof(HasNoAssemblies));
    }
}
