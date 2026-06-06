using System.Windows;
using System.Windows.Controls;
using CommunityHub.Application.Dto.Citizen;
using CommunityHub.Ui.ViewModels.Citizen;

namespace CommunityHub.Ui.Views.Citizen;

public partial class CitizenMeetingsWindow : Page
{
    private readonly long _userId;
    private readonly CitizenMeetingsViewModel _viewModel;

    public CitizenMeetingsWindow(long userId)
    {
        InitializeComponent();
        _userId = userId;
        _viewModel = new CitizenMeetingsViewModel(userId);
        LoadMeetings();
    }

    private void LoadMeetings()
    {
        List<CitizenMeetingCardViewModel> cards = _viewModel.LoadMeetingCards();
        MeetingsItemsControl.ItemsSource = cards;
        bool hasMeetings = cards.Count > 0;
        EmptyMeetingsTextBlock.Visibility = hasMeetings ? Visibility.Collapsed : Visibility.Visible;
        MeetingsItemsControl.Visibility = hasMeetings ? Visibility.Visible : Visibility.Collapsed;
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        CitizenShellNavigation.Navigate(this, new CitizenHomeWindow(_userId));
    }

    private void SaveVoteButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not CitizenMeetingCardViewModel card)
        {
            return;
        }

        MeetingVoteResultDto result = _viewModel.SaveVote(card.MeetingId, card.SelectedDate);
        MessageBox.Show(result.Message, result.Success ? "Uspeh" : "Greška", MessageBoxButton.OK,
            result.Success ? MessageBoxImage.Information : MessageBoxImage.Error);

        if (result.Success)
        {
            LoadMeetings();
        }
    }

    private void HomeNavButton_Click(object sender, RoutedEventArgs e)
    {
        CitizenShellNavigation.Navigate(this, new CitizenHomeWindow(_userId));
    }

    private void MyDistrictNavButton_Click(object sender, RoutedEventArgs e)
    {
        CitizenShellNavigation.Navigate(this, new MyDistrictWindow(_userId));
    }

    private void EventsNavButton_Click(object sender, RoutedEventArgs e)
    {
        CitizenShellNavigation.Navigate(this, new CitizenEventsWindow(_userId));
    }
}
