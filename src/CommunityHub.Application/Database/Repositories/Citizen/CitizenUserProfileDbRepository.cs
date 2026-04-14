using System.Data;

namespace CommunityHub.Application.Database.Repositories.Citizen;

public class CitizenUserProfileDbRepository
{
    public string? GetAddressByUserId(long userId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT address
            FROM users
            WHERE id = @userId";

        AddParameter(cmd, "@userId", userId);

        object? result = cmd.ExecuteScalar();
        return result == null || result == DBNull.Value ? null : result.ToString();
    }

    private void AddParameter(IDbCommand command, string name, object value)
    {
        IDbDataParameter param = command.CreateParameter();
        param.ParameterName = name;
        param.Value = value;
        command.Parameters.Add(param);
    }
}

