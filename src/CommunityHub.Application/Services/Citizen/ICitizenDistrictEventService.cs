using CommunityHub.Application.Dto.Citizen;

namespace CommunityHub.Application.Services.Citizen;

public interface ICitizenDistrictEventService
{
    long? GetCitizenDistrictId(long citizenId);
    List<DistrictEventListItemDto> GetMyDistrictEvents(long citizenId);
    DistrictEventDetailsDto? GetEventDetailsForCitizen(long citizenId, long eventId);
    EventActionResultDto CreateEvent(long citizenId, CreateDistrictEventDto dto);
    EventActionResultDto ApplyAsVolunteer(long citizenId, VolunteerSignupDto dto);
    List<VolunteerAttendanceDto> GetAttendanceCandidates(long organizerId, long eventId);
    EventActionResultDto ConfirmAttendance(long organizerId, long eventId, IReadOnlyList<VolunteerAttendanceDto> attendances);
}
