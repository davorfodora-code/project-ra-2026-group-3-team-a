using System.Windows.Controls;

namespace CommunityHub.Ui.Views.BuildingManager.Pages;

public partial class RequestsPage : Page
{
    private readonly BuildingManagerWindow _shell;

    public RequestsPage(BuildingManagerWindow shell)
    {
        InitializeComponent();
        _shell = shell;
    }
}
