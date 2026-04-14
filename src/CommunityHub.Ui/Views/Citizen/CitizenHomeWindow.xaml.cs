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
        MessageBox.Show(
            "Prikaz i pretraga kvartova se dodaje u sledećem commitu.",
            "U izradi",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
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

