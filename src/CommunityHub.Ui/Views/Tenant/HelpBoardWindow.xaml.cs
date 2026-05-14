using System.Windows;
using System.Windows.Controls;
using CommunityHub.Application.Services.Tenant.Dto;
using CommunityHub.Ui.ViewModels.Tenant;

namespace CommunityHub.Ui.Views.Tenant;

public partial class HelpBoardWindow : Window
{
    private readonly long _userId;
    private readonly HelpBoardViewModel _viewModel;

    public HelpBoardWindow(long userId)
    {
        InitializeComponent();
        _userId = userId;
        _viewModel = new HelpBoardViewModel(userId);

        if (!_viewModel.IsInBuilding())
        {
            MessageBox.Show(
                "Niste član nijedne zgrade. Molimo podnesite zahtev za pristup zgradi.",
                "Oglasna tabla", MessageBoxButton.OK, MessageBoxImage.Information);
            BuildingSelectorPanel.Visibility = Visibility.Collapsed;
            return;
        }

        AdsDataGrid.ItemsSource = _viewModel.Ads;
        SetupBuildingSelector();
        _viewModel.LoadAds();
    }

    private void SetupBuildingSelector()
    {
        if (_viewModel.Buildings.Count > 1)
        {
            BuildingSelectorPanel.Visibility = Visibility.Visible;
            BuildingCombo.ItemsSource = _viewModel.Buildings;
            BuildingCombo.SelectedIndex = 0;
        }
        else
        {
            BuildingSelectorPanel.Visibility = Visibility.Collapsed;
        }
    }

    private void RefreshAds() => _viewModel.LoadAds();

    private void BuildingCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (BuildingCombo.SelectedItem is TenantBuildingDto selected)
        {
            _viewModel.SelectBuilding(selected.BuildingId);
            RefreshAds();
        }
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        new TenantHomeWindow(_userId).Show();
        Close();
    }

    private void CreateAdButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_viewModel.IsInBuilding() || _viewModel.SelectedBuildingId == null)
        {
            MessageBox.Show("Niste član nijedne zgrade.", "Greška", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        CreateAdvertisementForm form = new CreateAdvertisementForm(_userId, _viewModel.SelectedBuildingId.Value);
        form.Owner = this;
        if (form.ShowDialog() == true && form.Dto != null)
        {
            var (success, matchingAds) = _viewModel.CreateAd(form.Dto);
            if (success)
            {
                RefreshAds();
                ShowMatchingAds(matchingAds);
            }
        }
    }

    private void ShowMatchingAds(List<AdvertisementDto> matchingAds)
    {
        if (matchingAds.Count == 0)
        {
            MessageBox.Show(
                "Oglas je kreiran uspešno!\n\nNisu pronađeni podudarni oglasi.",
                "Oglas kreiran", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        MatchingAdsWindow matchingWindow = new MatchingAdsWindow(matchingAds);
        matchingWindow.Owner = this;
        matchingWindow.ShowDialog();
    }

    private void ArchiveButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag != null)
        {
            long adId = Convert.ToInt64(btn.Tag);
            var result = MessageBox.Show(
                "Da li ste sigurni da želite da arhivirate oglas?",
                "Potvrda arhiviranja", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _viewModel.ArchiveAd(adId);
                RefreshAds();
            }
        }
    }
}
