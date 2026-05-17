using System.Windows;
using CommunityHub.Application.Dto.Citizen;
using CommunityHub.Ui.ViewModels.Citizen;

namespace CommunityHub.Ui.Views.Citizen;

public partial class MyDistrictWindow : Window
{
    private readonly long _userId;
    private readonly MyDistrictViewModel _viewModel;

    public MyDistrictWindow(long userId)
    {
        InitializeComponent();
        _userId = userId;
        _viewModel = new MyDistrictViewModel(userId);
        LoadData();
    }

    private void LoadData()
    {
        DistrictListItemDto? district = _viewModel.GetMyDistrict();
        if (district == null)
        {
            MessageBox.Show("Niste član nijednog kvarta.", "Informacija", MessageBoxButton.OK, MessageBoxImage.Information);
            new CitizenHomeWindow(_userId).Show();
            Close();
            return;
        }

        DistrictNameTextBlock.Text = $"Kvart {district.Name}";
        DistrictDescriptionTextBlock.Text = district.Description;
        DistrictLocationTextBlock.Text = district.LocationDisplay;

        CitizenTrustRecordDto? myTrustRecord = _viewModel.GetMyTrustRecord();
        MyJoinDateTextBlock.Text = myTrustRecord == null ? "-" : myTrustRecord.JoinDate.ToString("dd.MM.yyyy.");

        StreetsListBox.ItemsSource = ParseStreetRows(district.StreetsDisplay);
        DistrictImagesListBox.ItemsSource = district.ImageUrls.Count == 0 ? new List<string> { "Nema dostupnih slika." } : district.ImageUrls;
        TrustRecordsDataGrid.ItemsSource = _viewModel.GetDistrictTrustRows();
    }

    private static List<string> ParseStreetRows(string streetsDisplay)
    {
        if (string.IsNullOrWhiteSpace(streetsDisplay) || streetsDisplay == "-")
        {
            return new List<string> { "Nema definisanih ulica." };
        }

        return streetsDisplay
            .Split(",", StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        new CitizenHomeWindow(_userId).Show();
        Close();
    }
}
