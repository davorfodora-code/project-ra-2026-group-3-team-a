using System.Windows;
using System.Windows.Controls;
using CommunityHub.Application.Dto.Citizen;
using CommunityHub.Ui.ViewModels.Citizen;

namespace CommunityHub.Ui.Views.Citizen;

public partial class MyDistrictWindow : Page
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
            DistrictNameTextBlock.Text = "Moj kvart";
            DistrictDescriptionTextBlock.Text = "Niste član nijednog kvarta.";
            DistrictLocationTextBlock.Text = "-";
            MyJoinDateTextBlock.Text = "-";
            StreetsListBox.ItemsSource = new List<string> { "Nema dostupnih podataka." };
            SetDistrictImages(Array.Empty<string>());
            TrustRecordsDataGrid.ItemsSource = new List<MyDistrictTrustRowViewModel>();
            return;
        }

        DistrictNameTextBlock.Text = $"Kvart {district.Name}";
        DistrictDescriptionTextBlock.Text = district.Description;
        DistrictLocationTextBlock.Text = district.LocationDisplay;

        CitizenTrustRecordDto? myTrustRecord = _viewModel.GetMyTrustRecord();
        MyJoinDateTextBlock.Text = myTrustRecord == null ? "-" : myTrustRecord.JoinDate.ToString("dd.MM.yyyy.");

        StreetsListBox.ItemsSource = ParseStreetRows(district.StreetsDisplay);
        SetDistrictImages(district.ImageUrls);
        TrustRecordsDataGrid.ItemsSource = _viewModel.GetDistrictTrustRows();
    }

    private void SetDistrictImages(IReadOnlyList<string> imageUrls)
    {
        if (imageUrls.Count == 0)
        {
            DistrictImagesListBox.ItemsSource = null;
            DistrictImagesListBox.Visibility = Visibility.Collapsed;
            NoDistrictImagesTextBlock.Visibility = Visibility.Visible;
            return;
        }

        DistrictImagesListBox.ItemsSource = imageUrls;
        DistrictImagesListBox.Visibility = Visibility.Visible;
        NoDistrictImagesTextBlock.Visibility = Visibility.Collapsed;
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

    private void HomeNavButton_Click(object sender, RoutedEventArgs e)
    {
        CitizenShellNavigation.Navigate(this, new CitizenHomeWindow(_userId));
    }

    private void EventsNavButton_Click(object sender, RoutedEventArgs e)
    {
        CitizenShellNavigation.Navigate(this, new CitizenEventsWindow(_userId));
    }
}
