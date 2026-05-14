namespace CommunityHub.Ui.ViewModels.BuildingManager;

// VM za stranicu sa kalendarom slobodnih termina prostorije.
// Prima podatke o zahtevu i listu zauzetih datuma, izlaže SelectedDate
// koji se vezuje za WPF Calendar kontrolu u code-behind-u.
public class RoomCalendarViewModel : BaseViewModel
{
    private readonly long _requestId;
    private readonly TimeSpan _duration;
    private readonly Action<long, DateTime, DateTime> _onConfirmed;

    private DateTime? _selectedDate;

    public RoomCalendarViewModel(
        RoomRentalRequestItemViewModel item,
        List<DateOnly> bookedDates,
        Action<long, DateTime, DateTime> onConfirmed)
    {
        _requestId   = item.RequestId;
        _duration    = item.RequestedEndDate.Date - item.RequestedStartDate.Date;
        _onConfirmed = onConfirmed;

        RoomName     = item.RoomName;
        BuildingLine = $"Zgrada: {item.BuildingAddress}";
        TenantLine   = $"Stanar: {item.TenantFullName}";
        PeriodLine   = $"Željeni period: {item.RequestedStartDate:dd.MM.} - {item.RequestedEndDate:dd.MM.}";
        TypeLine     = $"Iznajmljivanje na: {item.RentalTypeLine.ToLower()}";
        BookedDates  = bookedDates;
    }

    // Zaglavlje i info-box
    public string RoomName     { get; }
    public string BuildingLine { get; }
    public string TenantLine   { get; }
    public string PeriodLine   { get; }
    public string TypeLine     { get; }

    // Zauzeti datumi — setuju se kao BlackoutDates na Calendar kontrolu u code-behind-u
    public List<DateOnly> BookedDates { get; }

    // Datum koji je upravnik odabrao na kalendaru
    public DateTime? SelectedDate
    {
        get => _selectedDate;
        set
        {
            if (SetProperty(ref _selectedDate, value))
                OnPropertyChanged(nameof(CanConfirm));
        }
    }

    // Trajanje rezervacije u danima (0 = jednodnevno)
    public int DurationDays => (int)_duration.TotalDays;

    public bool CanConfirm => _selectedDate.HasValue;

    // Poziva callback sa predloženim terminom i vraća true ako je uspešno
    public bool TryConfirm()
    {
        if (!CanConfirm) return false;
        DateTime start = _selectedDate!.Value.Date;
        DateTime end   = start + _duration;
        _onConfirmed(_requestId, start, end);
        return true;
    }
}
