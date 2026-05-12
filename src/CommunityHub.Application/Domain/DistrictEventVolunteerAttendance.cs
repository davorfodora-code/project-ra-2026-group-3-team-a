namespace CommunityHub.Application.Domain;

public class DistrictEventVolunteerAttendance
{
    public long Id { get; private set; }
    public User Volunteer { get; private set; }
    public bool Attended { get; private set; }

    public DistrictEventVolunteerAttendance(long id, User volunteer, bool attended)
    {
        Id = id;
        Volunteer = volunteer;
        Attended = attended;
    }

    public DistrictEventVolunteerAttendance(User volunteer, bool attended)
    {
        Id = 0;
        Volunteer = volunteer;
        Attended = attended;
    }

    public void SetAttendance(bool attended)
    {
        Attended = attended;
    }
}
