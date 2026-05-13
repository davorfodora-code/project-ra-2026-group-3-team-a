using CommunityHub.Application.Database.Repositories.Citizen;
using CommunityHub.Application.Domain;
using CommunityHub.Application.Dto.Citizen;

namespace CommunityHub.Application.Services.Citizen;

public class CitizenDistrictEventService
{
    private readonly CitizenDistrictEventDbRepository _eventRepository;
    private readonly CitizenDistrictEventApplicationDbRepository _applicationRepository;
    private readonly CitizenDistrictMembershipDbRepository _membershipRepository;
    private readonly CitizenDistrictDbRepository _districtRepository;

    public CitizenDistrictEventService()
    {
        _eventRepository = new CitizenDistrictEventDbRepository();
        _applicationRepository = new CitizenDistrictEventApplicationDbRepository();
        _membershipRepository = new CitizenDistrictMembershipDbRepository();
        _districtRepository = new CitizenDistrictDbRepository();
    }

    public long? GetCitizenDistrictId(long citizenId)
    {
        return _membershipRepository.GetMembershipDistrictId(citizenId);
    }

    public EventActionResultDto CreateEvent(long citizenId, CreateDistrictEventDto dto)
    {
        long? membershipDistrictId = _membershipRepository.GetMembershipDistrictId(citizenId);
        if (membershipDistrictId == null)
        {
            return Failure("Samo clan kvarta moze da organizuje dogadjaj.");
        }

        if (membershipDistrictId != dto.DistrictId)
        {
            return Failure("Dogadjaj mozete organizovati samo u svom kvartu.");
        }

        District? district = _districtRepository.GetAll().FirstOrDefault(d => d.Id == dto.DistrictId);
        if (district == null)
        {
            return Failure("Kvart nije pronadjen.");
        }

        User organizerStub = CreateCitizenStub(citizenId);
        List<DistrictEventRequiredItem> requiredItems = dto.RequiredItems
            .Where(i => !string.IsNullOrWhiteSpace(i))
            .Select(i => new DistrictEventRequiredItem(i.Trim()))
            .ToList();

        try
        {
            DistrictEvent districtEvent = new DistrictEvent(
                district,
                organizerStub,
                dto.Name,
                dto.Description,
                dto.EventDate,
                dto.StartTime,
                dto.Duration,
                dto.MinVolunteers,
                requiredItems);

            _eventRepository.Create(districtEvent);
            return Success("Dogadjaj je uspesno kreiran.");
        }
        catch (InvalidOperationException ex)
        {
            return Failure(ex.Message);
        }
    }

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

