using CommunityHub.Application.Services.BuildingManager.Dto;

namespace CommunityHub.Ui.ViewModels.Tenant;

public class BuildingCardVm : BaseViewModel
{
    private int _imageIndex;

    public BuildingDto Building { get; }

    public string? CurrentImagePath => Building.ImagePaths.Count > 0
        ? Building.ImagePaths[_imageIndex]
        : null;

    public bool HasImages => Building.ImagePaths.Count > 0;

    public BuildingCardVm(BuildingDto building)
    {
        Building = building;
    }

    public void Prev()
    {
        if (Building.ImagePaths.Count == 0) return;
        _imageIndex = (_imageIndex - 1 + Building.ImagePaths.Count) % Building.ImagePaths.Count;
        OnPropertyChanged(nameof(CurrentImagePath));
    }

    public void Next()
    {
        if (Building.ImagePaths.Count == 0) return;
        _imageIndex = (_imageIndex + 1) % Building.ImagePaths.Count;
        OnPropertyChanged(nameof(CurrentImagePath));
    }
}
