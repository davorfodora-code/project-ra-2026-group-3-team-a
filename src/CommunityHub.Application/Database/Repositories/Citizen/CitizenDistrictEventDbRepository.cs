using System.Data;
using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Database.Repositories.Citizen;

public class CitizenDistrictEventDbRepository
{
    public long Create(DistrictEvent districtEvent)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO district_events
                (district_id, organizer_id, name, description, event_date, start_time, duration_minutes, min_volunteers, status, created_at)
            VALUES
                (@districtId, @organizerId, @name, @description, @eventDate, @startTime, @durationMinutes, @minVolunteers, @status, @createdAt)
            RETURNING id";

        AddParameter(cmd, "@districtId", districtEvent.District.Id);
        AddParameter(cmd, "@organizerId", districtEvent.Organizer.Id);
        AddParameter(cmd, "@name", districtEvent.Name);
        AddParameter(cmd, "@description", districtEvent.Description);
        AddParameter(cmd, "@eventDate", districtEvent.EventDate.Date);
        AddParameter(cmd, "@startTime", districtEvent.StartTime);
        AddParameter(cmd, "@durationMinutes", Convert.ToInt32(districtEvent.Duration.TotalMinutes));
        AddParameter(cmd, "@minVolunteers", districtEvent.MinVolunteers);
        AddParameter(cmd, "@status", districtEvent.Status.ToString());
        AddParameter(cmd, "@createdAt", DateTime.Now);

