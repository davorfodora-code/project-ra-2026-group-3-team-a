using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace CommunityHub.Ui.Views.DistrictCoordinator;

/// <summary>
/// Prebacuje postojeći (već pokrenuti) MediaElement u fullscreen prozor sa punim
/// kontrolama (play/pauza, premotavanje, brzina, izlaz) i vraća ga na originalno
/// mjesto. Time se izbjegava crni ekran koji nastaje pri kreiranju novog
/// MediaElement-a u novom prozoru.
/// </summary>
internal static class VideoFullscreenHelper
{
    private static readonly double[] Speeds = [0.5, 1.0, 1.5, 2.0];

    public static void Show(MediaElement player, Window? owner)
    {
        if (player.Parent is not Grid originalParent) return;

        int originalIndex = originalParent.Children.IndexOf(player);
        var savedUnloaded = player.UnloadedBehavior;
        var savedStretch  = player.Stretch;

        // Spriječi da uklanjanje iz vizuelnog stabla zaustavi reprodukciju
        player.UnloadedBehavior = MediaState.Manual;

        bool dragging = false;
        bool playing  = true;
        int  speedIdx = Array.IndexOf(Speeds, player.SpeedRatio);
        if (speedIdx < 0) speedIdx = 1;

        // ── Kontrole ────────────────────────────────────────────────────────
        var seek = new Slider
        {
            Minimum              = 0,
            Maximum              = 1,
            Value                = 0,
            IsMoveToPointEnabled = true,
            VerticalAlignment    = VerticalAlignment.Center
        };

        var timeText = new TextBlock
        {
            Text              = "0:00 / 0:00",
            Foreground        = Brushes.White,
            FontSize          = 13,
            Opacity           = 0.9,
            VerticalAlignment = VerticalAlignment.Center,
            Margin            = new Thickness(14, 0, 0, 0),
            MinWidth          = 100
        };

        var playPauseBtn = MakeButton("⏸");
        var stopBtn      = MakeButton("■");
        var speedBtn     = MakeButton($"{Speeds[speedIdx]}x");
        var exitBtn      = MakeButton("⤢  Izađi  (Esc)");

        // Donja traka: red sa seek-om + red sa dugmadima
        var bottomBar = new Border
        {
            Background          = new SolidColorBrush(Color.FromArgb(0xCC, 0, 0, 0)),
            Padding             = new Thickness(20, 12, 20, 14),
            VerticalAlignment   = VerticalAlignment.Bottom,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        var seekRow = new Grid { Margin = new Thickness(0, 0, 0, 10) };
        seekRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        seekRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        Grid.SetColumn(seek, 0);
        Grid.SetColumn(timeText, 1);
        seekRow.Children.Add(seek);
        seekRow.Children.Add(timeText);

        var buttonRow = new StackPanel { Orientation = Orientation.Horizontal };
        buttonRow.Children.Add(playPauseBtn);
        buttonRow.Children.Add(stopBtn);
        buttonRow.Children.Add(speedBtn);
        buttonRow.Children.Add(exitBtn);

        var barStack = new StackPanel();
        barStack.Children.Add(seekRow);
        barStack.Children.Add(buttonRow);
        bottomBar.Child = barStack;

        var grid = new Grid { Background = Brushes.Black };

        var win = new Window
        {
            WindowStyle   = WindowStyle.None,
            WindowState   = WindowState.Maximized,
            ResizeMode    = ResizeMode.NoResize,
            Background    = Brushes.Black,
            Content       = grid,
            ShowInTaskbar = false
        };
        if (owner != null) win.Owner = owner;

        // Premjesti plejer u fullscreen prozor (reprodukcija se nastavlja)
        originalParent.Children.Remove(player);
        player.Stretch = Stretch.Uniform;
        grid.Children.Add(player);
        grid.Children.Add(bottomBar);

        // ── Tajmer za praćenje pozicije ──────────────────────────────────────
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
        timer.Tick += (_, _) =>
        {
            if (dragging || !player.NaturalDuration.HasTimeSpan) return;
            seek.Maximum   = player.NaturalDuration.TimeSpan.TotalSeconds;
            seek.Value     = player.Position.TotalSeconds;
            timeText.Text  = $"{Fmt(player.Position)} / {Fmt(player.NaturalDuration.TimeSpan)}";
        };
        if (player.NaturalDuration.HasTimeSpan)
            seek.Maximum = player.NaturalDuration.TimeSpan.TotalSeconds;
        timer.Start();

        // ── Događaji kontrola ────────────────────────────────────────────────
        seek.PreviewMouseLeftButtonDown += (_, _) => dragging = true;
        seek.PreviewMouseLeftButtonUp   += (_, _) =>
        {
            dragging = false;
            if (player.NaturalDuration.HasTimeSpan)
                player.Position = TimeSpan.FromSeconds(seek.Value);
        };

        playPauseBtn.Click += (_, _) =>
        {
            if (playing) { player.Pause(); playPauseBtn.Content = "▶"; }
            else         { player.Play();  playPauseBtn.Content = "⏸"; }
            playing = !playing;
        };

        stopBtn.Click += (_, _) =>
        {
            player.Pause();
            player.Position      = TimeSpan.Zero;
            playing              = false;
            playPauseBtn.Content = "▶";
            seek.Value           = 0;
            timeText.Text        = player.NaturalDuration.HasTimeSpan
                ? $"0:00 / {Fmt(player.NaturalDuration.TimeSpan)}"
                : "0:00 / 0:00";
        };

        speedBtn.Click += (_, _) =>
        {
            speedIdx          = (speedIdx + 1) % Speeds.Length;
            player.SpeedRatio = Speeds[speedIdx];
            speedBtn.Content  = $"{Speeds[speedIdx]}x";
        };

        exitBtn.Click += (_, _) => win.Close();
        win.KeyDown   += (_, e) =>
        {
            switch (e.Key)
            {
                case Key.Escape:
                case Key.F11:
                    win.Close();
                    break;
                case Key.Space:
                    playPauseBtn.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                    break;
            }
        };

        win.Closing += (_, _) =>
        {
            timer.Stop();
            // Vrati plejer na originalno mjesto
            grid.Children.Remove(player);
            player.Stretch          = savedStretch;
            player.UnloadedBehavior = savedUnloaded;
            originalParent.Children.Insert(originalIndex, player);
        };

        win.ShowDialog();
    }

    private static Button MakeButton(object content) => new()
    {
        Content         = content,
        Foreground      = Brushes.White,
        Background      = new SolidColorBrush(Color.FromArgb(0x33, 0xFF, 0xFF, 0xFF)),
        BorderThickness = new Thickness(0),
        Padding         = new Thickness(16, 7, 16, 7),
        Margin          = new Thickness(0, 0, 10, 0),
        FontSize        = 14,
        MinWidth        = 46,
        Cursor          = Cursors.Hand
    };

    private static string Fmt(TimeSpan t)
        => t.TotalHours >= 1 ? t.ToString(@"h\:mm\:ss") : t.ToString(@"m\:ss");
}
