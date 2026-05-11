using System.Windows;
using CommunityHub.Ui.ViewModels.DistrictCoordinator;

namespace CommunityHub.Ui.Views.DistrictCoordinator;


public partial class AccessRequestsWindow : Window
{
    private readonly AccessRequestsViewModel _vm;

    public AccessRequestsWindow(long coordinatorId)
    {
        InitializeComponent();

        _vm = new AccessRequestsViewModel(coordinatorId);
        _vm.NotificationRequested += OnNotificationRequested;
        DataContext = _vm;

        Loaded   += (_, _) => _vm.Initialize();
        Closed   += (_, _) => _vm.NotificationRequested -= OnNotificationRequested;
    }

    private void OnNotificationRequested(string message, bool isSuccess)
    {
        MessageBox.Show(
            message,
            isSuccess ? "Uspeh" : "Greška",
            MessageBoxButton.OK,
            isSuccess ? MessageBoxImage.Information : MessageBoxImage.Error);
    }
}