        long eventId = Convert.ToInt64(cmd.ExecuteScalar());
        InsertRequiredItems(connection, eventId, districtEvent.RequiredItems);
        return eventId;
    }

    public List<DistrictEvent> GetAllByDistrictId(long districtId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = CreateBaseEventQuery(connection);
        cmd.CommandText += " WHERE e.district_id = @districtId ORDER BY e.event_date, e.start_time";
        AddParameter(cmd, "@districtId", districtId);

        return ReadEventsWithDetails(connection, cmd);
    }

    public DistrictEvent? GetById(long eventId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = CreateBaseEventQuery(connection);
        cmd.CommandText += " WHERE e.id = @eventId";
        AddParameter(cmd, "@eventId", eventId);

        List<DistrictEvent> events = ReadEventsWithDetails(connection, cmd);
        return events.FirstOrDefault();
    }

    public void UpdateStatus(long eventId, DistrictEventStatus status)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = "UPDATE district_events SET status = @status WHERE id = @eventId";
        AddParameter(cmd, "@status", status.ToString());
        AddParameter(cmd, "@eventId", eventId);
        cmd.ExecuteNonQuery();
    }

    private static IDbCommand CreateBaseEventQuery(IDbConnection connection)
    {
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT
                e.id,
                e.name,
                e.description,
                e.event_date,
                e.start_time,
                e.duration_minutes,
                e.min_volunteers,
                e.status,
                d.id AS district_id,
                d.name AS district_name,
                d.description AS district_description,
                d.budget AS district_budget,
                l.id AS location_id,
                l.city,
                l.country,
                o.id AS organizer_id,
                o.username AS organizer_username,
                o.password AS organizer_password,
                o.name AS organizer_name,
                o.surname AS organizer_surname,
                o.birthday AS organizer_birthday,
                o.role AS organizer_role,
                o.address AS organizer_address
            FROM district_events e
            JOIN districts d ON d.id = e.district_id
            JOIN locations l ON l.id = d.location_id
            JOIN users o ON o.id = e.organizer_id";
        return cmd;
    }

    private static List<DistrictEvent> ReadEventsWithDetails(IDbConnection connection, IDbCommand cmd)
    {
        List<DistrictEvent> events = new List<DistrictEvent>();

        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            events.Add(MapEventShell(reader));
        }

        foreach (DistrictEvent districtEvent in events)
        {
            districtEvent.RequiredItems.Clear();
            districtEvent.RequiredItems.AddRange(GetRequiredItems(connection, districtEvent.Id));

            districtEvent.VolunteerApplications.Clear();
            districtEvent.VolunteerApplications.AddRange(GetVolunteerApplications(connection, districtEvent.Id));

            districtEvent.VolunteerAttendances.Clear();
            districtEvent.VolunteerAttendances.AddRange(GetVolunteerAttendances(connection, districtEvent.Id));
        }

        return events;
    }

    private static DistrictEvent MapEventShell(IDataReader reader)
    {
        Location location = new Location(
            reader.GetInt64(reader.GetOrdinal("location_id")),
            reader.GetString(reader.GetOrdinal("city")),
            reader.GetString(reader.GetOrdinal("country")));

        User coordinatorStub = new User(0, "", "", "", "", DateTime.MinValue, "DistrictCoordinator");
        District district = new District(
            reader.GetInt64(reader.GetOrdinal("district_id")),
            reader.GetString(reader.GetOrdinal("district_name")),
            reader.GetString(reader.GetOrdinal("district_description")),
            location,
            reader.GetDecimal(reader.GetOrdinal("district_budget")),
            coordinatorStub);

        User organizer = new User(
            reader.GetInt64(reader.GetOrdinal("organizer_id")),
            reader.GetString(reader.GetOrdinal("organizer_username")),
            reader.GetString(reader.GetOrdinal("organizer_password")),
            reader.GetString(reader.GetOrdinal("organizer_name")),
            reader.GetString(reader.GetOrdinal("organizer_surname")),
            reader.GetDateTime(reader.GetOrdinal("organizer_birthday")),
            reader.GetString(reader.GetOrdinal("organizer_role")),
            ReadNullableString(reader, "organizer_address"));

        object rawTime = reader.GetValue(reader.GetOrdinal("start_time"));
        TimeSpan startTime = rawTime switch
        {
            TimeOnly t => t.ToTimeSpan(),
            TimeSpan ts => ts,
            _ => throw new InvalidCastException($"Unexpected type for start_time: {rawTime.GetType()}")
        };

        int durationMinutes = reader.GetInt32(reader.GetOrdinal("duration_minutes"));
        DistrictEventStatus status = Enum.Parse<DistrictEventStatus>(reader.GetString(reader.GetOrdinal("status")));

        return new DistrictEvent(
            reader.GetInt64(reader.GetOrdinal("id")),
            district,
            organizer,
            reader.GetString(reader.GetOrdinal("name")),
            reader.GetString(reader.GetOrdinal("description")),
            reader.GetDateTime(reader.GetOrdinal("event_date")),
            startTime,
            TimeSpan.FromMinutes(durationMinutes),
            reader.GetInt32(reader.GetOrdinal("min_volunteers")),
            status,
            new List<DistrictEventRequiredItem>(),
            new List<DistrictEventVolunteerApplication>(),
            new List<DistrictEventVolunteerAttendance>());
    }

    private static List<DistrictEventRequiredItem> GetRequiredItems(IDbConnection connection, long eventId)
    {
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT id, item_name, assigned_volunteer_id
            FROM district_event_required_items
            WHERE event_id = @eventId
            ORDER BY id";
        AddParameter(cmd, "@eventId", eventId);

        List<DistrictEventRequiredItem> items = new List<DistrictEventRequiredItem>();
        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            long? assignedVolunteerId = ReadNullableInt64(reader, "assigned_volunteer_id");
            items.Add(new DistrictEventRequiredItem(
                reader.GetInt64(reader.GetOrdinal("id")),
                reader.GetString(reader.GetOrdinal("item_name")),
                assignedVolunteerId));
        }

        return items;
    }

    private static List<DistrictEventVolunteerApplication> GetVolunteerApplications(IDbConnection connection, long eventId)
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

        List<DistrictEventVolunteerApplication> applications = new List<DistrictEventVolunteerApplication>();
        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            User citizen = new User(
                reader.GetInt64(reader.GetOrdinal("citizen_id")),
                reader.GetString(reader.GetOrdinal("username")),
                reader.GetString(reader.GetOrdinal("password")),
                reader.GetString(reader.GetOrdinal("name")),
                reader.GetString(reader.GetOrdinal("surname")),
                reader.GetDateTime(reader.GetOrdinal("birthday")),
                reader.GetString(reader.GetOrdinal("role")),
                ReadNullableString(reader, "address"));

            long applicationId = reader.GetInt64(reader.GetOrdinal("id"));
            List<long> selectedItemIds = GetSelectedItemIds(connection, applicationId);

            applications.Add(new DistrictEventVolunteerApplication(
                applicationId,
                citizen,
                reader.GetDateTime(reader.GetOrdinal("applied_at")),
                selectedItemIds));
        }

        return applications;
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

        List<long> selectedItemIds = new List<long>();
        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            selectedItemIds.Add(reader.GetInt64(reader.GetOrdinal("required_item_id")));
        }

        return selectedItemIds;
    }

    private static List<DistrictEventVolunteerAttendance> GetVolunteerAttendances(IDbConnection connection, long eventId)
    {
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
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
        AddParameter(cmd, "@eventId", eventId);

        List<DistrictEventVolunteerAttendance> attendances = new List<DistrictEventVolunteerAttendance>();
        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            User volunteer = new User(
                reader.GetInt64(reader.GetOrdinal("volunteer_id")),
                reader.GetString(reader.GetOrdinal("username")),
                reader.GetString(reader.GetOrdinal("password")),
                reader.GetString(reader.GetOrdinal("name")),
                reader.GetString(reader.GetOrdinal("surname")),
                reader.GetDateTime(reader.GetOrdinal("birthday")),
                reader.GetString(reader.GetOrdinal("role")),
                ReadNullableString(reader, "address"));

            attendances.Add(new DistrictEventVolunteerAttendance(
                reader.GetInt64(reader.GetOrdinal("id")),
                volunteer,
                reader.GetBoolean(reader.GetOrdinal("attended"))));
        }

        return attendances;
    }

    private static void InsertRequiredItems(IDbConnection connection, long eventId, IEnumerable<DistrictEventRequiredItem> items)
    {
        foreach (DistrictEventRequiredItem item in items)
        {
            IDbCommand cmd = connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO district_event_required_items (event_id, item_name, assigned_volunteer_id)
                VALUES (@eventId, @itemName, @assignedVolunteerId)";

            AddParameter(cmd, "@eventId", eventId);
            AddParameter(cmd, "@itemName", item.Name);
            AddParameter(cmd, "@assignedVolunteerId", (object?)item.AssignedVolunteerId ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }
    }

    private static long? ReadNullableInt64(IDataReader reader, string columnName)
    {
        int ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetInt64(ordinal);
    }

    private static string? ReadNullableString(IDataReader reader, string columnName)
    {
        int ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }

    private static void AddParameter(IDbCommand command, string name, object value)
    {
        IDbDataParameter param = command.CreateParameter();
        param.ParameterName = name;
        param.Value = value;
        command.Parameters.Add(param);
    }
}
