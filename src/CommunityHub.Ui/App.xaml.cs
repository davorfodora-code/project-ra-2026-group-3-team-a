using System;
using System.Windows;
using System.Windows.Threading;
using CommunityHub.Ui.Settings;
using CommunityHub.Ui.ViewModels.DistrictCoordinator;
using CommunityHub.Ui.Views.DistrictCoordinator;

namespace CommunityHub.Ui;

public partial class App
{
    private readonly DispatcherTimer _meetingTimer;

    public App()
    {
        DispatcherUnhandledException += OnDispatcherUnhandledException;

        // Ucitaj globalna podesavanja iz JSON fajla
        AppSettings.Load();

        _meetingTimer = new DispatcherTimer
        {
            Interval = System.TimeSpan.FromHours(1)
        };
        _meetingTimer.Tick += OnMeetingTimerTick;
        _meetingTimer.Start();
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(
            $"Neočekivana greška:\n\n{e.Exception.GetType().Name}: {e.Exception.Message}\n\n{e.Exception.StackTrace}",
            "Greška aplikacije", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }

    private void OnMeetingTimerTick(object? sender, System.EventArgs e)
    {

        foreach (Window window in Current.Windows)
        {
            if (window is CoordinatorHomeWindow shell &&
                shell.DataContext is CoordinatorShellViewModel vm)
            {
                vm.Stats.RunResolveCycle();
            }
        }
    }
}
