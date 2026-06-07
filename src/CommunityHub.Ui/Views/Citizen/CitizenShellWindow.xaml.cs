using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityHub.Application.Database.Repositories;
using CommunityHub.Application.Services.Citizen;

namespace CommunityHub.Ui.Views.Citizen;

public partial class CitizenShellWindow : Window
{
    public long UserId { get; }
    private bool _hasMembership;
    private readonly CitizenMembershipService _membershipService;
    private string _activeTopAction = string.Empty;

    public CitizenShellWindow(long userId)
    {
        InitializeComponent();
        UserId = userId;
        _membershipService = new CitizenMembershipService();
        _hasMembership = _membershipService.HasMembership(userId);
        ConfigureNavigationState();
        PopulateProfileSummary();
        MainFrame.Navigate(new CitizenHomeWindow(userId));
        MainFrame.Navigated += (_, _) => OnPageNavigated(MainFrame.Content as Page);
    }

    public void Navigate(Page page)
    {
        MainFrame.Navigate(page);
    }

    public void RefreshMembershipState()
    {
        _hasMembership = _membershipService.HasMembership(UserId);
        _activeTopAction = string.Empty;
        ConfigureNavigationState();
        PopulateProfileSummary();
        Navigate(new CitizenHomeWindow(UserId));
    }

    private void ConfigureNavigationState()
    {
        NonMemberNavPanel.Visibility = _hasMembership ? Visibility.Collapsed : Visibility.Visible;
        MemberNavPanel.Visibility = _hasMembership ? Visibility.Visible : Visibility.Collapsed;
    }

    private void PopulateProfileSummary()
    {
        var user = new UserDbRepository().GetById(UserId);
        if (user == null)
        {
            ProfileNameTextBlock.Text = "Profil";
            ProfileInitialsTextBlock.Text = "?";
            return;
        }

        ProfileNameTextBlock.Text = $"{user.Name} {user.Surname}";
        char firstInitial = user.Name.Length > 0 ? char.ToUpper(user.Name[0]) : '?';
        char secondInitial = user.Surname.Length > 0 ? char.ToUpper(user.Surname[0]) : '?';
        ProfileInitialsTextBlock.Text = $"{firstInitial}{secondInitial}";
    }

    private void OnPageNavigated(Page? page)
    {
        UpdatePageTitle(page);
        UpdateSelectionStates(page);
    }

    private void UpdatePageTitle(Page? page)
    {
        PageTitleTextBlock.Text = page switch
        {
            CitizenHomeWindow => "Početna",
            DistrictSearchWindow => "Pretraga kvartova",
            MyDistrictRequestsWindow => "Moji zahtevi",
            MyDistrictWindow => "Moj kvart",
            CitizenEventsWindow => "Događaji",
            CitizenMeetingsWindow => "Sastanci",
            CitizenCityFacilitiesWindow => "Gradski objekti",
            CitizenCoordinatorReviewsWindow => "Recenzije",
            CitizenDonationsWindow => "Donacije",
            CitizenProfileWindow => "Profil",
            _ => string.Empty
        };
    }

    private void UpdateSelectionStates(Page? page)
    {
        SetButtonSelected(NmHomeButton, page is CitizenHomeWindow && !_hasMembership);
        SetButtonSelected(NmSearchButton, page is DistrictSearchWindow && !_hasMembership);
        SetButtonSelected(NmRequestsButton, page is MyDistrictRequestsWindow && !_hasMembership);

        SetButtonSelected(MHomeButton, page is CitizenHomeWindow && _hasMembership);
        SetButtonSelected(MMyDistrictButton, page is MyDistrictWindow);
        SetButtonSelected(MEventsButton, page is CitizenEventsWindow);
        SetButtonSelected(MMeetingsButton, page is CitizenMeetingsWindow);
        SetButtonSelected(MObjectsButton, page is CitizenCityFacilitiesWindow);
        SetButtonSelected(MReviewsButton, page is CitizenCoordinatorReviewsWindow);
        SetButtonSelected(MDonationsButton, page is CitizenDonationsWindow);

        if (page is CitizenProfileWindow)
        {
            _activeTopAction = "profile";
        }
        else if (_activeTopAction == "profile")
        {
            _activeTopAction = string.Empty;
        }

        SetButtonSelected(HelpTopButton, _activeTopAction == "help");
        SetButtonSelected(SettingsTopButton, _activeTopAction == "settings");
        SetButtonSelected(ProfileTopButton, _activeTopAction == "profile");
    }

    private static void SetButtonSelected(Button button, bool selected)
    {
        button.Tag = selected ? "active" : "normal";
    }

    private void HomeNavButton_Click(object sender, RoutedEventArgs e)
    {
        _activeTopAction = string.Empty;
        Navigate(new CitizenHomeWindow(UserId));
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (Keyboard.Modifiers == ModifierKeys.Control)
        {
            HandleControlShortcuts(e);
            return;
        }

        if (Keyboard.Modifiers != ModifierKeys.None)
        {
            return;
        }

        switch (e.Key)
        {
            case Key.F1:
                HelpTopButton_Click(this, new RoutedEventArgs());
                e.Handled = true;
                break;
            case Key.F2:
                SettingsTopButton_Click(this, new RoutedEventArgs());
                e.Handled = true;
                break;
            case Key.Escape:
                HandleEscapeShortcut();
                e.Handled = true;
                break;
        }
    }

