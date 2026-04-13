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
        cmd.CommandText = @"
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

        AddParameter(cmd, "@coordinatorId", coordinatorId);

        List<DistrictAccessRequest> requests = new List<DistrictAccessRequest>();

        using IDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            string? userAddress = reader.IsDBNull(reader.GetOrdinal("address"))
                ? null
                : reader.GetString(reader.GetOrdinal("address"));

            User user = new User(
                reader.GetInt64(reader.GetOrdinal("user_id")),
                reader.GetString(reader.GetOrdinal("username")),
                reader.GetString(reader.GetOrdinal("password")),
                reader.GetString(reader.GetOrdinal("name")),
                reader.GetString(reader.GetOrdinal("surname")),
                reader.GetDateTime(reader.GetOrdinal("birthday")),
                reader.GetString(reader.GetOrdinal("role")),
                userAddress);

            Location location = new Location(
                reader.GetInt64(reader.GetOrdinal("location_id")),
                reader.GetString(reader.GetOrdinal("city")),
                reader.GetString(reader.GetOrdinal("country")));

            User coordinator = new User(coordinatorId, "", "", "", "", DateTime.MinValue, "DistrictCoordinator");

            District district = new District(
                reader.GetInt64(reader.GetOrdinal("district_id")),
                reader.GetString(reader.GetOrdinal("district_name")),
                reader.GetString(reader.GetOrdinal("description")),
                location,
                reader.GetDecimal(reader.GetOrdinal("budget")),
                coordinator);

            string statusStr = reader.GetString(reader.GetOrdinal("status"));
            DistrictAccessRequestStatus status = Enum.Parse<DistrictAccessRequestStatus>(statusStr);

            string? rejectionReason = reader.IsDBNull(reader.GetOrdinal("rejection_reason"))
                ? null
                : reader.GetString(reader.GetOrdinal("rejection_reason"));

            DistrictAccessRequest request = new DistrictAccessRequest(
                reader.GetInt64(reader.GetOrdinal("id")),
                user,
                district,
                reader.GetDateTime(reader.GetOrdinal("creation_date")),
                status,
                rejectionReason);

            requests.Add(request);
        }

        return requests;
    }

    public void Approve(long requestId)
    {
        using IDbConnection connection = PostgresConnection.CreateConnection();

        IDbCommand selectCmd = connection.CreateCommand();
        selectCmd.CommandText = @"
            SELECT user_id, district_id
            FROM district_access_requests
            WHERE id = @requestId";

        AddParameter(selectCmd, "@requestId", requestId);

        long userId;
        long districtId;

        using (IDataReader reader = selectCmd.ExecuteReader())
        {
            if (!reader.Read())
                throw new InvalidOperationException("Zahtev nije pronadjen.");

            userId = reader.GetInt64(reader.GetOrdinal("user_id"));
            districtId = reader.GetInt64(reader.GetOrdinal("district_id"));
        }

        DateTime now = DateTime.Now;

        IDbCommand updateCmd = connection.CreateCommand();
        updateCmd.CommandText = @"
            UPDATE district_access_requests
            SET status = 'Approved'
            WHERE id = @requestId";

        AddParameter(updateCmd, "@requestId", requestId);
        updateCmd.ExecuteNonQuery();

        IDbCommand membershipCmd = connection.CreateCommand();
        membershipCmd.CommandText = @"
            INSERT INTO district_memberships (district_id, user_id, join_date)
            VALUES (@districtId, @userId, @joinDate)";

        AddParameter(membershipCmd, "@districtId", districtId);
        AddParameter(membershipCmd, "@userId", userId);
        AddParameter(membershipCmd, "@joinDate", now);
        membershipCmd.ExecuteNonQuery();
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
            string? userAddress = reader.IsDBNull(reader.GetOrdinal("address"))
                ? null
                : reader.GetString(reader.GetOrdinal("address"));

            User user = new User(
                reader.GetInt64(reader.GetOrdinal("user_id")),
                reader.GetString(reader.GetOrdinal("username")),
                reader.GetString(reader.GetOrdinal("password")),
                reader.GetString(reader.GetOrdinal("name")),
                reader.GetString(reader.GetOrdinal("surname")),
                reader.GetDateTime(reader.GetOrdinal("birthday")),
                reader.GetString(reader.GetOrdinal("role")),
                userAddress);

            DistrictMembership membership = new DistrictMembership(
                reader.GetInt64(reader.GetOrdinal("id")),
                null!,
                user,
                reader.GetDateTime(reader.GetOrdinal("join_date")));

            memberships.Add(membership);
        }

        return memberships;
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
            Location location = new Location(
                reader.GetInt64(reader.GetOrdinal("location_id")),
                reader.GetString(reader.GetOrdinal("city")),
                reader.GetString(reader.GetOrdinal("country")));

            User coordinator = new User(coordinatorId, "", "", "", "", DateTime.MinValue, "DistrictCoordinator");

            District district = new District(
                reader.GetInt64(reader.GetOrdinal("id")),
                reader.GetString(reader.GetOrdinal("name")),
                reader.GetString(reader.GetOrdinal("description")),
                location,
                reader.GetDecimal(reader.GetOrdinal("budget")),
                coordinator);

            districts.Add(district);
        }

        return districts;
    }

    private void AddParameter(IDbCommand command, string name, object value)
    {
        IDbDataParameter parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
}
