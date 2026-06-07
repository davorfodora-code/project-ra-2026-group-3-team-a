using CommunityHub.Application;
using CommunityHub.Application.Services.BuildingManager;
using CommunityHub.Application.Services.BuildingManager.Dto;

namespace CommunityHub.Ui.ViewModels.BuildingManager;

// VM za standalone pregled kalendara zauzetosti prostorije
// (samo prikaz, bez akcija — koristi se iz Prostorije taba u detaljima zgrade)
public class CommonRoomCalendarViewModel : BaseViewModel
{
    private readonly CommonRoomRequestService _service;

    public CommonRoomCalendarViewModel(CommonRoomDto room, BuildingDto building)
    {
        _service = new CommonRoomRequestService(Injector.Get<ICommonRoomRequestRepository>());

        RoomName        = room.Name;
        BuildingLine    = $"Zgrada: {building.AddressDisplay}";
        FloorLine       = $"Sprat: {room.FloorDisplay}";
        DescriptionLine = $"Opis: {room.Description}";
        TypeLine        = room.IsMultiDay
            ? "Iznajmljivanje: vise dana"
            : "Iznajmljivanje: jedan dan";

        BookedDates = _service.GetBookedDatesForRoom(room.Id);
    }

    public string RoomName        { get; }
    public string BuildingLine    { get; }
    public string FloorLine       { get; }
    public string DescriptionLine { get; }
    public string TypeLine        { get; }

    // Zauzeti datumi — setuju se kao BlackoutDates na Calendar kontroli u code-behind-u
    public List<DateOnly> BookedDates { get; }
}
