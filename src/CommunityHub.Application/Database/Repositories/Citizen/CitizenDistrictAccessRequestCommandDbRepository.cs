using System.Data;

namespace CommunityHub.Application.Database.Repositories.Citizen;

internal sealed class CitizenDistrictAccessRequestCommandDbRepository
{
    public long Create(long userId, long districtId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO district_access_requests (user_id, district_id, creation_date, status)
            VALUES (@userId, @districtId, @creationDate, 'Pending')
            RETURNING id";

        AddParameter(cmd, "@userId", userId);
        AddParameter(cmd, "@districtId", districtId);
        AddParameter(cmd, "@creationDate", DateTime.Now);

        return Convert.ToInt64(cmd.ExecuteScalar());
    }

    public void Delete(long requestId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM district_access_requests WHERE id = @requestId";

        AddParameter(cmd, "@requestId", requestId);
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
