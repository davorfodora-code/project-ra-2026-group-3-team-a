using System.Windows.Controls;

namespace CommunityHub.Ui.Views.BuildingManager.Pages;

public partial class HomePage : Page
{
    private readonly BuildingManagerWindow _shell;

    public HomePage(BuildingManagerWindow shell)
    {
        InitializeComponent();
        _shell = shell;
    }
}
