using System.Windows;
using System.Windows.Controls;
using CommunityHub.Application.Services.Citizen;
using CommunityHub.Ui.ViewModels.Citizen;

namespace CommunityHub.Ui.Views.Citizen;

public partial class CitizenHomeWindow : Page
{
    private readonly CitizenHomeViewModel _viewModel;
    private readonly CitizenMembershipService _membershipService;
    private readonly bool _hasMembership;

    public CitizenHomeWindow(long userId)
    {
        InitializeComponent();
        _viewModel = new CitizenHomeViewModel(userId);
        _membershipService = new CitizenMembershipService();
        _hasMembership = _membershipService.HasMembership(userId);
        MeetingsButton.Visibility = _hasMembership ? Visibility.Visible : Visibility.Collapsed;
        ConfigureHomeState();
    }

    private void ConfigureHomeState()
    {
        if (_hasMembership)
        {
            MemberNavPanel.Visibility = Visibility.Visible;
            NonMemberNavPanel.Visibility = Visibility.Collapsed;
            WelcomeDescriptionTextBlock.Text =
                "Vaše članstvo je aktivno. Dostupne su osnovne i aktivnosti u kvartu.";
            MembershipStatusTextBlock.Text = "Članstvo je aktivno i povezano sa vašim kvartom.";
            MembershipActionButton.Content = "Otvori moj kvart";
            FindDistrictButton.Visibility = Visibility.Collapsed;
            OpenRequestsButton.Visibility = Visibility.Collapsed;
            DistrictEventsButton.Visibility = Visibility.Visible;
            QuickRequestTitleTextBlock.Text = "Imate pristup funkcionalnostima člana.";
            QuickRequestDescriptionTextBlock.Text =
                "Sada možete koristiti događaje i sastanke. Ostali moduli će biti dostupni kada backend podrška bude završena.";
            StateHintTextBlock.Text = "Nastavite preko desne navigacije.";
            return;
        }

        MemberNavPanel.Visibility = Visibility.Collapsed;
        NonMemberNavPanel.Visibility = Visibility.Visible;
        WelcomeDescriptionTextBlock.Text =
            "Trenutno niste član nijednog kvarta. Da biste koristili događaje, izveštaje, donacije i ostale funkcionalnosti, prvo pronađite svoj kvart i pošaljite zahtev ili se automatski učlanite.";
        MembershipStatusTextBlock.Text = "Nemate aktivno članstvo ni u jednom kvartu.";
        MembershipActionButton.Content = "Pronađi kvart";
        FindDistrictButton.Visibility = Visibility.Visible;
        OpenRequestsButton.Visibility = Visibility.Visible;
        DistrictEventsButton.Visibility = Visibility.Collapsed;
        QuickRequestTitleTextBlock.Text = "Nemate aktivan zahtev.";
        QuickRequestDescriptionTextBlock.Text = "Kada pošaljete zahtev za pristup, ovde će biti prikazan njegov status.";
        StateHintTextBlock.Text = "Otvorite pretragu kvartova i pronađite kvart koji odgovara vašoj adresi.";
    }

    private void DistrictSearchButton_Click(object sender, RoutedEventArgs e)
    {
        CitizenShellNavigation.Navigate(this, new DistrictSearchWindow(_viewModel.UserId));
    }

    private void MyDistrictRequestsButton_Click(object sender, RoutedEventArgs e)
    {
        CitizenShellNavigation.Navigate(this, new MyDistrictRequestsWindow(_viewModel.UserId));
    }

    private void MeetingsButton_Click(object sender, RoutedEventArgs e)
    {
        CitizenShellNavigation.Navigate(this, new CitizenMeetingsWindow(_viewModel.UserId));
    }

    private void DistrictEventsButton_Click(object sender, RoutedEventArgs e)
    {
        CitizenShellNavigation.Navigate(this, new CitizenEventsWindow(_viewModel.UserId));
    }

    private void MyDistrictButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_membershipService.HasMembership(_viewModel.UserId))
        {
            MessageBox.Show("Niste član nijednog kvarta. Prvo pošaljite zahtev za pristup kvartu.", "Informacija",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        CitizenShellNavigation.Navigate(this, new MyDistrictWindow(_viewModel.UserId));
    }

    private void ProfileButton_Click(object sender, RoutedEventArgs e)
    {
        CitizenShellNavigation.Navigate(this, new CitizenProfileWindow(_viewModel.UserId));
    }

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        CitizenShellNavigation.Logout(this);
    }

    private void MembershipActionButton_Click(object sender, RoutedEventArgs e)
    {
        if (_hasMembership)
        {
            MyDistrictButton_Click(sender, e);
            return;
        }

        DistrictSearchButton_Click(sender, e);
    }

    private static void ShowUnavailableMessage(string moduleName)
    {
        MessageBox.Show(
            $"{moduleName} će biti dostupno kada taj deo backend-a bude završen.",
            "Trenutno nije dostupno",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void ReportsNotImplementedButton_Click(object sender, RoutedEventArgs e) => ShowUnavailableMessage("Izveštaji");
    private void ObjectsNotImplementedButton_Click(object sender, RoutedEventArgs e) => ShowUnavailableMessage("Gradski objekti");
    private void ReviewsNotImplementedButton_Click(object sender, RoutedEventArgs e) => ShowUnavailableMessage("Recenzije");
    private void DonationsNotImplementedButton_Click(object sender, RoutedEventArgs e) => ShowUnavailableMessage("Donacije");
}
