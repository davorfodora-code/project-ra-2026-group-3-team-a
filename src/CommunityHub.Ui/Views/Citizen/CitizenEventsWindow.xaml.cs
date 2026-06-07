using System.Windows;
using System.Windows.Controls;
using CommunityHub.Application.Dto.Citizen;
using CommunityHub.Ui.ViewModels.Citizen;

namespace CommunityHub.Ui.Views.Citizen;

public partial class CitizenEventsWindow : Page
{
    private readonly long _userId;
    private readonly CitizenEventsViewModel _viewModel;

    public CitizenEventsWindow(long userId)
    {
        InitializeComponent();
        _userId = userId;
        _viewModel = new CitizenEventsViewModel(userId);
        LoadEvents();
    }

    private void LoadEvents()
    {
        EventsDataGrid.ItemsSource = _viewModel.LoadEvents();
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        LoadEvents();
    }

    private void CreateEventButton_Click(object sender, RoutedEventArgs e)
    {
        long? districtId = _viewModel.GetCitizenDistrictId();
        if (districtId == null)
        {
            MessageBox.Show("Samo clan kvarta moze da organizuje dogadjaj.", "Upozorenje",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        CreateDistrictEventWindow createWindow = new CreateDistrictEventWindow(districtId.Value);
        createWindow.Owner = Window.GetWindow(this);
        bool? dialogResult = createWindow.ShowDialog();
        if (dialogResult != true || createWindow.Dto == null)
        {
            return;
        }

        EventActionResultDto result = _viewModel.CreateEvent(createWindow.Dto);
        MessageBox.Show(result.Message, result.Success ? "Uspeh" : "Greška", MessageBoxButton.OK,
            result.Success ? MessageBoxImage.Information : MessageBoxImage.Error);

        if (result.Success)
        {
            LoadEvents();
        }
    }

    public void OpenCreateEventDialog()
    {
        CreateEventButton_Click(this, new RoutedEventArgs());
    }

    private void VolunteerSignupButton_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetSelectedEvent(out DistrictEventListItemDto selected))
            return;

        VolunteerSignupDto? signupDto = OpenVolunteerSignupDialog(selected.EventId);
        if (signupDto == null)
            return;

        EventActionResultDto result = _viewModel.ApplyAsVolunteer(signupDto);
        MessageBox.Show(result.Message, result.Success ? "Uspeh" : "Greška", MessageBoxButton.OK,
            result.Success ? MessageBoxImage.Information : MessageBoxImage.Error);

        if (result.Success)
        {
            LoadEvents();
        }
    }

    private bool TryGetSelectedEvent(out DistrictEventListItemDto selected)
    {
        selected = null!;
        if (EventsDataGrid.SelectedItem is DistrictEventListItemDto selectedEvent)
        {
            selected = selectedEvent;
            return true;
        }

        MessageBox.Show("Izaberite dogadjaj.", "Upozorenje", MessageBoxButton.OK, MessageBoxImage.Warning);
        return false;
    }

    private VolunteerSignupDto? OpenVolunteerSignupDialog(long eventId)
    {
        DistrictEventDetailsDto? details = _viewModel.GetEventDetails(eventId);
        if (details == null)
        {
            MessageBox.Show("Dogadjaj nije dostupan.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }

        VolunteerSignupWindow signupWindow = new(details)
        {
            Owner = Window.GetWindow(this)
        };

        bool? dialogResult = signupWindow.ShowDialog();
        return dialogResult == true ? signupWindow.Dto : null;
    }

    private void AttendanceButton_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetSelectedEvent(out DistrictEventListItemDto selected))
            return;

        List<VolunteerAttendanceDto> candidates = _viewModel.GetAttendanceCandidates(selected.EventId);
        if (candidates.Count == 0)
        {
            MessageBox.Show("Nema dostupnih volontera za evidenciju prisustva.", "Informacija",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        EventAttendanceWindow attendanceWindow = new EventAttendanceWindow(candidates);
        attendanceWindow.Owner = Window.GetWindow(this);
        bool? dialogResult = attendanceWindow.ShowDialog();
        if (dialogResult != true)
        {
            return;
        }

        EventActionResultDto result = _viewModel.ConfirmAttendance(selected.EventId, attendanceWindow.Attendances);
        MessageBox.Show(result.Message, result.Success ? "Uspeh" : "Greška", MessageBoxButton.OK,
            result.Success ? MessageBoxImage.Information : MessageBoxImage.Error);
    }

    private void HomeNavButton_Click(object sender, RoutedEventArgs e)
    {
        CitizenShellNavigation.Navigate(this, new CitizenHomeWindow(_userId));
    }

    private void MyDistrictNavButton_Click(object sender, RoutedEventArgs e)
    {
        CitizenShellNavigation.Navigate(this, new MyDistrictWindow(_userId));
    }
}
