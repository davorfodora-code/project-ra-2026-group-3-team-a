using System;
using System.Windows;
using System.Windows.Controls;
using CommunityHub.Ui.ViewModels.DistrictCoordinator;

namespace CommunityHub.Ui.Views.DistrictCoordinator;

public partial class ForumView : UserControl
{
    public ForumView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        Loaded += OnLoaded;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is ForumViewModel oldVm)
        {
            oldVm.CreateForumDialogRequested -= OnCreateForumDialog;
            oldVm.NotificationRequested -= OnNotification;
        }
        if (e.NewValue is ForumViewModel newVm)
        {
            newVm.CreateForumDialogRequested += OnCreateForumDialog;
            newVm.NotificationRequested += OnNotification;
            InitializeViewModel();
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        InitializeViewModel();
    }

    private void InitializeViewModel()
    {
        if (DataContext is ForumViewModel vm)
            vm.Initialize();
    }

    private void OnCreateForumDialog(Action<bool> callback)
    {
        if (DataContext is not ForumViewModel vm) return;

        CreateForumDialog dialog = new CreateForumDialog(vm.CoordinatorId)
        {
            Owner = Window.GetWindow(this)
        };
        callback(dialog.ShowDialog() == true);
    }

    private void OnNotification(string message, bool isSuccess)
    {
        MessageBox.Show(message, isSuccess ? "Uspeh" : "Greška",
            MessageBoxButton.OK,
            isSuccess ? MessageBoxImage.Information : MessageBoxImage.Error);
    }
}
