using System.Data;
using CommunityHub.Application.Services.Citizen;

namespace CommunityHub.Application.Database.Repositories.Citizen;

public class CitizenDistrictMembershipDbRepository : ICitizenDistrictMembershipRepository
{
    public bool HasMembership(long userId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT 1
            FROM district_memberships
            WHERE user_id = @userId
            LIMIT 1";

        AddParameter(cmd, "@userId", userId);

        object? result = cmd.ExecuteScalar();
        return result != null;
    }

    public long? GetMembershipDistrictId(long userId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT district_id
            FROM district_memberships
            WHERE user_id = @userId
            ORDER BY join_date DESC
            LIMIT 1";

        AddParameter(cmd, "@userId", userId);

        object? result = cmd.ExecuteScalar();
        return result == null || result == DBNull.Value ? null : Convert.ToInt64(result);
    }

    public long Create(long userId, long districtId, DateTime? joinDate = null)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO district_memberships (district_id, user_id, join_date)
            VALUES (@districtId, @userId, @joinDate)
            RETURNING id";

        AddParameter(cmd, "@districtId", districtId);
        AddParameter(cmd, "@userId", userId);
        AddParameter(cmd, "@joinDate", (object)(joinDate ?? DateTime.Now));

        return Convert.ToInt64(cmd.ExecuteScalar());
    }

    private void AddParameter(IDbCommand command, string name, object value)
    {
        IDbDataParameter param = command.CreateParameter();
        param.ParameterName = name;
        param.Value = value;
        command.Parameters.Add(param);
    }
}

