using CommunityHub.Application.Services.BuildingManager.Dto;

namespace CommunityHub.Ui.ViewModels.BuildingManager;

// VM za jednu karticu zahteva za iznajmljivanje.
// Obavija CommonRoomRequestDto i izlaze display properties za XAML binding.
public class RoomRentalRequestItemViewModel : BaseViewModel
{
    private readonly CommonRoomRequestDto _dto;

    public RoomRentalRequestItemViewModel(CommonRoomRequestDto dto)
    {
        _dto = dto;
    }

    public long RequestId       => _dto.RequestId;
    public long RoomId          => _dto.RoomId;
    public string TenantFullName => _dto.TenantFullName;
    public string RoomName      => _dto.RoomName;
    public string BuildingLine  => $"Zgrada: {_dto.BuildingAddress}";
    public string RoomLine      => $"Prostorija: {_dto.RoomName}";
    public string DateLine      => $"Datum: {_dto.RequestedRangeLine}";
    public string RentalTypeLine => _dto.RentalTypeLine;
    public string StatusLine    => $"Status: {_dto.StatusLine}";

    // Dugme "Izaberi dan" se prikazuje samo dok je zahtev na čekanju
    public bool CanPropose => _dto.Status == "Pending";
}
