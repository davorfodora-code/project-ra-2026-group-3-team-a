using System.Windows;
using CommunityHub.Application.Dto.Citizen;
using CommunityHub.Ui.ViewModels.Citizen;

namespace CommunityHub.Ui.Views.Citizen;

public partial class CitizenEventsWindow : Window
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

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        new CitizenHomeWindow(_userId).Show();
        Close();
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
        createWindow.Owner = this;
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

    private void VolunteerSignupButton_Click(object sender, RoutedEventArgs e)
    {
        if (EventsDataGrid.SelectedItem is not DistrictEventListItemDto selected)
        {
            MessageBox.Show("Izaberite dogadjaj.", "Upozorenje", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        DistrictEventDetailsDto? details = _viewModel.GetEventDetails(selected.EventId);
        if (details == null)
        {
            MessageBox.Show("Dogadjaj nije dostupan.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        VolunteerSignupWindow signupWindow = new VolunteerSignupWindow(details);
        signupWindow.Owner = this;
        bool? dialogResult = signupWindow.ShowDialog();
        if (dialogResult != true || signupWindow.Dto == null)
        {
            return;
        }

        EventActionResultDto result = _viewModel.ApplyAsVolunteer(signupWindow.Dto);
        MessageBox.Show(result.Message, result.Success ? "Uspeh" : "Greška", MessageBoxButton.OK,
            result.Success ? MessageBoxImage.Information : MessageBoxImage.Error);

        if (result.Success)
        {
            LoadEvents();
        }
    }

    private void AttendanceButton_Click(object sender, RoutedEventArgs e)
    {
        if (EventsDataGrid.SelectedItem is not DistrictEventListItemDto selected)
        {
            MessageBox.Show("Izaberite dogadjaj.", "Upozorenje", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        List<VolunteerAttendanceDto> candidates = _viewModel.GetAttendanceCandidates(selected.EventId);
        if (candidates.Count == 0)
        {
            MessageBox.Show("Nema dostupnih volontera za evidenciju prisustva.", "Informacija",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        EventAttendanceWindow attendanceWindow = new EventAttendanceWindow(candidates);
        attendanceWindow.Owner = this;
        bool? dialogResult = attendanceWindow.ShowDialog();
        if (dialogResult != true)
        {
            return;
        }

        EventActionResultDto result = _viewModel.ConfirmAttendance(selected.EventId, attendanceWindow.Attendances);
        MessageBox.Show(result.Message, result.Success ? "Uspeh" : "Greška", MessageBoxButton.OK,
            result.Success ? MessageBoxImage.Information : MessageBoxImage.Error);
    }
}
