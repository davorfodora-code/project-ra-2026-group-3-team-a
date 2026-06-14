using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CommunityHub.Application.Database.Repositories;
using CommunityHub.Application.Services.Tenant.Dto;
using CommunityHub.Ui.ViewModels.Tenant;
using CommunityHub.Ui.Views.Tenant.Pages;
using FontAwesome.Sharp;

namespace CommunityHub.Ui.Views.Tenant;

public partial class TenantWindow : Window
{
    public readonly long UserId;

    private string _currentSectionTitle = "Početna";
    private (Border Highlight, IconBlock Icon, TextBlock Text, string Title)[] _navItems = null!;
    private NotificationsViewModel? _notifVm;

    public TenantWindow(long userId)
    {
        InitializeComponent();
        UserId = userId;

        var user = new UserDbRepository().GetById(userId);
        UserInitialsBlock.Text = ComputeInitials(user?.Name, user?.Surname);

        _navItems =
        [
            (NavHomeHighlight,        NavHomeIcon,         NavHomeText,         "Početna"),
            (NavZgradeHighlight,      NavZgradeIcon,       NavZgradeText,       "Zgrade"),
            (NavOglasnaHighlight,     NavOglasnaIcon,      NavOglasnaText,      "Oglasna tabla"),
            (NavZakazivanjeHighlight, NavZakazivanjeIcon,  NavZakazivanjeText,  "Zakazivanje"),
            (NavZajednickeHighlight,  NavZajednickeIcon,   NavZajednickeText,   "Zajedničke prostorije"),
            (NavProblemiHighlight,    NavProblemiIcon,     NavProblemiText,     "Problemi"),
            (NavSkupstineHighlight,   NavSkupstineIcon,    NavSkupstineText,    "Skupštine"),
            (NavIzvestajiHighlight,   NavIzvestajiIcon,    NavIzvestajiText,    "Izveštaji"),
        ];

        SetActiveNav(0);
        MainFrame.Navigate(new TenantHomePage(this));

        _notifVm = new NotificationsViewModel(UserId);
        _notifVm.Load();
        UpdateNotificationBadge();
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
        _currentSectionTitle = _navItems[index].Title;
        TopBarTitle.Text = _currentSectionTitle;
    }

    public void SetBreadcrumb(string subSection)
    {
        TopBarTitle.Text = $"{_currentSectionTitle}  ›  {subSection}";
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

    private void NavZajednicke_Click(object sender, RoutedEventArgs e)
    {
        SetActiveNav(4);
        MainFrame.Navigate(new CommonRoomsPage(this));
    }

    private void NavProblemi_Click(object sender, RoutedEventArgs e)
    {
        SetActiveNav(5);
        MainFrame.Navigate(new IssuesPage(this));
    }

    private void NavSkupstine_Click(object sender, RoutedEventArgs e)
    {
        SetActiveNav(6);
        MainFrame.Navigate(new AssembliesPage(this));
    }

    private void NavIzvestaji_Click(object sender, RoutedEventArgs e)
    {
        SetActiveNav(7);
        MainFrame.Navigate(new ReportsPage(this));
    }

    private void Avatar_Click(object sender, MouseButtonEventArgs e)
    {
        foreach (var item in _navItems)
        {
            item.Highlight.Visibility = Visibility.Collapsed;
            item.Icon.Foreground = new SolidColorBrush(Color.FromRgb(0xB0, 0xBE, 0xC5));
            item.Text.Foreground = new SolidColorBrush(Color.FromRgb(0xB0, 0xBE, 0xC5));
        }
        TopBarTitle.Text = "Profil";
        MainFrame.Navigate(new TenantProfilePage(this));
    }

    private static string ComputeInitials(string? name, string? surname)
    {
        char n = !string.IsNullOrEmpty(name) ? char.ToUpper(name[0]) : '?';
        char s = !string.IsNullOrEmpty(surname) ? char.ToUpper(surname[0]) : '?';
        return $"{n}{s}";
    }

    private void Bell_Click(object sender, RoutedEventArgs e)
    {
        _notifVm!.Load();
        NotificationsList.ItemsSource = _notifVm.Notifications;
        NoNotificationsText.Visibility = _notifVm.UnreadCount == 0
            ? Visibility.Visible : Visibility.Collapsed;
        NotificationsPanel.Visibility = NotificationsPanel.Visibility == Visibility.Visible
            ? Visibility.Collapsed : Visibility.Visible;
    }

    private void CloseNotifications_Click(object sender, RoutedEventArgs e)
        => NotificationsPanel.Visibility = Visibility.Collapsed;

    private void NotificationAction_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not NotificationDto dto) return;
        _notifVm!.MarkAsRead(dto.Id);
        _notifVm.Load();
        UpdateNotificationBadge();
        NotificationsPanel.Visibility = Visibility.Collapsed;

        if (dto.Type == "Rezervacija termina")
        {
            SetActiveNav(3);
            MainFrame.Navigate(new SchedulingPage(this));
        }
        else
        {
            new AdSlotsWindow(dto, UserId) { Owner = this }.ShowDialog();
        }
    }

    public void UpdateNotificationBadge()
    {
        int count = _notifVm?.UnreadCount ?? 0;
        NotificationBadge.Visibility = count > 0 ? Visibility.Visible : Visibility.Collapsed;
        NotificationBadgeText.Text = count.ToString();
    }
}
