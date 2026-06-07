using System.Data;
using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Database.Repositories.Citizen;

internal sealed class CitizenDistrictEventWriteDbRepository
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

    public void UpdateStatus(long eventId, DistrictEventStatus status)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = "UPDATE district_events SET status = @status WHERE id = @eventId";
        AddParameter(cmd, "@status", status.ToString());
        AddParameter(cmd, "@eventId", eventId);
        cmd.ExecuteNonQuery();
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

    private static void AddParameter(IDbCommand command, string name, object value)
    {
        IDbDataParameter param = command.CreateParameter();
        param.ParameterName = name;
        param.Value = value;
        command.Parameters.Add(param);
    }
}
