using System.Windows;

namespace CommunityHub.Ui.Views;

public partial class HomeWindow : Window
{
    private readonly long _userId;

    public HomeWindow(long userId)
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

    private void RegisterBuildingButton_Click(object sender, RoutedEventArgs e)
    {
        BuildingRegistrationWindow window = new BuildingRegistrationWindow(_userId);
        window.Show();
    }

}
