using System.Data;
using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Database.Repositories.Citizen;

internal sealed class CitizenDistrictEventReadDbRepository
{
    private readonly CitizenDistrictEventDetailsDbRepository _detailsRepository = new();

    public List<DistrictEvent> GetAllByDistrictId(long districtId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = CreateBaseEventQueryCommand(connection);
        cmd.CommandText += " WHERE e.district_id = @districtId ORDER BY e.event_date, e.start_time";
        AddParameter(cmd, "@districtId", districtId);

        return ReadEventsWithDetails(connection, cmd);
    }

    public DistrictEvent? GetById(long eventId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = CreateBaseEventQueryCommand(connection);
        cmd.CommandText += " WHERE e.id = @eventId";
        AddParameter(cmd, "@eventId", eventId);

        List<DistrictEvent> events = ReadEventsWithDetails(connection, cmd);
        return events.FirstOrDefault();
    }

    private List<DistrictEvent> ReadEventsWithDetails(IDbConnection connection, IDbCommand cmd)
    {
        List<DistrictEvent> events = new();

        using (IDataReader reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                events.Add(CitizenDistrictEventMapper.MapEventShell(reader));
            }
        }

        foreach (DistrictEvent districtEvent in events)
        {
            districtEvent.RequiredItems.Clear();
            districtEvent.RequiredItems.AddRange(_detailsRepository.GetRequiredItems(connection, districtEvent.Id));

            districtEvent.VolunteerApplications.Clear();
            districtEvent.VolunteerApplications.AddRange(_detailsRepository.GetVolunteerApplications(connection, districtEvent.Id));

            districtEvent.VolunteerAttendances.Clear();
            districtEvent.VolunteerAttendances.AddRange(_detailsRepository.GetVolunteerAttendances(connection, districtEvent.Id));
        }

        return events;
    }

    private static IDbCommand CreateBaseEventQueryCommand(IDbConnection connection)
    {
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = BaseEventQuery;
        return cmd;
    }

    private static void AddParameter(IDbCommand command, string name, object value)
    {
        IDbDataParameter param = command.CreateParameter();
        param.ParameterName = name;
        param.Value = value;
        command.Parameters.Add(param);
    }

    private const string BaseEventQuery = @"
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
}
