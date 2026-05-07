using System.Windows.Controls;

namespace CommunityHub.Ui.Views.BuildingManager.Pages;

public partial class BuildingsPage : Page
{
    private readonly BuildingManagerWindow _shell;

    public BuildingsPage(BuildingManagerWindow shell)
    {
        InitializeComponent();
        _shell = shell;
    }
}
