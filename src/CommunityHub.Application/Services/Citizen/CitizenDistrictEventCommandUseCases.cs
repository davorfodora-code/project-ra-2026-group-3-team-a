using CommunityHub.Application.Domain;
using CommunityHub.Application.Dto.Citizen;

namespace CommunityHub.Application.Services.Citizen;

internal sealed class CitizenDistrictEventCommandUseCases
{
    private readonly ICitizenDistrictEventRepository _eventRepository;
    private readonly ICitizenDistrictEventApplicationRepository _applicationRepository;
    private readonly ICitizenDistrictMembershipRepository _membershipRepository;
    private readonly ICitizenDistrictRepository _districtRepository;

    public CitizenDistrictEventCommandUseCases(
        ICitizenDistrictEventRepository eventRepository,
        ICitizenDistrictEventApplicationRepository applicationRepository,
        ICitizenDistrictMembershipRepository membershipRepository,
        ICitizenDistrictRepository districtRepository)
    {
        _eventRepository = eventRepository;
        _applicationRepository = applicationRepository;
        _membershipRepository = membershipRepository;
        _districtRepository = districtRepository;
    }

    public EventActionResultDto CreateEvent(long citizenId, CreateDistrictEventDto dto)
    {
        EventActionResultDto? membershipValidation = ValidateOrganizerMembership(citizenId, dto.DistrictId);
        if (membershipValidation != null)
        {
            return membershipValidation;
        }

        District? district = GetDistrict(dto.DistrictId);
        if (district == null)
        {
            return Failure("Kvart nije pronadjen.");
        }

        try
        {
            DistrictEvent districtEvent = BuildNewDistrictEvent(citizenId, dto, district);
            _eventRepository.Create(districtEvent);
            return Success("Dogadjaj je uspesno kreiran.");
        }
        catch (InvalidOperationException ex)
        {
            return Failure(ex.Message);
        }
    }

    public EventActionResultDto ApplyAsVolunteer(long citizenId, VolunteerSignupDto dto)
    {
        EventActionResultDto? accessValidation = TryLoadVolunteerTargetEvent(citizenId, dto.EventId, out DistrictEvent? districtEvent);
        if (accessValidation != null)
        {
            return accessValidation;
        }

        bool alreadyApplied = _applicationRepository.ExistsByEventAndCitizen(dto.EventId, citizenId);
        if (alreadyApplied && (dto.SelectedItemIds == null || dto.SelectedItemIds.Count == 0))
        {
            return Failure("Vec ste prijavljeni kao volonter. Izaberite bar jednu slobodnu stavku za dopunu prijave.");
        }

        try
        {
            ApplyVolunteerRegistration(dto, citizenId, districtEvent!, alreadyApplied);

            return Success(alreadyApplied
                ? "Uspesno ste dopunili stavke koje donosite."
                : "Uspesno ste prijavljeni kao volonter.");
        }
        catch (InvalidOperationException ex)
        {
            return Failure(ex.Message);
        }
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

    private EventActionResultDto? ValidateOrganizerMembership(long citizenId, long districtId)
    {
        long? membershipDistrictId = _membershipRepository.GetMembershipDistrictId(citizenId);
        if (membershipDistrictId == null)
        {
            return Failure("Samo clan kvarta moze da organizuje dogadjaj.");
        }

        if (membershipDistrictId != districtId)
        {
            return Failure("Dogadjaj mozete organizovati samo u svom kvartu.");
        }

        return null;
    }

    private District? GetDistrict(long districtId)
    {
        return _districtRepository.GetAll().FirstOrDefault(d => d.Id == districtId);
    }

    private static DistrictEvent BuildNewDistrictEvent(long citizenId, CreateDistrictEventDto dto, District district)
    {
        List<DistrictEventRequiredItem> requiredItems = dto.RequiredItems
            .Where(i => !string.IsNullOrWhiteSpace(i))
            .Select(i => new DistrictEventRequiredItem(i.Trim()))
            .ToList();

        return new DistrictEvent(
            district,
            CreateCitizenStub(citizenId),
            dto.Name,
            dto.Description,
            dto.EventDate,
            dto.StartTime,
            dto.Duration,
            dto.MinVolunteers,
            requiredItems);
    }

    private EventActionResultDto? TryLoadVolunteerTargetEvent(long citizenId, long eventId, out DistrictEvent? districtEvent)
    {
        districtEvent = null;

        long? districtId = _membershipRepository.GetMembershipDistrictId(citizenId);
        if (districtId == null)
        {
            return Failure("Samo clan kvarta moze da se prijavi kao volonter.");
        }

        districtEvent = _eventRepository.GetById(eventId);
        if (districtEvent == null)
        {
            return Failure("Dogadjaj nije pronadjen.");
        }

        if (districtEvent.District.Id != districtId.Value)
        {
            return Failure("Nije dozvoljena prijava na dogadjaj van vaseg kvarta.");
        }

        RefreshAndPersistStatus(districtEvent);
        return null;
    }

    private void ApplyVolunteerRegistration(VolunteerSignupDto dto, long citizenId, DistrictEvent districtEvent, bool alreadyApplied)
    {
        DistrictEventStatus previousStatus = districtEvent.Status;
        DateTime operationTime = DateTime.Now;

        if (alreadyApplied)
        {
            districtEvent.AddItemsForExistingVolunteer(citizenId, operationTime, dto.SelectedItemIds);
            _applicationRepository.AddItemsToExistingApplication(dto.EventId, citizenId, dto.SelectedItemIds);
        }
        else
        {
            districtEvent.RegisterVolunteer(CreateCitizenStub(citizenId), operationTime, dto.SelectedItemIds);
            _applicationRepository.CreateApplication(dto.EventId, citizenId, operationTime, dto.SelectedItemIds);
        }

        if (districtEvent.Status != previousStatus)
        {
            _eventRepository.UpdateStatus(dto.EventId, districtEvent.Status);
        }
    }

    private static User CreateCitizenStub(long citizenId) => new(citizenId, "", "", "", "", DateTime.MinValue, "Citizen");
    private static EventActionResultDto Success(string message) => new() { Success = true, Message = message };
    private static EventActionResultDto Failure(string message) => new() { Success = false, Message = message };
}
