using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CommunityHub.Application.Services.Tenant.Dto;
using CommunityHub.Ui.ViewModels.Tenant;

namespace CommunityHub.Ui.Views.Tenant.Pages;

public partial class HelpBoardPage : System.Windows.Controls.Page
{
    private readonly TenantWindow _shell;
    private readonly long _userId;
    private readonly HelpBoardViewModel _viewModel;

    public HelpBoardPage(TenantWindow shell)
    {
        InitializeComponent();
        _shell = shell;
        _userId = shell.UserId;

        _viewModel = new HelpBoardViewModel(_userId);

        if (!_viewModel.IsInBuilding())
        {
            MessageBox.Show(
                "Niste član nijedne zgrade. Molimo podnesite zahtev za pristup zgradi.",
                "Oglasna tabla", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        BuildingCombo.ItemsSource = _viewModel.Buildings;
        BuildingCombo.SelectedIndex = 0;
        RefreshAll();
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
            .Select((g, i) => new CategoryStat(g.Key, g.Count(), PieChartHelper.CategoryColors[i % PieChartHelper.CategoryColors.Length]))
            .OrderByDescending(x => x.Count)
            .ToList();
        CategoryLegend.ItemsSource = groups;
        PieChartHelper.Draw(PieChartCanvas, groups);
    }

    private void TabBoard_Click(object sender, RoutedEventArgs e) =>
        SwitchTab(TabBoardBtn, TabBoardText, BoardPanel, TabMyPostsBtn, TabMyPostsText, MyPostsPanel);

    private void TabMyPosts_Click(object sender, RoutedEventArgs e) =>
        SwitchTab(TabMyPostsBtn, TabMyPostsText, MyPostsPanel, TabBoardBtn, TabBoardText, BoardPanel);

    private static void SwitchTab(
        Button activeBtn, TextBlock activeText, UIElement showPanel,
        Button inactiveBtn, TextBlock inactiveText, UIElement hidePanel)
    {
        activeBtn.Tag   = "active";
        inactiveBtn.Tag = null;
        activeText.FontWeight   = FontWeights.SemiBold;
        activeText.Foreground   = Brushes.Black;
        inactiveText.FontWeight = FontWeights.Normal;
        inactiveText.Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88));
        showPanel.Visibility = Visibility.Visible;
        hidePanel.Visibility = Visibility.Collapsed;
    }

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
        form.Owner = System.Windows.Window.GetWindow(this);

        if (form.ShowDialog() == true && form.Dto != null)
        {
            var dto = form.Dto;
            var (success, newAdId, matchingAds) = _viewModel.CreateAd(dto);
            if (!success) return;
            RefreshAll();
            if (matchingAds.Count == 0)
            {
                MessageBox.Show("Oglas je kreiran uspešno!\n\nNisu pronađeni podudarni oglasi.",
                    "Oglas kreiran", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var matchWin = new MatchingAdsWindow(matchingAds, _viewModel, newAdId, dto.StartDate, dto.EndDate);
            matchWin.Owner = Window.GetWindow(this);
            matchWin.ShowDialog();
        }
    }

    private void ArchiveButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag == null) return;
        long adId = Convert.ToInt64(btn.Tag);
        if (MessageBox.Show("Da li ste sigurni da želite da arhivirate oglas?",
            "Potvrda", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
        _viewModel.ArchiveAd(adId);
        RefreshAll();
    }
}
