using System.Windows;
using System.Windows.Controls;
using CommunityHub.Application.Services.BuildingManager.Dto;
using CommunityHub.Ui.ViewModels.BuildingManager;

namespace CommunityHub.Ui.Views.BuildingManager.Pages;

public partial class CommonRoomCalendarPage : Page
{
    private readonly BuildingManagerWindow _shell;
    private readonly CommonRoomCalendarViewModel _vm;

    public CommonRoomCalendarPage(BuildingManagerWindow shell, CommonRoomDto room, BuildingDto building)
    {
        InitializeComponent();
        _shell = shell;
        _vm = new CommonRoomCalendarViewModel(room, building);
        DataContext = _vm;

        // BlackoutDates se ne mogu bindovati u XAML-u — popunjavamo ih ovde
        foreach (DateOnly date in _vm.BookedDates)
            RoomCalendar.BlackoutDates.Add(
                new CalendarDateRange(date.ToDateTime(TimeOnly.MinValue)));
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
    }

    private void HelpButton_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            "Pregled zauzetosti prostorije.\n" +
            "Precrtani datumi su vec rezervisani (odobreni ili predlozeni zahtevi).",
            "Pomoc", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
