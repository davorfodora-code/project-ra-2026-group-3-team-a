using System.Windows;
using System.Windows.Controls;
using CommunityHub.Ui.ViewModels.DistrictCoordinator;

namespace CommunityHub.Ui.Views.DistrictCoordinator;

public partial class AccessRequestsView : UserControl
{
    public AccessRequestsView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        Loaded += OnLoaded;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is AccessRequestsViewModel oldVm)
            oldVm.NotificationRequested -= OnNotification;
        if (e.NewValue is AccessRequestsViewModel newVm)
            newVm.NotificationRequested += OnNotification;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is AccessRequestsViewModel vm)
            vm.Initialize();
    }

    private static void OnNotification(string message, bool isSuccess)
    {
        MessageBox.Show(
            message,
            isSuccess ? "Uspeh" : "Greška",
            MessageBoxButton.OK,
            isSuccess ? MessageBoxImage.Information : MessageBoxImage.Error);
    }
}
