using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using CommunityHub.Application.Services.Tenant.Dto;
using CommunityHub.Ui.ViewModels.Tenant;

namespace CommunityHub.Ui.Views.Tenant.Pages;

public partial class IssuesPage : System.Windows.Controls.Page
{
    private readonly TenantWindow _shell;
    private readonly IssuesViewModel _vm;

    public IssuesPage(TenantWindow shell)
    {
        InitializeComponent();
        _shell = shell;
        _vm = new IssuesViewModel(shell.UserId);
        _vm.Load();

        BuildingFilter.Items.Add(new TenantBuildingDto { BuildingId = 0, DisplayName = "Sve zgrade" });
        foreach (var b in _vm.Buildings)
            BuildingFilter.Items.Add(b);
        BuildingFilter.DisplayMemberPath = "DisplayName";
        BuildingFilter.SelectedIndex = 0;

        Refresh();
        _shell.SetBreadcrumb("Prijave problema");
    }

    private void Refresh()
    {
        IssuesList.ItemsSource = _vm.Issues;
        NoIssuesText.Visibility = _vm.Issues.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        DrawStatsChart();
    }

    private void DrawStatsChart()
    {
        StatsChart.Children.Clear();
        int[] counts = [_vm.UnresolvedCount, _vm.PotentiallyResolvedCount, _vm.ResolvedCount];
        string[] labels = ["Nerešen", "Pot. rešen", "Rešen"];
        Brush[] colors =
        [
            new SolidColorBrush(Color.FromRgb(0xE7, 0x4C, 0x3C)),
            new SolidColorBrush(Color.FromRgb(0xF3, 0x9C, 0x12)),
            new SolidColorBrush(Color.FromRgb(0x27, 0xAE, 0x60))
        ];
        int maxCount = Math.Max(1, counts.Max());
        double barW = 80, maxH = 90, baseY = 100, gap = 80;
        for (int i = 0; i < 3; i++)
        {
            double x = 80 + i * (barW + gap);
            double h = Math.Max(counts[i] / (double)maxCount * maxH, 2);
            AddBar(x, baseY - h, barW, h, colors[i]);
            AddLabel(counts[i].ToString(), x + barW / 2 - 7, baseY - h - 22, FontWeights.SemiBold, 14);
            AddLabel(labels[i], x, baseY + 6, FontWeights.Normal, 13, barW);
        }
    }

    private void AddBar(double x, double y, double w, double h, Brush fill)
    {
        var r = new Rectangle { Width = w, Height = h, Fill = fill, RadiusX = 4, RadiusY = 4 };
        Canvas.SetLeft(r, x); Canvas.SetTop(r, y);
        StatsChart.Children.Add(r);
    }

    private void AddLabel(string text, double x, double y, FontWeight fw, double fs, double width = 0)
    {
        var tb = new TextBlock { Text = text, FontSize = fs, FontWeight = fw,
            Foreground = Brushes.DimGray, TextAlignment = TextAlignment.Center };
        if (width > 0) tb.Width = width;
        Canvas.SetLeft(tb, x); Canvas.SetTop(tb, y);
        StatsChart.Children.Add(tb);
    }

    private void BuildingFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (BuildingFilter.SelectedItem is not TenantBuildingDto b) return;
        _vm.ApplyFilter(b.BuildingId);
        IssuesList.ItemsSource = _vm.Issues;
        NoIssuesText.Visibility = _vm.Issues.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        DrawStatsChart();
    }

    private void ReportProblem_Click(object sender, RoutedEventArgs e)
    {
        if (_vm.Buildings.Count == 0)
        {
            MessageBox.Show("Niste član nijedne zgrade.", "Prijava problema",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        var dialog = new ReportIssueDialog(_vm.Buildings) { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() != true || dialog.Result == null) return;
        dialog.Result.TenantId = _vm.TenantId;
        _vm.Report(dialog.Result);
        _vm.Load();
        Refresh();
    }

    private void ConfirmResolved_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not long id) return;
        _vm.ConfirmResolved(id);
        Refresh();
    }

    private void DenyResolved_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not long id) return;
        _vm.DenyResolved(id);
        Refresh();
    }
}
