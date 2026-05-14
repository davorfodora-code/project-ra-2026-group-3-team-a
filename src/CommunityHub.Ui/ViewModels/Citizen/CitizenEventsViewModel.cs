using CommunityHub.Application;
using CommunityHub.Application.Dto.Citizen;
using CommunityHub.Application.Services.Citizen;

namespace CommunityHub.Ui.ViewModels.Citizen;

public class CitizenEventsViewModel
{
    private readonly ICitizenDistrictEventService _service;
    private readonly long _citizenId;

    public CitizenEventsViewModel(long citizenId)
    {
        _citizenId = citizenId;
        _service = Injector.Get<ICitizenDistrictEventService>();
    }

    public long? GetCitizenDistrictId()
    {
        return _service.GetCitizenDistrictId(_citizenId);
    }

    public List<DistrictEventListItemDto> LoadEvents()
    {
        return _service.GetMyDistrictEvents(_citizenId);
    }

    public DistrictEventDetailsDto? GetEventDetails(long eventId)
    {
        return _service.GetEventDetailsForCitizen(_citizenId, eventId);
    }

    public EventActionResultDto CreateEvent(CreateDistrictEventDto dto)
    {
        return _service.CreateEvent(_citizenId, dto);
    }

    public EventActionResultDto ApplyAsVolunteer(VolunteerSignupDto dto)
    {
        return _service.ApplyAsVolunteer(_citizenId, dto);
    }

    public List<VolunteerAttendanceDto> GetAttendanceCandidates(long eventId)
    {
        return _service.GetAttendanceCandidates(_citizenId, eventId);
    }

    public EventActionResultDto ConfirmAttendance(long eventId, IReadOnlyList<VolunteerAttendanceDto> attendances)
    {
        return _service.ConfirmAttendance(_citizenId, eventId, attendances);
    }
}
