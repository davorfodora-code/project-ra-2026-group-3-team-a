using System;
using System.Collections.Generic;
using System.Windows;
using CommunityHub.Application.Domain;
using CommunityHub.Application.Dto.DistrictCoordinator;
using CommunityHub.Ui.ViewModels.DistrictCoordinator;

namespace CommunityHub.Ui.Views.DistrictCoordinator;

public partial class MeetingsWindow : Window
{
    private readonly MeetingsViewModel _vm;

    public MeetingsWindow(long coordinatorId)
    {
        InitializeComponent();

        _vm = new MeetingsViewModel(coordinatorId);
        _vm.ScheduleMeetingDialogRequested += OnScheduleMeetingDialog;
        _vm.ResolveTieDialogRequested      += OnResolveTieDialog;
        _vm.NotificationRequested          += OnNotification;

        DataContext = _vm;
        Loaded += (_, _) => _vm.Initialize();
    }

    private void OnScheduleMeetingDialog(long districtId, MeetingType? recommended, Action<bool> callback)
    {
        ScheduleMeetingDialog dialog = new ScheduleMeetingDialog(districtId, recommended) { Owner = this };
        bool? result = dialog.ShowDialog();
        callback(result == true);
    }

    private void OnResolveTieDialog(MeetingDto meeting, List<DateTime> tiedDates, Action<DateTime?> callback)
    {
        ResolveTieDialog dialog = new ResolveTieDialog(tiedDates) { Owner = this };
        callback(dialog.ShowDialog() == true ? dialog.ChosenDate : null);
    }

    private void OnNotification(string message, string title)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
