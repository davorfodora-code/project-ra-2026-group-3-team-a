using System.Windows;

namespace CommunityHub.Ui.Views.Tenant;

public partial class TenantHomeWindow : Window
{
    private readonly long _userId;

    public TenantHomeWindow(long userId)
    {
        InitializeComponent();
        _userId = userId;
    }

    private void ProfileButton_Click(object sender, RoutedEventArgs e)
    {
        ProfileWindow profileWindow = new ProfileWindow(_userId);
        profileWindow.Show();
        this.Close();
    }

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        LogInForm loginForm = new LogInForm();
        loginForm.Show();
        this.Close();
    }

    private void BuildingSearchButton_Click(object sender, RoutedEventArgs e)
    {
        BuildingSearchWindow window = new BuildingSearchWindow(_userId);
        window.Show();
        this.Close();
    }

    private void MyRequestsButton_Click(object sender, RoutedEventArgs e)
    {
        MyBuildingRequestsWindow window = new MyBuildingRequestsWindow(_userId);
        window.Show();
        this.Close();
    }
}
