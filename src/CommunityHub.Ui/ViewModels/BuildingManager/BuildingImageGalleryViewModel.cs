using System.Collections.Generic;

namespace CommunityHub.Ui.ViewModels.BuildingManager;

// Galerija slika zgrade — drzi trenutni indeks i navigaciju (Next/Prev)
public class BuildingImageGalleryViewModel : BaseViewModel
{
    private readonly List<string> _imagePaths;
    private int _index;

    public BuildingImageGalleryViewModel(List<string> imagePaths)
    {
        _imagePaths = imagePaths;
    }

    public bool    HasImages         => _imagePaths.Count > 0;
    public bool    HasMultipleImages => _imagePaths.Count > 1;
    public string? CurrentImagePath  => HasImages ? _imagePaths[_index] : null;

    public void NextImage()
    {
        if (_index < _imagePaths.Count - 1) Move(_index + 1);
    }

    public void PreviousImage()
    {
        if (_index > 0) Move(_index - 1);
    }

    private void Move(int newIndex)
    {
        _index = newIndex;
        OnPropertyChanged(nameof(CurrentImagePath));
    }
}
