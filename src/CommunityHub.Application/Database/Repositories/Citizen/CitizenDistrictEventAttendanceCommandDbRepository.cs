using System.Data;

namespace CommunityHub.Application.Database.Repositories.Citizen;

internal sealed class CitizenDistrictEventAttendanceCommandDbRepository
{
    public void UpsertAttendance(long eventId, long volunteerId, bool attended, DateTime markedAt)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO district_event_volunteer_attendances (event_id, volunteer_id, attended, marked_at)
            VALUES (@eventId, @volunteerId, @attended, @markedAt)
            ON CONFLICT (event_id, volunteer_id)
            DO UPDATE SET attended = EXCLUDED.attended, marked_at = EXCLUDED.marked_at";

        AddParameter(cmd, "@eventId", eventId);
        AddParameter(cmd, "@volunteerId", volunteerId);
        AddParameter(cmd, "@attended", attended);
        AddParameter(cmd, "@markedAt", markedAt);
        cmd.ExecuteNonQuery();
    }

    private static void AddParameter(IDbCommand command, string name, object value)
    {
        IDbDataParameter param = command.CreateParameter();
        param.ParameterName = name;
        param.Value = value;
        command.Parameters.Add(param);
    }
}
