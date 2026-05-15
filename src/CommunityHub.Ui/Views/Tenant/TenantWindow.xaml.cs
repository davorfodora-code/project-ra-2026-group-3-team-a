using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CommunityHub.Application.Database.Repositories;
using CommunityHub.Ui.Views.Tenant.Pages;
using FontAwesome.Sharp;

namespace CommunityHub.Ui.Views.Tenant;

public partial class TenantWindow : Window
{
    public readonly long UserId;

    private (Border Highlight, IconBlock Icon, TextBlock Text, string Title)[] _navItems = null!;

    public TenantWindow(long userId)
    {
        InitializeComponent();
        UserId = userId;

        var user = new UserDbRepository().GetById(userId);
        UserInitialsBlock.Text = ComputeInitials(user?.Name, user?.Surname);

        _navItems =
        [
            (NavHomeHighlight,       NavHomeIcon,        NavHomeText,        "Home"),
            (NavZgradeHighlight,     NavZgradeIcon,      NavZgradeText,      "Zgrade"),
            (NavOglasnaHighlight,    NavOglasnaIcon,     NavOglasnaText,     "Oglasna tabla"),
            (NavZakazivanjeHighlight, NavZakazivanjeIcon, NavZakazivanjeText, "Zakazivanje"),
        ];

        SetActiveNav(0);
        MainFrame.Navigate(new TenantHomePage(this));
    }

    public void SetActiveNav(int index)
    {
        var inactive = new SolidColorBrush(Color.FromRgb(0xB0, 0xBE, 0xC5));
        for (int i = 0; i < _navItems.Length; i++)
        {
            bool active = i == index;
            _navItems[i].Highlight.Visibility = active ? Visibility.Visible : Visibility.Collapsed;
            Brush color = active ? Brushes.White : inactive;
            _navItems[i].Icon.Foreground = color;
            _navItems[i].Text.Foreground = color;
        }
        TopBarTitle.Text = _navItems[index].Title;
    }

    private void NavHome_Click(object sender, RoutedEventArgs e)
    {
        SetActiveNav(0);
        MainFrame.Navigate(new TenantHomePage(this));
    }

    private void NavZgrade_Click(object sender, RoutedEventArgs e)
    {
        SetActiveNav(1);
        MainFrame.Navigate(new BuildingSearchPage(this));
    }

    private void NavOglasna_Click(object sender, RoutedEventArgs e)
    {
        SetActiveNav(2);
        MainFrame.Navigate(new HelpBoardPage(this));
    }

    private void NavZakazivanje_Click(object sender, RoutedEventArgs e)
    {
        SetActiveNav(3);
        MainFrame.Navigate(new SchedulingPage(this));
    }

    private void Avatar_Click(object sender, MouseButtonEventArgs e)
    {
        new ProfileWindow(UserId).Show();
    }

    private static string ComputeInitials(string? name, string? surname)
    {
        char n = !string.IsNullOrEmpty(name) ? char.ToUpper(name[0]) : '?';
        char s = !string.IsNullOrEmpty(surname) ? char.ToUpper(surname[0]) : '?';
        return $"{n}{s}";
    }
}
