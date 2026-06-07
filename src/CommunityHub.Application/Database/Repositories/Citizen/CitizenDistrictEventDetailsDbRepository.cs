using System.Data;
using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Database.Repositories.Citizen;

internal sealed class CitizenDistrictEventDetailsDbRepository
{
    public List<DistrictEventRequiredItem> GetRequiredItems(IDbConnection connection, long eventId)
    {
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = RequiredItemsQuery;
        AddParameter(cmd, "@eventId", eventId);

        return ReadList(cmd, MapRequiredItem);
    }

    public List<DistrictEventVolunteerApplication> GetVolunteerApplications(IDbConnection connection, long eventId)
    {
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = VolunteerApplicationsQuery;
        AddParameter(cmd, "@eventId", eventId);

        List<VolunteerApplicationShell> shells = ReadList(cmd, MapVolunteerApplicationShell);
        return shells.Select(shell => BuildApplication(connection, shell)).ToList();
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
        return ReadList(cmd, MapVolunteerAttendance);
    }

    private static DistrictEventRequiredItem MapRequiredItem(IDataReader reader)
    {
        long? assignedVolunteerId = CitizenDistrictEventMapper.ReadNullableInt64(reader, "assigned_volunteer_id");
        return new DistrictEventRequiredItem(
            reader.GetInt64(reader.GetOrdinal("id")),
            reader.GetString(reader.GetOrdinal("item_name")),
            assignedVolunteerId);
    }

    private static VolunteerApplicationShell MapVolunteerApplicationShell(IDataReader reader)
    {
        User citizen = CitizenDistrictEventMapper.MapUser(reader, "citizen_id", "address");
        return new VolunteerApplicationShell(
            reader.GetInt64(reader.GetOrdinal("id")),
            citizen,
            reader.GetDateTime(reader.GetOrdinal("applied_at")));
    }

    private static DistrictEventVolunteerApplication BuildApplication(
        IDbConnection connection,
        VolunteerApplicationShell shell)
    {
        return new DistrictEventVolunteerApplication(
            shell.ApplicationId,
            shell.Citizen,
            shell.AppliedAt,
            GetSelectedItemIds(connection, shell.ApplicationId));
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

    private static List<T> ReadList<T>(IDbCommand command, Func<IDataReader, T> map)
    {
        List<T> items = new();
        using IDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            items.Add(map(reader));
        }

        return items;
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

    private const string RequiredItemsQuery = @"
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

    private const string VolunteerApplicationsQuery = @"
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
