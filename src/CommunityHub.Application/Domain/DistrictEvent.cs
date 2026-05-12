namespace CommunityHub.Application.Domain;

public class DistrictEvent
{
    public long Id { get; private set; }
    public District District { get; private set; }
    public User Organizer { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public DateTime EventDate { get; private set; }
    public TimeSpan StartTime { get; private set; }
    public TimeSpan Duration { get; private set; }
    public int MinVolunteers { get; private set; }
    public DistrictEventStatus Status { get; private set; }
    public List<DistrictEventRequiredItem> RequiredItems { get; private set; }
    public List<DistrictEventVolunteerApplication> VolunteerApplications { get; private set; }
    public List<DistrictEventVolunteerAttendance> VolunteerAttendances { get; private set; }

    public DistrictEvent(
        long id,
        District district,
        User organizer,
        string name,
        string description,
        DateTime eventDate,
        TimeSpan startTime,
        TimeSpan duration,
        int minVolunteers,
        DistrictEventStatus status,
        IEnumerable<DistrictEventRequiredItem> requiredItems,
        IEnumerable<DistrictEventVolunteerApplication>? volunteerApplications = null,
        IEnumerable<DistrictEventVolunteerAttendance>? volunteerAttendances = null)
    {
        Id = id;
        District = district;
        Organizer = organizer;
        Name = name;
        Description = description;
        EventDate = eventDate.Date;
        StartTime = startTime;
        Duration = duration;
        MinVolunteers = minVolunteers;
        Status = status;
        RequiredItems = requiredItems.ToList();
        VolunteerApplications = volunteerApplications?.ToList() ?? new List<DistrictEventVolunteerApplication>();
        VolunteerAttendances = volunteerAttendances?.ToList() ?? new List<DistrictEventVolunteerAttendance>();

        EnsureValidCreationData();
    }

    public DistrictEvent(
        District district,
        User organizer,
        string name,
        string description,
        DateTime eventDate,
        TimeSpan startTime,
        TimeSpan duration,
        int minVolunteers,
        IEnumerable<DistrictEventRequiredItem> requiredItems)
    {
        Id = 0;
        District = district;
        Organizer = organizer;
        Name = name;
        Description = description;
        EventDate = eventDate.Date;
        StartTime = startTime;
        Duration = duration;
        MinVolunteers = minVolunteers;
        Status = DistrictEventStatus.UPripremi;
        RequiredItems = requiredItems.ToList();
        VolunteerApplications = new List<DistrictEventVolunteerApplication>();
        VolunteerAttendances = new List<DistrictEventVolunteerAttendance>();

        EnsureValidCreationData();
    }

    public DateTime GetEventStartDateTime()
    {
        return EventDate.Date.Add(StartTime);
    }

    public DateTime GetEventEndDateTime()
    {
        return GetEventStartDateTime().Add(Duration);
    }

    public bool CanAcceptVolunteerApplications(DateTime now)
    {
        return Status == DistrictEventStatus.UPripremi && now < GetEventStartDateTime();
    }

    public bool HasVolunteer(long citizenId)
    {
        return VolunteerApplications.Any(a => a.Citizen.Id == citizenId);
    }

    public void RegisterVolunteer(User citizen, DateTime now, IEnumerable<long>? selectedItemIds = null)
    {
        if (!CanAcceptVolunteerApplications(now))
            throw new InvalidOperationException("Prijava nije dozvoljena jer dogadjaj nije u pripremi.");
        if (HasVolunteer(citizen.Id))
            throw new InvalidOperationException("Gradjanin je vec prijavljen kao volonter.");

        List<long> chosenIds = selectedItemIds?.Distinct().ToList() ?? new List<long>();

        foreach (long itemId in chosenIds)
        {
            DistrictEventRequiredItem item = RequiredItems.FirstOrDefault(i => i.Id == itemId)
                ?? throw new InvalidOperationException("Odabrana stavka ne postoji.");
            item.AssignToVolunteer(citizen.Id);
        }

        VolunteerApplications.Add(new DistrictEventVolunteerApplication(citizen, now, chosenIds));
        TryScheduleIfReady();
    }

    public bool IsReadyToSchedule()
    {
        return VolunteerApplications.Count >= MinVolunteers && RequiredItems.All(i => i.IsCovered);
    }

    public void TryScheduleIfReady()
    {
        if (Status == DistrictEventStatus.UPripremi && IsReadyToSchedule())
        {
            Status = DistrictEventStatus.Zakazan;
        }
    }

    public void RefreshStatus(DateTime now)
    {
        if (Status == DistrictEventStatus.Otkazan || Status == DistrictEventStatus.Zavrsen)
        {
            return;
        }

        if (Status == DistrictEventStatus.Zakazan)
        {
            if (now >= GetEventEndDateTime())
            {
                Status = DistrictEventStatus.Zavrsen;
            }

            return;
        }

        if (IsReadyToSchedule())
        {
            Status = DistrictEventStatus.Zakazan;
            return;
        }

        DateTime cancellationThreshold = GetEventStartDateTime().AddHours(-12);
        if (now >= cancellationThreshold)
        {
            Status = DistrictEventStatus.Otkazan;
        }
    }

    public bool CanOrganizerMarkAttendance(long organizerId, DateTime now)
    {
        RefreshStatus(now);
        return Organizer.Id == organizerId && Status == DistrictEventStatus.Zavrsen;
    }

    public void MarkVolunteerAttendance(long organizerId, long volunteerId, bool attended, DateTime now)
    {
        if (!CanOrganizerMarkAttendance(organizerId, now))
            throw new InvalidOperationException("Prisustvo moze da evidentira samo organizator nakon zavrsetka dogadjaja.");

        DistrictEventVolunteerApplication volunteerApplication = VolunteerApplications
            .FirstOrDefault(a => a.Citizen.Id == volunteerId)
            ?? throw new InvalidOperationException("Nije moguce evidentirati prisustvo za neprijavljenog volontera.");

        DistrictEventVolunteerAttendance? existing = VolunteerAttendances
            .FirstOrDefault(a => a.Volunteer.Id == volunteerApplication.Citizen.Id);

        if (existing != null)
        {
            existing.SetAttendance(attended);
            return;
        }

        VolunteerAttendances.Add(new DistrictEventVolunteerAttendance(volunteerApplication.Citizen, attended));
    }

    private void EnsureValidCreationData()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new InvalidOperationException("Naziv dogadjaja je obavezan.");
        if (MinVolunteers <= 0)
            throw new InvalidOperationException("Minimalan broj volontera mora biti veci od 0.");
        if (Duration <= TimeSpan.Zero)
            throw new InvalidOperationException("Trajanje dogadjaja mora biti vece od 0.");
        if (RequiredItems.Count == 0)
            throw new InvalidOperationException("Spisak potrebnih stvari ne sme biti prazan.");
        if (RequiredItems.Any(i => string.IsNullOrWhiteSpace(i.Name)))
            throw new InvalidOperationException("Naziv svake potrebne stvari je obavezan.");
    }
}
