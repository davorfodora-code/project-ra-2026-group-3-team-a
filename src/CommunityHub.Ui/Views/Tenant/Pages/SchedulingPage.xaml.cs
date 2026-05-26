using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using CommunityHub.Ui.ViewModels.Tenant;

namespace CommunityHub.Ui.Views.Tenant.Pages;

public partial class SchedulingPage : System.Windows.Controls.Page
{
    private readonly TenantWindow _shell;
    private readonly SchedulingViewModel _viewModel;

    private static readonly Brush DarkBrush  = new SolidColorBrush(Color.FromRgb(0x4A, 0x5D, 0x6E));
    private static readonly Brush LightBrush = new SolidColorBrush(Color.FromRgb(0xBD, 0xC3, 0xC7));

    private const double ChartBarW    = 26;
    private const double ChartBarGap  = 5;
    private const double ChartDayW    = 90;
    private const double ChartMaxH    = 80;
    private const double ChartBaseY   = 90;
    private const double ChartLeftPad = 8;

    public SchedulingPage(TenantWindow shell)
    {
        InitializeComponent();
        _shell = shell;
        _viewModel = new SchedulingViewModel(shell.UserId);
        _viewModel.Load();
        Refresh();
        _shell.SetBreadcrumb("Moje rezervacije");
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
        int maxCount = Math.Max(1, stats.Max(s => Math.Max(s.OfferingCount, s.SeekingCount)));
        for (int i = 0; i < stats.Length; i++)
        {
            double dayX = ChartLeftPad + i * ChartDayW;
            DrawDayBars(stats[i], dayX, maxCount);
            AddDayLabel(stats[i].DayLabel, dayX);
        }
    }

    private void DrawDayBars(DayScheduleStat stat, double dayX, int maxCount)
    {
        double pairW  = ChartBarW * 2 + ChartBarGap;
        double barX0  = dayX + (ChartDayW - pairW) / 2.0;
        double offerH = Math.Max((stat.OfferingCount / (double)maxCount) * ChartMaxH, 1);
        double seekH  = Math.Max((stat.SeekingCount  / (double)maxCount) * ChartMaxH, 1);
        AddBar(barX0,                            ChartBaseY - offerH,      ChartBarW, offerH, DarkBrush);
        AddCount(barX0,                          ChartBaseY - offerH - 18, ChartBarW, stat.OfferingCount);
        AddBar(barX0 + ChartBarW + ChartBarGap,  ChartBaseY - seekH,       ChartBarW, seekH,  LightBrush);
        AddCount(barX0 + ChartBarW + ChartBarGap, ChartBaseY - seekH - 18, ChartBarW, stat.SeekingCount);
    }

    private void AddDayLabel(string label, double dayX)
    {
        var dayLbl = new TextBlock
        {
            Text       = label,
            FontSize   = 13,
            Foreground = new SolidColorBrush(Color.FromRgb(0x44, 0x44, 0x44)),
        };
        Canvas.SetLeft(dayLbl, dayX + (ChartDayW - 28) / 2.0);
        Canvas.SetTop(dayLbl, ChartBaseY + 6);
        WeeklyChartCanvas.Children.Add(dayLbl);
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
        _shell.SetBreadcrumb("Moje rezervacije");
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
        _shell.SetBreadcrumb("Rezervisano na mom oglasu");
    }
}
