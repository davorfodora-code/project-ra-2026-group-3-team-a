using System.Windows;
using System.Windows.Controls;
using CommunityHub.Application.Services.Tenant.Dto;

namespace CommunityHub.Ui.Views.Tenant;

public partial class RentRequestDialog : Window
{
    private readonly CommonRoomDto _room;

    public CreateCommonRoomRequestDto? Result { get; private set; }

    public RentRequestDialog(CommonRoomDto room)
    {
        InitializeComponent();
        _room = room;

        RoomNameText.Text  = room.Name;
        RoomDescText.Text  = room.Description;
        RoomFloorText.Text = room.FloorDisplay;
        RoomTypeText.Text  = room.RentalTypeDisplay;

        StartDatePicker.DisplayDateStart = DateTime.Today;
        EndDatePicker.DisplayDateStart   = DateTime.Today;
    }

    private void DatePicker_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (EndDatePicker.SelectedDate.HasValue && StartDatePicker.SelectedDate.HasValue
            && EndDatePicker.SelectedDate < StartDatePicker.SelectedDate)
        {
            EndDatePicker.SelectedDate = StartDatePicker.SelectedDate;
        }
        if (EndDatePicker != null)
            EndDatePicker.DisplayDateStart = StartDatePicker.SelectedDate ?? DateTime.Today;

        ValidationText.Visibility = Visibility.Collapsed;
    }

    private void Submit_Click(object sender, RoutedEventArgs e)
    {
        var start = StartDatePicker.SelectedDate;
        var end   = EndDatePicker.SelectedDate;
        string? error = ValidateDateSelection(start, end);
        if (error != null)
        {
            ValidationText.Text = error;
            ValidationText.Visibility = Visibility.Visible;
            return;
        }
        Result = new CreateCommonRoomRequestDto
        {
            RoomId    = _room.RoomId,
            StartDate = start!.Value.Date,
            EndDate   = end!.Value.Date,
        };
        DialogResult = true;
    }

    private static string? ValidateDateSelection(DateTime? start, DateTime? end)
    {
        if (start == null || end == null)
            return "Molimo izaberite oba datuma.";
        if (start.Value.Date < DateTime.Today || end.Value.Date < DateTime.Today)
            return "Datumi ne mogu biti u prošlosti.";
        if (end.Value.Date < start.Value.Date)
            return "Datum završetka ne može biti pre datuma početka.";
        return null;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
