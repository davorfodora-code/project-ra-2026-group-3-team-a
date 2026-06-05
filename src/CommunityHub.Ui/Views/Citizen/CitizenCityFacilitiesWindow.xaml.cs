using System.Windows;
using System.Windows.Controls;
using CommunityHub.Application.Dto.Citizen;
using CommunityHub.Ui.ViewModels.Citizen;

namespace CommunityHub.Ui.Views.Citizen;

public partial class CitizenCityFacilitiesWindow : Page
{
    private readonly long _userId;
    private readonly CitizenCityFacilitiesViewModel _viewModel;

    public CitizenCityFacilitiesWindow(long userId)
    {
        InitializeComponent();
        _userId = userId;
        _viewModel = new CitizenCityFacilitiesViewModel(userId);

        YearTextBox.Text = DateTime.Now.Year.ToString();
        MonthComboBox.SelectedIndex = 0;

        LoadFacilities();
        LoadStatistics();
    }

    private void LoadFacilities()
    {
        List<CityFacilityListItemDto> facilities = _viewModel.LoadFacilities();
        FacilitiesDataGrid.ItemsSource = facilities;
    }

    private void LoadStatistics()
    {
        if (!int.TryParse(YearTextBox.Text, out int year))
        {
            MessageBox.Show("Unesite ispravnu godinu.", "Upozorenje", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        int? month = null;
        if (MonthComboBox.SelectedItem is ComboBoxItem item &&
            item.Tag is string tag &&
            int.TryParse(tag, out int parsedMonth))
        {
            month = parsedMonth;
        }

        CityFacilityStatisticsDto statistics = _viewModel.LoadStatistics(year, month);
        TotalReservationsTextBlock.Text = $"Ukupan broj rezervacija: {statistics.TotalReservations}";
        StatisticsDataGrid.ItemsSource = statistics.FacilityStats;
    }

    private void LoadHistory(long facilityId)
    {
        HistoryDataGrid.ItemsSource = _viewModel.LoadHistory(facilityId);
    }

    private void FacilitiesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (FacilitiesDataGrid.SelectedItem is not CityFacilityListItemDto selected)
        {
            SelectedFacilityTextBlock.Text = "Izaberite objekat iz tabele da biste videli istoriju obilazaka.";
            HistoryDataGrid.ItemsSource = null;
            return;
        }

        SelectedFacilityTextBlock.Text = $"Istorija obilazaka: {selected.Name}";
        LoadHistory(selected.Id);
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        LoadFacilities();
        LoadStatistics();
    }

    private void VoteButton_Click(object sender, RoutedEventArgs e)
    {
        if (FacilitiesDataGrid.SelectedItem is not CityFacilityListItemDto selected)
        {
            MessageBox.Show("Izaberite gradski objekat.", "Upozorenje", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        CityFacilityActionResultDto result = _viewModel.Vote(selected.Id);
        MessageBox.Show(
            result.Message,
            result.Success ? "Uspeh" : "Greška",
            MessageBoxButton.OK,
            result.Success ? MessageBoxImage.Information : MessageBoxImage.Error);

        if (result.Success)
        {
            LoadFacilities();
        }
    }

    private void StatisticsButton_Click(object sender, RoutedEventArgs e)
    {
        LoadStatistics();
    }

    private void HistoryButton_Click(object sender, RoutedEventArgs e)
    {
        if (FacilitiesDataGrid.SelectedItem is not CityFacilityListItemDto selected)
        {
            MessageBox.Show("Izaberite gradski objekat.", "Upozorenje", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        LoadHistory(selected.Id);
    }
}