    private void HandleControlShortcuts(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.D1:
            case Key.NumPad1:
                HomeNavButton_Click(this, new RoutedEventArgs());
                e.Handled = true;
                break;
            case Key.D2:
            case Key.NumPad2:
                if (_hasMembership)
                {
                    MyDistrictNavButton_Click(this, new RoutedEventArgs());
                }
                else
                {
                    SearchNavButton_Click(this, new RoutedEventArgs());
                }

                e.Handled = true;
                break;
            case Key.D3:
            case Key.NumPad3:
                if (_hasMembership)
                {
                    EventsNavButton_Click(this, new RoutedEventArgs());
                }
                else
                {
                    RequestsNavButton_Click(this, new RoutedEventArgs());
                }

                e.Handled = true;
                break;
            case Key.D4:
            case Key.NumPad4:
                if (EnsureMemberShortcutAccess("Izveštaji"))
                {
                    ReportsNotImplementedButton_Click(this, new RoutedEventArgs());
                }

                e.Handled = true;
                break;
            case Key.D5:
            case Key.NumPad5:
                if (EnsureMemberShortcutAccess("Sastanci"))
                {
                    MeetingsNavButton_Click(this, new RoutedEventArgs());
                }

                e.Handled = true;
                break;
            case Key.D6:
            case Key.NumPad6:
                if (EnsureMemberShortcutAccess("Gradski objekti"))
                {
                    ObjectsNavButton_Click(this, new RoutedEventArgs());
                }

                e.Handled = true;
                break;
            case Key.D7:
            case Key.NumPad7:
                if (EnsureMemberShortcutAccess("Recenzije"))
                {
                    ReviewsNavButton_Click(this, new RoutedEventArgs());
                }

                e.Handled = true;
                break;
            case Key.D8:
            case Key.NumPad8:
                if (EnsureMemberShortcutAccess("Donacije"))
                {
                    DonationsNavButton_Click(this, new RoutedEventArgs());
                }

                e.Handled = true;
                break;
            case Key.D9:
            case Key.NumPad9:
                ProfileTopButton_Click(this, new RoutedEventArgs());
                e.Handled = true;
                break;
            case Key.N:
                HandleCreateNewShortcut();
                e.Handled = true;
                break;
        }
    }

    private bool EnsureMemberShortcutAccess(string moduleName)
    {
        if (_hasMembership)
        {
            return true;
        }

        MessageBox.Show(
            $"{moduleName} je dostupan tek kada postanete član kvarta.",
            "Funkcionalnost nije dostupna",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
        return false;
    }

    private void HandleCreateNewShortcut()
    {
        if (MainFrame.Content is CitizenEventsWindow eventsPage)
        {
            eventsPage.OpenCreateEventDialog();
            return;
        }

        MessageBox.Show(
            "Ctrl+N je dostupan na stranici Događaji za kreiranje novog događaja.",
            "Prečica nije dostupna",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void HandleEscapeShortcut()
    {
        if (MainFrame.CanGoBack)
        {
            _activeTopAction = string.Empty;
            MainFrame.GoBack();
            return;
        }

        HomeNavButton_Click(this, new RoutedEventArgs());
    }

    private void SearchNavButton_Click(object sender, RoutedEventArgs e)
    {
        _activeTopAction = string.Empty;
        Navigate(new DistrictSearchWindow(UserId));
    }

    private void RequestsNavButton_Click(object sender, RoutedEventArgs e)
    {
        _activeTopAction = string.Empty;
        Navigate(new MyDistrictRequestsWindow(UserId));
    }

    private void MyDistrictNavButton_Click(object sender, RoutedEventArgs e)
    {
        _activeTopAction = string.Empty;
        Navigate(new MyDistrictWindow(UserId));
    }

    private void EventsNavButton_Click(object sender, RoutedEventArgs e)
    {
        _activeTopAction = string.Empty;
        Navigate(new CitizenEventsWindow(UserId));
    }

    private void MeetingsNavButton_Click(object sender, RoutedEventArgs e)
    {
        _activeTopAction = string.Empty;
        Navigate(new CitizenMeetingsWindow(UserId));
    }

    private void ProfileTopButton_Click(object sender, RoutedEventArgs e)
    {
        _activeTopAction = "profile";
        Navigate(new CitizenProfileWindow(UserId));
    }

    private void HelpTopButton_Click(object sender, RoutedEventArgs e)
    {
        _activeTopAction = "help";
        UpdateSelectionStates(MainFrame.Content as Page);
        ShowUnavailableMessage("Pomoć");
    }

    private void SettingsTopButton_Click(object sender, RoutedEventArgs e)
    {
        _activeTopAction = "settings";
        UpdateSelectionStates(MainFrame.Content as Page);
        ShowUnavailableMessage("Podešavanja");
    }

    private void ReportsNotImplementedButton_Click(object sender, RoutedEventArgs e) => ShowUnavailableMessage("Izveštaji");
    private void ObjectsNotImplementedButton_Click(object sender, RoutedEventArgs e) => ShowUnavailableMessage("Gradski objekti");
    private void ReviewsNotImplementedButton_Click(object sender, RoutedEventArgs e) => ShowUnavailableMessage("Recenzije");
    private void DonationsNotImplementedButton_Click(object sender, RoutedEventArgs e) => ShowUnavailableMessage("Donacije");

    private void ObjectsNavButton_Click(object sender, RoutedEventArgs e)
    {
        _activeTopAction = string.Empty;
        Navigate(new CitizenCityFacilitiesWindow(UserId));
    }

    private void ReviewsNavButton_Click(object sender, RoutedEventArgs e)
    {
        _activeTopAction = string.Empty;
        Navigate(new CitizenCoordinatorReviewsWindow(UserId));
    }

    private void DonationsNavButton_Click(object sender, RoutedEventArgs e)
    {
        _activeTopAction = string.Empty;
        Navigate(new CitizenDonationsWindow(UserId));
    }

    private static void ShowUnavailableMessage(string moduleName)
    {
        MessageBox.Show(
            $"{moduleName} će biti dostupno kada taj deo backend-a bude završen.",
            "Trenutno nije dostupno",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}