        return events.Select(e => MapToListItemDto(e, citizenId)).ToList();
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
        return MapToDetailsDto(districtEvent, citizenId);
    }

    public EventActionResultDto ApplyAsVolunteer(long citizenId, VolunteerSignupDto dto)
    {
        long? districtId = _membershipRepository.GetMembershipDistrictId(citizenId);
        if (districtId == null)
        {
            return Failure("Samo clan kvarta moze da se prijavi kao volonter.");
        }

        DistrictEvent? districtEvent = _eventRepository.GetById(dto.EventId);
        if (districtEvent == null)
        {
            return Failure("Dogadjaj nije pronadjen.");
        }

        if (districtEvent.District.Id != districtId.Value)
        {
            return Failure("Nije dozvoljena prijava na dogadjaj van vaseg kvarta.");
        }

        RefreshAndPersistStatus(districtEvent);

        bool alreadyApplied = _applicationRepository.ExistsByEventAndCitizen(dto.EventId, citizenId);
        if (alreadyApplied && (dto.SelectedItemIds == null || dto.SelectedItemIds.Count == 0))
        {
            return Failure("Vec ste prijavljeni kao volonter. Izaberite bar jednu slobodnu stavku za dopunu prijave.");
        }

        try
        {
            DistrictEventStatus previousStatus = districtEvent.Status;
            if (alreadyApplied)
            {
                districtEvent.AddItemsForExistingVolunteer(citizenId, DateTime.Now, dto.SelectedItemIds);
                _applicationRepository.AddItemsToExistingApplication(dto.EventId, citizenId, dto.SelectedItemIds);
            }
            else
            {
                districtEvent.RegisterVolunteer(CreateCitizenStub(citizenId), DateTime.Now, dto.SelectedItemIds);
                _applicationRepository.CreateApplication(dto.EventId, citizenId, DateTime.Now, dto.SelectedItemIds);
            }

            if (districtEvent.Status != previousStatus)
            {
                _eventRepository.UpdateStatus(dto.EventId, districtEvent.Status);
            }

            return Success(alreadyApplied
                ? "Uspesno ste dopunili stavke koje donosite."
                : "Uspesno ste prijavljeni kao volonter.");
        }
        catch (InvalidOperationException ex)
        {
            return Failure(ex.Message);
        }
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

    public EventActionResultDto ConfirmAttendance(long organizerId, long eventId, IReadOnlyList<VolunteerAttendanceDto> attendances)
    {
        DistrictEvent? districtEvent = _eventRepository.GetById(eventId);
        if (districtEvent == null)
        {
            return Failure("Dogadjaj nije pronadjen.");
        }

        RefreshAndPersistStatus(districtEvent);

        if (!districtEvent.CanOrganizerMarkAttendance(organizerId, DateTime.Now))
        {
            return Failure("Prisustvo moze da evidentira samo organizator nakon zavrsetka dogadjaja.");
        }

        try
        {
            foreach (VolunteerAttendanceDto attendance in attendances)
            {
                districtEvent.MarkVolunteerAttendance(organizerId, attendance.VolunteerId, attendance.Attended, DateTime.Now);
                _applicationRepository.UpsertAttendance(eventId, attendance.VolunteerId, attendance.Attended, DateTime.Now);
            }

            return Success("Prisustvo volontera je uspesno evidentirano.");
        }
        catch (InvalidOperationException ex)
        {
            return Failure(ex.Message);
        }
    }

    private void RefreshAndPersistStatus(DistrictEvent districtEvent)
    {
        DistrictEventStatus oldStatus = districtEvent.Status;
        districtEvent.RefreshStatus(DateTime.Now);
        if (districtEvent.Status != oldStatus)
        {
            _eventRepository.UpdateStatus(districtEvent.Id, districtEvent.Status);
        }
    }

    private static DistrictEventListItemDto MapToListItemDto(DistrictEvent districtEvent, long citizenId)
    {
        return new DistrictEventListItemDto
        {
            EventId = districtEvent.Id,
            Name = districtEvent.Name,
            Description = districtEvent.Description,
            EventDate = districtEvent.EventDate,
            StartTime = districtEvent.StartTime,
            Duration = districtEvent.Duration,
            MinVolunteers = districtEvent.MinVolunteers,
            CurrentVolunteers = districtEvent.VolunteerApplications.Count,
            Status = districtEvent.Status,
            IsOrganizer = districtEvent.Organizer.Id == citizenId,
            RequiredItems = MapRequiredItems(districtEvent)
        };
    }

    private static DistrictEventDetailsDto MapToDetailsDto(DistrictEvent districtEvent, long citizenId)
    {
        return new DistrictEventDetailsDto
        {
            EventId = districtEvent.Id,
            DistrictId = districtEvent.District.Id,
            DistrictName = districtEvent.District.Name,
            OrganizerId = districtEvent.Organizer.Id,
            OrganizerName = $"{districtEvent.Organizer.Name} {districtEvent.Organizer.Surname}".Trim(),
            Name = districtEvent.Name,
            Description = districtEvent.Description,
            EventDate = districtEvent.EventDate,
            StartTime = districtEvent.StartTime,
            Duration = districtEvent.Duration,
            MinVolunteers = districtEvent.MinVolunteers,
            CurrentVolunteers = districtEvent.VolunteerApplications.Count,
            Status = districtEvent.Status,
            IsCitizenAlreadyApplied = districtEvent.VolunteerApplications.Any(a => a.Citizen.Id == citizenId),
            RequiredItems = MapRequiredItems(districtEvent)
        };
    }

    private static List<EventRequiredItemDto> MapRequiredItems(DistrictEvent districtEvent)
    {
        Dictionary<long, string> citizenNames = districtEvent.VolunteerApplications
            .GroupBy(a => a.Citizen.Id)
            .ToDictionary(
                g => g.Key,
                g => $"{g.First().Citizen.Name} {g.First().Citizen.Surname}".Trim());

        return districtEvent.RequiredItems
            .Select(item => new EventRequiredItemDto
            {
                ItemId = item.Id,
                Name = item.Name,
                AssignedVolunteerId = item.AssignedVolunteerId,
                AssignedVolunteerName = ResolveVolunteerName(citizenNames, item.AssignedVolunteerId)
            })
            .ToList();
    }

    private static string? ResolveVolunteerName(Dictionary<long, string> citizenNames, long? volunteerId)
    {
        if (!volunteerId.HasValue)
        {
            return null;
        }

        return citizenNames.TryGetValue(volunteerId.Value, out string? name) ? name : null;
    }

    private static User CreateCitizenStub(long citizenId)
    {
        return new User(citizenId, "", "", "", "", DateTime.MinValue, "Citizen");
    }

    private static EventActionResultDto Success(string message) => new() { Success = true, Message = message };
    private static EventActionResultDto Failure(string message) => new() { Success = false, Message = message };
}
