using CommunityHub.Application.Services.BuildingManager.Dto;

namespace CommunityHub.Ui.ViewModels.BuildingManager;

// VM za jednu karticu zahteva. Obavija BuildingAccessRequestDto
// i drzi UI stanje koje je specificno za karticu (otvoren panel za odbijanje, tekst razloga).
public class AccessRequestItemViewModel : BaseViewModel
{
    private readonly BuildingAccessRequestDto _dto;
    private bool _isRejectPanelOpen;
    private string _rejectReason = string.Empty;

    public AccessRequestItemViewModel(BuildingAccessRequestDto dto)
    {
        _dto = dto;
    }

    public long RequestId => _dto.RequestId;
    public string TenantFullName => _dto.TenantFullName;
    public string Status => _dto.Status;
    public bool IsPending => _dto.IsPending;

    public string LocationLine => $"{_dto.BuildingAddress}, {_dto.Neighborhood}, {_dto.Location}";
    public string ApartmentLine => $"Stan: {_dto.ApartmentNumber}";
    public string DateLine => $"Datum: {_dto.DateDisplay}";
    public string StatusLine => $"Status: {_dto.StatusDisplay}";

    public bool IsRejectPanelOpen
    {
        get => _isRejectPanelOpen;
        set => SetProperty(ref _isRejectPanelOpen, value);
    }

    public string RejectReason
    {
        get => _rejectReason;
        set => SetProperty(ref _rejectReason, value);
    }
}
