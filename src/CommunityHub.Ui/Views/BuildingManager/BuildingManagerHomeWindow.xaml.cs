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

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        LogInForm loginForm = new LogInForm();
        loginForm.Show();
        this.Close();
    }
}
