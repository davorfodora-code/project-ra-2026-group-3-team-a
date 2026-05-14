using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using CommunityHub.Application.Domain;
using CommunityHub.Application.Services.DistrictCoordinator.Dto;
using CommunityHub.Ui.ViewModels.DistrictCoordinator;

namespace CommunityHub.Ui.Views.DistrictCoordinator;

public partial class StatisticsView : UserControl
{
    public StatisticsView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        Loaded += OnLoaded;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is StatisticsViewModel oldVm)
        {
            oldVm.ScheduleMeetingDialogRequested -= OnScheduleMeetingDialog;
            oldVm.ResolveTieDialogRequested      -= OnResolveTieDialog;
            oldVm.NotificationRequested          -= OnNotification;
        }
        if (e.NewValue is StatisticsViewModel newVm)
        {
            newVm.ScheduleMeetingDialogRequested += OnScheduleMeetingDialog;
            newVm.ResolveTieDialogRequested      += OnResolveTieDialog;
            newVm.NotificationRequested          += OnNotification;
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is StatisticsViewModel vm)
            vm.Initialize();
    }

    private void OnScheduleMeetingDialog(long districtId, MeetingType? recommended, Action<bool> callback)
    {
        ScheduleMeetingDialog dialog = new ScheduleMeetingDialog(districtId, recommended)
        {
            Owner = Window.GetWindow(this)
        };
        callback(dialog.ShowDialog() == true);
    }

    private void OnResolveTieDialog(MeetingDto meeting, List<DateTime> tiedDates, Action<DateTime?> callback)
    {
        ResolveTieDialog dialog = new ResolveTieDialog(tiedDates) { Owner = Window.GetWindow(this) };
        callback(dialog.ShowDialog() == true ? dialog.ChosenDate : null);
    }

    private void OnNotification(string message, string title, bool isInfo)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK,
            isInfo ? MessageBoxImage.Information : MessageBoxImage.Error);
    }
}
