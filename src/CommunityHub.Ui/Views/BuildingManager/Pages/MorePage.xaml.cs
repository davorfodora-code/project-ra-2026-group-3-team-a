using System.Windows.Controls;

namespace CommunityHub.Ui.Views.BuildingManager.Pages;

public partial class MorePage : Page
{
    private readonly BuildingManagerWindow _shell;

    public MorePage(BuildingManagerWindow shell)
    {
        InitializeComponent();
        _shell = shell;
    }
}
