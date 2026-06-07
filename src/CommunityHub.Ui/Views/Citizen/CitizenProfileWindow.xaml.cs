using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using CommunityHub.Application;
using CommunityHub.Application.Database.Repositories;
using CommunityHub.Application.Domain;
using CommunityHub.Application.Dto.Citizen;
using CommunityHub.Application.Services.Citizen;
using CommunityHub.Ui.ViewModels.Citizen;

namespace CommunityHub.Ui.Views.Citizen;

public partial class CitizenProfileWindow : Page
{
    private readonly long _userId;
    private readonly UserDbRepository _userRepository;
    private readonly ICitizenTrustRecordService _trustRecordService;
    private readonly MyDistrictViewModel _myDistrictViewModel;

    public CitizenProfileWindow(long userId)
    {
        InitializeComponent();
        _userId = userId;
        _userRepository = new UserDbRepository();
        _trustRecordService = Injector.Get<ICitizenTrustRecordService>();
        _myDistrictViewModel = new MyDistrictViewModel(userId);
        LoadProfile();
    }

    private void LoadProfile()
    {
        User? user = _userRepository.GetWithPosts(_userId);
        if (user == null)
        {
            MessageBox.Show("Korisnik nije pronađen.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            CitizenShellNavigation.Navigate(this, new CitizenHomeWindow(_userId));
            return;
        }

        UserFullNameTextBlock.Text = $"{user.Name} {user.Surname}";
        UsernameTextBlock.Text = user.Username;
        BirthDateTextBlock.Text = user.BirthDay.ToString("dd.MM.yyyy.");
        AddressTextBlock.Text = string.IsNullOrWhiteSpace(user.Address) ? "-" : user.Address;
        RoleTextBlock.Text = "Građanin";

        List<Post> posts = user.Posts ?? [];
        PostsItemsControl.ItemsSource = new ObservableCollection<Post>(posts);
        NoPostsTextBlock.Visibility = posts.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

        LoadTrustRecord();
    }

    private void LoadTrustRecord()
    {
        CitizenTrustRecordDto? trustRecord = _trustRecordService.GetMyTrustRecord(_userId);
        if (trustRecord == null)
        {
            DistrictTextBlock.Text = "-";
            TrustJoinDateTextBlock.Text = "-";
            TrustOrganizedCountTextBlock.Text = "-";
            TrustVolunteerCountTextBlock.Text = "-";
            TrustLevelTextBlock.Text = "Niste član kvarta";
            TrustInfoTextBlock.Text = "Evidencija poverenja postaje dostupna nakon učlanjenja u kvart.";
            NonMemberNavPanel.Visibility = Visibility.Visible;
            MemberNavPanel.Visibility = Visibility.Collapsed;
            return;
        }

        DistrictListItemDto? district = _myDistrictViewModel.GetMyDistrict();
        DistrictTextBlock.Text = district == null ? "-" : district.Name;
        TrustJoinDateTextBlock.Text = trustRecord.JoinDate.ToString("dd.MM.yyyy.");
        TrustOrganizedCountTextBlock.Text = trustRecord.OrganizedCompletedEventsCount.ToString();
        TrustVolunteerCountTextBlock.Text = trustRecord.ConfirmedVolunteerEventsCount.ToString();
        TrustLevelTextBlock.Text = MyDistrictViewModel.FormatTrustLevel(trustRecord.TrustLevel);
        TrustInfoTextBlock.Text = string.Empty;
        NonMemberNavPanel.Visibility = Visibility.Collapsed;
        MemberNavPanel.Visibility = Visibility.Visible;
    }

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        CitizenShellNavigation.Logout(this);
    }

    private void HomeNavButton_Click(object sender, RoutedEventArgs e)
    {
        CitizenShellNavigation.Navigate(this, new CitizenHomeWindow(_userId));
    }

    private void SearchNavButton_Click(object sender, RoutedEventArgs e)
    {
        CitizenShellNavigation.Navigate(this, new DistrictSearchWindow(_userId));
    }

    private void RequestsNavButton_Click(object sender, RoutedEventArgs e)
    {
        CitizenShellNavigation.Navigate(this, new MyDistrictRequestsWindow(_userId));
    }

    private void MyDistrictNavButton_Click(object sender, RoutedEventArgs e)
    {
        CitizenShellNavigation.Navigate(this, new MyDistrictWindow(_userId));
    }

    private void EventsNavButton_Click(object sender, RoutedEventArgs e)
    {
        CitizenShellNavigation.Navigate(this, new CitizenEventsWindow(_userId));
    }

    private void MeetingsNavButton_Click(object sender, RoutedEventArgs e)
    {
        CitizenShellNavigation.Navigate(this, new CitizenMeetingsWindow(_userId));
    }

    private static void ShowUnavailableMessage(string moduleName)
    {
        MessageBox.Show(
            $"{moduleName} će biti dostupno kada taj deo backend-a bude završen.",
            "Trenutno nije dostupno",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void ObjectsNotImplementedButton_Click(object sender, RoutedEventArgs e)
    {
        CitizenShellNavigation.Navigate(this, new CitizenCityFacilitiesWindow(_userId));    
    }

    private void ReportsNotImplementedButton_Click(object sender, RoutedEventArgs e) => ShowUnavailableMessage("Izveštaji");
    private void ReviewsNotImplementedButton_Click(object sender, RoutedEventArgs e)
    {
        CitizenShellNavigation.Navigate(this, new CitizenCoordinatorReviewsWindow(_userId));
    }
    private void DonationsNotImplementedButton_Click(object sender, RoutedEventArgs e) => ShowUnavailableMessage("Donacije");
}
