using System.Windows;

namespace CommunityHub.Ui.Views.Citizen;

public partial class CitizenHomeWindow : Window
{
    private readonly long _userId;

    public CitizenHomeWindow(long userId)
    {
        InitializeComponent();
        _userId = userId;
    }

    private void DistrictSearchButton_Click(object sender, RoutedEventArgs e)
    {
        DistrictSearchWindow window = new DistrictSearchWindow(_userId);
        window.Show();
        Close();
    }

    private void MyDistrictRequestsButton_Click(object sender, RoutedEventArgs e)
    {
        MyDistrictRequestsWindow window = new MyDistrictRequestsWindow(_userId);
        window.Show();
        Close();
    }

    private void ProfileButton_Click(object sender, RoutedEventArgs e)
    {
        ProfileWindow profileWindow = new ProfileWindow(_userId);
        profileWindow.Show();
        Close();
    }

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        LogInForm loginForm = new LogInForm();
        loginForm.Show();
        Close();
    }
}

