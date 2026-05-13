using System.Windows;
using CommunityHub.Ui.ViewModels.DistrictCoordinator;

namespace CommunityHub.Ui.Views.DistrictCoordinator;

public partial class CoordinatorHomeWindow : Window
{
    private readonly CoordinatorShellViewModel _vm;

    public CoordinatorHomeWindow(long userId)
    {
        InitializeComponent();

        _vm = new CoordinatorShellViewModel(userId);
        _vm.LogoutRequested       += OnLogout;
        _vm.NotificationRequested += OnNotification;

        DataContext = _vm;
        Loaded += (_, _) => _vm.Initialize();
    }

    private void OnLogout()
    {
        new LogInForm().Show();
        Close();
    }

    private void OnNotification(string message)
    {
        MessageBox.Show(message, "Obavijest", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
