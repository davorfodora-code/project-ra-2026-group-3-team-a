using System;
using System.Data;
using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Database.Repositories.DistrictCoordinator;

public class DistrictAccessRequestDbRepository
{
    public long Create(long userId, long districtId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO district_access_requests (user_id, district_id, creation_date, status)
            VALUES (@userId, @districtId, @creationDate, @status)
            RETURNING id";

        AddParameter(command, "@userId", userId);
        AddParameter(command, "@districtId", districtId);
        AddParameter(command, "@creationDate", DateTime.Now);
        AddParameter(command, "@status", DistrictAccessRequestStatus.Pending.ToString());

        return Convert.ToInt64(command.ExecuteScalar());
    }

    public bool ExistsPending(long userId, long districtId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand command = connection.CreateCommand();
        command.CommandText = @"
            SELECT 1
            FROM district_access_requests
            WHERE user_id = @userId
              AND district_id = @districtId
              AND status = @status
            LIMIT 1";

        AddParameter(command, "@userId", userId);
        AddParameter(command, "@districtId", districtId);
        AddParameter(command, "@status", DistrictAccessRequestStatus.Pending.ToString());

        object? result = command.ExecuteScalar();
        return result != null;
    }

    private void AddParameter(IDbCommand command, string name, object value)
    {
        IDbDataParameter parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
}
