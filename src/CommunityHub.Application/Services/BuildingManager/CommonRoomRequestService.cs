using CommunityHub.Application.Domain;
using CommunityHub.Application.Services.BuildingManager.Dto;

namespace CommunityHub.Application.Services.BuildingManager;

public class CommonRoomRequestService
{
    private readonly ICommonRoomRequestRepository _repository;

    public CommonRoomRequestService(ICommonRoomRequestRepository repository)
    {
        _repository = repository;
    }

    // Svi zahtevi za iznajmljivanje u zgradama kojima upravlja dati upravnik
    public List<CommonRoomRequestDto> GetByManagerId(long managerId)
    {
        return _repository.GetByManagerId(managerId)
            .Select(MapToDto)
            .ToList();
    }

    // Upravnik predlaže konkretni termin stanaru (status → ProposedCounter)
    public void ProposeDate(long requestId, DateTime proposedStart, DateTime proposedEnd)
    {
        _repository.ProposeDate(requestId, proposedStart, proposedEnd);
    }

    // Odobrava zahtev za jednodnevnu prostoriju sa izabranim danom (status → Approved)
    public void ApproveWithDate(long requestId, DateTime date)
    {
        _repository.ApproveWithDate(requestId, date);
    }

    // Lista svih zauzetih dana za prostoriju (za prikaz X-eva na kalendaru)
    public List<DateOnly> GetBookedDatesForRoom(long roomId)
    {
        return _repository.GetBookedRangesForRoom(roomId)
            .SelectMany(range => ExpandRange(range.Start, range.End))
            .Distinct()
            .ToList();
    }

    // Razvija period (start, end) u listu pojedinačnih datuma
    private static IEnumerable<DateOnly> ExpandRange(DateTime start, DateTime end)
    {
        for (DateOnly d = DateOnly.FromDateTime(start); d <= DateOnly.FromDateTime(end); d = d.AddDays(1))
            yield return d;
    }

    private CommonRoomRequestDto MapToDto(CommonRoomRequest r) => new CommonRoomRequestDto
    {
        RequestId          = r.Id,
        TenantFullName     = $"{r.Tenant.Name} {r.Tenant.Surname}",
        RoomId             = r.CommonRoom.Id,
        RoomName           = r.CommonRoom.Name,
        BuildingAddress    = $"{r.CommonRoom.Building.Address.Street} {r.CommonRoom.Building.Address.StreetNumber}",
        RequestedStartDate = r.RequestedStartDate,
        RequestedEndDate   = r.RequestedEndDate,
        IsMultiDay         = r.CommonRoom.IsMultiDay,
        ProposedStartDate  = r.ProposedStartDate,
        ProposedEndDate    = r.ProposedEndDate,
        Status             = r.Status
    };
}
