using CommunityHub.Application;
using CommunityHub.Application.Dto.Citizen;

namespace CommunityHub.Application.Services.Citizen;

public class CitizenDistrictEventService : ICitizenDistrictEventService
{
    private readonly CitizenDistrictEventQueryUseCases _queryUseCases;
    private readonly CitizenDistrictEventCommandUseCases _commandUseCases;

    public CitizenDistrictEventService()
    {
        ICitizenDistrictEventRepository eventRepository = Injector.Get<ICitizenDistrictEventRepository>();
        ICitizenDistrictEventApplicationRepository applicationRepository = Injector.Get<ICitizenDistrictEventApplicationRepository>();
        ICitizenDistrictMembershipRepository membershipRepository = Injector.Get<ICitizenDistrictMembershipRepository>();
        ICitizenDistrictRepository districtRepository = Injector.Get<ICitizenDistrictRepository>();

        _queryUseCases = new CitizenDistrictEventQueryUseCases(eventRepository, membershipRepository);
        _commandUseCases = new CitizenDistrictEventCommandUseCases(
            eventRepository,
            applicationRepository,
            membershipRepository,
            districtRepository);
    }

    public long? GetCitizenDistrictId(long citizenId) => _queryUseCases.GetCitizenDistrictId(citizenId);

    public EventActionResultDto CreateEvent(long citizenId, CreateDistrictEventDto dto) => _commandUseCases.CreateEvent(citizenId, dto);

    public List<DistrictEventListItemDto> GetMyDistrictEvents(long citizenId) => _queryUseCases.GetMyDistrictEvents(citizenId);

    public DistrictEventDetailsDto? GetEventDetailsForCitizen(long citizenId, long eventId) => _queryUseCases.GetEventDetailsForCitizen(citizenId, eventId);

    public EventActionResultDto ApplyAsVolunteer(long citizenId, VolunteerSignupDto dto) => _commandUseCases.ApplyAsVolunteer(citizenId, dto);

    public List<VolunteerAttendanceDto> GetAttendanceCandidates(long organizerId, long eventId) => _queryUseCases.GetAttendanceCandidates(organizerId, eventId);

    public EventActionResultDto ConfirmAttendance(long organizerId, long eventId, IReadOnlyList<VolunteerAttendanceDto> attendances)
        => _commandUseCases.ConfirmAttendance(organizerId, eventId, attendances);
}
