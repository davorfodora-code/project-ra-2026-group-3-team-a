using System.Data;

namespace CommunityHub.Application.Database.Repositories.Citizen;

internal sealed class CitizenDistrictEventApplicationQueryDbRepository
{
    public bool ExistsByEventAndCitizen(long eventId, long citizenId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT 1
            FROM district_event_applications
            WHERE event_id = @eventId
              AND citizen_id = @citizenId
            LIMIT 1";
        AddParameter(cmd, "@eventId", eventId);
        AddParameter(cmd, "@citizenId", citizenId);

        object? result = cmd.ExecuteScalar();
        return result != null;
    }

    private static void AddParameter(IDbCommand command, string name, object value)
    {
        IDbDataParameter param = command.CreateParameter();
        param.ParameterName = name;
        param.Value = value;
        command.Parameters.Add(param);
    }
}
