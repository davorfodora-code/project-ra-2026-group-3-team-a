using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using CommunityHub.Ui.ViewModels.DistrictCoordinator;

namespace CommunityHub.Ui.Views.DistrictCoordinator;

public partial class CoordinatorHomeWindow : Window
{
    private readonly CoordinatorShellViewModel _vm;

    // Sidebar player state
    private readonly DispatcherTimer _sidebarTimer;
    private bool _sidebarDragging;
    private bool _sidebarPlaying;
    private int  _sidebarSpeedIdx = 1;
    private static readonly double[] Speeds = [0.5, 1.0, 1.5, 2.0];

    public CoordinatorHomeWindow(long userId)
    {
        InitializeComponent();

        _sidebarTimer          = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
        _sidebarTimer.Tick    += SidebarTimerTick;

        _vm = new CoordinatorShellViewModel(userId);
        _vm.LogoutRequested       += OnLogout;
        _vm.NotificationRequested += OnNotification;
        _vm.PropertyChanged       += VmPropertyChanged;

        DataContext = _vm;
        Loaded += (_, _) => _vm.Initialize();
    }

    // ── VM property changes ────────────────────────────────────────────────

    private void VmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CoordinatorShellViewModel.ContextHelpTutorial))
            LoadSidebarVideo();
        else if (e.PropertyName == nameof(CoordinatorShellViewModel.IsContextHelpOpen))
        {
            if (_vm.IsContextHelpOpen) LoadSidebarVideo();
            else                       StopSidebarVideo();
        }
    }

    private void LoadSidebarVideo()
    {
        StopSidebarVideo();
        var path = _vm.ContextHelpTutorial?.VideoPath;
        SidebarVideoPlayer.Source = path != null && File.Exists(path) ? new Uri(path) : null;
    }

    private void StopSidebarVideo()
    {
        _sidebarTimer.Stop();
        SidebarVideoPlayer.Stop();
        _sidebarPlaying              = false;
        SidebarPlayPauseBtn.Content  = "▶";
        SidebarSeek.Value            = 0;
        SidebarTime.Text             = "0:00 / 0:00";
        SidebarPoster.Visibility     = Visibility.Visible;
    }

    // ── MediaElement events ────────────────────────────────────────────────

    private void SidebarVideo_MediaOpened(object sender, RoutedEventArgs e)
    {
        if (SidebarVideoPlayer.NaturalDuration.HasTimeSpan)
        {
            SidebarSeek.Maximum = SidebarVideoPlayer.NaturalDuration.TimeSpan.TotalSeconds;
            SidebarTime.Text    = $"0:00 / {Fmt(SidebarVideoPlayer.NaturalDuration.TimeSpan)}";
        }
        SidebarVideoPlayer.SpeedRatio = Speeds[_sidebarSpeedIdx];
        _sidebarPlaying             = false;
        SidebarPlayPauseBtn.Content = "▶";
    }

    private void SidebarVideo_MediaEnded(object sender, RoutedEventArgs e)
    {
        _sidebarTimer.Stop();
        SidebarVideoPlayer.Stop();
        _sidebarPlaying             = false;
        SidebarPlayPauseBtn.Content = "▶";
        SidebarSeek.Value           = 0;
        SidebarPoster.Visibility    = Visibility.Visible;
    }

    // ── Timer tick ─────────────────────────────────────────────────────────

    private void SidebarTimerTick(object? sender, EventArgs e)
    {
        if (!SidebarVideoPlayer.NaturalDuration.HasTimeSpan || _sidebarDragging) return;
        var pos = SidebarVideoPlayer.Position;
        var dur = SidebarVideoPlayer.NaturalDuration.TimeSpan;
        SidebarSeek.Value = pos.TotalSeconds;
        SidebarTime.Text  = $"{Fmt(pos)} / {Fmt(dur)}";
    }

    private static string Fmt(TimeSpan t)
        => t.TotalHours >= 1 ? t.ToString(@"h\:mm\:ss") : t.ToString(@"m\:ss");

    // ── Seek slider ────────────────────────────────────────────────────────

    private void SidebarSeek_DragStart(object sender, MouseButtonEventArgs e)
        => _sidebarDragging = true;

    private void SidebarSeek_DragEnd(object sender, MouseButtonEventArgs e)
    {
        _sidebarDragging = false;
        if (SidebarVideoPlayer.NaturalDuration.HasTimeSpan)
            SidebarVideoPlayer.Position = TimeSpan.FromSeconds(SidebarSeek.Value);
    }

    // ── Control buttons ────────────────────────────────────────────────────

    private void SidebarPlayPause_Click(object sender, RoutedEventArgs e)
    {
        if (_sidebarPlaying)
        {
            SidebarVideoPlayer.Pause();
            _sidebarPlaying             = false;
            SidebarPlayPauseBtn.Content = "▶";
            _sidebarTimer.Stop();
        }
        else
        {
            StartSidebarPlayback();
        }
    }

    private void SidebarPoster_Click(object sender, MouseButtonEventArgs e)
    {
        if (SidebarVideoPlayer.Source != null) StartSidebarPlayback();
    }

    private void StartSidebarPlayback()
    {
        SidebarPoster.Visibility    = Visibility.Collapsed;
        SidebarVideoPlayer.Play();
        _sidebarPlaying             = true;
        SidebarPlayPauseBtn.Content = "⏸";
        _sidebarTimer.Start();
    }

    private void SidebarStopBtn_Click(object sender, RoutedEventArgs e)
        => StopSidebarVideo();

    private void SidebarSpeed_Click(object sender, RoutedEventArgs e)
    {
        _sidebarSpeedIdx             = (_sidebarSpeedIdx + 1) % Speeds.Length;
        SidebarVideoPlayer.SpeedRatio = Speeds[_sidebarSpeedIdx];
        SidebarSpeedBtn.Content      = $"{Speeds[_sidebarSpeedIdx]}x";
    }

    private void SidebarFullscreen_Click(object sender, RoutedEventArgs e)
    {
        if (SidebarVideoPlayer.Source == null) return;
        VideoFullscreenHelper.Show(SidebarVideoPlayer, this);
    }

    // ── App events ─────────────────────────────────────────────────────────

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
