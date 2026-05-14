using System.Windows;
using System.Windows.Controls;
using CommunityHub.Ui.ViewModels.BuildingManager;

namespace CommunityHub.Ui.Views.BuildingManager.Pages;

public partial class RoomCalendarPage : Page
{
    private readonly BuildingManagerWindow _shell;
    private readonly RoomCalendarViewModel _vm;

    public RoomCalendarPage(
        BuildingManagerWindow shell,
        RoomRentalRequestItemViewModel item,
        List<DateOnly> bookedDates,
        Action<long, DateTime, DateTime> onConfirmed)
    {
        InitializeComponent();
        _shell = shell;
        _vm = new RoomCalendarViewModel(item, bookedDates, onConfirmed);
        DataContext = _vm;

        // BlackoutDates se ne mogu bindovati u XAML-u — popunjavamo ih ovde
        foreach (DateOnly date in bookedDates)
            RoomCalendar.BlackoutDates.Add(
                new CalendarDateRange(date.ToDateTime(TimeOnly.MinValue)));
    }

    private bool _updatingDates;

    private void RoomCalendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_updatingDates) return;

        DateTime? clicked = RoomCalendar.SelectedDate;
        _vm.SelectedDate = clicked;

        // Ako je zahtev višednevan, programski označi ceo period počev od kliknutog datuma
        if (clicked.HasValue && _vm.DurationDays > 0)
        {
            _updatingDates = true;
            try
            {
                RoomCalendar.SelectedDates.Clear();
                for (int i = 0; i <= _vm.DurationDays; i++)
                {
                    DateTime d = clicked.Value.AddDays(i);
                    if (!RoomCalendar.BlackoutDates.Contains(d))
                        RoomCalendar.SelectedDates.Add(d);
                }
            }
            finally { _updatingDates = false; }
        }
    }

    private void Confirm_Click(object sender, RoutedEventArgs e)
    {
        if (_vm.TryConfirm() && NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
    }

    private void HelpButton_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            "Izaberite slobodan datum na kalendaru.\n" +
            "Zauzeti datumi (već rezervisani) su onemogućeni za odabir.\n" +
            "Kliknite Potvrdi da pošaljete predlog termina stanaru.",
            "Pomoć", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
