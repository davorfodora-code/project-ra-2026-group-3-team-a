using CommunityHub.Ui.ViewModels.Common;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;


public class ImageSlotViewModel : ViewModelBase
{
    private readonly int _slotIndex;     // 1..4
    private string? _filePath;
    private string _displayName;

    public ImageSlotViewModel(int slotIndex)
    {
        _slotIndex   = slotIndex;
        _displayName = $"Slika {slotIndex}";
    }

    public int SlotIndex => _slotIndex;

    public string? FilePath
    {
        get => _filePath;
        set
        {
            if (SetProperty(ref _filePath, value))
            {
                DisplayName = value == null
                    ? $"Slika {_slotIndex}"
                    : System.IO.Path.GetFileName(value);
                OnPropertyChanged(nameof(HasImage));
                OnPropertyChanged(nameof(HasNoImage));
            }
        }
    }

    public string DisplayName
    {
        get => _displayName;
        private set => SetProperty(ref _displayName, value);
    }

    public bool HasImage   => _filePath != null;
    public bool HasNoImage => _filePath == null;
}
