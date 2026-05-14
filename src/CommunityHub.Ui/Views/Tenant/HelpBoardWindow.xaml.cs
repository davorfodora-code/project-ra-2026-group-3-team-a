using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using CommunityHub.Application.Database.Repositories;
using CommunityHub.Application.Services.Tenant.Dto;
using CommunityHub.Ui.ViewModels.Tenant;

namespace CommunityHub.Ui.Views.Tenant;

public partial class HelpBoardWindow : Window
{
    private readonly long _userId;
    private readonly HelpBoardViewModel _viewModel;

    private static readonly Brush[] CategoryColors =
    [
        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E74C3C")),
        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3498DB")),
        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2ECC71")),
        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F39C12")),
        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9B59B6")),
        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1ABC9C")),
    ];

    public HelpBoardWindow(long userId)
    {
        InitializeComponent();
        _userId = userId;

        var user = new UserDbRepository().GetById(userId);
        UserInitialsBlock.Text = ComputeInitials(user?.Name, user?.Surname);

        _viewModel = new HelpBoardViewModel(userId);

        if (!_viewModel.IsInBuilding())
        {
            MessageBox.Show(
                "Niste član nijedne zgrade. Molimo podnesite zahtev za pristup zgradi.",
                "Oglasna tabla", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        SetupBuildingSelector();
        RefreshAll();
    }

    private void SetupBuildingSelector()
    {
        BuildingCombo.ItemsSource = _viewModel.Buildings;
        BuildingCombo.SelectedIndex = 0;
    }

    private void RefreshAll()
    {
        _viewModel.LoadAds();
        RefreshBoardAds();
        RefreshMyAds();
        RefreshCategoryChart();
    }

    private void RefreshBoardAds()
    {
        var ads = _viewModel.Ads.ToList();
        BoardAdsPanel.ItemsSource = ads;
        NoBoardAdsText.Visibility = ads.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void RefreshMyAds()
    {
        var myAds = _viewModel.Ads.Where(a => a.IsOwn).ToList();
        MyAdsPanel.ItemsSource = myAds;
        NoMyAdsText.Visibility = myAds.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void RefreshCategoryChart()
    {
        var groups = _viewModel.Ads
            .GroupBy(a => a.Category)
            .Select((g, i) => new CategoryStat
            {
                Category = g.Key,
                Count = g.Count(),
                Color = CategoryColors[i % CategoryColors.Length]
            })
            .OrderByDescending(x => x.Count)
            .ToList();

        CategoryLegend.ItemsSource = groups;
        DrawPieChart(groups);
    }

    private void DrawPieChart(List<CategoryStat> stats)
    {
        PieChartCanvas.Children.Clear();
        int total = stats.Sum(s => s.Count);
        if (total == 0) return;

        double cx = 60, cy = 60, r = 54;
        double startDeg = -90;

        foreach (var stat in stats)
        {
            double sweep = (stat.Count / (double)total) * 360;
            PieChartCanvas.Children.Add(CreateSlice(cx, cy, r, startDeg, sweep, stat.Color));
            startDeg += sweep;
        }
    }

    private static Path CreateSlice(double cx, double cy, double r, double startDeg, double sweepDeg, Brush fill)
    {
        double ToRad(double deg) => deg * Math.PI / 180;
        var start = new System.Windows.Point(cx + r * Math.Cos(ToRad(startDeg)), cy + r * Math.Sin(ToRad(startDeg)));
        var end = new System.Windows.Point(cx + r * Math.Cos(ToRad(startDeg + sweepDeg)), cy + r * Math.Sin(ToRad(startDeg + sweepDeg)));

        var figure = new PathFigure { StartPoint = new System.Windows.Point(cx, cy) };
        figure.Segments.Add(new LineSegment(start, true));
        figure.Segments.Add(new ArcSegment(end, new System.Windows.Size(r, r), 0, sweepDeg > 180, SweepDirection.Clockwise, true));
        figure.IsClosed = true;

        var geo = new PathGeometry();
        geo.Figures.Add(figure);
        return new Path { Data = geo, Fill = fill, Stroke = Brushes.White, StrokeThickness = 1.5 };
    }

    // ── Tab logika ────────────────────────────────────────────────────────────

    private void TabBoard_Click(object sender, RoutedEventArgs e)
    {
        TabBoardBtn.Tag = "active";
        TabMyPostsBtn.Tag = null;
        TabBoardText.FontWeight = FontWeights.SemiBold;
        TabBoardText.Foreground = Brushes.Black;
        TabMyPostsText.FontWeight = FontWeights.Normal;
        TabMyPostsText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#888888"));
        BoardPanel.Visibility = Visibility.Visible;
        MyPostsPanel.Visibility = Visibility.Collapsed;
    }

    private void TabMyPosts_Click(object sender, RoutedEventArgs e)
    {
        TabMyPostsBtn.Tag = "active";
        TabBoardBtn.Tag = null;
        TabMyPostsText.FontWeight = FontWeights.SemiBold;
        TabMyPostsText.Foreground = Brushes.Black;
        TabBoardText.FontWeight = FontWeights.Normal;
        TabBoardText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#888888"));
        MyPostsPanel.Visibility = Visibility.Visible;
        BoardPanel.Visibility = Visibility.Collapsed;
    }

    // ── Navigacija ────────────────────────────────────────────────────────────

    private void NavHome_Click(object sender, RoutedEventArgs e)
    {
        new TenantHomeWindow(_userId).Show();
        Close();
    }

    private void NavBuildings_Click(object sender, RoutedEventArgs e)
    {
        new BuildingSearchWindow(_userId).Show();
        Close();
    }

    private void Avatar_Click(object sender, MouseButtonEventArgs e)
    {
        new ProfileWindow(_userId).Show();
    }

    // ── Akcije ────────────────────────────────────────────────────────────────

    private void BuildingCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (BuildingCombo.SelectedItem is TenantBuildingDto selected)
        {
            _viewModel.SelectBuilding(selected.BuildingId);
            RefreshAll();
        }
    }

    private void CreateAdButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_viewModel.IsInBuilding() || _viewModel.SelectedBuildingId == null)
        {
            MessageBox.Show("Niste član nijedne zgrade.", "Greška", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var buildingName = (BuildingCombo.SelectedItem as TenantBuildingDto)?.DisplayName ?? string.Empty;
        var form = new CreateAdvertisementForm(_userId, _viewModel.SelectedBuildingId.Value, buildingName);
        form.Owner = this;

        if (form.ShowDialog() == true && form.Dto != null)
        {
            var (success, matchingAds) = _viewModel.CreateAd(form.Dto);
            if (success)
            {
                RefreshAll();
                ShowMatchingAds(matchingAds);
            }
        }
    }

    private void ShowMatchingAds(List<AdvertisementDto> matchingAds)
    {
        if (matchingAds.Count == 0)
        {
            MessageBox.Show("Oglas je kreiran uspešno!\n\nNisu pronađeni podudarni oglasi.",
                "Oglas kreiran", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var matchWin = new MatchingAdsWindow(matchingAds);
        matchWin.Owner = this;
        matchWin.ShowDialog();
    }

    private void ArchiveButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag != null)
        {
            long adId = Convert.ToInt64(btn.Tag);
            var result = MessageBox.Show("Da li ste sigurni da želite da arhivirate oglas?",
                "Potvrda", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _viewModel.ArchiveAd(adId);
                RefreshAll();
            }
        }
    }

    private static string ComputeInitials(string? name, string? surname)
    {
        char n = !string.IsNullOrEmpty(name) ? char.ToUpper(name[0]) : '?';
        char s = !string.IsNullOrEmpty(surname) ? char.ToUpper(surname[0]) : '?';
        return $"{n}{s}";
    }
}

public class CategoryStat
{
    public string Category { get; set; } = string.Empty;
    public int Count { get; set; }
    public Brush Color { get; set; } = Brushes.Gray;
}
