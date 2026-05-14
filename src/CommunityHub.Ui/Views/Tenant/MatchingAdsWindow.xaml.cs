using System.Windows;
using CommunityHub.Application.Services.Tenant.Dto;

namespace CommunityHub.Ui.Views.Tenant;

public partial class MatchingAdsWindow : Window
{
    public MatchingAdsWindow(List<AdvertisementDto> matchingAds)
    {
        InitializeComponent();
        MatchingGrid.ItemsSource = matchingAds;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
