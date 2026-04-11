using System.Windows;

namespace CommunityHub.Ui.Views.BuildingManager;

public partial class BuildingManagerHomeWindow : Window
{
    private readonly long _userId;

    public BuildingManagerHomeWindow(long userId)
    {
        InitializeComponent();
        _userId = userId;
    }

    private void RegisterBuildingButton_Click(object sender, RoutedEventArgs e)
    {
        BuildingRegistrationWindow window = new BuildingRegistrationWindow(_userId);
        window.Show();
    }

    private void AccessRequestsButton_Click(object sender, RoutedEventArgs e)
    {
        AccessRequestsWindow window = new AccessRequestsWindow(_userId);
        window.Show();
    }

    private void MembershipsButton_Click(object sender, RoutedEventArgs e)
    {
        BuildingMembershipsWindow window = new BuildingMembershipsWindow(_userId);
        window.Show();
    }

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        LogInForm loginForm = new LogInForm();
        loginForm.Show();
        this.Close();
    }
}
