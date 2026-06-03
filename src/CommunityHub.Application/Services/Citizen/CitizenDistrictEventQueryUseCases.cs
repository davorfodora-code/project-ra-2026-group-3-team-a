using CommunityHub.Application.Domain;
using CommunityHub.Application.Dto.Citizen;

namespace CommunityHub.Application.Services.Citizen;

internal sealed class CitizenDistrictEventQueryUseCases
{
    private readonly ICitizenDistrictEventRepository _eventRepository;
    private readonly ICitizenDistrictMembershipRepository _membershipRepository;
    private readonly ICitizenTrustRecordService _trustRecordService;

    public CitizenDistrictEventQueryUseCases(
        ICitizenDistrictEventRepository eventRepository,
        ICitizenDistrictMembershipRepository membershipRepository,
        ICitizenTrustRecordService trustRecordService)
    {
        _eventRepository = eventRepository;
        _membershipRepository = membershipRepository;
        _trustRecordService = trustRecordService;
    }

    public long? GetCitizenDistrictId(long citizenId) => _membershipRepository.GetMembershipDistrictId(citizenId);

    public List<DistrictEventListItemDto> GetMyDistrictEvents(long citizenId)
    {
        long? districtId = _membershipRepository.GetMembershipDistrictId(citizenId);
        if (districtId == null)
        {
            return new List<DistrictEventListItemDto>();
        }

        List<DistrictEvent> events = _eventRepository.GetAllByDistrictId(districtId.Value);
        foreach (DistrictEvent districtEvent in events)
        {
            RefreshAndPersistStatus(districtEvent);
        }

        return events.Select(e => CitizenDistrictEventDtoMapper.MapToListItemDto(e, citizenId)).ToList();
    }

    public DistrictEventDetailsDto? GetEventDetailsForCitizen(long citizenId, long eventId)
    {
        long? districtId = _membershipRepository.GetMembershipDistrictId(citizenId);
        if (districtId == null)
        {
            return null;
        }

        DistrictEvent? districtEvent = _eventRepository.GetById(eventId);
        if (districtEvent == null || districtEvent.District.Id != districtId.Value)
        {
            return null;
        }

        RefreshAndPersistStatus(districtEvent);
        return CitizenDistrictEventDtoMapper.MapToDetailsDto(districtEvent, citizenId);
    }

    public List<VolunteerAttendanceDto> GetAttendanceCandidates(long organizerId, long eventId)
    {
        DistrictEvent? districtEvent = _eventRepository.GetById(eventId);
        if (districtEvent == null || districtEvent.Organizer.Id != organizerId)
        {
            return new List<VolunteerAttendanceDto>();
        }

        RefreshAndPersistStatus(districtEvent);

        Dictionary<long, bool> attendanceMap = districtEvent.VolunteerAttendances
            .ToDictionary(a => a.Volunteer.Id, a => a.Attended);

        return districtEvent.VolunteerApplications
            .Select(a => new VolunteerAttendanceDto
            {
                VolunteerId = a.Citizen.Id,
                VolunteerName = $"{a.Citizen.Name} {a.Citizen.Surname}".Trim(),
                Attended = attendanceMap.TryGetValue(a.Citizen.Id, out bool attended) && attended
            })
            .OrderBy(v => v.VolunteerName)
            .ToList();
    }

    private void RefreshAndPersistStatus(DistrictEvent districtEvent)
    {
        DistrictEventStatus oldStatus = districtEvent.Status;
        districtEvent.RefreshStatus(DateTime.Now);
        if (districtEvent.Status != oldStatus)
        {
            _eventRepository.UpdateStatus(districtEvent.Id, districtEvent.Status);
            _trustRecordService.RefreshTrustForCitizen(districtEvent.Organizer.Id, districtEvent.District.Id);
        }
    }
}
