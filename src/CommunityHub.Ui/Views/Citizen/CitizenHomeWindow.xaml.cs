using System.Windows;
using CommunityHub.Ui.ViewModels.Citizen;

namespace CommunityHub.Ui.Views.Citizen;

public partial class CitizenHomeWindow : Window
{
    private readonly CitizenHomeViewModel _viewModel;

    public CitizenHomeWindow(long userId)
    {
        InitializeComponent();
        _viewModel = new CitizenHomeViewModel(userId);
    }

    private void DistrictSearchButton_Click(object sender, RoutedEventArgs e)
    {
        DistrictSearchWindow window = new DistrictSearchWindow(_viewModel.UserId);
        window.Show();
        Close();
    }

    private void MyDistrictRequestsButton_Click(object sender, RoutedEventArgs e)
    {
        MyDistrictRequestsWindow window = new MyDistrictRequestsWindow(_viewModel.UserId);
        window.Show();
        Close();
    }

    private void DistrictEventsButton_Click(object sender, RoutedEventArgs e)
    {
        CitizenEventsWindow window = new CitizenEventsWindow(_viewModel.UserId);
        window.Show();
        Close();
    }

    private void MyDistrictButton_Click(object sender, RoutedEventArgs e)
    {
        MyDistrictWindow window = new MyDistrictWindow(_viewModel.UserId);
        window.Show();
        Close();
    }

    private void ProfileButton_Click(object sender, RoutedEventArgs e)
    {
        ProfileWindow profileWindow = new ProfileWindow(_viewModel.UserId);
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

