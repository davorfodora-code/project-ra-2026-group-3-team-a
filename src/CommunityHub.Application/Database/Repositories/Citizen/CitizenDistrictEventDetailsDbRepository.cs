using System.Data;
using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Database.Repositories.Citizen;

internal sealed class CitizenDistrictEventDetailsDbRepository
{
    public List<DistrictEventRequiredItem> GetRequiredItems(IDbConnection connection, long eventId)
    {
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT
                ri.id,
                ri.item_name,
                COALESCE(
                    ri.assigned_volunteer_id,
                    (
                        SELECT a.citizen_id
                        FROM district_event_application_items ai
                        JOIN district_event_applications a ON a.id = ai.application_id
                        WHERE ai.required_item_id = ri.id
                        LIMIT 1
                    )
                ) AS assigned_volunteer_id
            FROM district_event_required_items ri
            WHERE ri.event_id = @eventId
            ORDER BY ri.id";
        AddParameter(cmd, "@eventId", eventId);

        List<DistrictEventRequiredItem> items = new();
        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            long? assignedVolunteerId = CitizenDistrictEventMapper.ReadNullableInt64(reader, "assigned_volunteer_id");
            items.Add(new DistrictEventRequiredItem(
                reader.GetInt64(reader.GetOrdinal("id")),
                reader.GetString(reader.GetOrdinal("item_name")),
                assignedVolunteerId));
        }

        return items;
    }

    public List<DistrictEventVolunteerApplication> GetVolunteerApplications(IDbConnection connection, long eventId)
    {
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT
                a.id,
                a.applied_at,
                u.id AS citizen_id,
                u.username,
                u.password,
                u.name,
                u.surname,
                u.birthday,
                u.role,
                u.address
            FROM district_event_applications a
            JOIN users u ON u.id = a.citizen_id
            WHERE a.event_id = @eventId
            ORDER BY a.applied_at";
        AddParameter(cmd, "@eventId", eventId);

        List<VolunteerApplicationShell> shells = new();
        using (IDataReader reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                User citizen = CitizenDistrictEventMapper.MapUser(reader, "citizen_id", "address");
                shells.Add(new VolunteerApplicationShell(
                    reader.GetInt64(reader.GetOrdinal("id")),
                    citizen,
                    reader.GetDateTime(reader.GetOrdinal("applied_at"))));
            }
        }

        List<DistrictEventVolunteerApplication> applications = new();
        foreach (VolunteerApplicationShell shell in shells)
        {
            List<long> selectedItemIds = GetSelectedItemIds(connection, shell.ApplicationId);
            applications.Add(new DistrictEventVolunteerApplication(
                shell.ApplicationId,
                shell.Citizen,
                shell.AppliedAt,
                selectedItemIds));
        }

        return applications;
    }

    public List<DistrictEventVolunteerAttendance> GetVolunteerAttendances(IDbConnection connection, long eventId)
    {
        IDbCommand cmd = CreateVolunteerAttendanceCommand(connection, eventId);
        return ReadVolunteerAttendances(cmd);
    }

    private static IDbCommand CreateVolunteerAttendanceCommand(IDbConnection connection, long eventId)
    {
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = VolunteerAttendancesQuery;
        AddParameter(cmd, "@eventId", eventId);
        return cmd;
    }

    private static List<DistrictEventVolunteerAttendance> ReadVolunteerAttendances(IDbCommand cmd)
    {
        List<DistrictEventVolunteerAttendance> attendances = new();
        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            attendances.Add(MapVolunteerAttendance(reader));
        }

        return attendances;
    }

    private static DistrictEventVolunteerAttendance MapVolunteerAttendance(IDataReader reader)
    {
        User volunteer = CitizenDistrictEventMapper.MapUser(reader, "volunteer_id", "address");
        return new DistrictEventVolunteerAttendance(
            reader.GetInt64(reader.GetOrdinal("id")),
            volunteer,
            reader.GetBoolean(reader.GetOrdinal("attended")));
    }

    private static List<long> GetSelectedItemIds(IDbConnection connection, long applicationId)
    {
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT required_item_id
            FROM district_event_application_items
            WHERE application_id = @applicationId
            ORDER BY id";
        AddParameter(cmd, "@applicationId", applicationId);

        List<long> selectedItemIds = new();
        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            selectedItemIds.Add(reader.GetInt64(reader.GetOrdinal("required_item_id")));
        }

        return selectedItemIds;
    }

    private static void AddParameter(IDbCommand command, string name, object value)
    {
        IDbDataParameter param = command.CreateParameter();
        param.ParameterName = name;
        param.Value = value;
        command.Parameters.Add(param);
    }

    private sealed class VolunteerApplicationShell
    {
        public long ApplicationId { get; }
        public User Citizen { get; }
        public DateTime AppliedAt { get; }

        public VolunteerApplicationShell(long applicationId, User citizen, DateTime appliedAt)
        {
            ApplicationId = applicationId;
            Citizen = citizen;
            AppliedAt = appliedAt;
        }
    }

    private const string VolunteerAttendancesQuery = @"
            SELECT
                a.id,
                a.attended,
                u.id AS volunteer_id,
                u.username,
                u.password,
                u.name,
                u.surname,
                u.birthday,
                u.role,
                u.address
            FROM district_event_volunteer_attendances a
            JOIN users u ON u.id = a.volunteer_id
            WHERE a.event_id = @eventId
            ORDER BY a.id";
}
