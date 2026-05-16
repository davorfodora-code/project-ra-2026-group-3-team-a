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

        if (start == null || end == null)
        {
            ValidationText.Text = "Molimo izaberite oba datuma.";
            ValidationText.Visibility = Visibility.Visible;
            return;
        }

        if (start.Value.Date < DateTime.Today || end.Value.Date < DateTime.Today)
        {
            ValidationText.Text = "Datumi ne mogu biti u prošlosti.";
            ValidationText.Visibility = Visibility.Visible;
            return;
        }

        if (end.Value.Date < start.Value.Date)
        {
            ValidationText.Text = "Datum završetka ne može biti pre datuma početka.";
            ValidationText.Visibility = Visibility.Visible;
            return;
        }

        Result = new CreateCommonRoomRequestDto
        {
            RoomId    = _room.RoomId,
            StartDate = start.Value.Date,
            EndDate   = end.Value.Date,
        };

        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
