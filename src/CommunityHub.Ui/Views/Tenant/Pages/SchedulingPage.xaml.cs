using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using CommunityHub.Ui.ViewModels.Tenant;

namespace CommunityHub.Ui.Views.Tenant.Pages;

public partial class SchedulingPage : System.Windows.Controls.Page
{
    private readonly SchedulingViewModel _viewModel;

    private static readonly Brush DarkBrush  = new SolidColorBrush(Color.FromRgb(0x4A, 0x5D, 0x6E));
    private static readonly Brush LightBrush = new SolidColorBrush(Color.FromRgb(0xBD, 0xC3, 0xC7));

    public SchedulingPage(TenantWindow shell)
    {
        InitializeComponent();
        _viewModel = new SchedulingViewModel(shell.UserId);
        _viewModel.Load();
        Refresh();
    }

    private void Refresh()
    {
        RefreshMyBookings();
        RefreshBookedOnPost();
    }

    private void RefreshMyBookings()
    {
        WeekLabelText.Text = _viewModel.WeekLabel;
        DrawWeeklyChart(_viewModel.WeekStats);

        var bookings = _viewModel.MyBookings;
        MyBookingsListPanel.ItemsSource = bookings;
        NoMyBookingsText.Visibility = bookings.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void RefreshBookedOnPost()
    {
        var posts = _viewModel.BookedOnMyPosts;
        BookedOnPostListPanel.ItemsSource = posts;
        NoBookedText.Visibility = posts.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void DrawWeeklyChart(DayScheduleStat[] stats)
    {
        WeeklyChartCanvas.Children.Clear();
        if (stats.Length == 0) return;

        const double barW    = 26;
        const double barGap  = 5;
        const double dayW    = 90;
        const double maxH    = 80;
        const double baseY   = 90;
        const double leftPad = 8;

        int maxCount = Math.Max(1, stats.Max(s => Math.Max(s.OfferingCount, s.SeekingCount)));

        for (int i = 0; i < stats.Length; i++)
        {
            var stat   = stats[i];
            double dayX = leftPad + i * dayW;
            double pairW = barW * 2 + barGap;
            double barX0 = dayX + (dayW - pairW) / 2.0;

            double offerH = Math.Max((stat.OfferingCount / (double)maxCount) * maxH, 1);
            double seekH  = Math.Max((stat.SeekingCount  / (double)maxCount) * maxH, 1);

            // Offering bar (dark, left)
            AddBar(barX0,          baseY - offerH, barW, offerH, DarkBrush);
            AddCount(barX0,          baseY - offerH - 18, barW, stat.OfferingCount);

            // Seeking bar (light, right)
            AddBar(barX0 + barW + barGap, baseY - seekH, barW, seekH, LightBrush);
            AddCount(barX0 + barW + barGap, baseY - seekH - 18, barW, stat.SeekingCount);

            // Day label
            var dayLbl = new TextBlock
            {
                Text       = stat.DayLabel,
                FontSize   = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(0x44, 0x44, 0x44)),
            };
            Canvas.SetLeft(dayLbl, dayX + (dayW - 28) / 2.0);
            Canvas.SetTop(dayLbl, baseY + 6);
            WeeklyChartCanvas.Children.Add(dayLbl);
        }
    }

    private void AddBar(double x, double y, double w, double h, Brush fill)
    {
        var rect = new Rectangle
        {
            Width   = w,
            Height  = h,
            Fill    = fill,
            RadiusX = 3,
            RadiusY = 3,
        };
        Canvas.SetLeft(rect, x);
        Canvas.SetTop(rect, y);
        WeeklyChartCanvas.Children.Add(rect);
    }

    private void AddCount(double barX, double y, double barW, int count)
    {
        var tb = new TextBlock
        {
            Text       = count.ToString(),
            FontSize   = 12,
            FontWeight = FontWeights.SemiBold,
            Foreground = Brushes.Black,
        };
        Canvas.SetLeft(tb, barX + barW / 2.0 - 5);
        Canvas.SetTop(tb, y);
        WeeklyChartCanvas.Children.Add(tb);
    }

    private void TabMyBookings_Click(object sender, RoutedEventArgs e)
    {
        TabMyBookingsBtn.Tag    = "active";
        TabBookedOnPostBtn.Tag  = null;
        TabMyBookingsText.FontWeight  = FontWeights.SemiBold;
        TabMyBookingsText.Foreground  = Brushes.Black;
        TabMyBookingsText.FontSize    = 19;
        TabBookedOnPostText.FontWeight = FontWeights.Normal;
        TabBookedOnPostText.Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88));
        TabBookedOnPostText.FontSize   = 18;
        MyBookingsPanel.Visibility  = Visibility.Visible;
        BookedOnPostPanel.Visibility = Visibility.Collapsed;
    }

    private void TabBookedOnPost_Click(object sender, RoutedEventArgs e)
    {
        TabBookedOnPostBtn.Tag  = "active";
        TabMyBookingsBtn.Tag    = null;
        TabBookedOnPostText.FontWeight = FontWeights.SemiBold;
        TabBookedOnPostText.Foreground = Brushes.Black;
        TabBookedOnPostText.FontSize   = 19;
        TabMyBookingsText.FontWeight  = FontWeights.Normal;
        TabMyBookingsText.Foreground  = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88));
        TabMyBookingsText.FontSize    = 18;
        BookedOnPostPanel.Visibility  = Visibility.Visible;
        MyBookingsPanel.Visibility   = Visibility.Collapsed;
    }
}
