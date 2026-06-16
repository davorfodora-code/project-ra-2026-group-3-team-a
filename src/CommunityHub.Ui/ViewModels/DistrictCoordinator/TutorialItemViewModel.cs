using System.Collections.Generic;
using CommunityHub.Ui.ViewModels.Common;
using FontAwesome.Sharp;

namespace CommunityHub.Ui.ViewModels.DistrictCoordinator;

public record ShortcutEntry(string Keys, string Description);

public class TutorialItemViewModel : ViewModelBase
{
    private bool _isSelected;

    public int      Number    { get; init; }
    public string   Title     { get; init; } = string.Empty;
    public string?  VideoPath { get; init; }
    public IconChar Icon      { get; init; } = IconChar.CirclePlay;

    public IReadOnlyList<ShortcutEntry> WindowShortcuts { get; init; } = [];

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}
