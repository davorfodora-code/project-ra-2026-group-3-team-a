using System.Windows.Controls;

namespace CommunityHub.Ui.Views.BuildingManager.Pages;

public partial class AssembliesPage : Page
{
    private readonly BuildingManagerWindow _shell;

    public AssembliesPage(BuildingManagerWindow shell)
    {
        InitializeComponent();
        _shell = shell;
    }
}
