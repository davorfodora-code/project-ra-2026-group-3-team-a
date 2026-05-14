using System.Windows;
using System.Windows.Input;
using CommunityHub.Application;
using CommunityHub.Application.Database.Repositories;
using CommunityHub.Application.Services.Tenant;

namespace CommunityHub.Ui.Views.Tenant;

public partial class TenantHomeWindow : Window
{
    private readonly long _userId;

    public TenantHomeWindow(long userId)
    {
        InitializeComponent();
        _userId = userId;

        var user = new UserDbRepository().GetById(userId);
        WelcomeText.Text = $"Welcome, {user?.Name ?? "Korisnik"}";
        UserInitialsBlock.Text = ComputeInitials(user?.Name, user?.Surname);

        LoadStats();
    }

    private void LoadStats()
    {
        var requestService = new BuildingAccessRequestService(
            Injector.Get<IBuildingAccessRequestRepository>(),
            Injector.Get<IBuildingMembershipRepository>());
        PendingRequestsCount.Text = requestService.GetAllByTenant(_userId)
            .Count(r => r.Status == "Pending").ToString();

        var adService = new AdvertisementService(Injector.Get<IAdvertisementRepository>());
        var buildings = adService.GetBuildingsForTenant(_userId);
        if (buildings.Count > 0)
            HelpPostsCount.Text = adService.GetActiveBoardAds(_userId, buildings[0].BuildingId).Count.ToString();
    }

    private void Avatar_Click(object sender, MouseButtonEventArgs e)
    {
        new ProfileWindow(_userId).Show();
    }

    private void NavBuildings_Click(object sender, RoutedEventArgs e)
    {
        new BuildingSearchWindow(_userId).Show();
        Close();
    }

    private void NavHelpBoard_Click(object sender, RoutedEventArgs e)
    {
        new HelpBoardWindow(_userId).Show();
        Close();
    }

    private static string ComputeInitials(string? name, string? surname)
    {
        char n = !string.IsNullOrEmpty(name) ? char.ToUpper(name[0]) : '?';
        char s = !string.IsNullOrEmpty(surname) ? char.ToUpper(surname[0]) : '?';
        return $"{n}{s}";
    }
}
