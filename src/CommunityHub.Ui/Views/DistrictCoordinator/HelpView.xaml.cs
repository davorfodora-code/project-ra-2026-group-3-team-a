using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using CommunityHub.Ui.ViewModels.DistrictCoordinator;

namespace CommunityHub.Ui.Views.DistrictCoordinator;

public partial class HelpView : UserControl
{
    private readonly DispatcherTimer _timer;
    private bool _dragging;
    private bool _playing;
    private int  _speedIdx = 1;
    private static readonly double[] Speeds = [0.5, 1.0, 1.5, 2.0];

    public HelpView()
    {
        InitializeComponent();
        _timer       = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
        _timer.Tick += TimerTick;
        DataContextChanged += OnDataContextChanged;
    }

    // ── DataContext / VM wiring ────────────────────────────────────────────

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is INotifyPropertyChanged oldVm) oldVm.PropertyChanged -= VmPropertyChanged;
        if (e.NewValue is INotifyPropertyChanged newVm) newVm.PropertyChanged += VmPropertyChanged;
    }

    private void VmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(HelpViewModel.SelectedTutorial))
            LoadVideo();
    }

    private void LoadVideo()
    {
        ResetPlayer();
        var path = (DataContext as HelpViewModel)?.SelectedTutorial?.VideoPath;
        VideoPlayer.Source = path != null && File.Exists(path) ? new Uri(path) : null;
    }

    private void ResetPlayer()
    {
        _timer.Stop();
        VideoPlayer.Stop();
        _playing                   = false;
        DetailPlayPauseBtn.Content = "▶";
        DetailSeek.Value           = 0;
        DetailTime.Text            = "0:00 / 0:00";
        DetailPoster.Visibility    = Visibility.Visible;
    }

    // ── MediaElement events ────────────────────────────────────────────────

    private void VideoPlayer_MediaOpened(object sender, RoutedEventArgs e)
    {
        if (VideoPlayer.NaturalDuration.HasTimeSpan)
        {
            DetailSeek.Maximum = VideoPlayer.NaturalDuration.TimeSpan.TotalSeconds;
            DetailTime.Text    = $"0:00 / {Fmt(VideoPlayer.NaturalDuration.TimeSpan)}";
        }
        VideoPlayer.SpeedRatio     = Speeds[_speedIdx];
        _playing                   = false;
        DetailPlayPauseBtn.Content = "▶";
    }

    private void VideoPlayer_MediaEnded(object sender, RoutedEventArgs e)
    {
        _timer.Stop();
        VideoPlayer.Stop();
        _playing                   = false;
        DetailPlayPauseBtn.Content = "▶";
        DetailSeek.Value           = 0;
        DetailPoster.Visibility    = Visibility.Visible;
    }

    // ── Timer tick ─────────────────────────────────────────────────────────

    private void TimerTick(object? sender, EventArgs e)
    {
        if (!VideoPlayer.NaturalDuration.HasTimeSpan || _dragging) return;
        var pos = VideoPlayer.Position;
        var dur = VideoPlayer.NaturalDuration.TimeSpan;
        DetailSeek.Value = pos.TotalSeconds;
        DetailTime.Text  = $"{Fmt(pos)} / {Fmt(dur)}";
    }

    private static string Fmt(TimeSpan t)
        => t.TotalHours >= 1 ? t.ToString(@"h\:mm\:ss") : t.ToString(@"m\:ss");

    // ── Seek slider ────────────────────────────────────────────────────────

    private void DetailSeek_DragStart(object sender, MouseButtonEventArgs e)
        => _dragging = true;

    private void DetailSeek_DragEnd(object sender, MouseButtonEventArgs e)
    {
        _dragging = false;
        if (VideoPlayer.NaturalDuration.HasTimeSpan)
            VideoPlayer.Position = TimeSpan.FromSeconds(DetailSeek.Value);
    }

    // ── Control buttons ────────────────────────────────────────────────────

    private void DetailPlayPause_Click(object sender, RoutedEventArgs e)
    {
        if (_playing)
        {
            VideoPlayer.Pause();
            _playing                   = false;
            DetailPlayPauseBtn.Content = "▶";
            _timer.Stop();
        }
        else
        {
            StartPlayback();
        }
    }

    private void DetailPoster_Click(object sender, MouseButtonEventArgs e)
    {
        if (VideoPlayer.Source != null) StartPlayback();
    }

    private void StartPlayback()
    {
        DetailPoster.Visibility    = Visibility.Collapsed;
        VideoPlayer.Play();
        _playing                   = true;
        DetailPlayPauseBtn.Content = "⏸";
        _timer.Start();
    }

    private void DetailStopBtn_Click(object sender, RoutedEventArgs e)
        => ResetPlayer();

    private void DetailSpeed_Click(object sender, RoutedEventArgs e)
    {
        _speedIdx               = (_speedIdx + 1) % Speeds.Length;
        VideoPlayer.SpeedRatio  = Speeds[_speedIdx];
        DetailSpeedBtn.Content  = $"{Speeds[_speedIdx]}x";
    }

    private void DetailFullscreen_Click(object sender, RoutedEventArgs e)
    {
        if (VideoPlayer.Source == null) return;
        VideoFullscreenHelper.Show(VideoPlayer, Window.GetWindow(this));
    }
}
