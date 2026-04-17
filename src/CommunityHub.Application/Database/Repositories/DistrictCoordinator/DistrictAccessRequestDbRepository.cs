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

    public List<DistrictAccessRequest> GetByCoordinatorId(long coordinatorId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = BuildGetByCoordinatorQuery();
        AddParameter(cmd, "@coordinatorId", coordinatorId);

        List<DistrictAccessRequest> requests = new List<DistrictAccessRequest>();
        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            requests.Add(MapAccessRequest(reader, coordinatorId));
        }

        return requests;
    }

    private static string BuildGetByCoordinatorQuery()
    {
        return @"
            SELECT r.id, r.creation_date, r.status, r.rejection_reason,
                   u.id AS user_id, u.username, u.password, u.name, u.surname, u.birthday, u.role, u.address,
                   d.id AS district_id, d.name AS district_name, d.description, d.budget,
                   l.id AS location_id, l.city, l.country
            FROM district_access_requests r
            JOIN users u ON u.id = r.user_id
            JOIN districts d ON d.id = r.district_id
            JOIN locations l ON l.id = d.location_id
            WHERE d.coordinator_id = @coordinatorId
            ORDER BY r.creation_date DESC";
    }

    private static DistrictAccessRequest MapAccessRequest(IDataReader reader, long coordinatorId)
    {
        User user = MapUserFromReader(reader);
        District district = MapDistrictFromReader(reader, coordinatorId);

        DistrictAccessRequestStatus status = Enum.Parse<DistrictAccessRequestStatus>(
            reader.GetString(reader.GetOrdinal("status")));
        string? rejectionReason = ReadNullableString(reader, "rejection_reason");

        return new DistrictAccessRequest(
            reader.GetInt64(reader.GetOrdinal("id")),
            user, district,
            reader.GetDateTime(reader.GetOrdinal("creation_date")),
            status, rejectionReason);
    }

    private static User MapUserFromReader(IDataReader reader)
    {
        string? userAddress = ReadNullableString(reader, "address");
        return new User(
            reader.GetInt64(reader.GetOrdinal("user_id")),
            reader.GetString(reader.GetOrdinal("username")),
            reader.GetString(reader.GetOrdinal("password")),
            reader.GetString(reader.GetOrdinal("name")),
            reader.GetString(reader.GetOrdinal("surname")),
            reader.GetDateTime(reader.GetOrdinal("birthday")),
            reader.GetString(reader.GetOrdinal("role")),
            userAddress);
    }

    private static District MapDistrictFromReader(IDataReader reader, long coordinatorId)
    {
        Location location = new Location(
            reader.GetInt64(reader.GetOrdinal("location_id")),
            reader.GetString(reader.GetOrdinal("city")),
            reader.GetString(reader.GetOrdinal("country")));

        User coordinator = new User(coordinatorId, "", "", "", "", DateTime.MinValue, "DistrictCoordinator");

        return new District(
            reader.GetInt64(reader.GetOrdinal("district_id")),
            reader.GetString(reader.GetOrdinal("district_name")),
            reader.GetString(reader.GetOrdinal("description")),
            location,
            reader.GetDecimal(reader.GetOrdinal("budget")),
            coordinator);
    }

    private static string? ReadNullableString(IDataReader reader, string column)
    {
        int ord = reader.GetOrdinal(column);
        return reader.IsDBNull(ord) ? null : reader.GetString(ord);
    }

    public void Approve(long requestId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        (long userId, long districtId) = GetRequestUserAndDistrict(connection, requestId);
        UpdateRequestStatus(connection, requestId, "Approved");
        InsertMembership(connection, districtId, userId);
    }

    private (long userId, long districtId) GetRequestUserAndDistrict(IDbConnection connection, long requestId)
    {
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT user_id, district_id
            FROM district_access_requests
            WHERE id = @requestId";

        AddParameter(cmd, "@requestId", requestId);

        using IDataReader reader = cmd.ExecuteReader();
        if (!reader.Read())
            throw new InvalidOperationException("Zahtev nije pronadjen.");

        long userId = reader.GetInt64(reader.GetOrdinal("user_id"));
        long districtId = reader.GetInt64(reader.GetOrdinal("district_id"));
        return (userId, districtId);
    }

    private void UpdateRequestStatus(IDbConnection connection, long requestId, string status)
    {
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            UPDATE district_access_requests
            SET status = @status
            WHERE id = @requestId";

        AddParameter(cmd, "@requestId", requestId);
        AddParameter(cmd, "@status", status);
        cmd.ExecuteNonQuery();
    }

    private void InsertMembership(IDbConnection connection, long districtId, long userId)
    {
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO district_memberships (district_id, user_id, join_date)
            VALUES (@districtId, @userId, @joinDate)";

        AddParameter(cmd, "@districtId", districtId);
        AddParameter(cmd, "@userId", userId);
        AddParameter(cmd, "@joinDate", DateTime.Now);
        cmd.ExecuteNonQuery();
    }

    public void Reject(long requestId, string? reason)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            UPDATE district_access_requests
            SET status = 'Rejected',
                rejection_reason = @reason
            WHERE id = @requestId";

        AddParameter(cmd, "@requestId", requestId);
        AddParameter(cmd, "@reason", (object?)reason ?? DBNull.Value);
        cmd.ExecuteNonQuery();
    }

    public List<DistrictMembership> GetMembershipsByDistrictId(long districtId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT m.id, m.join_date,
                   u.id AS user_id, u.username, u.password, u.name, u.surname, u.birthday, u.role, u.address
            FROM district_memberships m
            JOIN users u ON u.id = m.user_id
            WHERE m.district_id = @districtId
            ORDER BY m.join_date DESC";

        AddParameter(cmd, "@districtId", districtId);

        List<DistrictMembership> memberships = new List<DistrictMembership>();
        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            memberships.Add(MapMembership(reader));
        }
        return memberships;
    }

    private static DistrictMembership MapMembership(IDataReader reader)
    {
        User user = MapUserFromReader(reader);
        return new DistrictMembership(
            reader.GetInt64(reader.GetOrdinal("id")),
            null!,
            user,
            reader.GetDateTime(reader.GetOrdinal("join_date")));
    }

    public List<District> GetDistrictsByCoordinatorId(long coordinatorId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT d.id, d.name, d.description, d.budget,
                   l.id AS location_id, l.city, l.country
            FROM districts d
            JOIN locations l ON l.id = d.location_id
            WHERE d.coordinator_id = @coordinatorId
            ORDER BY d.name";

        AddParameter(cmd, "@coordinatorId", coordinatorId);

        List<District> districts = new List<District>();
        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            districts.Add(MapDistrictBasic(reader, coordinatorId));
        }
        return districts;
    }

    private static District MapDistrictBasic(IDataReader reader, long coordinatorId)
    {
        Location location = new Location(
            reader.GetInt64(reader.GetOrdinal("location_id")),
            reader.GetString(reader.GetOrdinal("city")),
            reader.GetString(reader.GetOrdinal("country")));

        User coordinator = new User(coordinatorId, "", "", "", "", DateTime.MinValue, "DistrictCoordinator");

        return new District(
            reader.GetInt64(reader.GetOrdinal("id")),
            reader.GetString(reader.GetOrdinal("name")),
            reader.GetString(reader.GetOrdinal("description")),
            location,
            reader.GetDecimal(reader.GetOrdinal("budget")),
            coordinator);
    }

    private void AddParameter(IDbCommand command, string name, object value)
    {
        IDbDataParameter parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
}
