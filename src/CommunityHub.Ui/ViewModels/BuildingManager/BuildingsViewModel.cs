using System.Collections.ObjectModel;
using CommunityHub.Application;
using CommunityHub.Application.Services.BuildingManager;
using CommunityHub.Application.Services.BuildingManager.Dto;

namespace CommunityHub.Ui.ViewModels.BuildingManager;

public class BuildingsViewModel : BaseViewModel
{
    private readonly BuildingService _service;
    private readonly long _managerId;

    private ObservableCollection<BuildingDto> _buildings = new();
    private bool _isEmpty;

    public BuildingsViewModel(long managerId)
    {
        _managerId = managerId;
        _service   = new BuildingService(Injector.Get<IBuildingRepository>());
    }

    public ObservableCollection<BuildingDto> Buildings
    {
        get => _buildings;
        private set => SetProperty(ref _buildings, value);
    }

    public bool IsEmpty
    {
        get => _isEmpty;
        private set => SetProperty(ref _isEmpty, value);
    }

    public void Load()
    {
        List<BuildingDto> result = _service.GetByManagerId(_managerId);
        Buildings = new ObservableCollection<BuildingDto>(result);
        IsEmpty = result.Count == 0;
    }
}
