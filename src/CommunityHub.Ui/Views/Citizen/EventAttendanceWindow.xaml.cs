using System.Windows;
using CommunityHub.Application.Dto.Citizen;
using CommunityHub.Ui.ViewModels.Citizen;

namespace CommunityHub.Ui.Views.Citizen;

public partial class EventAttendanceWindow : Window
{
    private readonly EventAttendanceViewModel _viewModel;
    private readonly List<VolunteerAttendanceDto> _rows;

    public EventAttendanceWindow(List<VolunteerAttendanceDto> rows)
    {
        InitializeComponent();
        _viewModel = new EventAttendanceViewModel();
        _rows = rows;
        AttendanceDataGrid.ItemsSource = _rows;
    }

    public IReadOnlyList<VolunteerAttendanceDto> Attendances => _viewModel.BuildPayload(_rows);

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
