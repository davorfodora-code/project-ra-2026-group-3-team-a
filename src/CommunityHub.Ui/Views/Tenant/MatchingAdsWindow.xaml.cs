using System.Windows;
using System.Windows.Controls;
using CommunityHub.Application.Services.Tenant.Dto;
using CommunityHub.Ui.ViewModels.Tenant;

namespace CommunityHub.Ui.Views.Tenant;

public partial class MatchingAdsWindow : Window
{
    private readonly HelpBoardViewModel _viewModel;
    private readonly long _newAdId;

    public MatchingAdsWindow(List<AdvertisementDto> matchingAds, HelpBoardViewModel viewModel, long newAdId)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _newAdId = newAdId;
        AdsListBox.ItemsSource = matchingAds;
    }

    private void AdsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (AdsListBox.SelectedItem is not AdvertisementDto selected)
            return;

        var slots = _viewModel.GetAvailableSlots(selected.Id);

        if (slots.Count == 0)
        {
            SlotsListBox.Visibility = Visibility.Collapsed;
            NoSlotsText.Text = "Nema slobodnih termina za ovaj oglas.";
            NoSlotsText.Visibility = Visibility.Visible;
            ReserveButton.IsEnabled = false;
        }
        else
        {
            NoSlotsText.Visibility = Visibility.Collapsed;
            SlotsListBox.ItemsSource = slots;
            SlotsListBox.SelectedIndex = -1;
            SlotsListBox.Visibility = Visibility.Visible;
            ReserveButton.IsEnabled = false;
        }
    }

    private void SlotsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ReserveButton.IsEnabled = SlotsListBox.SelectedItem is SlotDto;
    }

    private void ReserveButton_Click(object sender, RoutedEventArgs e)
    {
        if (SlotsListBox.SelectedItem is not SlotDto slot) return;

        try
        {
            _viewModel.ReserveSlot(slot.Id, _newAdId);
            MessageBox.Show($"Termin {slot.DisplayText} je uspešno rezervisan.",
                "Rezervacija potvrđena", MessageBoxButton.OK, MessageBoxImage.Information);
            Close();
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Termin nije dostupan", MessageBoxButton.OK, MessageBoxImage.Warning);
            AdsListBox_SelectionChanged(AdsListBox, null!);
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
