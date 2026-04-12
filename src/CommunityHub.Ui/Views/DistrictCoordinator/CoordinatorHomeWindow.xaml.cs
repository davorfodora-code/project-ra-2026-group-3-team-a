using System.Windows;

namespace CommunityHub.Ui.Views.DistrictCoordinator;

public partial class CoordinatorHomeWindow : Window
{
    private readonly long _userId;

    public CoordinatorHomeWindow(long userId)
    {
        InitializeComponent();
        _userId = userId;
    }

    private void RegisterDistrictButton_Click(object sender, RoutedEventArgs e)
    {
        DistrictRegWindow window = new DistrictRegWindow(_userId);
        window.Show();
    }

    private void AccessRequestsButton_Click(object sender, RoutedEventArgs e)
    {
        AccessRequestsWindow window = new AccessRequestsWindow(_userId);
        window.Show();
    }

    private void MembershipsButton_Click(object sender, RoutedEventArgs e)
    {
        MembershipsWindow window = new MembershipsWindow(_userId);
        window.Show();
    }

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        LogInForm loginForm = new LogInForm();
        loginForm.Show();
        this.Close();
    }
}
