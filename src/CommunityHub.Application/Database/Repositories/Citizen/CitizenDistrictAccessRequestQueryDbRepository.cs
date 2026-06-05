using System.Data;
using CommunityHub.Application.Domain;

namespace CommunityHub.Application.Database.Repositories.Citizen;

internal sealed class CitizenDistrictAccessRequestQueryDbRepository
{
    public bool ExistsPending(long userId, long districtId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT 1
            FROM district_access_requests
            WHERE user_id = @userId
              AND district_id = @districtId
              AND status = 'Pending'
            LIMIT 1";

        AddParameter(cmd, "@userId", userId);
        AddParameter(cmd, "@districtId", districtId);

        object? result = cmd.ExecuteScalar();
        return result != null;
    }

    public List<DistrictAccessRequest> GetAllByUserId(long userId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT r.id, r.creation_date, r.status, r.rejection_reason,
                   u.id AS user_id, u.username, u.password, u.name, u.surname, u.birthday, u.role, u.address,
                   d.id AS district_id, d.name AS district_name, d.description, d.budget,
                   l.id AS location_id, l.city, l.country
            FROM district_access_requests r
            JOIN users u ON u.id = r.user_id
            JOIN districts d ON d.id = r.district_id
            JOIN locations l ON l.id = d.location_id
            WHERE r.user_id = @userId
            ORDER BY r.creation_date DESC";

        AddParameter(cmd, "@userId", userId);

        List<DistrictAccessRequest> requests = new();

        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            requests.Add(CitizenDistrictAccessRequestMapper.MapAccessRequestRow(reader));
        }

        return requests;
    }

    private static void AddParameter(IDbCommand command, string name, object value)
    {
        IDbDataParameter param = command.CreateParameter();
        param.ParameterName = name;
        param.Value = value;
        command.Parameters.Add(param);
    }
}
